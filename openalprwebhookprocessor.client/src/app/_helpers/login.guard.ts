import { Injectable, inject } from '@angular/core';
import { type CanActivate, type ActivatedRouteSnapshot, Router, type RouterStateSnapshot } from '@angular/router';
import { of, type Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

import { AccountService } from 'app/_services';

@Injectable({ providedIn: 'root' })
export class LoginGuard implements CanActivate {
  private readonly router = inject(Router);
  private readonly accountService = inject(AccountService);

  canActivate(_route: ActivatedRouteSnapshot, _state: RouterStateSnapshot): Observable<boolean> {
    const user = this.accountService.userValue;

    // If user is already authenticated, redirect to home
    if (user.id) {
      void this.router.navigate(['/']);
      return of(false);
    }

    // Check authentication status with server (for page refreshes)
    return this.accountService.checkAuthenticationStatus().pipe(
      map(user => {
        if (user.id) {
          void this.router.navigate(['/']);
          return false;
        } else {
          return true; // Allow access to login page
        }
      }),
      catchError(() => {
        return of(true); // Allow access to login page if auth check fails
      }),
    );
  }
}
