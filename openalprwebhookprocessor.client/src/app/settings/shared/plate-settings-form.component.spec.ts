import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { provideNativeDateAdapter } from '@angular/material/core';

import { PlateSettingsFormComponent, type FormConfig } from './plate-settings-form.component';
import type { IPlateSetting } from './plate-setting.interface';

interface TestPlateSetting extends IPlateSetting {
  id: string;
  plateNumber: string;
  strictMatch: boolean;
  description: string;
}

describe('PlateSettingsFormComponent', () => {
  let component: PlateSettingsFormComponent<TestPlateSetting>;
  let fixture: ComponentFixture<PlateSettingsFormComponent<TestPlateSetting>>;

  const mockConfig: FormConfig = {
    entityName: 'alert',
    addButtonText: 'Add Alert',
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        PlateSettingsFormComponent,
        ReactiveFormsModule,
      ],
      providers: [provideNativeDateAdapter()],
    }).compileComponents();

    fixture = TestBed.createComponent(PlateSettingsFormComponent<TestPlateSetting>);
    component = fixture.componentInstance;

    fixture.componentRef.setInput('config', mockConfig);
    fixture.componentRef.setInput('isSubmitting', false);
    fixture.componentRef.setInput('existingPlateNumbers', []);

    fixture.detectChanges();
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize form with default values', () => {
      expect(component.settingForm).toBeTruthy();
      expect(component.settingForm.get('plateNumber')?.value).toBe('');
      expect(component.settingForm.get('strictMatch')?.value).toBe(true);
      expect(component.settingForm.get('description')?.value).toBe('');
    });

    it('should add duplicate validator on init', () => {
      component.ngOnInit();

      const { plateNumberControl } = component;
      expect(plateNumberControl?.hasValidator).toBeTruthy();
    });
  });

  describe('Form Validation', () => {
    it('should require plate number', () => {
      const { plateNumberControl } = component;
      plateNumberControl?.setValue('');
      plateNumberControl?.markAsTouched();

      expect(plateNumberControl?.hasError('required')).toBe(true);
      expect(component.hasPlateNumberRequiredError).toBe(true);
    });

    it('should require minimum length for plate number', () => {
      const { plateNumberControl } = component;
      plateNumberControl?.setValue('A');
      plateNumberControl?.markAsTouched();

      expect(plateNumberControl?.hasError('minlength')).toBe(true);
      expect(component.hasPlateNumberMinLengthError).toBe(true);
    });

    it('should accept valid plate number', () => {
      const { plateNumberControl } = component;
      plateNumberControl?.setValue('ABC123');

      expect(plateNumberControl?.hasError('required')).toBe(false);
      expect(plateNumberControl?.hasError('minlength')).toBe(false);
    });

    it('should detect duplicate plate numbers', () => {
      fixture.componentRef.setInput('existingPlateNumbers', ['ABC123', 'XYZ789']);
      fixture.detectChanges();
      component.ngOnInit();

      const { plateNumberControl } = component;
      plateNumberControl?.setValue('abc123'); // Case insensitive check

      expect(plateNumberControl?.hasError('duplicate')).toBe(true);
      expect(component.hasPlateNumberDuplicateError).toBe(true);
    });

    it('should not show duplicate error for new plate numbers', () => {
      fixture.componentRef.setInput('existingPlateNumbers', ['ABC123', 'XYZ789']);
      fixture.detectChanges();
      component.ngOnInit();

      const { plateNumberControl } = component;
      plateNumberControl?.setValue('NEW123');

      expect(plateNumberControl?.hasError('duplicate')).toBe(false);
      expect(component.hasPlateNumberDuplicateError).toBe(false);
    });

    it('should require strictMatch to be set', () => {
      const strictMatchControl = component.settingForm.get('strictMatch');
      strictMatchControl?.setValue(null);

      expect(strictMatchControl?.hasError('required')).toBe(true);
    });

    it('should allow empty description', () => {
      const { descriptionControl } = component;
      descriptionControl?.setValue('');

      expect(descriptionControl?.valid).toBe(true);
    });
  });

  describe('Computed Properties', () => {
    it('should return correct description length', () => {
      component.descriptionControl?.setValue('Test description');

      expect(component.descriptionLength).toBe(16);
    });

    it('should return 0 for empty description', () => {
      component.descriptionControl?.setValue('');

      expect(component.descriptionLength).toBe(0);
    });

    it('should return correct add button text when not submitting', () => {
      fixture.componentRef.setInput('isSubmitting', false);
      fixture.detectChanges();

      expect(component.addButtonText).toBe('Add Alert');
    });

    it('should return "Adding..." when submitting', () => {
      fixture.componentRef.setInput('isSubmitting', true);
      fixture.detectChanges();

      expect(component.addButtonText).toBe('Adding...');
    });
  });

  describe('Form Submission', () => {
    beforeEach(() => {
      component.settingForm.patchValue({
        plateNumber: 'ABC123',
        strictMatch: true,
        description: 'Test description',
      });
    });

    it('should emit addSetting event with form data when form is valid', () => {
      spyOn(component.addSetting, 'emit');
      fixture.componentRef.setInput('isSubmitting', false);
      fixture.detectChanges();

      component.onSubmit();

      expect(component.addSetting.emit).toHaveBeenCalledWith({
        plateNumber: 'ABC123',
        strictMatch: true,
        description: 'Test description',
      });
    });

    it('should trim whitespace from form values', () => {
      spyOn(component.addSetting, 'emit');
      component.settingForm.patchValue({
        plateNumber: '  ABC123  ',
        description: '  Test description  ',
      });

      component.onSubmit();

      expect(component.addSetting.emit).toHaveBeenCalledWith({
        plateNumber: 'ABC123',
        strictMatch: true,
        description: 'Test description',
      });
    });

    it('should not emit when form is invalid', () => {
      spyOn(component.addSetting, 'emit');
      component.settingForm.patchValue({
        plateNumber: '', // Invalid
      });

      component.onSubmit();

      expect(component.addSetting.emit).not.toHaveBeenCalled();
    });

    it('should not emit when submitting', () => {
      spyOn(component.addSetting, 'emit');
      fixture.componentRef.setInput('isSubmitting', true);
      fixture.detectChanges();

      component.onSubmit();

      expect(component.addSetting.emit).not.toHaveBeenCalled();
    });

    it('should handle empty description correctly', () => {
      spyOn(component.addSetting, 'emit');
      component.settingForm.patchValue({
        description: null,
      });

      component.onSubmit();

      expect(component.addSetting.emit).toHaveBeenCalledWith({
        plateNumber: 'ABC123',
        strictMatch: true,
        description: '',
      });
    });
  });

  describe('Form Reset', () => {
    beforeEach(() => {
      component.settingForm.patchValue({
        plateNumber: 'ABC123',
        strictMatch: false,
        description: 'Test description',
      });
      component.plateNumberControl?.setErrors({ duplicate: true });
    });

    it('should reset form to default values', () => {
      component.resetForm();

      expect(component.settingForm.get('plateNumber')?.value).toBe('');
      expect(component.settingForm.get('strictMatch')?.value).toBe(true);
      expect(component.settingForm.get('description')?.value).toBe('');
    });

    it('should clear plate number errors', () => {
      component.resetForm();

      expect(component.plateNumberControl?.errors).toBeNull();
    });
  });

  describe('Template Integration', () => {
    it('should display config entity name in title', () => {
      const titleElement = fixture.debugElement.query(By.css('mat-card-title'));

      expect(titleElement.nativeElement.textContent.trim()).toBe('Add New Alert');
    });

    it('should show correct subtitle based on entity name', () => {
      const subtitleElement = fixture.debugElement.query(By.css('mat-card-subtitle'));

      expect(subtitleElement.nativeElement.textContent).toContain('monitor specific license plates');
    });

    it('should show ignore subtitle for ignore rule entity', () => {
      const ignoreConfig: FormConfig = {
        entityName: 'ignore rule',
        addButtonText: 'Add Ignore Rule',
      };
      fixture.componentRef.setInput('config', ignoreConfig);
      fixture.detectChanges();

      const subtitleElement = fixture.debugElement.query(By.css('mat-card-subtitle'));

      expect(subtitleElement.nativeElement.textContent).toContain('Create a new rule to ignore specific license plates');
    });

    it('should display plate number required error', () => {
      component.plateNumberControl?.setValue('');
      component.plateNumberControl?.markAsTouched();
      fixture.detectChanges();

      const errorElement = fixture.debugElement.query(By.css('mat-error'));
      expect(errorElement.nativeElement.textContent.trim()).toBe('Plate number is required');
    });

    it('should display plate number min length error', () => {
      component.plateNumberControl?.setValue('A');
      component.plateNumberControl?.markAsTouched();
      fixture.detectChanges();

      const errorElement = fixture.debugElement.query(By.css('mat-error'));
      expect(errorElement.nativeElement.textContent.trim()).toBe('Plate number must be at least 2 characters');
    });

    it('should display duplicate error', () => {
      fixture.componentRef.setInput('existingPlateNumbers', ['ABC123']);
      fixture.detectChanges();
      component.ngOnInit();

      component.plateNumberControl?.setValue('ABC123');
      component.plateNumberControl?.markAsTouched();
      fixture.detectChanges();

      const errorElement = fixture.debugElement.query(By.css('mat-error'));
      expect(errorElement.nativeElement.textContent.trim()).toBe('This plate number already exists');
    });

    it('should display character count for description', () => {
      component.descriptionControl?.setValue('Test');
      fixture.detectChanges();

      const hintElement = fixture.debugElement.query(By.css('mat-hint'));
      expect(hintElement.nativeElement.textContent.trim()).toBe('4/200');
    });

    it('should disable submit button when form is invalid', () => {
      component.settingForm.patchValue({ plateNumber: '' });
      fixture.detectChanges();

      const submitButton = fixture.debugElement.query(By.css('button[type="submit"]'));
      expect(submitButton.nativeElement.disabled).toBe(true);
    });

    it('should disable submit button when submitting', () => {
      component.settingForm.patchValue({ plateNumber: 'ABC123' });
      fixture.componentRef.setInput('isSubmitting', true);
      fixture.detectChanges();

      const submitButton = fixture.debugElement.query(By.css('button[type="submit"]'));
      expect(submitButton.nativeElement.disabled).toBe(true);
    });

    it('should show spinner when submitting', () => {
      fixture.componentRef.setInput('isSubmitting', true);
      fixture.detectChanges();

      const spinner = fixture.debugElement.query(By.css('mat-spinner'));
      expect(spinner).toBeTruthy();
    });

    it('should show add icon when not submitting', () => {
      fixture.componentRef.setInput('isSubmitting', false);
      fixture.detectChanges();

      const addIcon = fixture.debugElement.query(By.css('button[type="submit"] mat-icon'));
      expect(addIcon.nativeElement.textContent.trim()).toBe('add');
    });

    it('should trigger form submission on submit button click', () => {
      spyOn(component, 'onSubmit');
      component.settingForm.patchValue({ plateNumber: 'ABC123' });
      fixture.detectChanges();

      const submitButton = fixture.debugElement.query(By.css('button[type="submit"]'));
      submitButton.nativeElement.click();

      expect(component.onSubmit).toHaveBeenCalled();
    });

    it('should trigger form reset on reset button click', () => {
      spyOn(component, 'resetForm');

      const resetButton = fixture.debugElement.query(By.css('button[type="button"]'));
      resetButton.nativeElement.click();

      expect(component.resetForm).toHaveBeenCalled();
    });

    it('should disable reset button when submitting', () => {
      fixture.componentRef.setInput('isSubmitting', true);
      fixture.detectChanges();

      const resetButton = fixture.debugElement.query(By.css('button[type="button"]'));
      expect(resetButton.nativeElement.disabled).toBe(true);
    });
  });

  describe('Accessibility', () => {
    it('should have proper form labels', () => {
      const plateLabel = fixture.debugElement.query(By.css('mat-label'));
      expect(plateLabel.nativeElement.textContent.trim()).toBe('License Plate Number');
    });

    it('should have proper input placeholders', () => {
      const plateInput = fixture.debugElement.query(By.css('input[formControlName="plateNumber"]'));
      expect(plateInput.nativeElement.placeholder).toBe('e.g., ABC123');
    });

    it('should have proper button labels', () => {
      const submitButton = fixture.debugElement.query(By.css('button[type="submit"]'));
      const resetButton = fixture.debugElement.query(By.css('button[type="button"]'));

      expect(submitButton.nativeElement.textContent.trim()).toContain('Add Alert');
      expect(resetButton.nativeElement.textContent.trim()).toBe('Reset');
    });
  });

  describe('Edge Cases', () => {
    it('should handle null config gracefully', () => {
      fixture.componentRef.setInput('config', null);
      fixture.detectChanges();

      expect(component.addButtonText).toBe('');
    });

    it('should handle undefined existing plate numbers', () => {
      fixture.componentRef.setInput('existingPlateNumbers', undefined as any);
      fixture.detectChanges();
      component.ngOnInit();

      const { plateNumberControl } = component;
      plateNumberControl?.setValue('ABC123');

      expect(plateNumberControl?.hasError('duplicate')).toBe(false);
    });

    it('should handle case-insensitive duplicate validation', () => {
      fixture.componentRef.setInput('existingPlateNumbers', ['ABC123']);
      fixture.detectChanges();
      component.ngOnInit();

      const { plateNumberControl } = component;
      plateNumberControl?.setValue('abc123');

      expect(plateNumberControl?.hasError('duplicate')).toBe(true);
    });

    it('should not validate duplicates for empty plate numbers', () => {
      fixture.componentRef.setInput('existingPlateNumbers', ['ABC123']);
      fixture.detectChanges();
      component.ngOnInit();

      const { plateNumberControl } = component;
      plateNumberControl?.setValue('');

      expect(plateNumberControl?.hasError('duplicate')).toBe(false);
    });
  });
});
