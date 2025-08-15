import { Injectable } from '@angular/core';
import type { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import type { Observable } from 'rxjs';

@Injectable()
export class CookieInterceptor implements HttpInterceptor {
  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // For cookie authentication, ensure credentials are included with all API requests
    if (request.url.includes('/api/')) {
      request = request.clone({
        withCredentials: true, // Include cookies with all API requests
      });
    }

    return next.handle(request);
  }
}
