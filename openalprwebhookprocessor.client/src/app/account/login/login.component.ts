import { Component, inject, type OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, type FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { first } from 'rxjs/operators';
import { OnPushBaseComponent } from '../../_helpers/onpush-base.component';

import { AccountService } from 'app/_services';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { ThemeStorage, type DocsSiteTheme } from 'app/theme-picker/theme-storage/theme-storage';
import { StyleManager } from 'app/theme-picker/style-manager/style-manager.component';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCheckboxModule } from '@angular/material/checkbox';

@Component({
  selector: 'app-login',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatCheckboxModule,
  ],
  templateUrl: 'login.component.html',
  styleUrl: 'login.component.css',
})
export class LoginComponent extends OnPushBaseComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly accountService = inject(AccountService);
  private readonly snackbarService = inject(SnackbarService);
  private readonly themeStorage = inject(ThemeStorage);
  private readonly styleManager = inject(StyleManager);

  form: FormGroup;
  loading = false;
  passkeyLoading = false;
  submitted = false;
  public canRegister = false;
  public currentThemeName = '';
  public themeLoaded = false;

  ngOnInit() {
    this.subscribeAndMarkForCheck(
      this.accountService.canRegister(),
      (result) => {
        this.canRegister = result;
      },
    );

    this.form = this.formBuilder.group({
      username: ['', Validators.required],
      password: ['', Validators.required],
      rememberMe: [false],
    });

    void this.setThemeFromStorage();

    this.subscribeAndMarkForCheck(
      this.themeStorage.onThemeUpdate,
      (theme: DocsSiteTheme) => {
        this.currentThemeName = theme.name;
        this.themeLoaded = true;
      },
    );
  }

  private async setThemeFromStorage() {
    const storedThemeName = this.themeStorage.getStoredThemeName();

    const themes: DocsSiteTheme[] = [
      {
        primary: '#3F51B5',
        accent: '#E91E63',
        displayName: 'Light',
        name: 'indigo-pink',
      },
      {
        primary: '#E91E63',
        accent: '#607D8B',
        displayName: 'Dark',
        name: 'pink-bluegrey',
      },
    ];

    const themeName = storedThemeName ?? 'indigo-pink';
    const currentTheme = themes.find(theme => theme.name === themeName);
    if (currentTheme) {
      this.currentThemeName = currentTheme.name;

      try {
        // Always load the theme CSS file
        const themeUrl = `assets/themes/${currentTheme.name}.css`;
        await this.styleManager.setStyle('theme', themeUrl);
      } catch (error) {
        console.warn('Failed to load theme CSS:', error);
      }
    } else {
      this.currentThemeName = 'indigo-pink';
    }

    this.markForCheck();
    this.themeLoaded = true;
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

    this.subscribeAndMarkForCheck(
      this.accountService.login(this.f.username.value, this.f.password.value, this.f.rememberMe.value).pipe(first()),
      (response) => {
        this.loading = false;
        if (response.twoFactorEnabled) {
          void this.router.navigate(['/account/verify-2fa'], {
            queryParams: {
              userId: response.id,
              username: response.username,
              hasPasskeys: response.hasPasskeys,
              rememberMe: this.f.rememberMe.value,
              returnUrl: this.route.snapshot.queryParams['returnUrl'] ?? '/',
            },
          });
        } else {
          const returnUrl = this.route.snapshot.queryParams['returnUrl'] ?? '/';
          void this.router.navigateByUrl(returnUrl);
        }
      },
      (error) => {
        this.snackbarService.create(error as string, SnackBarType.Error);
        this.loading = false;
        this.markForCheck();
      },
    );
  }

  async authenticateWithPasskey() {
    const username = this.f.username.value;
    if (!username) {
      this.snackbarService.create('Please enter your username first', SnackBarType.Error);
      return;
    }

    this.passkeyLoading = true;
    this.markForCheck();

    try {
      // Check if WebAuthn is supported
      if (!window.navigator.credentials || !window.PublicKeyCredential) {
        throw new Error('Passkeys are not supported in this browser');
      }

      // Step 1: Get authentication options from server
      const optionsResponse = await this.accountService.authenticatePasskey(username).pipe(first()).toPromise();
      
      if (!optionsResponse?.options) {
        throw new Error('Failed to get authentication options');
      }

      // Step 2: Get credential using WebAuthn API
      const credential = await navigator.credentials.get({
        publicKey: this.convertAuthenticationOptions(optionsResponse.options)
      }) as PublicKeyCredential;

      if (!credential) {
        throw new Error('Failed to authenticate with passkey');
      }

      // Step 3: Send credential to server for verification
      const assertionResponse = this.encodeAssertionResponse(credential);
      
      const user = await this.accountService.completePasskeyAuthentication(
        username,
        assertionResponse,
        this.f.rememberMe.value
      ).pipe(first()).toPromise();

      if (user) {
        const returnUrl = this.route.snapshot.queryParams['returnUrl'] ?? '/';
        void this.router.navigateByUrl(returnUrl);
      }

    } catch (error) {
      console.error('Passkey authentication error:', error);
      this.snackbarService.create(
        `Passkey authentication failed: ${error instanceof Error ? error.message : 'Unknown error'}`, 
        SnackBarType.Error
      );
    } finally {
      this.passkeyLoading = false;
      this.markForCheck();
    }
  }

  private convertAuthenticationOptions(options: any): PublicKeyCredentialRequestOptions {
    return {
      ...options,
      challenge: this.base64urlToBuffer(options.challenge),
      allowCredentials: options.allowCredentials?.map((cred: any) => ({
        ...cred,
        id: this.base64urlToBuffer(cred.id)
      }))
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
        userHandle: response.userHandle ? this.bufferToBase64url(response.userHandle) : null
      }
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
