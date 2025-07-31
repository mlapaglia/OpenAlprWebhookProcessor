import { Injectable, inject } from '@angular/core';
import { type CanActivate, type ActivatedRouteSnapshot, Router, type RouterStateSnapshot } from '@angular/router';

import { AccountService } from 'app/_services';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  private readonly router = inject(Router);
  private readonly accountService = inject(AccountService);

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
    const { jwtToken } = this.accountService.userValue;
    if (jwtToken) {
      return true;
    }

    void this.router.navigate(['/account/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }
}
