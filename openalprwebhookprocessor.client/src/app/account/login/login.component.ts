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
}
