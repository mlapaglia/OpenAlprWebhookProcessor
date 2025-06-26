import { Injectable, inject } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable } from 'rxjs';

import { AccountService } from 'app/_services';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
    private accountService = inject(AccountService);


    intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
        const user = this.accountService.userValue;
        const isLoggedIn = user && user.jwtToken;
        if (isLoggedIn) {
            request = request.clone({
                setHeaders: {
                    Authorization: `Bearer ${user.jwtToken}`
                }
            });
        }

        return next.handle(request);
    }
}