import { Component, inject, type OnInit, type OnDestroy } from '@angular/core';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, type FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { first } from 'rxjs/operators';
import { Subscription } from 'rxjs';

import { AccountService } from 'app/_services';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { ThemeStorage, type DocsSiteTheme } from 'app/theme-picker/theme-storage/theme-storage';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCheckboxModule } from '@angular/material/checkbox';

@Component({
  selector: 'app-login',
  templateUrl: 'login.component.html',
  styleUrl: 'login.component.css',
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
})
export class LoginComponent implements OnInit, OnDestroy {
  private readonly formBuilder = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly accountService = inject(AccountService);
  private readonly snackbarService = inject(SnackbarService);
  private readonly themeStorage = inject(ThemeStorage);

  form: FormGroup;
  loading = false;
  submitted = false;
  public canRegister = false;
  public isDarkTheme = false;
  public currentThemeName = '';

  private themeSubscription = new Subscription();

  ngOnInit() {
    this.accountService.canRegister().subscribe((result) => {
      this.canRegister = result;
    });

    this.form = this.formBuilder.group({
      username: ['', Validators.required],
      password: ['', Validators.required],
      rememberMe: [false],
    });

    // Set initial theme
    this.setThemeFromStorage();

    // Subscribe to theme changes
    this.themeSubscription = this.themeStorage.onThemeUpdate.subscribe((theme: DocsSiteTheme) => {
      this.isDarkTheme = theme.isDark ?? false;
      this.currentThemeName = theme.name;
    });
  }

  ngOnDestroy() {
    this.themeSubscription.unsubscribe();
  }

  private setThemeFromStorage() {
    const storedThemeName = this.themeStorage.getStoredThemeName();

    // Define available themes (matching theme-picker component)
    const themes: DocsSiteTheme[] = [
      { primary: '#673AB7', accent: '#FFC107', displayName: 'Deep Purple & Amber', name: 'deeppurple-amber', isDark: false },
      { primary: '#3F51B5', accent: '#E91E63', displayName: 'Indigo & Pink', name: 'indigo-pink', isDark: false, isDefault: true },
      { primary: '#E91E63', accent: '#607D8B', displayName: 'Pink & Blue-grey', name: 'pink-bluegrey', isDark: true },
      { primary: '#9C27B0', accent: '#4CAF50', displayName: 'Purple & Green', name: 'purple-green', isDark: true },
    ];

    if (storedThemeName) {
      const currentTheme = themes.find(theme => theme.name === storedThemeName);
      this.isDarkTheme = currentTheme?.isDark ?? false;
      this.currentThemeName = currentTheme?.name ?? 'indigo-pink';
    } else {
      // Default to light theme if no theme is stored
      this.isDarkTheme = false;
      this.currentThemeName = 'indigo-pink';
    }
  }

  get f() {
    return this.form.controls;
  }

  onSubmit() {
    this.submitted = true;

    if (this.form.invalid) {
      return;
    }

    this.loading = true;
    this.accountService.login(this.f.username.value, this.f.password.value, this.f.rememberMe.value)
      .pipe(first())
      .subscribe({
        next: () => {
          const returnUrl = this.route.snapshot.queryParams['returnUrl'] ?? '/';
          void this.router.navigateByUrl(returnUrl);
        },
        error: (error) => {
          this.snackbarService.create(error, SnackBarType.Error);
          this.loading = false;
        },
      });
  }
}
