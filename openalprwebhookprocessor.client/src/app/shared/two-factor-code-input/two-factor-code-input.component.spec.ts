import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';

import { TwoFactorCodeInputComponent } from './two-factor-code-input.component';

describe('TwoFactorCodeInputComponent', () => {
  let component: TwoFactorCodeInputComponent;
  let fixture: ComponentFixture<TwoFactorCodeInputComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        TwoFactorCodeInputComponent,
        ReactiveFormsModule,
      ],
      providers: [FormBuilder],
    }).compileComponents();

    fixture = TestBed.createComponent(TwoFactorCodeInputComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  describe('component initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.loading()).toBe(false);
      expect(component.submitText()).toBe('Verify Code');
      expect(component.submitted).toBe(false);
    });

    it('should initialize form with correct validators', () => {
      expect(component.form).toBeDefined();
      expect(component.form.get('code')?.hasError('required')).toBe(true);

      // Set an invalid value to test pattern validation
      component.form.get('code')?.setValue('12345');
      expect(component.form.get('code')?.hasError('pattern')).toBe(true);
    });
  });

  describe('form controls getter', () => {
    it('should return form controls', () => {
      const controls = component.f;
      expect(controls.code).toBeDefined();
    });
  });

  describe('form validation and submission', () => {
    it('should not submit when form is invalid', () => {
      spyOn(component.codeSubmitted, 'emit');

      component.onSubmit();

      expect(component.submitted).toBe(true);
      expect(component.codeSubmitted.emit).not.toHaveBeenCalled();
    });

    it('should not submit with invalid code format', () => {
      spyOn(component.codeSubmitted, 'emit');
      component.form.patchValue({ code: '12345' }); // Too short

      component.onSubmit();

      expect(component.submitted).toBe(true);
      expect(component.codeSubmitted.emit).not.toHaveBeenCalled();
    });

    it('should submit when form is valid', () => {
      spyOn(component.codeSubmitted, 'emit');
      component.form.patchValue({ code: '123456' });

      component.onSubmit();

      expect(component.submitted).toBe(true);
      expect(component.codeSubmitted.emit).toHaveBeenCalledWith('123456');
    });
  });

  describe('input properties', () => {
    it('should accept custom submit text', () => {
      fixture.componentRef.setInput('submitText', 'Custom Submit Text');
      fixture.detectChanges();

      expect(component.submitText()).toBe('Custom Submit Text');
    });

    it('should accept loading state', () => {
      fixture.componentRef.setInput('loading', true);
      fixture.detectChanges();

      expect(component.loading()).toBe(true);
    });
  });
});
