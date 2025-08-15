import { Component, type OnInit, type OnDestroy, inject, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, type FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
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
  submitted = false;
  userId!: string;
  rememberMe = false;
  returnUrl = '/';

  public isDarkTheme = false;
  public currentThemeName = '';
  public themeLoaded = false;

  ngOnInit() {
    this.form = this.formBuilder.group({
      code: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
    });

    // Get parameters from query string
    this.userId = this.route.snapshot.queryParams['userId'];
    this.rememberMe = this.route.snapshot.queryParams['rememberMe'] === 'true';
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] ?? '/';

    if (!this.userId) {
      void this.router.navigate(['/account/login']);
    }

    // Initialize theme
    this.initializeTheme();
  }

  override ngOnDestroy() {
    super.ngOnDestroy();
  }

  private initializeTheme() {
    // Set initial theme
    const storedTheme = this.themeStorage.getStoredThemeName();
    this.currentThemeName = storedTheme ?? 'indigo-pink';
    this.themeLoaded = true;
    this.markForCheck();

    // Subscribe to theme changes
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
}
