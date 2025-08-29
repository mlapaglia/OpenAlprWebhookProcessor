import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { of, throwError, Subject } from 'rxjs';

import { LoginComponent } from './login.component';
import { AccountService } from './../account.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { ThemeStorage, type DocsSiteTheme } from 'app/theme-picker/theme-storage/theme-storage';
import { StyleManager } from 'app/theme-picker/style-manager/style-manager.component';
import type { User } from 'app/_models';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let mockAccountService: jasmine.SpyObj<AccountService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockActivatedRoute: jasmine.SpyObj<ActivatedRoute>;
  let mockSnackbarService: jasmine.SpyObj<SnackbarService>;
  let mockThemeStorage: jasmine.SpyObj<ThemeStorage>;
  let mockStyleManager: jasmine.SpyObj<StyleManager>;
  let themeUpdateSubject: Subject<DocsSiteTheme>;

  beforeEach(async () => {
    themeUpdateSubject = new Subject<DocsSiteTheme>();

    mockAccountService = jasmine.createSpyObj('AccountService', ['canRegister', 'login']);
    mockRouter = jasmine.createSpyObj('Router', ['navigateByUrl', 'navigate']);
    mockActivatedRoute = jasmine.createSpyObj('ActivatedRoute', [], {
      snapshot: { queryParams: {} },
    });
    mockSnackbarService = jasmine.createSpyObj('SnackbarService', ['create']);
    mockThemeStorage = jasmine.createSpyObj('ThemeStorage', ['getStoredThemeName'], {
      onThemeUpdate: themeUpdateSubject.asObservable(),
    });
    mockStyleManager = jasmine.createSpyObj('StyleManager', ['setStyle', 'removeStyle']);

    mockAccountService.canRegister.and.returnValue(of(true));
    mockStyleManager.setStyle.and.returnValue(Promise.resolve());

    await TestBed.configureTestingModule({
      imports: [
        LoginComponent,
        ReactiveFormsModule,
        NoopAnimationsModule,
        MatCardModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        MatIconModule,
        MatProgressSpinnerModule,
        MatCheckboxModule,
      ],
      providers: [
        FormBuilder,
        { provide: AccountService, useValue: mockAccountService },
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: SnackbarService, useValue: mockSnackbarService },
        { provide: ThemeStorage, useValue: mockThemeStorage },
        { provide: StyleManager, useValue: mockStyleManager },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
  });

  describe('component initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.loading).toBe(false);
      expect(component.submitted).toBe(false);
      expect(component.canRegister).toBe(false);
      expect(component.currentThemeName).toBe('');
      expect(component.themeLoaded).toBe(false);
    });

    it('should initialize form with correct validators', () => {
      component.ngOnInit();

      expect(component.form).toBeDefined();
      expect(component.form.get('username')?.hasError('required')).toBe(true);
      expect(component.form.get('password')?.hasError('required')).toBe(true);
      expect(component.form.get('rememberMe')?.value).toBe(false);
    });

    it('should check if registration is allowed on init', () => {
      component.ngOnInit();

      expect(mockAccountService.canRegister).toHaveBeenCalled();
      expect(component.canRegister).toBe(true);
    });

    it('should set theme from storage on init', async () => {
      mockThemeStorage.getStoredThemeName.and.returnValue('pink-bluegrey');

      component.ngOnInit();
      await fixture.whenStable();

      expect(component.currentThemeName).toBe('pink-bluegrey');
      expect(component.themeLoaded).toBe(true);
    });

    it('should use default theme when no theme is stored', async () => {
      mockThemeStorage.getStoredThemeName.and.returnValue(null);

      component.ngOnInit();
      await fixture.whenStable();

      expect(component.currentThemeName).toBe('indigo-pink');
      expect(component.themeLoaded).toBe(true);
    });

    it('should subscribe to theme changes', () => {
      component.ngOnInit();

      const newTheme: DocsSiteTheme = {
        name: 'purple-green',
        displayName: 'Purple & Green',
        primary: '#9C27B0',
        accent: '#4CAF50',
      };

      themeUpdateSubject.next(newTheme);

      expect(component.currentThemeName).toBe('purple-green');
    });
  });

  describe('form controls getter', () => {
    it('should return form controls', () => {
      component.ngOnInit();

      const controls = component.f;

      expect(controls.username).toBeDefined();
      expect(controls.password).toBeDefined();
      expect(controls.rememberMe).toBeDefined();
    });
  });

  describe('form validation', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should not submit when form is invalid', () => {
      component.onSubmit();

      expect(component.submitted).toBe(true);
      expect(mockAccountService.login).not.toHaveBeenCalled();
    });

    it('should submit when form is valid', () => {
      const user: User = {
        id: 123,
        isDeleting: false,
        password: 'testpass',
        username: 'testuser',
        firstName: 'Test',
        lastName: 'User',
        twoFactorEnabled: false,
        hasPasskeys: false,
      };

      component.ngOnInit();
      mockAccountService.login.and.returnValue(of(user));

      component.form.patchValue({
        username: 'testuser',
        password: 'testpass',
        rememberMe: true,
      });

      component.onSubmit();

      expect(component.submitted).toBe(true);
      expect(component.loading).toBe(false); // Loading becomes false after successful login
      expect(mockAccountService.login).toHaveBeenCalledWith('testuser', 'testpass', true);
    });
  });

  describe('login process', () => {
    beforeEach(() => {
      component.ngOnInit();
      component.form.patchValue({
        username: 'testuser',
        password: 'testpass',
        rememberMe: false,
      });
    });

    it('should navigate to default route on successful login without 2FA', () => {
      const loginResponse: User = {
        id: 123,
        isDeleting: false,
        password: 'testpass',
        username: 'testuser',
        firstName: 'Test',
        lastName: 'User',
        twoFactorEnabled: false,
        hasPasskeys: false,
      };
      mockAccountService.login.and.returnValue(of(loginResponse));

      component.onSubmit();

      expect(mockRouter.navigateByUrl).toHaveBeenCalledWith('/');
    });

    it('should navigate to return URL on successful login without 2FA', () => {
      const loginResponse: User = {
        id: 123,
        isDeleting: false,
        password: 'testpass',
        username: 'testuser',
        firstName: 'Test',
        lastName: 'User',
        twoFactorEnabled: false,
        hasPasskeys: false,
      };
      mockAccountService.login.and.returnValue(of(loginResponse));
      mockActivatedRoute.snapshot.queryParams = { returnUrl: '/dashboard' };

      component.onSubmit();

      expect(mockRouter.navigateByUrl).toHaveBeenCalledWith('/dashboard');
    });

    it('should navigate to 2FA verification when 2FA is required', () => {
      const loginResponse: User = {
        id: 123,
        isDeleting: false,
        password: 'testpass',
        username: 'testuser',
        firstName: 'Test',
        lastName: 'User',
        twoFactorEnabled: true,
        hasPasskeys: false,
      };
      mockAccountService.login.and.returnValue(of(loginResponse));
      mockRouter.navigate.and.returnValue(Promise.resolve(true));

      component.onSubmit();

      expect(mockRouter.navigate).toHaveBeenCalledWith(['/account/verify-2fa'], {
        queryParams: {
          userId: 123,
          username: 'testuser',
          hasPasskeys: false,
          rememberMe: false,
          returnUrl: '/',
        },
      });
    });

    it('should navigate to 2FA verification with return URL when 2FA is required', () => {
      const loginResponse: User = {
        id: 456,
        isDeleting: false,
        password: 'testpass',
        username: 'testuser',
        firstName: 'Test',
        lastName: 'User',
        twoFactorEnabled: true,
        hasPasskeys: false,
      };
      mockAccountService.login.and.returnValue(of(loginResponse));
      mockActivatedRoute.snapshot.queryParams = { returnUrl: '/dashboard' };
      mockRouter.navigate.and.returnValue(Promise.resolve(true));

      component.form.patchValue({ rememberMe: true });

      component.onSubmit();

      expect(mockRouter.navigate).toHaveBeenCalledWith(['/account/verify-2fa'], {
        queryParams: {
          userId: 456,
          username: 'testuser',
          hasPasskeys: false,
          rememberMe: true,
          returnUrl: '/dashboard',
        },
      });
    });

    it('should show error message on failed login', () => {
      mockAccountService.login.and.returnValue(throwError(() => 'Invalid credentials'));

      component.onSubmit();

      expect(mockSnackbarService.create).toHaveBeenCalledWith('Invalid credentials', SnackBarType.Error);
      expect(component.loading).toBe(false);
    });
  });

  describe('theme functionality', () => {
    it('should handle theme with isDark undefined', () => {
      component.ngOnInit();

      const themeWithoutIsDark: DocsSiteTheme = {
        name: 'test-theme',
        primary: '#000000',
        accent: '#FFFFFF',
        displayName: 'Test Theme',
      };

      themeUpdateSubject.next(themeWithoutIsDark);

      expect(component.currentThemeName).toBe('test-theme');
    });

    it('should handle unknown stored theme name', () => {
      mockThemeStorage.getStoredThemeName.and.returnValue('unknown-theme');

      component.ngOnInit();

      expect(component.currentThemeName).toBe('indigo-pink');
      expect(component.themeLoaded).toBe(true);
    });
  });

  describe('component cleanup', () => {
    it('should call super.ngOnDestroy for cleanup', () => {
      component.ngOnInit();
      spyOn(Object.getPrototypeOf(Object.getPrototypeOf(component)), 'ngOnDestroy');

      component.ngOnDestroy();

      expect(Object.getPrototypeOf(Object.getPrototypeOf(component)).ngOnDestroy).toHaveBeenCalled();
    });
  });

  describe('registration availability', () => {
    it('should handle registration not allowed', () => {
      mockAccountService.canRegister.and.returnValue(of(false));

      component.ngOnInit();

      expect(component.canRegister).toBe(false);
    });
  });
});
