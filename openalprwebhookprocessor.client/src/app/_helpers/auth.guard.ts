import { Injectable, inject } from '@angular/core';
import { Router, CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';

import { AccountService } from 'app/_services';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
    private router = inject(Router);
    private accountService = inject(AccountService);


    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
        const jwtToken = this.accountService.userValue.jwtToken;
        if (jwtToken) {
            return true;
        }

        this.router.navigate(['/account/login'], { queryParams: { returnUrl: state.url }});
        return false;
    }
}