import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, type Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { User } from 'app/_models';

@Injectable({ providedIn: 'root' })
export class AccountService {
  private readonly router = inject(Router);
  private readonly http = inject(HttpClient);

  private readonly userSubject: BehaviorSubject<User>;
  public user: Observable<User>;

  constructor() {
    this.userSubject = new BehaviorSubject<User>(new User());
    this.user = this.userSubject.asObservable();
  }

  public get userValue(): User {
    return this.userSubject.value;
  }

  checkAuthenticationStatus() {
    return this.http.get<User>('/api/auth/me').pipe(
      map((user) => {
        this.userSubject.next(user);
        return user;
      }),
      catchError(() => {
        this.userSubject.next(new User());
        return of(new User());
      }),
    );
  }

  login(username: string, password: string, rememberMe: boolean = false) {
    return this.http.post<User>('/api/auth/authenticate', { username, password, rememberMe })
      .pipe(map((response) => {
        if (response.twoFactorEnabled) {
          return response;
        }

        this.userSubject.next(response);
        return response;
      }));
  }

  verifyTwoFactor(userId: string, code: string, rememberMe: boolean = false) {
    return this.http.post<User>('/api/auth/verify-2fa', { userId, code, rememberMe })
      .pipe(map((user) => {
        this.userSubject.next(user);
        return user;
      }));
  }

  getTwoFactorStatus() {
    return this.http.get<{ isTwoFactorEnabled: boolean, hasAuthenticator: boolean }>('/api/twofactor/status');
  }

  setupTwoFactor() {
    return this.http.post<{ sharedKey: string, qrCodeUri: string }>('/api/twofactor/setup', {});
  }

  enableTwoFactor(code: string) {
    return this.http.post<{ message: string, recoveryCodes: string[] }>('/api/twofactor/enable', { code });
  }

  disableTwoFactor() {
    return this.http.post<{ message: string }>('/api/twofactor/disable', {});
  }

  getRecoveryCodes() {
    return this.http.get<{ recoveryCodes: string[] }>('/api/twofactor/recovery-codes');
  }

  getTwoFactorStatusForUser(userId: string) {
    return this.http.get<{ isTwoFactorEnabled: boolean, hasAuthenticator: boolean }>(`/api/users/${userId}/twofactor/status`);
  }

  setupTwoFactorForUser(userId: string) {
    return this.http.post<{ sharedKey: string, qrCodeUri: string }>(`/api/users/${userId}/twofactor/setup`, {});
  }

  enableTwoFactorForUser(userId: string, code: string) {
    return this.http.post<{ message: string, recoveryCodes: string[] }>(`/api/users/${userId}/twofactor/enable`, { code });
  }

  disableTwoFactorForUser(userId: string) {
    return this.http.post<{ message: string }>(`/api/users/${userId}/twofactor/disable`, {});
  }

  getRecoveryCodesForUser(userId: string) {
    return this.http.get<{ recoveryCodes: string[] }>(`/api/users/${userId}/twofactor/recovery-codes`);
  }

  logout() {
    this.http.post<null>('/api/auth/logout', {}).subscribe({
      next: () => {
        this.finalizeLogout();
      },
      error: () => {
        this.finalizeLogout();
      },
    });
  }

  finalizeLogout() {
    this.userSubject.next(new User());
    void this.router.navigate(['/account/login']);
  }

  canRegister() {
    return this.http.get<boolean>('/api/users/canregister');
  }

  add(user: User) {
    return this.http.post('/api/users/add', user);
  }

  register(user: User) {
    return this.http.post('/api/auth/register', user);
  }

  getAll() {
    return this.http.get<User[]>('/api/users');
  }

  getById(id: string) {
    return this.http.get<User>(`/api/users/${id}`);
  }

  update(id, params) {
    return this.http.post(`/api/users/${id}`, params)
      .pipe(map((x) => {
        // update stored user if the logged in user updated their own record
        if (id == this.userValue.id) {
          const user = { ...this.userValue, ...params };
          localStorage.setItem('user', JSON.stringify(user));

          this.userSubject.next(user);
        }
        return x;
      }));
  }

  delete(id: number) {
    return this.http.delete(`/api/users/${id}`)
      .pipe(map((x) => {
        // auto logout if the logged in user deleted their own record
        if (id == this.userValue.id) {
          this.logout();
        }
        return x;
      }));
  }

  // Passkey methods
  registerPasskey(name?: string) {
    return this.http.post<{ options: any }>('/api/auth/passkey/register', { name });
  }

  completePasskeyRegistration(attestationResponse: string, name?: string) {
    return this.http.post<{ message: string, success: boolean }>('/api/auth/passkey/complete-registration', { 
      attestationResponse, 
      name 
    });
  }

  authenticatePasskey(username: string) {
    return this.http.post<{ options: any }>('/api/auth/passkey/authenticate', { username });
  }

  completePasskeyAuthentication(username: string, assertionResponse: string, rememberMe: boolean = false) {
    return this.http.post<User>('/api/auth/passkey/complete-authentication', { 
      username, 
      assertionResponse, 
      rememberMe 
    }).pipe(map((user) => {
      this.userSubject.next(user);
      return user;
    }));
  }

  getPasskeys() {
    return this.http.get<{ passkeys: any[] }>('/api/auth/passkey/list');
  }

  deletePasskey(passkeyId: number) {
    return this.http.delete<{ message: string, success: boolean }>(`/api/auth/passkey/${passkeyId}`);
  }
}
