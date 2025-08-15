import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import type { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { of, throwError } from 'rxjs';
import { LoginGuard } from './login.guard';
import { AccountService } from '../account/account.service';
import { User } from '../_models/user';

describe('LoginGuard', () => {
  let guard: LoginGuard;
  let accountService: jasmine.SpyObj<AccountService>;
  let router: jasmine.SpyObj<Router>;
  let route: ActivatedRouteSnapshot;
  let state: RouterStateSnapshot;

  beforeEach(() => {
    const accountServiceSpy = jasmine.createSpyObj('AccountService', ['checkAuthenticationStatus'], {
      userValue: new User(),
    });
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        LoginGuard,
        { provide: AccountService, useValue: accountServiceSpy },
        { provide: Router, useValue: routerSpy },
      ],
    });

    guard = TestBed.inject(LoginGuard);
    accountService = TestBed.inject(AccountService) as jasmine.SpyObj<AccountService>;
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;

    route = {} as ActivatedRouteSnapshot;
    state = { url: '/account/login' } as RouterStateSnapshot;
  });

  it('should be created', () => {
    expect(guard).toBeTruthy();
  });

  describe('canActivate', () => {
    it('should redirect to home when user is already authenticated', (done) => {
      // Arrange
      const authenticatedUser = new User();
      authenticatedUser.id = 123;
      Object.defineProperty(accountService, 'userValue', { value: authenticatedUser });

      // Act
      guard.canActivate(route, state).subscribe(result => {
        // Assert
        expect(result).toBe(false);
        expect(accountService.checkAuthenticationStatus).not.toHaveBeenCalled();
        expect(router.navigate).toHaveBeenCalledWith(['/']);
        done();
      });
    });

    it('should redirect to home when server confirms authentication', (done) => {
      // Arrange
      const unauthenticatedUser = new User(); // No ID
      const authenticatedUser = new User();
      authenticatedUser.id = 456;

      Object.defineProperty(accountService, 'userValue', { value: unauthenticatedUser });
      accountService.checkAuthenticationStatus.and.returnValue(of(authenticatedUser));

      // Act
      guard.canActivate(route, state).subscribe(result => {
        // Assert
        expect(result).toBe(false);
        expect(accountService.checkAuthenticationStatus).toHaveBeenCalled();
        expect(router.navigate).toHaveBeenCalledWith(['/']);
        done();
      });
    });

    it('should allow access to login page when user is not authenticated', (done) => {
      // Arrange
      const unauthenticatedUser = new User(); // No ID
      Object.defineProperty(accountService, 'userValue', { value: unauthenticatedUser });
      accountService.checkAuthenticationStatus.and.returnValue(of(unauthenticatedUser));

      // Act
      guard.canActivate(route, state).subscribe(result => {
        // Assert
        expect(result).toBe(true);
        expect(accountService.checkAuthenticationStatus).toHaveBeenCalled();
        expect(router.navigate).not.toHaveBeenCalled();
        done();
      });
    });

    it('should allow access to login page when authentication check fails', (done) => {
      // Arrange
      const unauthenticatedUser = new User(); // No ID
      Object.defineProperty(accountService, 'userValue', { value: unauthenticatedUser });
      accountService.checkAuthenticationStatus.and.returnValue(throwError('Authentication failed'));

      // Act
      guard.canActivate(route, state).subscribe(result => {
        // Assert
        expect(result).toBe(true);
        expect(accountService.checkAuthenticationStatus).toHaveBeenCalled();
        expect(router.navigate).not.toHaveBeenCalled();
        done();
      });
    });

    it('should handle user with falsy id as unauthenticated', (done) => {
      // Arrange
      const userWithFalsyId = new User();
      userWithFalsyId.id = 0; // Falsy ID
      Object.defineProperty(accountService, 'userValue', { value: userWithFalsyId });
      accountService.checkAuthenticationStatus.and.returnValue(of(userWithFalsyId));

      // Act
      guard.canActivate(route, state).subscribe(result => {
        // Assert
        expect(result).toBe(true);
        expect(accountService.checkAuthenticationStatus).toHaveBeenCalled();
        expect(router.navigate).not.toHaveBeenCalled();
        done();
      });
    });

    it('should redirect to home immediately for authenticated user without server check', (done) => {
      // Arrange
      const authenticatedUser = new User();
      authenticatedUser.id = 999;
      Object.defineProperty(accountService, 'userValue', { value: authenticatedUser });

      // Act
      guard.canActivate(route, state).subscribe(result => {
        // Assert
        expect(result).toBe(false);
        expect(accountService.checkAuthenticationStatus).not.toHaveBeenCalled();
        expect(router.navigate).toHaveBeenCalledWith(['/']);
        done();
      });
    });

    it('should handle server returning authenticated user after initial check', (done) => {
      // Arrange
      const initialUser = new User(); // No ID
      const serverUser = new User();
      serverUser.id = 789;

      Object.defineProperty(accountService, 'userValue', { value: initialUser });
      accountService.checkAuthenticationStatus.and.returnValue(of(serverUser));

      // Act
      guard.canActivate(route, state).subscribe(result => {
        // Assert
        expect(result).toBe(false);
        expect(accountService.checkAuthenticationStatus).toHaveBeenCalled();
        expect(router.navigate).toHaveBeenCalledWith(['/']);
        done();
      });
    });

    it('should not make unnecessary server calls when user is already authenticated', (done) => {
      // Arrange
      const authenticatedUser = new User();
      authenticatedUser.id = 555;
      authenticatedUser.username = 'testuser';
      Object.defineProperty(accountService, 'userValue', { value: authenticatedUser });

      // Act
      guard.canActivate(route, state).subscribe(result => {
        // Assert
        expect(result).toBe(false);
        expect(accountService.checkAuthenticationStatus).not.toHaveBeenCalled();
        expect(router.navigate).toHaveBeenCalledWith(['/']);
        done();
      });
    });

    it('should handle multiple authentication checks correctly', (done) => {
      // Arrange
      const unauthenticatedUser = new User();
      Object.defineProperty(accountService, 'userValue', { value: unauthenticatedUser });
      accountService.checkAuthenticationStatus.and.returnValue(of(unauthenticatedUser));

      let callCount = 0;

      // Act - First call
      guard.canActivate(route, state).subscribe(result => {
        expect(result).toBe(true);
        callCount++;

        // Act - Second call
        guard.canActivate(route, state).subscribe(secondResult => {
          expect(secondResult).toBe(true);
          callCount++;

          // Assert
          expect(callCount).toBe(2);
          expect(accountService.checkAuthenticationStatus).toHaveBeenCalledTimes(2);
          expect(router.navigate).not.toHaveBeenCalled();
          done();
        });
      });
    });
  });
});
