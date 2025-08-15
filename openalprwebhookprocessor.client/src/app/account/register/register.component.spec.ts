import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';

import { RegisterComponent } from './register.component';
import { AccountService } from '../account.service';
import { AlertService } from 'app/_services';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;
  let mockAccountService: jasmine.SpyObj<AccountService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockActivatedRoute: jasmine.SpyObj<ActivatedRoute>;
  let mockAlertService: jasmine.SpyObj<AlertService>;

  beforeEach(async () => {
    mockAccountService = jasmine.createSpyObj('AccountService', ['register', 'canRegister']);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    mockActivatedRoute = {} as jasmine.SpyObj<ActivatedRoute>;
    mockAlertService = jasmine.createSpyObj('AlertService', ['clear', 'success', 'error']);

    // Setup default mock behavior - registration is allowed
    mockAccountService.canRegister.and.returnValue(of(true));

    await TestBed.configureTestingModule({
      imports: [
        RegisterComponent,
        ReactiveFormsModule,
        NoopAnimationsModule,
      ],
      providers: [
        FormBuilder,
        { provide: AccountService, useValue: mockAccountService },
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: AlertService, useValue: mockAlertService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
  });

  describe('component initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.loading).toBe(false);
      expect(component.submitted).toBe(false);
    });

    it('should initialize form with correct validators', () => {
      component.ngOnInit();

      expect(component.form).toBeDefined();
      expect(component.form.get('firstName')?.hasError('required')).toBe(true);
      expect(component.form.get('lastName')?.hasError('required')).toBe(true);
      expect(component.form.get('username')?.hasError('required')).toBe(true);
      expect(component.form.get('password')?.hasError('required')).toBe(true);
    });

    it('should validate password minimum length', () => {
      component.ngOnInit();

      component.form.patchValue({ password: '12345' });
      expect(component.form.get('password')?.hasError('minlength')).toBe(true);

      component.form.patchValue({ password: 'Password123' }); // 8+ chars, uppercase, lowercase, digit
      expect(component.form.get('password')?.hasError('minlength')).toBe(false);
    });
  });

  describe('form controls getter', () => {
    it('should return form controls', () => {
      component.ngOnInit();

      const controls = component.f;

      expect(controls.firstName).toBeDefined();
      expect(controls.lastName).toBeDefined();
      expect(controls.username).toBeDefined();
      expect(controls.password).toBeDefined();
    });
  });

  describe('form validation', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should not submit when form is invalid', () => {
      component.onSubmit();

      expect(component.submitted).toBe(true);
      expect(mockAlertService.clear).toHaveBeenCalled();
      expect(mockAccountService.register).not.toHaveBeenCalled();
    });

    it('should submit when form is valid', () => {
      mockAccountService.register.and.returnValue(of({}));

      component.form.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        password: 'Password123',
      });

      component.onSubmit();

      expect(component.submitted).toBe(true);
      expect(component.loading).toBe(true);
      expect(mockAlertService.clear).toHaveBeenCalled();
      expect(mockAccountService.register).toHaveBeenCalledWith(jasmine.objectContaining({
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        password: 'Password123',
      }));
    });
  });

  describe('registration process', () => {
    beforeEach(() => {
      component.ngOnInit();
      component.form.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        password: 'Password123',
      });
    });

    it('should show success message and navigate to login on successful registration', () => {
      mockAccountService.register.and.returnValue(of({}));

      component.onSubmit();

      expect(mockAlertService.success).toHaveBeenCalledWith('Registration successful', true);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['../login'], { relativeTo: mockActivatedRoute });
    });

    it('should show error message on failed registration', () => {
      const errorMessage = 'Username already exists';
      mockAccountService.register.and.returnValue(throwError(() => errorMessage));

      component.onSubmit();

      expect(mockAlertService.error).toHaveBeenCalledWith('Username already exists', true);
      expect(component.loading).toBe(false);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should show specific server error message on failed registration', () => {
      const errorMessage = 'Registration failed: Email \'asdf\' is invalid.';
      mockAccountService.register.and.returnValue(throwError(() => errorMessage));

      component.onSubmit();

      expect(mockAlertService.error).toHaveBeenCalledWith('Registration failed: Email \'asdf\' is invalid.', true);
      expect(component.loading).toBe(false);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });
  });

  describe('registration permission check', () => {
    it('should redirect to login if registration is not allowed', () => {
      mockAccountService.canRegister.and.returnValue(of(false));

      component.ngOnInit();

      expect(mockRouter.navigate).toHaveBeenCalledWith(['../login'], { relativeTo: mockActivatedRoute });
    });

    it('should redirect to login if canRegister check fails', () => {
      mockAccountService.canRegister.and.returnValue(throwError(() => 'Server error'));

      component.ngOnInit();

      expect(mockRouter.navigate).toHaveBeenCalledWith(['../login'], { relativeTo: mockActivatedRoute });
    });

    it('should continue initialization if registration is allowed', () => {
      mockAccountService.canRegister.and.returnValue(of(true));

      component.ngOnInit();

      expect(mockRouter.navigate).not.toHaveBeenCalled();
      expect(component.form).toBeDefined();
    });
  });

  describe('form field validation messages', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should show required error for empty fields', () => {
      component.form.patchValue({
        firstName: '',
        lastName: '',
        username: '',
        password: '',
      });
      component.form.markAllAsTouched();

      expect(component.form.get('firstName')?.hasError('required')).toBe(true);
      expect(component.form.get('lastName')?.hasError('required')).toBe(true);
      expect(component.form.get('username')?.hasError('required')).toBe(true);
      expect(component.form.get('password')?.hasError('required')).toBe(true);
    });

    it('should show minlength error for short password', () => {
      component.form.patchValue({ password: '123' });
      component.form.get('password')?.markAsTouched();

      expect(component.form.get('password')?.hasError('minlength')).toBe(true);
      expect(component.form.get('password')?.errors?.['minlength'].requiredLength).toBe(8);
    });

    it('should be valid when all fields are properly filled', () => {
      component.form.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        password: 'Password123',
      });

      expect(component.form.valid).toBe(true);
    });
  });
});
