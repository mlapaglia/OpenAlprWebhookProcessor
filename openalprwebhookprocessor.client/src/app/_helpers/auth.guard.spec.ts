import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import type { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthGuard } from './auth.guard';
import { AccountService } from '../account/account.service';
import { User } from '../_models/user';

describe('AuthGuard', () => {
  let guard: AuthGuard;
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
        AuthGuard,
        { provide: AccountService, useValue: accountServiceSpy },
        { provide: Router, useValue: routerSpy },
      ],
    });

    guard = TestBed.inject(AuthGuard);
    accountService = TestBed.inject(AccountService) as jasmine.SpyObj<AccountService>;
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;

    route = {} as ActivatedRouteSnapshot;
    state = { url: '/test-route' } as RouterStateSnapshot;
  });

  it('should be created', () => {
    expect(guard).toBeTruthy();
  });

  describe('canActivate', () => {
    it('should allow access when user is already authenticated', (done) => {
      // Arrange
      const authenticatedUser = new User();
      authenticatedUser.id = 123;
      Object.defineProperty(accountService, 'userValue', { value: authenticatedUser });

      // Act
      guard.canActivate(route, state).subscribe(result => {
        // Assert
        expect(result).toBe(true);
        expect(accountService.checkAuthenticationStatus).not.toHaveBeenCalled();
        expect(router.navigate).not.toHaveBeenCalled();
        done();
      });
    });

    it('should allow access when server confirms authentication', (done) => {
      // Arrange
      const unauthenticatedUser = new User(); // No ID
      const authenticatedUser = new User();
      authenticatedUser.id = 456;

      Object.defineProperty(accountService, 'userValue', { value: unauthenticatedUser });
      accountService.checkAuthenticationStatus.and.returnValue(of(authenticatedUser));

      // Act
      guard.canActivate(route, state).subscribe(result => {
        // Assert
        expect(result).toBe(true);
        expect(accountService.checkAuthenticationStatus).toHaveBeenCalled();
        expect(router.navigate).not.toHaveBeenCalled();
        done();
      });
    });

    it('should redirect to login when user is not authenticated', (done) => {
      // Arrange
      const unauthenticatedUser = new User(); // No ID
      Object.defineProperty(accountService, 'userValue', { value: unauthenticatedUser });
      accountService.checkAuthenticationStatus.and.returnValue(of(unauthenticatedUser));

      // Act
      guard.canActivate(route, state).subscribe(result => {
        // Assert
        expect(result).toBe(false);
        expect(accountService.checkAuthenticationStatus).toHaveBeenCalled();
        expect(router.navigate).toHaveBeenCalledWith(['/account/login'], {
          queryParams: { returnUrl: '/test-route' },
        });
        done();
      });
    });

    it('should redirect to login when authentication check fails', (done) => {
      // Arrange
      const unauthenticatedUser = new User(); // No ID
      Object.defineProperty(accountService, 'userValue', { value: unauthenticatedUser });
      accountService.checkAuthenticationStatus.and.returnValue(throwError(() => new Error('Authentication failed')));

      // Act
      guard.canActivate(route, state).subscribe(result => {
        // Assert
        expect(result).toBe(false);
        expect(accountService.checkAuthenticationStatus).toHaveBeenCalled();
        expect(router.navigate).toHaveBeenCalledWith(['/account/login'], {
          queryParams: { returnUrl: '/test-route' },
        });
        done();
      });
    });

    it('should preserve the current URL as returnUrl when redirecting to login', (done) => {
      // Arrange
      const unauthenticatedUser = new User();
      const customState = { url: '/settings/users' } as RouterStateSnapshot;
      Object.defineProperty(accountService, 'userValue', { value: unauthenticatedUser });
      accountService.checkAuthenticationStatus.and.returnValue(of(unauthenticatedUser));

      // Act
      guard.canActivate(route, customState).subscribe(result => {
        // Assert
        expect(result).toBe(false);
        expect(router.navigate).toHaveBeenCalledWith(['/account/login'], {
          queryParams: { returnUrl: '/settings/users' },
        });
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
        expect(result).toBe(false);
        expect(router.navigate).toHaveBeenCalledWith(['/account/login'], {
          queryParams: { returnUrl: '/test-route' },
        });
        done();
      });
    });

    it('should handle server returning user with valid id after initial check', (done) => {
      // Arrange
      const initialUser = new User(); // No ID
      const serverUser = new User();
      serverUser.id = 789;

      Object.defineProperty(accountService, 'userValue', { value: initialUser });
      accountService.checkAuthenticationStatus.and.returnValue(of(serverUser));

      // Act
      guard.canActivate(route, state).subscribe(result => {
        // Assert
        expect(result).toBe(true);
        expect(accountService.checkAuthenticationStatus).toHaveBeenCalled();
        expect(router.navigate).not.toHaveBeenCalled();
        done();
      });
    });
  });
});
