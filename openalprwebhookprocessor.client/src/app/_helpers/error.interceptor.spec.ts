import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import type { HttpHandler } from '@angular/common/http';
import { HttpRequest, HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { throwError, of } from 'rxjs';
import { ErrorInterceptor } from './error.interceptor';

describe('ErrorInterceptor', () => {
  let interceptor: ErrorInterceptor;
  let router: jasmine.SpyObj<Router>;
  let httpHandler: jasmine.SpyObj<HttpHandler>;

  beforeEach(() => {
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    const httpHandlerSpy = jasmine.createSpyObj('HttpHandler', ['handle']);

    TestBed.configureTestingModule({
      providers: [
        ErrorInterceptor,
        { provide: Router, useValue: routerSpy },
      ],
    });

    interceptor = TestBed.inject(ErrorInterceptor);
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    httpHandler = httpHandlerSpy;
  });

  it('should be created', () => {
    expect(interceptor).toBeTruthy();
  });

  describe('intercept', () => {
    it('should redirect to login on 401 error for non-auth-check requests', (done) => {
      // Arrange
      const request = new HttpRequest('GET', '/api/users');
      const errorResponse = new HttpErrorResponse({
        error: 'Unauthorized',
        status: 401,
        statusText: 'Unauthorized',
      });

      httpHandler.handle.and.returnValue(throwError(errorResponse));

      // Act
      interceptor.intercept(request, httpHandler).subscribe({
        error: (error) => {
          // Assert
          expect(router.navigate).toHaveBeenCalledWith(['/account/login']);
          expect(error).toBe('Unauthorized');
          done();
        },
      });
    });

    it('should redirect to login on 403 error for non-auth-check requests', (done) => {
      // Arrange
      const request = new HttpRequest('GET', '/api/settings');
      const errorResponse = new HttpErrorResponse({
        error: { message: 'Forbidden' },
        status: 403,
        statusText: 'Forbidden',
      });

      httpHandler.handle.and.returnValue(throwError(errorResponse));

      // Act
      interceptor.intercept(request, httpHandler).subscribe({
        error: (error) => {
          // Assert
          expect(router.navigate).toHaveBeenCalledWith(['/account/login']);
          expect(error).toBe('Forbidden');
          done();
        },
      });
    });

    it('should NOT redirect to login on 401 error for auth check requests', (done) => {
      // Arrange
      const request = new HttpRequest('GET', '/api/auth/me');
      const errorResponse = new HttpErrorResponse({
        error: 'Unauthorized',
        status: 401,
        statusText: 'Unauthorized',
      });

      httpHandler.handle.and.returnValue(throwError(errorResponse));

      // Act
      interceptor.intercept(request, httpHandler).subscribe({
        error: (error) => {
          // Assert
          expect(router.navigate).not.toHaveBeenCalled();
          expect(error).toBe('Unauthorized');
          done();
        },
      });
    });

    it('should NOT redirect to login on 403 error for auth check requests', (done) => {
      // Arrange
      const request = new HttpRequest('GET', '/api/auth/me');
      const errorResponse = new HttpErrorResponse({
        error: { message: 'Forbidden access' },
        status: 403,
        statusText: 'Forbidden',
      });

      httpHandler.handle.and.returnValue(throwError(errorResponse));

      // Act
      interceptor.intercept(request, httpHandler).subscribe({
        error: (error) => {
          // Assert
          expect(router.navigate).not.toHaveBeenCalled();
          expect(error).toBe('Forbidden access');
          done();
        },
      });
    });

    it('should not redirect on other HTTP error codes', (done) => {
      // Arrange
      const request = new HttpRequest('GET', '/api/data');
      const errorResponse = new HttpErrorResponse({
        error: 'Server Error',
        status: 500,
        statusText: 'Internal Server Error',
      });

      httpHandler.handle.and.returnValue(throwError(errorResponse));

      // Act
      interceptor.intercept(request, httpHandler).subscribe({
        error: (error) => {
          // Assert
          expect(router.navigate).not.toHaveBeenCalled();
          expect(error).toBe('Internal Server Error');
          done();
        },
      });
    });

    it('should pass through successful requests without interference', (done) => {
      // Arrange
      const request = new HttpRequest('GET', '/api/data');
      const successResponse = new HttpResponse({ body: { data: 'success' }, status: 200 });

      httpHandler.handle.and.returnValue(of(successResponse));

      // Act
      interceptor.intercept(request, httpHandler).subscribe({
        next: (response) => {
          // Assert
          expect(router.navigate).not.toHaveBeenCalled();
          expect(response).toEqual(successResponse);
          done();
        },
      });
    });

    it('should extract error message from error.message when available', (done) => {
      // Arrange
      const request = new HttpRequest('GET', '/api/test');
      const errorResponse = new HttpErrorResponse({
        error: { message: 'Custom error message' },
        status: 401,
        statusText: 'Unauthorized',
      });

      httpHandler.handle.and.returnValue(throwError(errorResponse));

      // Act
      interceptor.intercept(request, httpHandler).subscribe({
        error: (error) => {
          // Assert
          expect(error).toBe('Custom error message');
          done();
        },
      });
    });

    it('should fallback to statusText when error.message is not available', (done) => {
      // Arrange
      const request = new HttpRequest('GET', '/api/test');
      const errorResponse = new HttpErrorResponse({
        error: 'Simple error',
        status: 401,
        statusText: 'Unauthorized',
      });

      httpHandler.handle.and.returnValue(throwError(errorResponse));

      // Act
      interceptor.intercept(request, httpHandler).subscribe({
        error: (error) => {
          // Assert
          expect(error).toBe('Unauthorized');
          done();
        },
      });
    });

    it('should handle auth check requests with different URL patterns', (done) => {
      // Arrange
      const requests = [
        new HttpRequest('GET', 'https://example.com/api/auth/me'),
        new HttpRequest('GET', '/api/auth/me?param=value'),
        new HttpRequest('GET', '/api/auth/me/status'),
      ];

      let completedRequests = 0;
      const errorResponse = new HttpErrorResponse({
        status: 401,
        statusText: 'Unauthorized',
      });

      requests.forEach(request => {
        httpHandler.handle.and.returnValue(throwError(errorResponse));

        // Act
        interceptor.intercept(request, httpHandler).subscribe({
          error: () => {
            completedRequests++;

            // Assert - None should trigger redirect
            if (completedRequests === requests.length) {
              expect(router.navigate).not.toHaveBeenCalled();
              done();
            }
          },
        });
      });
    });

    it('should handle requests with null or undefined error objects', (done) => {
      // Arrange
      const request = new HttpRequest('GET', '/api/test');
      const errorResponse = new HttpErrorResponse({
        error: null,
        status: 401,
        statusText: 'Unauthorized',
      });

      httpHandler.handle.and.returnValue(throwError(errorResponse));

      // Act
      interceptor.intercept(request, httpHandler).subscribe({
        error: (error) => {
          // Assert
          expect(error).toBe('Unauthorized');
          expect(router.navigate).toHaveBeenCalledWith(['/account/login']);
          done();
        },
      });
    });
  });
});
