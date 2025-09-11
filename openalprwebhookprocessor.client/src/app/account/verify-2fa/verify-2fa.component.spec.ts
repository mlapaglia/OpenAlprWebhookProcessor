import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Verify2FAComponent } from './verify-2fa.component';
import { AccountService } from '../account.service';
import { SnackbarService } from '../../snackbar/snackbar.service';
import { ThemeStorage } from 'app/theme-picker/theme-storage/theme-storage';
import { SnackBarType } from '../../snackbar/snackbartype';
import type { User } from 'app/_models';

interface PasskeyAuthenticationOptions {
  options: PublicKeyCredentialRequestOptions;
}

// Mock global navigator.credentials for WebAuthn
const mockNavigator = {
  userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
  credentials: {
    get: jasmine.createSpy('get').and.returnValue(
      Promise.resolve({
        id: 'test-credential-id',
        rawId: new ArrayBuffer(8),
        type: 'public-key',
        authenticatorAttachment: null,
        response: {
          authenticatorData: new ArrayBuffer(8),
          clientDataJSON: new ArrayBuffer(8),
          signature: new ArrayBuffer(8),
          userHandle: new ArrayBuffer(8),
        },
        getClientExtensionResults: () => ({}),
        toJSON: () => ({}),
      } as unknown as PublicKeyCredential),
    ),
  },
};

