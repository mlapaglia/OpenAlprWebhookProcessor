import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

import { User2FAComponent } from './user-2fa.component';
import { AccountService, AlertService } from 'app/_services';

interface TwoFactorStatus {
  isTwoFactorEnabled: boolean;
  hasAuthenticator: boolean;
}

interface TwoFactorSetup {
  qrCodeUri: string;
  sharedKey: string;
}

interface EnableTwoFactorResult {
  message: string;
  recoveryCodes: string[];
}

interface RecoveryCodesResult {
  recoveryCodes: string[];
}

describe('User2FAComponent', () => {
  let component: User2FAComponent;
  let fixture: ComponentFixture<User2FAComponent>;
  let mockAccountService: jasmine.SpyObj<AccountService>;
  let mockAlertService: jasmine.SpyObj<AlertService>;
  let mockDialog: jasmine.SpyObj<MatDialog>;
  let mockDialogRef: jasmine.SpyObj<MatDialogRef<User2FAComponent>>;

  beforeEach(async () => {
    mockAccountService = jasmine.createSpyObj('AccountService', [
      'getTwoFactorStatusForUser',
      'setupTwoFactorForUser',
      'enableTwoFactorForUser',
      'disableTwoFactorForUser',
      'getRecoveryCodesForUser',
    ]);
    mockAlertService = jasmine.createSpyObj('AlertService', ['success', 'error']);
    mockDialog = jasmine.createSpyObj('MatDialog', ['open']);
    mockDialogRef = jasmine.createSpyObj('MatDialogRef', ['close']);

    await TestBed.configureTestingModule({
      imports: [User2FAComponent, NoopAnimationsModule],
      providers: [
        { provide: AccountService, useValue: mockAccountService },
        { provide: AlertService, useValue: mockAlertService },
        { provide: MatDialog, useValue: mockDialog },
        { provide: MatDialogRef, useValue: mockDialogRef },
        { provide: MAT_DIALOG_DATA, useValue: { userId: '123', userName: 'testuser' } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(User2FAComponent);
    component = fixture.componentInstance;
  });

  describe('ngOnInit', () => {
    it('should initialize with dialog data', () => {
      const mockStatus: TwoFactorStatus = {
        isTwoFactorEnabled: false,
        hasAuthenticator: false,
      };
      const mockSetup: TwoFactorSetup = {
        qrCodeUri: 'otpauth://test',
        sharedKey: 'TEST123',
      };

      mockAccountService.getTwoFactorStatusForUser.and.returnValue(of(mockStatus));
      mockAccountService.setupTwoFactorForUser.and.returnValue(of(mockSetup));

      component.ngOnInit();

      expect(component.userId).toBe('123');
      expect(component.userName).toBe('testuser');
      expect(mockAccountService.getTwoFactorStatusForUser).toHaveBeenCalledWith('123');
    });

    it('should setup 2FA when not enabled', () => {
      const mockStatus: TwoFactorStatus = {
        isTwoFactorEnabled: false,
        hasAuthenticator: false,
      };
      const mockSetup: TwoFactorSetup = {
        qrCodeUri: 'otpauth://test',
        sharedKey: 'TEST123',
      };

      mockAccountService.getTwoFactorStatusForUser.and.returnValue(of(mockStatus));
      mockAccountService.setupTwoFactorForUser.and.returnValue(of(mockSetup));

      component.ngOnInit();

      expect(component.twoFactorEnabled).toBe(false);
      expect(mockAccountService.setupTwoFactorForUser).toHaveBeenCalledWith('123');
      expect(component.qrCodeUri).toBe('otpauth://test');
      expect(component.sharedKey).toBe('TEST123');
    });
  });

  describe('onCodeSubmitted', () => {
    it('should enable 2FA with valid code', () => {
      const mockStatus: TwoFactorStatus = {
        isTwoFactorEnabled: false,
        hasAuthenticator: false,
      };
      const mockSetup: TwoFactorSetup = {
        qrCodeUri: 'otpauth://test',
        sharedKey: 'TEST123',
      };
      const mockResult: EnableTwoFactorResult = {
        message: 'Success',
        recoveryCodes: ['code1', 'code2', 'code3'],
      };

      // Setup all the required service calls for ngOnInit
      mockAccountService.getTwoFactorStatusForUser.and.returnValue(of(mockStatus));
      mockAccountService.setupTwoFactorForUser.and.returnValue(of(mockSetup));
      mockAccountService.enableTwoFactorForUser.and.returnValue(of(mockResult));

      // Initialize component with userId
      component.ngOnInit();

      component.onCodeSubmitted('123456');

      expect(mockAccountService.enableTwoFactorForUser).toHaveBeenCalledWith('123', '123456');
      expect(component.recoveryCodes).toEqual(['code1', 'code2', 'code3']);
      expect(component.twoFactorEnabled).toBe(true);
      expect(component.loading).toBe(false);
    });

    it('should handle error when enabling 2FA', () => {
      mockAccountService.enableTwoFactorForUser.and.returnValue(throwError(() => new Error('Invalid code')));

      component.onCodeSubmitted('123456');

      expect(mockAlertService.error).toHaveBeenCalledWith('Error: Invalid code');
      expect(component.loading).toBe(false);
    });
  });

  describe('onGenerateNewRecoveryCodes', () => {
    it('should generate new recovery codes', () => {
      const mockStatus: TwoFactorStatus = {
        isTwoFactorEnabled: false,
        hasAuthenticator: false,
      };
      const mockSetup: TwoFactorSetup = {
        qrCodeUri: 'otpauth://test',
        sharedKey: 'TEST123',
      };
      const mockResult: RecoveryCodesResult = {
        recoveryCodes: ['new1', 'new2', 'new3'],
      };

      // Setup all the required service calls for ngOnInit
      mockAccountService.getTwoFactorStatusForUser.and.returnValue(of(mockStatus));
      mockAccountService.setupTwoFactorForUser.and.returnValue(of(mockSetup));
      mockAccountService.getRecoveryCodesForUser.and.returnValue(of(mockResult));

      // Initialize component with userId
      component.ngOnInit();

      component.onGenerateNewRecoveryCodes();

      expect(mockAccountService.getRecoveryCodesForUser).toHaveBeenCalledWith('123');
      expect(component.recoveryCodes).toEqual(['new1', 'new2', 'new3']);
      expect(mockAlertService.success).toHaveBeenCalledWith('New recovery codes have been generated for this user.');
    });
  });

  describe('onClose', () => {
    it('should close the dialog', () => {
      component.onClose();
      expect(mockDialogRef.close).toHaveBeenCalled();
    });
  });
});
