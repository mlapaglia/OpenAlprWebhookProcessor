import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { Router, ActivatedRoute } from '@angular/router';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';

import { Setup2FAComponent } from './setup-2fa.component';
import { AccountService } from '../account.service';
import { SnackbarService } from '../../snackbar/snackbar.service';
import { SnackBarType } from '../../snackbar/snackbartype';

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

interface DisableTwoFactorResult {
  message: string;
}

describe('Setup2FAComponent', () => {
  let component: Setup2FAComponent;
  let fixture: ComponentFixture<Setup2FAComponent>;
  let mockAccountService: jasmine.SpyObj<AccountService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockActivatedRoute: jasmine.SpyObj<ActivatedRoute>;
  let mockSnackbarService: jasmine.SpyObj<SnackbarService>;

  beforeEach(async () => {
    mockAccountService = jasmine.createSpyObj('AccountService', [
      'getTwoFactorStatus',
      'setupTwoFactor',
      'enableTwoFactor',
      'disableTwoFactor',
      'getRecoveryCodes',
    ]);
    mockRouter = jasmine.createSpyObj('Router', ['navigate'], { events: of() });
    mockActivatedRoute = jasmine.createSpyObj('ActivatedRoute', [], {
      snapshot: { queryParams: {} },
    });
    mockSnackbarService = jasmine.createSpyObj('SnackbarService', ['create']);

    await TestBed.configureTestingModule({
      imports: [
        Setup2FAComponent,
        NoopAnimationsModule,
      ],
      providers: [
        { provide: AccountService, useValue: mockAccountService },
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: SnackbarService, useValue: mockSnackbarService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Setup2FAComponent);
    component = fixture.componentInstance;
  });

  describe('component initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.loading).toBe(false);
      expect(component.setupLoading).toBe(false);
      expect(component.qrCodeUri).toBe('');
      expect(component.sharedKey).toBe('');
      expect(component.recoveryCodes).toEqual([]);
      expect(component.twoFactorEnabled).toBe(false);
    });

    it('should load two factor status on init', () => {
      const mockStatus: TwoFactorStatus = { isTwoFactorEnabled: false, hasAuthenticator: false };
      mockAccountService.getTwoFactorStatus.and.returnValue(of(mockStatus));
      mockAccountService.setupTwoFactor.and.returnValue(of({
        qrCodeUri: 'otpauth://test',
        sharedKey: 'TESTKEY123',
      }));

      component.ngOnInit();

      expect(mockAccountService.getTwoFactorStatus).toHaveBeenCalled();
      expect(component.twoFactorEnabled).toBe(false);
    });

    it('should not setup 2FA if already enabled', () => {
      const mockStatus: TwoFactorStatus = { isTwoFactorEnabled: true, hasAuthenticator: true };
      mockAccountService.getTwoFactorStatus.and.returnValue(of(mockStatus));

      component.ngOnInit();

      expect(mockAccountService.getTwoFactorStatus).toHaveBeenCalled();
      expect(mockAccountService.setupTwoFactor).not.toHaveBeenCalled();
      expect(component.twoFactorEnabled).toBe(true);
    });
  });



  describe('loadTwoFactorStatus', () => {
    it('should handle successful status load', () => {
      const mockStatus: TwoFactorStatus = { isTwoFactorEnabled: false, hasAuthenticator: false };
      mockAccountService.getTwoFactorStatus.and.returnValue(of(mockStatus));
      mockAccountService.setupTwoFactor.and.returnValue(of({
        qrCodeUri: 'otpauth://test',
        sharedKey: 'TESTKEY123',
      }));

      component.loadTwoFactorStatus();

      expect(component.twoFactorEnabled).toBe(false);
      expect(mockAccountService.setupTwoFactor).toHaveBeenCalled();
    });

    it('should handle error loading status', () => {
      const errorMessage = 'Failed to load status';
      mockAccountService.getTwoFactorStatus.and.returnValue(throwError(() => errorMessage));

      component.loadTwoFactorStatus();

      expect(mockSnackbarService.create).toHaveBeenCalledWith(errorMessage, SnackBarType.Error);
    });
  });

  describe('setupTwoFactor', () => {
    it('should setup 2FA successfully', () => {
      const mockSetup: TwoFactorSetup = {
        qrCodeUri: 'otpauth://totp/test',
        sharedKey: 'TESTKEY123',
      };
      mockAccountService.setupTwoFactor.and.returnValue(of(mockSetup));

      component.setupTwoFactor();

      expect(component.qrCodeUri).toBe('otpauth://totp/test');
      expect(component.sharedKey).toBe('TESTKEY123');
      expect(component.setupLoading).toBe(false);
    });

    it('should handle setup error', () => {
      const errorMessage = 'Setup failed';
      mockAccountService.setupTwoFactor.and.returnValue(throwError(() => errorMessage));

      component.setupTwoFactor();

      expect(mockSnackbarService.create).toHaveBeenCalledWith(errorMessage, SnackBarType.Error);
      expect(component.setupLoading).toBe(false);
    });
  });

  describe('code submission', () => {
    it('should handle code submission', () => {
      const mockResult: EnableTwoFactorResult = {
        message: 'Success',
        recoveryCodes: ['code1', 'code2'],
      };
      mockAccountService.enableTwoFactor.and.returnValue(of(mockResult));

      component.onCodeSubmitted('123456');

      expect(mockAccountService.enableTwoFactor).toHaveBeenCalledWith('123456');
      // After the observable completes, loading should be false
      expect(component.loading).toBe(false);
    });
  });

  describe('enableTwoFactor', () => {
    it('should enable 2FA successfully', () => {
      const mockResult: EnableTwoFactorResult = {
        message: 'Success',
        recoveryCodes: ['code1', 'code2', 'code3'],
      };
      mockAccountService.enableTwoFactor.and.returnValue(of(mockResult));

      component.onCodeSubmitted('123456');

      expect(component.recoveryCodes).toEqual(['code1', 'code2', 'code3']);
      expect(component.twoFactorEnabled).toBe(true);
      expect(component.loading).toBe(false);
      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Two-factor authentication has been enabled successfully!',
        SnackBarType.Successful,
      );
    });

    it('should handle enable error', () => {
      const errorMessage = 'Invalid code';
      mockAccountService.enableTwoFactor.and.returnValue(throwError(() => errorMessage));

      component.onCodeSubmitted('123456');

      expect(mockSnackbarService.create).toHaveBeenCalledWith(errorMessage, SnackBarType.Error);
      expect(component.loading).toBe(false);
    });
  });

  describe('disableTwoFactor', () => {
    beforeEach(() => {
      component.twoFactorEnabled = true;
      spyOn(window, 'confirm').and.returnValue(true);
    });

    it('should disable 2FA successfully', () => {
      const mockDisableResult: DisableTwoFactorResult = { message: 'Disabled successfully' };
      mockAccountService.disableTwoFactor.and.returnValue(of(mockDisableResult));
      mockAccountService.setupTwoFactor.and.returnValue(of({
        qrCodeUri: 'otpauth://test',
        sharedKey: 'NEWKEY123',
      }));

      component.disableTwoFactor();

      expect(component.twoFactorEnabled).toBe(false);
      // After disabling, setupTwoFactor is called which sets new values
      expect(component.qrCodeUri).toBe('otpauth://test');
      expect(component.sharedKey).toBe('NEWKEY123');
      expect(component.recoveryCodes).toEqual([]);
      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Two-factor authentication has been disabled.',
        SnackBarType.Successful,
      );
      expect(mockAccountService.setupTwoFactor).toHaveBeenCalled();
    });

    it('should handle disable error', () => {
      const errorMessage = 'Disable failed';
      mockAccountService.disableTwoFactor.and.returnValue(throwError(() => errorMessage));

      component.disableTwoFactor();

      expect(mockSnackbarService.create).toHaveBeenCalledWith(errorMessage, SnackBarType.Error);
    });

    it('should not disable if user cancels confirmation', () => {
      (window.confirm as jasmine.Spy).and.returnValue(false);

      component.disableTwoFactor();

      expect(mockAccountService.disableTwoFactor).not.toHaveBeenCalled();
    });
  });

  describe('generateNewRecoveryCodes', () => {
    it('should generate new recovery codes successfully', () => {
      const mockResult: RecoveryCodesResult = { recoveryCodes: ['new1', 'new2', 'new3'] };
      mockAccountService.getRecoveryCodes.and.returnValue(of(mockResult));

      component.onGenerateNewRecoveryCodes();

      expect(component.recoveryCodes).toEqual(['new1', 'new2', 'new3']);
      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'New recovery codes have been generated.',
        SnackBarType.Successful,
      );
    });

    it('should handle recovery codes generation error', () => {
      const errorMessage = 'Generation failed';
      mockAccountService.getRecoveryCodes.and.returnValue(throwError(() => errorMessage));

      component.onGenerateNewRecoveryCodes();

      expect(mockSnackbarService.create).toHaveBeenCalledWith(errorMessage, SnackBarType.Error);
    });
  });
});
