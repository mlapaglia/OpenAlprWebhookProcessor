import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';

import { UserPasskeysComponent } from './user-passkeys.component';
import { AccountService } from 'app/_services';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';

interface PasskeyInfo {
  id: number;
  name: string;
  regDate: string;
  aaGuid: string;
}

interface PasskeyListResponse {
  passkeys: PasskeyInfo[];
}

interface PasskeyRegistrationOptions {
  options: PublicKeyCredentialCreationOptions;
}

interface PasskeyRegistrationResult {
  success: boolean;
  message: string;
}

// Mock global navigator.credentials for WebAuthn
const mockNavigator = {
  userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
  credentials: {
    create: jasmine.createSpy('create').and.returnValue(
      Promise.resolve({
        id: 'test-credential-id',
        rawId: new ArrayBuffer(8),
        type: 'public-key',
        authenticatorAttachment: null,
        response: {
          attestationObject: new ArrayBuffer(8),
          clientDataJSON: new ArrayBuffer(8),
        },
        getClientExtensionResults: () => ({}),
        toJSON: () => ({}),
      } as unknown as PublicKeyCredential),
    ),
    get: jasmine.createSpy('get').and.returnValue(Promise.resolve(null)),
  },
};

describe('UserPasskeysComponent', () => {
  let component: UserPasskeysComponent;
  let fixture: ComponentFixture<UserPasskeysComponent>;
  let mockAccountService: jasmine.SpyObj<AccountService>;
  let mockSnackbarService: jasmine.SpyObj<SnackbarService>;
  let mockDialogRef: jasmine.SpyObj<MatDialogRef<UserPasskeysComponent>>;
  beforeEach(async () => {
    mockAccountService = jasmine.createSpyObj('AccountService', [
      'getPasskeys',
      'registerPasskey',
      'completePasskeyRegistration',
    ]);
    mockSnackbarService = jasmine.createSpyObj('SnackbarService', ['create']);
    mockDialogRef = jasmine.createSpyObj('MatDialogRef', ['close']);

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
      imports: [
        UserPasskeysComponent,
        ReactiveFormsModule,
      ],
      providers: [
        FormBuilder,
        { provide: AccountService, useValue: mockAccountService },
        { provide: SnackbarService, useValue: mockSnackbarService },
        { provide: MatDialogRef, useValue: mockDialogRef },
        { provide: MAT_DIALOG_DATA, useValue: { userId: '123', userName: 'testuser' } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(UserPasskeysComponent);
    component = fixture.componentInstance;
  });

  describe('ngOnInit', () => {
    it('should initialize with dialog data and load passkeys', () => {
      const mockPasskeys: PasskeyListResponse = {
        passkeys: [{ id: 1, name: 'Test Passkey', regDate: '2024-01-01', aaGuid: 'test-guid' }],
      };

      mockAccountService.getPasskeys.and.returnValue(of(mockPasskeys));

      component.ngOnInit();
      fixture.detectChanges();

      expect(component.userId).toBe('123');
      expect(component.userName).toBe('testuser');
      expect(component.registrationForm).toBeDefined();
      expect(component.registrationForm.get('name')?.hasError('required')).toBe(true);
      expect(mockAccountService.getPasskeys).toHaveBeenCalled();
      expect(component.passkeys).toEqual(mockPasskeys.passkeys);
    });

    it('should handle error when loading passkeys', () => {
      mockAccountService.getPasskeys.and.returnValue(throwError(() => new Error('Load failed')));

      component.ngOnInit();
      fixture.detectChanges();

      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Failed to load passkeys',
        SnackBarType.Error,
        jasmine.any(Error),
      );
      expect(component.loading).toBe(false);
    });
  });

  describe('loadPasskeys', () => {
    it('should set loading state and load passkeys successfully', () => {
      const mockPasskeys: PasskeyListResponse = {
        passkeys: [
          { id: 1, name: 'Test Passkey 1', regDate: '2024-01-01', aaGuid: 'guid1' },
          { id: 2, name: 'Test Passkey 2', regDate: '2024-01-02', aaGuid: 'guid2' },
        ],
      };

      mockAccountService.getPasskeys.and.returnValue(of(mockPasskeys));

      component.loadPasskeys();

      expect(component.loading).toBe(false);
      expect(component.passkeys).toEqual(mockPasskeys.passkeys);
      expect(mockAccountService.getPasskeys).toHaveBeenCalled();
    });
  });

  describe('registerPasskey', () => {
    beforeEach(() => {
      // Initialize component first
      const mockPasskeys: PasskeyListResponse = { passkeys: [] };
      mockAccountService.getPasskeys.and.returnValue(of(mockPasskeys));
      component.ngOnInit();
      fixture.detectChanges();
      
      // Ensure WebAuthn support is available for each test
      Object.defineProperty(globalThis, 'navigator', {
        value: mockNavigator,
        writable: true,
      });
    });

    it('should not proceed if form is invalid', async () => {
      expect(component.registrationForm.invalid).toBe(true);

      await component.registerPasskey();

      expect(mockAccountService.registerPasskey).not.toHaveBeenCalled();
      expect(component.registering).toBe(false);
    });

    it('should successfully register a passkey', async () => {
      // Set up form with valid data
      component.registrationForm.patchValue({ name: 'My Test Passkey' });

      const mockRegistrationOptions: PasskeyRegistrationOptions = {
        options: {
          rp: { id: 'test.com', name: 'Test' },
          user: { id: 'dGVzdC11c2VyLWlk' as unknown as ArrayBuffer, name: 'testuser', displayName: 'Test User' },
          challenge: 'dGVzdC1jaGFsbGVuZ2U' as unknown as ArrayBuffer,
          pubKeyCredParams: [],
        } as PublicKeyCredentialCreationOptions,
      };

      const mockRegistrationResult: PasskeyRegistrationResult = {
        success: true,
        message: 'Success',
      };

      const mockUpdatedPasskeys: PasskeyListResponse = {
        passkeys: [{ id: 1, name: 'My Test Passkey', regDate: '2024-01-01', aaGuid: 'test' }],
      };

      mockAccountService.registerPasskey.and.returnValue(of(mockRegistrationOptions));
      mockAccountService.completePasskeyRegistration.and.returnValue(of(mockRegistrationResult));
      mockAccountService.getPasskeys.and.returnValue(of(mockUpdatedPasskeys));

      await component.registerPasskey();

      expect(mockAccountService.registerPasskey).toHaveBeenCalledWith('My Test Passkey');
      expect(mockAccountService.completePasskeyRegistration).toHaveBeenCalled();
      expect(mockSnackbarService.create).toHaveBeenCalledWith('Passkey registered successfully', SnackBarType.Saved);
      expect(component.registrationForm.get('name')?.value).toBeNull();
      expect(component.registering).toBe(false);
    });

  });



  describe('form validation', () => {
    beforeEach(() => {
      const mockPasskeys: PasskeyListResponse = { passkeys: [] };
      mockAccountService.getPasskeys.and.returnValue(of(mockPasskeys));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should require name field', () => {
      const nameControl = component.registrationForm.get('name');
      expect(nameControl?.hasError('required')).toBe(true);

      nameControl?.setValue('Test Passkey');
      expect(nameControl?.hasError('required')).toBe(false);
    });

    it('should enforce maxlength validation', () => {
      const nameControl = component.registrationForm.get('name');
      const longName = 'a'.repeat(51);

      nameControl?.setValue(longName);
      expect(nameControl?.hasError('maxlength')).toBe(true);

      nameControl?.setValue('Valid Name');
      expect(nameControl?.hasError('maxlength')).toBe(false);
    });
  });
});
