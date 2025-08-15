import { Injectable, inject, Injector } from '@angular/core';
import type { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { type Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  private readonly injector = inject(Injector);


  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(catchError((err) => {
      // Don't intercept authentication check requests - let the guards handle 401/403
      if ([401, 403].includes(err.status) && !request.url.includes('/api/auth/me')) {
        // For cookie authentication, redirect to login page
        // This avoids circular dependency by not injecting AccountService
        const router = this.injector.get(Router);
        void router.navigate(['/account/login']);
      }

      const error = err.error?.message ?? err.statusText;
      return throwError(error);
    }));
  }
}
