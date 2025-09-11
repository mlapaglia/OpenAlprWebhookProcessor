import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { UserPasskeysComponent } from './user-passkeys.component';
import { AccountService } from 'app/_services';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { ConfirmationDialogComponent } from '../../../shared/confirmation-dialog/confirmation-dialog.component';

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

interface PasskeyDeleteResult {
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
      } as unknown as PublicKeyCredential)
    ),
  },
};

describe('UserPasskeysComponent', () => {
  let component: UserPasskeysComponent;
  let fixture: ComponentFixture<UserPasskeysComponent>;
  let mockAccountService: jasmine.SpyObj<AccountService>;
  let mockSnackbarService: jasmine.SpyObj<SnackbarService>;
  let mockDialog: jasmine.SpyObj<MatDialog>;
  let mockDialogRef: jasmine.SpyObj<MatDialogRef<UserPasskeysComponent>>;
  let mockConfirmationDialogRef: jasmine.SpyObj<MatDialogRef<ConfirmationDialogComponent>>;

  beforeEach(async () => {
    mockAccountService = jasmine.createSpyObj('AccountService', [
      'getPasskeys',
      'registerPasskey',
      'completePasskeyRegistration',
      'deletePasskey',
    ]);
    mockSnackbarService = jasmine.createSpyObj('SnackbarService', ['create']);
    mockDialog = jasmine.createSpyObj('MatDialog', ['open']);
    mockDialogRef = jasmine.createSpyObj('MatDialogRef', ['close']);
    mockConfirmationDialogRef = jasmine.createSpyObj('MatDialogRef', ['afterClosed']);

    // Mock global navigator for WebAuthn
    Object.defineProperty(globalThis, 'navigator', {
      value: mockNavigator,
      writable: true,
    });

    // Mock window.PublicKeyCredential
    Object.defineProperty(globalThis, 'PublicKeyCredential', {
      value: function () {},
      writable: true,
    });

    // Mock base64 encoding functions
    Object.defineProperty(globalThis, 'atob', {
      value: (str: string) => {
        // Simple base64 decode mock for testing
        return decodeURIComponent(escape(str));
      },
      writable: true,
    });
    Object.defineProperty(globalThis, 'btoa', {
      value: (str: string) => {
        // Simple base64 encode mock for testing
        return unescape(encodeURIComponent(str));
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
        { provide: MatDialog, useValue: mockDialog },
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
        passkeys: [
          { id: 1, name: 'Test Passkey', regDate: '2024-01-01', aaGuid: 'test-guid' },
        ],
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
        jasmine.any(Error)
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

    it('should handle WebAuthn not supported error', async () => {
      // Mock navigator.credentials as undefined
      Object.defineProperty(globalThis, 'navigator', {
        value: {
          userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
        },
        writable: true,
      });

      component.registrationForm.patchValue({ name: 'My Test Passkey' });

      await component.registerPasskey();

      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Failed to register passkey',
        SnackBarType.Error,
        'WebAuthn is not supported in this browser'
      );
      expect(component.registering).toBe(false);

      // Restore navigator
      Object.defineProperty(globalThis, 'navigator', {
        value: mockNavigator,
        writable: true,
      });
    });

    it('should handle registration options error', async () => {
      component.registrationForm.patchValue({ name: 'My Test Passkey' });

      mockAccountService.registerPasskey.and.returnValue(throwError(() => new Error('Options failed')));

      await component.registerPasskey();

      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Failed to register passkey',
        SnackBarType.Error,
        'Options failed'
      );
      expect(component.registering).toBe(false);
    });

    it('should handle credential creation failure', async () => {
      component.registrationForm.patchValue({ name: 'My Test Passkey' });

      const mockRegistrationOptions: PasskeyRegistrationOptions = {
        options: {
          rp: { id: 'test.com', name: 'Test' },
          user: { id: 'dGVzdC11c2VyLWlk' as unknown as ArrayBuffer, name: 'testuser', displayName: 'Test User' },
          challenge: 'dGVzdC1jaGFsbGVuZ2U' as unknown as ArrayBuffer,
          pubKeyCredParams: [],
        } as PublicKeyCredentialCreationOptions,
      };

      mockAccountService.registerPasskey.and.returnValue(of(mockRegistrationOptions));
      mockNavigator.credentials.create.and.returnValue(Promise.resolve(null));

      await component.registerPasskey();

      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Failed to register passkey',
        SnackBarType.Error,
        jasmine.any(String)
      );
      expect(component.registering).toBe(false);
    });

    it('should handle registration completion failure', async () => {
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
        success: false,
        message: 'Registration failed',
      };

      mockAccountService.registerPasskey.and.returnValue(of(mockRegistrationOptions));
      mockAccountService.completePasskeyRegistration.and.returnValue(of(mockRegistrationResult));

      await component.registerPasskey();

      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Failed to register passkey',
        SnackBarType.Error,
        jasmine.any(String)
      );
      expect(component.registering).toBe(false);
    });
  });

  describe('deletePasskey', () => {
    beforeEach(() => {
      // Initialize component
      const mockPasskeys: PasskeyListResponse = { passkeys: [] };
      mockAccountService.getPasskeys.and.returnValue(of(mockPasskeys));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should open confirmation dialog and delete passkey on confirmation', () => {
      const mockPasskey: PasskeyInfo = {
        id: 1,
        name: 'Test Passkey',
        regDate: '2024-01-01',
        aaGuid: 'test-guid',
      };

      const mockDeleteResult: PasskeyDeleteResult = {
        success: true,
        message: 'Deleted',
      };

      const mockUpdatedPasskeys: PasskeyListResponse = {
        passkeys: [],
      };

      mockConfirmationDialogRef.afterClosed.and.returnValue(of(true));
      mockDialog.open.and.returnValue(mockConfirmationDialogRef);
      mockAccountService.deletePasskey.and.returnValue(of(mockDeleteResult));
      mockAccountService.getPasskeys.and.returnValue(of(mockUpdatedPasskeys));

      component.deletePasskey(mockPasskey);

      expect(mockDialog.open).toHaveBeenCalledWith(
        ConfirmationDialogComponent,
        jasmine.objectContaining({
          width: '400px',
          data: jasmine.objectContaining({
            title: 'Delete Passkey',
            message: 'Are you sure you want to delete the passkey "Test Passkey"? This action cannot be undone.',
            confirmText: 'Delete',
            cancelText: 'Cancel',
            color: 'warn',
          }),
        })
      );

      expect(mockAccountService.deletePasskey).toHaveBeenCalledWith(1);
      expect(mockSnackbarService.create).toHaveBeenCalledWith('Passkey deleted successfully', SnackBarType.Saved);
    });

    it('should handle delete failure', () => {
      const mockPasskey: PasskeyInfo = {
        id: 1,
        name: 'Test Passkey',
        regDate: '2024-01-01',
        aaGuid: 'test-guid',
      };

      const mockDeleteResult: PasskeyDeleteResult = {
        success: false,
        message: 'Delete failed',
      };

      mockConfirmationDialogRef.afterClosed.and.returnValue(of(true));
      mockDialog.open.and.returnValue(mockConfirmationDialogRef);
      mockAccountService.deletePasskey.and.returnValue(of(mockDeleteResult));

      component.deletePasskey(mockPasskey);

      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Failed to delete passkey',
        SnackBarType.Error,
        'Delete failed'
      );
    });

    it('should handle delete service error', () => {
      const mockPasskey: PasskeyInfo = {
        id: 1,
        name: 'Test Passkey',
        regDate: '2024-01-01',
        aaGuid: 'test-guid',
      };

      mockConfirmationDialogRef.afterClosed.and.returnValue(of(true));
      mockDialog.open.and.returnValue(mockConfirmationDialogRef);
      mockAccountService.deletePasskey.and.returnValue(throwError(() => new Error('Service error')));

      component.deletePasskey(mockPasskey);

      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Failed to delete passkey',
        SnackBarType.Error,
        jasmine.any(Error)
      );
    });

    it('should not delete passkey if confirmation is cancelled', () => {
      const mockPasskey: PasskeyInfo = {
        id: 1,
        name: 'Test Passkey',
        regDate: '2024-01-01',
        aaGuid: 'test-guid',
      };

      mockConfirmationDialogRef.afterClosed.and.returnValue(of(false));
      mockDialog.open.and.returnValue(mockConfirmationDialogRef);

      component.deletePasskey(mockPasskey);

      expect(mockAccountService.deletePasskey).not.toHaveBeenCalled();
      expect(mockSnackbarService.create).not.toHaveBeenCalled();
    });
  });

  describe('onClose', () => {
    it('should close the dialog', () => {
      component.onClose();
      expect(mockDialogRef.close).toHaveBeenCalled();
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