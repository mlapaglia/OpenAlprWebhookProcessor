import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AccountService } from './account.service';
import { User } from 'app/_models';

describe('AccountService', () => {
  let service: AccountService;
  let httpMock: HttpTestingController;
  let mockRouter: jasmine.SpyObj<Router>;

  beforeEach(() => {
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        AccountService,
        { provide: Router, useValue: mockRouter },
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(AccountService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('initialization', () => {
    it('should create', () => {
      expect(service).toBeTruthy();
    });

    it('should initialize with empty user', () => {
      service.user.subscribe(user => {
        expect(user).toEqual(new User());
      });
    });
  });

  describe('checkAuthenticationStatus', () => {
    it('should return user when authenticated', () => {
      const mockUser = { id: 1, username: 'testuser', firstName: 'Test', lastName: 'User' } as User;

      service.checkAuthenticationStatus().subscribe(user => {
        expect(user).toEqual(mockUser);
        expect(service.userValue).toEqual(mockUser);
      });

      const req = httpMock.expectOne('/api/auth/me');
      expect(req.request.method).toBe('GET');
      req.flush(mockUser);
    });

    it('should return empty user when not authenticated', () => {
      service.checkAuthenticationStatus().subscribe(user => {
        expect(user).toEqual(new User());
        expect(service.userValue).toEqual(new User());
      });

      const req = httpMock.expectOne('/api/auth/me');
      expect(req.request.method).toBe('GET');
      req.error(new ProgressEvent('error'));
    });
  });

  describe('login', () => {
    it('should login successfully without 2FA', () => {
      const mockUser = { id: 1, username: 'testuser', twoFactorEnabled: false } as User;

      service.login('testuser', 'password', true).subscribe(user => {
        expect(user).toEqual(mockUser);
        expect(service.userValue).toEqual(mockUser);
      });

      const req = httpMock.expectOne('/api/auth/authenticate');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ username: 'testuser', password: 'password', rememberMe: true });
      req.flush(mockUser);
    });

    it('should return user without setting userValue when 2FA is enabled', () => {
      const mockUser = { id: 1, username: 'testuser', twoFactorEnabled: true } as User;

      service.login('testuser', 'password', false).subscribe(user => {
        expect(user).toEqual(mockUser);
        expect(service.userValue).toEqual(new User()); // Should remain empty
      });

      const req = httpMock.expectOne('/api/auth/authenticate');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ username: 'testuser', password: 'password', rememberMe: false });
      req.flush(mockUser);
    });

    it('should use default rememberMe value', () => {
      const mockUser = { id: 1, username: 'testuser', twoFactorEnabled: false } as User;

      service.login('testuser', 'password').subscribe();

      const req = httpMock.expectOne('/api/auth/authenticate');
      expect(req.request.body).toEqual({ username: 'testuser', password: 'password', rememberMe: false });
      req.flush(mockUser);
    });
  });

  describe('verifyTwoFactor', () => {
    it('should verify 2FA successfully', () => {
      const mockUser = { id: 1, username: 'testuser' } as User;

      service.verifyTwoFactor('user123', '123456', true).subscribe(user => {
        expect(user).toEqual(mockUser);
        expect(service.userValue).toEqual(mockUser);
      });

      const req = httpMock.expectOne('/api/auth/verify-2fa');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ userId: 'user123', code: '123456', rememberMe: true });
      req.flush(mockUser);
    });
  });

  describe('Two Factor Authentication methods', () => {
    it('should get 2FA status', () => {
      const mockStatus = { isTwoFactorEnabled: true, hasAuthenticator: true };

      service.getTwoFactorStatus().subscribe(status => {
        expect(status).toEqual(mockStatus);
      });

      const req = httpMock.expectOne('/api/twofactor/status');
      expect(req.request.method).toBe('GET');
      req.flush(mockStatus);
    });

    it('should setup 2FA', () => {
      const mockSetup = { sharedKey: 'ABC123', qrCodeUri: 'otpauth://...' };

      service.setupTwoFactor().subscribe(setup => {
        expect(setup).toEqual(mockSetup);
      });

      const req = httpMock.expectOne('/api/twofactor/setup');
      expect(req.request.method).toBe('POST');
      req.flush(mockSetup);
    });

    it('should enable 2FA', () => {
      const mockResponse = { message: 'Enabled', recoveryCodes: ['code1', 'code2'] };

      service.enableTwoFactor('123456').subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne('/api/twofactor/enable');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ code: '123456' });
      req.flush(mockResponse);
    });

    it('should disable 2FA', () => {
      const mockResponse = { message: 'Disabled' };

      service.disableTwoFactor().subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne('/api/twofactor/disable');
      expect(req.request.method).toBe('POST');
      req.flush(mockResponse);
    });

    it('should get recovery codes', () => {
      const mockCodes = { recoveryCodes: ['code1', 'code2'] };

      service.getRecoveryCodes().subscribe(codes => {
        expect(codes).toEqual(mockCodes);
      });

      const req = httpMock.expectOne('/api/twofactor/recovery-codes');
      expect(req.request.method).toBe('GET');
      req.flush(mockCodes);
    });
  });

  describe('Two Factor Authentication for users', () => {
    it('should get 2FA status for user', () => {
      const mockStatus = { isTwoFactorEnabled: true, hasAuthenticator: true };

      service.getTwoFactorStatusForUser('user123').subscribe(status => {
        expect(status).toEqual(mockStatus);
      });

      const req = httpMock.expectOne('/api/users/user123/twofactor/status');
      expect(req.request.method).toBe('GET');
      req.flush(mockStatus);
    });

    it('should setup 2FA for user', () => {
      const mockSetup = { sharedKey: 'ABC123', qrCodeUri: 'otpauth://...' };

      service.setupTwoFactorForUser('user123').subscribe(setup => {
        expect(setup).toEqual(mockSetup);
      });

      const req = httpMock.expectOne('/api/users/user123/twofactor/setup');
      expect(req.request.method).toBe('POST');
      req.flush(mockSetup);
    });

    it('should enable 2FA for user', () => {
      const mockResponse = { message: 'Enabled', recoveryCodes: ['code1', 'code2'] };

      service.enableTwoFactorForUser('user123', '123456').subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne('/api/users/user123/twofactor/enable');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ code: '123456' });
      req.flush(mockResponse);
    });

    it('should disable 2FA for user', () => {
      const mockResponse = { message: 'Disabled' };

      service.disableTwoFactorForUser('user123').subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne('/api/users/user123/twofactor/disable');
      expect(req.request.method).toBe('POST');
      req.flush(mockResponse);
    });

    it('should get recovery codes for user', () => {
      const mockCodes = { recoveryCodes: ['code1', 'code2'] };

      service.getRecoveryCodesForUser('user123').subscribe(codes => {
        expect(codes).toEqual(mockCodes);
      });

      const req = httpMock.expectOne('/api/users/user123/twofactor/recovery-codes');
      expect(req.request.method).toBe('GET');
      req.flush(mockCodes);
    });
  });

  describe('logout', () => {
    it('should logout successfully', () => {
      spyOn(service, 'finalizeLogout');

      service.logout();

      const req = httpMock.expectOne('/api/auth/logout');
      expect(req.request.method).toBe('POST');
      req.flush(null);

      expect(service.finalizeLogout).toHaveBeenCalled();
    });

    it('should finalize logout on error', () => {
      spyOn(service, 'finalizeLogout');

      service.logout();

      const req = httpMock.expectOne('/api/auth/logout');
      req.error(new ProgressEvent('error'));

      expect(service.finalizeLogout).toHaveBeenCalled();
    });
  });

  describe('passkey methods', () => {
    it('should register passkey', () => {
      const mockOptions = {
        challenge: new ArrayBuffer(32),
        rp: { name: 'Test App', id: 'test.com' },
        user: { id: new ArrayBuffer(8), name: 'testuser', displayName: 'Test User' },
        pubKeyCredParams: [{ type: 'public-key' as const, alg: -7 }],
      };
      const mockResponse = { options: mockOptions };

      service.registerPasskey('My Device').subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne('/api/auth/passkey/register');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ name: 'My Device' });
      req.flush(mockResponse);
    });

    it('should complete passkey registration', () => {
      const mockResponse = { message: 'Success', success: true };

      service.completePasskeyRegistration('mock-attestation', 'My Device').subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne('/api/auth/passkey/complete-registration');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ attestationResponse: 'mock-attestation', name: 'My Device' });
      req.flush(mockResponse);
    });

    it('should authenticate passkey', () => {
      const mockOptions = {
        challenge: new ArrayBuffer(32),
        allowCredentials: [{ type: 'public-key' as const, id: new ArrayBuffer(8) }],
      };
      const mockResponse = { options: mockOptions };

      service.authenticatePasskey('testuser').subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne('/api/auth/passkey/authenticate');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ username: 'testuser' });
      req.flush(mockResponse);
    });

    it('should complete passkey authentication', () => {
      const mockUser = { id: 1, username: 'testuser' } as User;

      service.completePasskeyAuthentication('testuser', 'mock-assertion', true).subscribe(user => {
        expect(user).toEqual(mockUser);
        expect(service.userValue).toEqual(mockUser);
      });

      const req = httpMock.expectOne('/api/auth/passkey/complete-authentication');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ username: 'testuser', assertionResponse: 'mock-assertion', rememberMe: true });
      req.flush(mockUser);
    });

    it('should get passkeys', () => {
      const mockPasskeys = {
        passkeys: [
          { id: 1, name: 'My Device', regDate: '2023-01-01', aaGuid: 'guid-123' },
          { id: 2, name: 'Another Device', regDate: '2023-01-02', aaGuid: 'guid-456' },
        ],
      };

      service.getPasskeys().subscribe(response => {
        expect(response).toEqual(mockPasskeys);
      });

      const req = httpMock.expectOne('/api/auth/passkey/list');
      expect(req.request.method).toBe('GET');
      req.flush(mockPasskeys);
    });

    it('should delete passkey', () => {
      const mockResponse = { message: 'Deleted', success: true };

      service.deletePasskey(1).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne('/api/auth/passkey/1');
      expect(req.request.method).toBe('DELETE');
      req.flush(mockResponse);
    });
  });

  describe('user management', () => {
    it('should check if registration is allowed', () => {
      service.canRegister().subscribe(canRegister => {
        expect(canRegister).toBe(true);
      });

      const req = httpMock.expectOne('/api/users/canregister');
      expect(req.request.method).toBe('GET');
      req.flush(true);
    });

    it('should add user', () => {
      const newUser = { username: 'newuser', firstName: 'New', lastName: 'User' } as User;

      service.add(newUser).subscribe();

      const req = httpMock.expectOne('/api/users/add');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(newUser);
      req.flush({});
    });

    it('should register user', () => {
      const newUser = { username: 'newuser', firstName: 'New', lastName: 'User' } as User;

      service.register(newUser).subscribe();

      const req = httpMock.expectOne('/api/auth/register');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(newUser);
      req.flush({});
    });

    it('should get all users', () => {
      const mockUsers = [
        { id: 1, username: 'user1' } as User,
        { id: 2, username: 'user2' } as User,
      ];

      service.getAll().subscribe(users => {
        expect(users).toEqual(mockUsers);
      });

      const req = httpMock.expectOne('/api/users');
      expect(req.request.method).toBe('GET');
      req.flush(mockUsers);
    });

    it('should get user by id', () => {
      const mockUser = { id: 1, username: 'user1' } as User;

      service.getById('1').subscribe(user => {
        expect(user).toEqual(mockUser);
      });

      const req = httpMock.expectOne('/api/users/1');
      expect(req.request.method).toBe('GET');
      req.flush(mockUser);
    });

    it('should update user', () => {
      const userId = 1;
      const updateParams = { firstName: 'Updated', lastName: 'Name' };

      service.update(userId, updateParams).subscribe();

      const req = httpMock.expectOne(`/api/users/${userId}`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(updateParams);
      req.flush({});
    });

    it('should update current user when updating own record', () => {
      const currentUser = { id: 1, username: 'currentuser', firstName: 'Current', lastName: 'User' } as User;
      service['userSubject'].next(currentUser);
      spyOn(localStorage, 'setItem');

      const updateParams = { firstName: 'Updated' };

      service.update(1, updateParams).subscribe();

      const req = httpMock.expectOne('/api/users/1');
      req.flush({});

      expect(service.userValue.firstName).toBe('Updated');
      expect(localStorage.setItem).toHaveBeenCalledWith('user', JSON.stringify({ ...currentUser, ...updateParams }));
    });

    it('should delete user', () => {
      service.delete(1).subscribe();

      const req = httpMock.expectOne('/api/users/1');
      expect(req.request.method).toBe('DELETE');
      req.flush({});
    });

    it('should logout when deleting own record', () => {
      const currentUser = { id: 1, username: 'currentuser' } as User;
      service['userSubject'].next(currentUser);
      spyOn(service, 'logout');

      service.delete(1).subscribe();

      const req = httpMock.expectOne('/api/users/1');
      req.flush({});

      expect(service.logout).toHaveBeenCalled();
    });
  });
});
