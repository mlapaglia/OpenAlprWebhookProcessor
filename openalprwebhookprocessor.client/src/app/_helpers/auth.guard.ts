import { Injectable, inject } from '@angular/core';
import { type CanActivate, type ActivatedRouteSnapshot, Router, type RouterStateSnapshot } from '@angular/router';
import { of, type Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

import { AccountService } from 'app/_services';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  private readonly router = inject(Router);
  private readonly accountService = inject(AccountService);

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
    const user = this.accountService.userValue;

    // If user is already authenticated, allow access
    if (user.id) {
      return of(true);
    }

    // Check authentication status with server (for page refreshes)
    return this.accountService.checkAuthenticationStatus().pipe(
      map(user => {
        if (user.id) {
          return true;
        } else {
          void this.router.navigate(['/account/login'], { queryParams: { returnUrl: state.url } });
          return false;
        }
      }),
      catchError(() => {
        void this.router.navigate(['/account/login'], { queryParams: { returnUrl: state.url } });
        return of(false);
      }),
    );
  }
}
