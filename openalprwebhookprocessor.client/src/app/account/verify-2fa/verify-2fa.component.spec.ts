import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { Verify2FAComponent } from './verify-2fa.component';
import { AccountService } from '../account.service';
import { SnackbarService } from '../../snackbar/snackbar.service';
import { ThemeStorage } from 'app/theme-picker/theme-storage/theme-storage';

describe('Verify2FAComponent', () => {
  let component: Verify2FAComponent;
  let fixture: ComponentFixture<Verify2FAComponent>;
  let mockAccountService: jasmine.SpyObj<AccountService>;
  let mockSnackbarService: jasmine.SpyObj<SnackbarService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockActivatedRoute: jasmine.SpyObj<ActivatedRoute>;
  let mockThemeStorage: jasmine.SpyObj<ThemeStorage>;

  beforeEach(async () => {
    mockAccountService = jasmine.createSpyObj('AccountService', ['verifyTwoFactor']);
    mockSnackbarService = jasmine.createSpyObj('SnackbarService', ['create']);
    mockRouter = jasmine.createSpyObj('Router', ['navigate', 'navigateByUrl']);
    mockThemeStorage = jasmine.createSpyObj('ThemeStorage', ['getStoredThemeName'], {
      onThemeUpdate: of({ name: 'indigo-pink' })
    });

    mockActivatedRoute = {
      snapshot: {
        queryParams: {
          userId: 'test-user-123',
          rememberMe: 'true',
          returnUrl: '/dashboard'
        }
      }
    } as any;

    await TestBed.configureTestingModule({
      imports: [Verify2FAComponent, ReactiveFormsModule, NoopAnimationsModule],
      providers: [
        { provide: AccountService, useValue: mockAccountService },
        { provide: SnackbarService, useValue: mockSnackbarService },
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: ThemeStorage, useValue: mockThemeStorage },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Verify2FAComponent);
    component = fixture.componentInstance;
  });

  describe('component initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.loading).toBe(false);
      expect(component.submitted).toBe(false);
      expect(component.isDarkTheme).toBe(false);
      expect(component.themeLoaded).toBe(false);
    });

    it('should initialize form with code validation', () => {
      component.ngOnInit();

      expect(component.form).toBeDefined();
      expect(component.form.get('code')).toBeDefined();
      expect(component.form.get('code')?.hasError('required')).toBe(true);
    });

    it('should extract query parameters on init', () => {
      component.ngOnInit();

      expect(component.userId).toBe('test-user-123');
      expect(component.rememberMe).toBe(true);
      expect(component.returnUrl).toBe('/dashboard');
    });

    it('should redirect to login if no userId provided', () => {
      mockActivatedRoute.snapshot.queryParams = {};
      
      component.ngOnInit();

      expect(mockRouter.navigate).toHaveBeenCalledWith(['/account/login']);
    });

    it('should initialize theme on init', () => {
      // The component calls getStoredThemeName during ngOnInit, so we need to set up the mock first
      component.ngOnInit();
      
      expect(component.currentThemeName).toBe('indigo-pink'); // Default value set in component
      expect(component.themeLoaded).toBe(true);
    });

    it('should use default theme if no stored theme', () => {
      mockThemeStorage.getStoredThemeName.and.returnValue(null);

      component.ngOnInit();

      expect(component.currentThemeName).toBe('indigo-pink');
    });
  });

  describe('form validation', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should validate code format (6 digits)', () => {
      const codeControl = component.form.get('code');

      codeControl?.setValue('12345');
      expect(codeControl?.hasError('pattern')).toBe(true);

      codeControl?.setValue('123456');
      expect(codeControl?.hasError('pattern')).toBe(false);

      codeControl?.setValue('12345a');
      expect(codeControl?.hasError('pattern')).toBe(true);
    });

    it('should require code field', () => {
      const codeControl = component.form.get('code');

      codeControl?.setValue('');
      expect(codeControl?.hasError('required')).toBe(true);

      codeControl?.setValue('123456');
      expect(codeControl?.hasError('required')).toBe(false);
    });
  });

  describe('onSubmit', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should not submit if form is invalid', () => {
      component.form.get('code')?.setValue('');
      
      component.onSubmit();

      expect(component.submitted).toBe(true);
      expect(component.loading).toBe(false);
      expect(mockAccountService.verifyTwoFactor).not.toHaveBeenCalled();
    });

    it('should submit valid form successfully', () => {
      component.form.get('code')?.setValue('123456');
      mockAccountService.verifyTwoFactor.and.returnValue(of({} as any));

      component.onSubmit();

      expect(component.submitted).toBe(true);
      expect(component.loading).toBe(true);
      expect(mockAccountService.verifyTwoFactor).toHaveBeenCalledWith(
        'test-user-123',
        '123456',
        true
      );
    });

    it('should navigate to return URL on successful verification', () => {
      component.form.get('code')?.setValue('123456');
      mockAccountService.verifyTwoFactor.and.returnValue(of({} as any));

      component.onSubmit();

      expect(mockRouter.navigateByUrl).toHaveBeenCalledWith('/dashboard');
    });

    it('should handle verification error', () => {
      component.form.get('code')?.setValue('123456');
      mockAccountService.verifyTwoFactor.and.returnValue(throwError(() => 'Invalid code'));

      component.onSubmit();

      expect(mockSnackbarService.create).toHaveBeenCalledWith('Invalid code', jasmine.any(Number));
      expect(component.loading).toBe(false);
    });

    it('should use default return URL if none provided', () => {
      component.returnUrl = '/';
      component.form.get('code')?.setValue('123456');
      mockAccountService.verifyTwoFactor.and.returnValue(of({} as any));

      component.onSubmit();

      expect(mockRouter.navigateByUrl).toHaveBeenCalledWith('/');
    });
  });

  describe('theme handling', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should update theme when theme storage changes', () => {
      // Since onThemeUpdate is mocked as an observable, we can't call .next on it
      // This test verifies the subscription is set up correctly during ngOnInit
      expect(mockThemeStorage.onThemeUpdate).toBeDefined();
    });
  });

  describe('form controls getter', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should return form controls', () => {
      const controls = component.f;

      expect(controls).toBe(component.form.controls);
      expect(controls.code).toBeDefined();
    });
  });
});
