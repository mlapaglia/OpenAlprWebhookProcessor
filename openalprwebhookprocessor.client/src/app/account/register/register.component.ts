import { Component, inject, type OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, Validators, ReactiveFormsModule, type FormGroup } from '@angular/forms';
import { first } from 'rxjs/operators';

import { AccountService } from 'app/_services';
import { ThemeStorage, type DocsSiteTheme } from 'app/theme-picker/theme-storage/theme-storage';
import { StyleManager } from 'app/theme-picker/style-manager/style-manager.component';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';

@Component({
  templateUrl: 'register.component.html',
  styleUrl: 'register.component.css',
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
  ],
})
export class RegisterComponent extends OnPushBaseComponent implements OnInit {
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
  public currentThemeName = '';
  public themeLoaded = false;

  ngOnInit() {
    this.subscribeAndMarkForCheck(
      this.accountService.canRegister(),
      (canRegister) => {
        if (!canRegister) {
          void this.router.navigate(['../login'], { relativeTo: this.route });
          return;
        }
      },
      (error) => {
        console.error('Error checking registration status:', error);
        void this.router.navigate(['../login'], { relativeTo: this.route });
      },
    );

    this.form = this.formBuilder.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      username: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(8), Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$/)]],
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

  // convenience getter for easy access to form fields
  get f() {
    return this.form.controls;
  }

  onSubmit() {
    this.submitted = true;
    this.markForCheck();

    if (this.form.invalid) {
      this.snackbarService.create('Registration failed', SnackBarType.Error, 'Form is invalid');
      return;
    }

    this.loading = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.accountService.register(this.form.value).pipe(first()),
      () => {
        this.snackbarService.create('Registration successful', SnackBarType.Successful);
        void this.router.navigate(['../login'], { relativeTo: this.route });
      },
      error => {
        this.snackbarService.create('Registration failed', SnackBarType.Error, String(error));
        this.loading = false;
        this.markForCheck();
      },
    );
  }
}
