import { Component, type OnInit, type OnDestroy, inject, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, type FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { first } from 'rxjs/operators';

import { AccountService } from '../account.service';
import { SnackbarService } from '../../snackbar/snackbar.service';
import { ThemeStorage, type DocsSiteTheme } from 'app/theme-picker/theme-storage/theme-storage';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { RefreshButtonComponent } from 'app/shared/refresh-button/refresh-button.component';

@Component({
  selector: 'app-verify-2fa',
  templateUrl: 'verify-2fa.component.html',
  styleUrl: 'verify-2fa.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatIconModule,
    RefreshButtonComponent,
  ],
})
export class Verify2FAComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  private readonly formBuilder = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly accountService = inject(AccountService);
  private readonly snackbarService = inject(SnackbarService);
  private readonly themeStorage = inject(ThemeStorage);

  form!: FormGroup;
  loading = false;
  passkeyLoading = false;
  submitted = false;
  userId!: string;
  username!: string;
  hasPasskeys = false;
  rememberMe = false;
  returnUrl = '/';

  public isDarkTheme = false;
  public currentThemeName = '';
  public themeLoaded = false;

  ngOnInit() {
    this.form = this.formBuilder.group({
      code: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
    });

    this.userId = this.route.snapshot.queryParams['userId'];
    this.username = this.route.snapshot.queryParams['username'];
    this.hasPasskeys = this.route.snapshot.queryParams['hasPasskeys'] === 'true';
    this.rememberMe = this.route.snapshot.queryParams['rememberMe'] === 'true';
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] ?? '/';

    if (!this.userId) {
      void this.router.navigate(['/account/login']);
    }

    this.initializeTheme();
  }

  override ngOnDestroy() {
    super.ngOnDestroy();
  }

  private initializeTheme() {
    const storedTheme = this.themeStorage.getStoredThemeName();
    this.currentThemeName = storedTheme ?? 'indigo-pink';
    this.themeLoaded = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.themeStorage.onThemeUpdate,
      (theme: DocsSiteTheme) => {
        this.currentThemeName = theme.name;
      },
    );
  }

  get f() {
    return this.form.controls;
  }

  onSubmit() {
    this.submitted = true;
    this.markForCheck();

    if (this.form.invalid) {
      return;
    }

    this.loading = true;
    this.markForCheck();

    this.accountService.verifyTwoFactor(this.userId, this.f.code.value, this.rememberMe)
      .pipe(first())
      .subscribe({
        next: () => {
          void this.router.navigateByUrl(this.returnUrl);
        },
        error: (error) => {
          this.snackbarService.create(error, SnackBarType.Error);
          this.loading = false;
          this.markForCheck();
        },
      });
  }

  async authenticateWithPasskey() {
    if (!this.username) {
      this.snackbarService.create('Username not available for passkey authentication', SnackBarType.Error);
      return;
    }

    this.passkeyLoading = true;
    this.markForCheck();

    try {
      if (!('credentials' in navigator) || !('PublicKeyCredential' in window)) {
        throw new Error('Passkeys are not supported in this browser');
      }

      const optionsResponse = await firstValueFrom(this.accountService.authenticatePasskey(this.username));



      const credential = await navigator.credentials.get({
        publicKey: this.convertAuthenticationOptions(optionsResponse.options),
      }) as PublicKeyCredential | null;

      if (!credential) {
        throw new Error('Failed to authenticate with passkey');
      }

      const assertionResponse = this.encodeAssertionResponse(credential);

      await firstValueFrom(this.accountService.completePasskeyAuthentication(
        this.username,
        assertionResponse,
        this.rememberMe,
      ));

      void this.router.navigateByUrl(this.returnUrl);

    } catch (error) {
      console.error('Passkey authentication error:', error);
      this.snackbarService.create(
        `Passkey authentication failed: ${error instanceof Error ? error.message : 'Unknown error'}`,
        SnackBarType.Error,
      );
    } finally {
      this.passkeyLoading = false;
      this.markForCheck();
    }
  }

  private convertAuthenticationOptions(options: PublicKeyCredentialRequestOptions): PublicKeyCredentialRequestOptions {
    return {
      ...options,
      challenge: this.base64urlToBuffer(options.challenge as unknown as string),
      allowCredentials: options.allowCredentials?.map((cred) => ({
        ...cred,
        id: this.base64urlToBuffer(cred.id as unknown as string),
      })),
    };
  }

  private encodeAssertionResponse(credential: PublicKeyCredential): string {
    const response = credential.response as AuthenticatorAssertionResponse;

    return JSON.stringify({
      id: credential.id,
      rawId: this.bufferToBase64url(credential.rawId),
      type: credential.type,
      response: {
        authenticatorData: this.bufferToBase64url(response.authenticatorData),
        clientDataJSON: this.bufferToBase64url(response.clientDataJSON),
        signature: this.bufferToBase64url(response.signature),
        userHandle: response.userHandle ? this.bufferToBase64url(response.userHandle) : null,
      },
    });
  }

  private base64urlToBuffer(base64url: string): ArrayBuffer {
    const base64 = base64url.replace(/-/g, '+').replace(/_/g, '/');
    const padded = base64.padEnd(base64.length + (4 - base64.length % 4) % 4, '=');
    const binary = atob(padded);
    const buffer = new ArrayBuffer(binary.length);
    const bytes = new Uint8Array(buffer);
    for (let i = 0; i < binary.length; i++) {
      bytes[i] = binary.charCodeAt(i);
    }
    return buffer;
  }

  private bufferToBase64url(buffer: ArrayBuffer): string {
    const bytes = new Uint8Array(buffer);
    let binary = '';
    for (let i = 0; i < bytes.byteLength; i++) {
      binary += String.fromCharCode(bytes[i]);
    }
    return btoa(binary).replace(/\+/g, '-').replace(/\//g, '_').replace(/=/g, '');
  }
}
