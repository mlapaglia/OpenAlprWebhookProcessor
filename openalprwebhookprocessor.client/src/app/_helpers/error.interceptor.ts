import { Injectable, inject } from '@angular/core';
import type { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import type { Observable } from 'rxjs';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { AccountService } from 'app/_services';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  private readonly accountService = inject(AccountService);

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(catchError((err) => {
      if ([401, 403].includes(err.status) && this.accountService.userValue) {
        this.accountService.logout();
      }

      const error = err.error?.message || err.statusText;
      return throwError(error);
    }));
  }
}