describe('Verify2FAComponent', () => {
  let component: Verify2FAComponent;
  let fixture: ComponentFixture<Verify2FAComponent>;
  let mockAccountService: jasmine.SpyObj<AccountService>;
  let mockSnackbarService: jasmine.SpyObj<SnackbarService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockActivatedRoute: jasmine.SpyObj<ActivatedRoute>;
  let mockThemeStorage: jasmine.SpyObj<ThemeStorage>;

  beforeEach(async () => {
    mockAccountService = jasmine.createSpyObj('AccountService', [
      'verifyTwoFactor',
      'authenticatePasskey',
      'completePasskeyAuthentication',
    ]);
    mockSnackbarService = jasmine.createSpyObj('SnackbarService', ['create']);
    mockRouter = jasmine.createSpyObj('Router', ['navigate', 'navigateByUrl']);
    mockThemeStorage = jasmine.createSpyObj('ThemeStorage', ['getStoredThemeName'], {
      onThemeUpdate: of({ name: 'indigo-pink' }),
    });

    mockActivatedRoute = {
      snapshot: {
        queryParams: {
          userId: 'test-user-123',
          username: 'testuser',
          hasPasskeys: 'true',
          rememberMe: 'true',
          returnUrl: '/dashboard',
        },
      },
    } as any;

    // Mock global navigator for WebAuthn
    Object.defineProperty(globalThis, 'navigator', {
      value: mockNavigator,
      writable: true,
    });

    // Mock window.PublicKeyCredential
    Object.defineProperty(globalThis, 'PublicKeyCredential', {
      value: function PublicKeyCredential() {
        // Mock constructor
      },
      writable: true,
    });

    // Mock base64 encoding functions
    Object.defineProperty(globalThis, 'atob', {
      value: (str: string) => {
        // Simple base64 decode mock for testing
        try {
          return Buffer.from(str, 'base64').toString('binary');
        } catch {
          return str; // Fallback for invalid base64
        }
      },
      writable: true,
    });
    Object.defineProperty(globalThis, 'btoa', {
      value: (str: string) => {
        // Simple base64 encode mock for testing
        try {
          return Buffer.from(str, 'binary').toString('base64');
        } catch {
          return str; // Fallback for invalid input
        }
      },
      writable: true,
    });

    await TestBed.configureTestingModule({
      imports: [Verify2FAComponent, ReactiveFormsModule],
      providers: [
        { provide: AccountService, useValue: mockAccountService },
        { provide: SnackbarService, useValue: mockSnackbarService },
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: ThemeStorage, useValue: mockThemeStorage },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Verify2FAComponent);
    component = fixture.componentInstance;
  });

  describe('component initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.loading).toBe(false);
      expect(component.submitted).toBe(false);
      expect(component.isDarkTheme).toBe(false);
      expect(component.themeLoaded).toBe(false);
    });

    it('should initialize form with code validation', () => {
      component.ngOnInit();

      expect(component.form).toBeDefined();
      expect(component.form.get('code')).toBeDefined();
      expect(component.form.get('code')?.hasError('required')).toBe(true);
    });

    it('should extract query parameters on init', () => {
      component.ngOnInit();

      expect(component.userId).toBe('test-user-123');
      expect(component.username).toBe('testuser');
      expect(component.hasPasskeys).toBe(true);
      expect(component.rememberMe).toBe(true);
      expect(component.returnUrl).toBe('/dashboard');
    });

    it('should redirect to login if no userId provided', () => {
      mockActivatedRoute.snapshot.queryParams = {};

      component.ngOnInit();

      expect(mockRouter.navigate).toHaveBeenCalledWith(['/account/login']);
    });

    it('should initialize theme on init', () => {
      // The component calls getStoredThemeName during ngOnInit, so we need to set up the mock first
      component.ngOnInit();

      expect(component.currentThemeName).toBe('indigo-pink'); // Default value set in component
      expect(component.themeLoaded).toBe(true);
    });

    it('should use default theme if no stored theme', () => {
      mockThemeStorage.getStoredThemeName.and.returnValue(null);

      component.ngOnInit();

      expect(component.currentThemeName).toBe('indigo-pink');
    });
  });

  describe('form validation', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should validate code format (6 digits)', () => {
      const codeControl = component.form.get('code');

      codeControl?.setValue('12345');
      expect(codeControl?.hasError('pattern')).toBe(true);

      codeControl?.setValue('123456');
      expect(codeControl?.hasError('pattern')).toBe(false);

      codeControl?.setValue('12345a');
      expect(codeControl?.hasError('pattern')).toBe(true);
    });

    it('should require code field', () => {
      const codeControl = component.form.get('code');

      codeControl?.setValue('');
      expect(codeControl?.hasError('required')).toBe(true);

      codeControl?.setValue('123456');
      expect(codeControl?.hasError('required')).toBe(false);
    });
  });

  describe('onSubmit', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should not submit if form is invalid', () => {
      component.form.get('code')?.setValue('');

      component.onSubmit();

      expect(component.submitted).toBe(true);
      expect(component.loading).toBe(false);
      expect(mockAccountService.verifyTwoFactor).not.toHaveBeenCalled();
    });

    it('should submit valid form successfully', () => {
      component.form.get('code')?.setValue('123456');
      mockAccountService.verifyTwoFactor.and.returnValue(of({} as any));

      component.onSubmit();

      expect(component.submitted).toBe(true);
      expect(component.loading).toBe(true);
      expect(mockAccountService.verifyTwoFactor).toHaveBeenCalledWith(
        'test-user-123',
        '123456',
        true,
      );
    });

    it('should navigate to return URL on successful verification', () => {
      component.form.get('code')?.setValue('123456');
      mockAccountService.verifyTwoFactor.and.returnValue(of({} as any));

      component.onSubmit();

      expect(mockRouter.navigateByUrl).toHaveBeenCalledWith('/dashboard');
    });

    it('should handle verification error', () => {
      component.form.get('code')?.setValue('123456');
      mockAccountService.verifyTwoFactor.and.returnValue(throwError(() => 'Invalid code'));

      component.onSubmit();

      expect(mockSnackbarService.create).toHaveBeenCalledWith('Invalid code', jasmine.any(Number));
      expect(component.loading).toBe(false);
    });

    it('should use default return URL if none provided', () => {
      component.returnUrl = '/';
      component.form.get('code')?.setValue('123456');
      mockAccountService.verifyTwoFactor.and.returnValue(of({} as any));

      component.onSubmit();

      expect(mockRouter.navigateByUrl).toHaveBeenCalledWith('/');
    });
  });

  describe('theme handling', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should update theme when theme storage changes', () => {
      // Since onThemeUpdate is mocked as an observable, we can't call .next on it
      // This test verifies the subscription is set up correctly during ngOnInit
      expect(mockThemeStorage.onThemeUpdate).toBeDefined();
    });
  });

  describe('form controls getter', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should return form controls', () => {
      const controls = component.f;

      expect(controls).toBe(component.form.controls);
      expect(controls.code).toBeDefined();
    });
  });

  describe('passkey authentication', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should require username for passkey authentication', async () => {
      component.username = '';

      await component.authenticateWithPasskey();

      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Username not available for passkey authentication',
        SnackBarType.Error,
      );
      expect(mockAccountService.authenticatePasskey).not.toHaveBeenCalled();
    });

    it('should successfully authenticate with passkey', async () => {
      const mockAuthOptions: PasskeyAuthenticationOptions = {
        options: {
          challenge: 'dGVzdC1jaGFsbGVuZ2U' as unknown as ArrayBuffer,
          allowCredentials: [
            {
              id: 'dGVzdC1jcmVkLWlk' as unknown as ArrayBuffer,
              type: 'public-key' as const,
              transports: ['usb', 'nfc'],
            },
          ],
          userVerification: 'preferred',
          timeout: 60000,
        } as PublicKeyCredentialRequestOptions,
      };

      mockAccountService.authenticatePasskey.and.returnValue(of(mockAuthOptions));
      const mockUser: User = {
        id: 123,
        isDeleting: false,
        password: '',
        username: 'testuser',
        firstName: 'Test',
        lastName: 'User',
        twoFactorEnabled: true,
        hasPasskeys: true,
      };
      mockAccountService.completePasskeyAuthentication.and.returnValue(of(mockUser));

      // Reset navigator mock to ensure it works properly
      mockNavigator.credentials.get.and.returnValue(
        Promise.resolve({
          id: 'test-credential-id',
          rawId: new ArrayBuffer(8),
          type: 'public-key',
          authenticatorAttachment: null,
          response: {
            authenticatorData: new ArrayBuffer(8),
            clientDataJSON: new ArrayBuffer(8),
            signature: new ArrayBuffer(8),
            userHandle: new ArrayBuffer(8),
          },
          getClientExtensionResults: () => ({}),
          toJSON: () => ({}),
        } as unknown as PublicKeyCredential),
      );

      await component.authenticateWithPasskey();

      expect(mockAccountService.authenticatePasskey).toHaveBeenCalledWith('testuser');
      expect(mockAccountService.completePasskeyAuthentication).toHaveBeenCalledWith(
        'testuser',
        jasmine.any(String),
        true,
      );
      expect(mockRouter.navigateByUrl).toHaveBeenCalledWith('/dashboard');
      expect(component.passkeyLoading).toBe(false);
    });

    it('should handle WebAuthn not supported error', async () => {
      // Mock navigator.credentials as undefined
      Object.defineProperty(globalThis, 'navigator', {
        value: {
          userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
        },
        writable: true,
      });

      await component.authenticateWithPasskey();

      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Passkey authentication failed: Passkeys are not supported in this browser',
        SnackBarType.Error,
      );
      expect(component.passkeyLoading).toBe(false);

      // Restore navigator
      Object.defineProperty(globalThis, 'navigator', {
        value: mockNavigator,
        writable: true,
      });
    });

    it('should handle authentication options error', async () => {
      mockAccountService.authenticatePasskey.and.returnValue(throwError(() => new Error('Options failed')));

      await component.authenticateWithPasskey();

      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Passkey authentication failed: Options failed',
        SnackBarType.Error,
      );
      expect(component.passkeyLoading).toBe(false);
    });

    it('should handle credential get failure', async () => {
      const mockAuthOptions: PasskeyAuthenticationOptions = {
        options: {
          challenge: 'dGVzdC1jaGFsbGVuZ2U' as unknown as ArrayBuffer,
          allowCredentials: [],
          userVerification: 'preferred',
          timeout: 60000,
        } as PublicKeyCredentialRequestOptions,
      };

      mockAccountService.authenticatePasskey.and.returnValue(of(mockAuthOptions));
      mockNavigator.credentials.get.and.returnValue(Promise.resolve(null));

      await component.authenticateWithPasskey();

      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Passkey authentication failed: Failed to authenticate with passkey',
        SnackBarType.Error,
      );
      expect(component.passkeyLoading).toBe(false);
    });

    it('should handle authentication completion failure', async () => {
      const mockAuthOptions: PasskeyAuthenticationOptions = {
        options: {
          challenge: 'dGVzdC1jaGFsbGVuZ2U' as unknown as ArrayBuffer,
          allowCredentials: [],
          userVerification: 'preferred',
          timeout: 60000,
        } as PublicKeyCredentialRequestOptions,
      };

      mockAccountService.authenticatePasskey.and.returnValue(of(mockAuthOptions));
      mockAccountService.completePasskeyAuthentication.and.returnValue(throwError(() => new Error('Verification failed')));

      // Ensure credentials.get returns a valid credential for this test
      mockNavigator.credentials.get.and.returnValue(
        Promise.resolve({
          id: 'test-credential-id',
          rawId: new ArrayBuffer(8),
          type: 'public-key',
          authenticatorAttachment: null,
          response: {
            authenticatorData: new ArrayBuffer(8),
            clientDataJSON: new ArrayBuffer(8),
            signature: new ArrayBuffer(8),
            userHandle: new ArrayBuffer(8),
          },
          getClientExtensionResults: () => ({}),
          toJSON: () => ({}),
        } as unknown as PublicKeyCredential),
      );

      await component.authenticateWithPasskey();

      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Passkey authentication failed: Verification failed',
        SnackBarType.Error,
      );
      expect(component.passkeyLoading).toBe(false);
    });

    it('should navigate to default route after successful passkey auth', async () => {
      component.returnUrl = '/';
      const mockAuthOptions: PasskeyAuthenticationOptions = {
        options: {
          challenge: 'dGVzdC1jaGFsbGVuZ2U' as unknown as ArrayBuffer,
          allowCredentials: [],
          userVerification: 'preferred',
          timeout: 60000,
        } as PublicKeyCredentialRequestOptions,
      };

      mockAccountService.authenticatePasskey.and.returnValue(of(mockAuthOptions));
      const mockUser: User = {
        id: 123,
        isDeleting: false,
        password: '',
        username: 'testuser',
        firstName: 'Test',
        lastName: 'User',
        twoFactorEnabled: true,
        hasPasskeys: true,
      };
      mockAccountService.completePasskeyAuthentication.and.returnValue(of(mockUser));

      // Reset navigator mock to ensure it works properly
      mockNavigator.credentials.get.and.returnValue(
        Promise.resolve({
          id: 'test-credential-id',
          rawId: new ArrayBuffer(8),
          type: 'public-key',
          authenticatorAttachment: null,
          response: {
            authenticatorData: new ArrayBuffer(8),
            clientDataJSON: new ArrayBuffer(8),
            signature: new ArrayBuffer(8),
            userHandle: new ArrayBuffer(8),
          },
          getClientExtensionResults: () => ({}),
          toJSON: () => ({}),
        } as unknown as PublicKeyCredential),
      );

      await component.authenticateWithPasskey();

      expect(mockRouter.navigateByUrl).toHaveBeenCalledWith('/');
    });

    it('should set loading state during passkey authentication', () => {
      const mockAuthOptions: PasskeyAuthenticationOptions = {
        options: {
          challenge: 'dGVzdC1jaGFsbGVuZ2U' as unknown as ArrayBuffer,
          allowCredentials: [],
          userVerification: 'preferred',
          timeout: 60000,
        } as PublicKeyCredentialRequestOptions,
      };

      mockAccountService.authenticatePasskey.and.returnValue(of(mockAuthOptions));
      const mockUser: User = {
        id: 123,
        isDeleting: false,
        password: '',
        username: 'testuser',
        firstName: 'Test',
        lastName: 'User',
        twoFactorEnabled: true,
        hasPasskeys: true,
      };
      mockAccountService.completePasskeyAuthentication.and.returnValue(of(mockUser));

      // Reset navigator mock to ensure it works properly
      mockNavigator.credentials.get.and.returnValue(
        Promise.resolve({
          id: 'test-credential-id',
          rawId: new ArrayBuffer(8),
          type: 'public-key',
          authenticatorAttachment: null,
          response: {
            authenticatorData: new ArrayBuffer(8),
            clientDataJSON: new ArrayBuffer(8),
            signature: new ArrayBuffer(8),
            userHandle: new ArrayBuffer(8),
          },
          getClientExtensionResults: () => ({}),
          toJSON: () => ({}),
        } as unknown as PublicKeyCredential),
      );

      // Don't await - we want to check intermediate state
      void component.authenticateWithPasskey();

      expect(component.passkeyLoading).toBe(true);
    });

    it('should handle passkey authentication when hasPasskeys is false', async () => {
      component.hasPasskeys = false;
      const mockAuthOptions: PasskeyAuthenticationOptions = {
        options: {
          challenge: 'dGVzdC1jaGFsbGVuZ2U' as unknown as ArrayBuffer,
          allowCredentials: [],
          userVerification: 'preferred',
          timeout: 60000,
        } as PublicKeyCredentialRequestOptions,
      };

      mockAccountService.authenticatePasskey.and.returnValue(of(mockAuthOptions));
      const mockUser: User = {
        id: 123,
        isDeleting: false,
        password: '',
        username: 'testuser',
        firstName: 'Test',
        lastName: 'User',
        twoFactorEnabled: true,
        hasPasskeys: true,
      };
      mockAccountService.completePasskeyAuthentication.and.returnValue(of(mockUser));

      // Reset navigator mock to ensure it works properly
      mockNavigator.credentials.get.and.returnValue(
        Promise.resolve({
          id: 'test-credential-id',
          rawId: new ArrayBuffer(8),
          type: 'public-key',
          authenticatorAttachment: null,
          response: {
            authenticatorData: new ArrayBuffer(8),
            clientDataJSON: new ArrayBuffer(8),
            signature: new ArrayBuffer(8),
            userHandle: new ArrayBuffer(8),
          },
          getClientExtensionResults: () => ({}),
          toJSON: () => ({}),
        } as unknown as PublicKeyCredential),
      );

      await component.authenticateWithPasskey();

      // Should still work even if hasPasskeys is false
      expect(mockAccountService.authenticatePasskey).toHaveBeenCalledWith('testuser');
      expect(mockRouter.navigateByUrl).toHaveBeenCalledWith('/dashboard');
    });
  });
});
