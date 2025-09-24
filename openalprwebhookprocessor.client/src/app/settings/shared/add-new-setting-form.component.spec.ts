import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { AddNewSettingFormComponent } from './add-new-setting-form.component';
import type { PlateSettingsConfig } from './plate-settings-table.component';
import type { IPlateSetting } from './plate-setting.interface';

// Test implementation of IPlateSetting
class TestPlateSetting implements IPlateSetting {
  id: string;
  plateNumber: string;
  strictMatch: boolean;
  description: string;

  constructor(init: Partial<IPlateSetting> = {}) {
    this.id = init.id ?? '';
    this.plateNumber = init.plateNumber ?? '';
    this.strictMatch = init.strictMatch ?? true;
    this.description = init.description ?? '';
  }
}

describe('AddNewSettingFormComponent', () => {
  let component: AddNewSettingFormComponent<TestPlateSetting>;
  let fixture: ComponentFixture<AddNewSettingFormComponent<TestPlateSetting>>;
  let mockConfig: PlateSettingsConfig<TestPlateSetting>;

  beforeEach(async () => {
    mockConfig = {
      title: 'Test Settings',
      subtitle: 'Test subtitle',
      emptyStateTitle: 'No settings',
      emptyStateDescription: 'Add some settings',
      addButtonText: 'Add Setting',
      entityName: 'setting',
      createNew: () => new TestPlateSetting(),
      service: {
        getAll: jasmine.createSpy('getAll'),
        upsert: jasmine.createSpy('upsert'),
      },
    };

    await TestBed.configureTestingModule({
      imports: [
        AddNewSettingFormComponent,
        ReactiveFormsModule,
      ],
      providers: [FormBuilder],
    }).compileComponents();

    fixture = TestBed.createComponent(AddNewSettingFormComponent<TestPlateSetting>);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('config', mockConfig);
  });

  afterEach(() => {
    fixture.destroy();
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize form with default values', () => {
      expect(component.settingForm).toBeDefined();
      expect(component.settingForm.get('plateNumber')?.value).toBe('');
      expect(component.settingForm.get('strictMatch')?.value).toBe(true);
      expect(component.settingForm.get('description')?.value).toBe('');
    });

    it('should have required validators on plateNumber', () => {
      const plateNumberControl = component.settingForm.get('plateNumber');

      plateNumberControl?.setValue('');
      expect(plateNumberControl?.hasError('required')).toBe(true);

      plateNumberControl?.setValue('A');
      expect(plateNumberControl?.hasError('minlength')).toBe(true);

      plateNumberControl?.setValue('AB');
      expect(plateNumberControl?.hasError('minlength')).toBe(false);
    });

    it('should have required validator on strictMatch', () => {
      const strictMatchControl = component.settingForm.get('strictMatch');

      strictMatchControl?.setValue(null);
      expect(strictMatchControl?.hasError('required')).toBe(true);

      strictMatchControl?.setValue(false);
      expect(strictMatchControl?.hasError('required')).toBe(false);
    });

    it('should not require description', () => {
      const descriptionControl = component.settingForm.get('description');

      descriptionControl?.setValue('');
      expect(descriptionControl?.hasError('required')).toBe(false);
    });
  });

  describe('Form Validation', () => {
    it('should validate required plate number', () => {
      component.settingForm.patchValue({
        plateNumber: '',
        strictMatch: true,
        description: 'Test',
      });

      const result = component['validateForm']();

      expect(result).toBe(false);
      expect(component.settingForm.invalid).toBe(true);
    });

    it('should validate minimum plate number length', () => {
      component.settingForm.patchValue({
        plateNumber: 'A',
        strictMatch: true,
        description: 'Test',
      });

      const result = component['validateForm']();

      expect(result).toBe(false);
      expect(component.settingForm.get('plateNumber')?.hasError('minlength')).toBe(true);
    });

    it('should prevent submission while adding', () => {
      component.settingForm.patchValue({
        plateNumber: 'ABC123',
        strictMatch: true,
        description: 'Test',
      });
      fixture.componentRef.setInput('isAddingSetting', true);

      const result = component['validateForm']();

      expect(result).toBe(false);
    });

    it('should detect duplicate plates', () => {
      fixture.componentRef.setInput('existingSettings', [
        new TestPlateSetting({ plateNumber: 'ABC123' }),
        new TestPlateSetting({ plateNumber: 'XYZ789' }),
      ]);

      component.settingForm.patchValue({
        plateNumber: 'abc123', // Case insensitive
        strictMatch: true,
        description: 'Test',
      });

      const result = component['validateForm']();

      expect(result).toBe(false);
      expect(component.settingForm.get('plateNumber')?.hasError('duplicate')).toBe(true);
    });

    it('should pass validation with valid data', () => {
      fixture.componentRef.setInput('existingSettings', [new TestPlateSetting({ plateNumber: 'EXISTING123' })]);

      component.settingForm.patchValue({
        plateNumber: 'NEW123',
        strictMatch: true,
        description: 'Valid description',
      });
      fixture.componentRef.setInput('isAddingSetting', false);

      const result = component['validateForm']();

      expect(result).toBe(true);
      expect(component.settingForm.valid).toBe(true);
    });
  });

  describe('Adding Settings', () => {
    it('should emit new setting on valid form submission', () => {
      spyOn(component.addSetting, 'emit');
      component.settingForm.patchValue({
        plateNumber: 'ABC123',
        strictMatch: false,
        description: 'Test description',
      });

      component.onAddSetting();

      expect(component.addSetting.emit).toHaveBeenCalledWith(
        jasmine.objectContaining({
          plateNumber: 'ABC123',
          strictMatch: false,
          description: 'Test description',
        }),
      );
    });

    it('should not emit on invalid form', () => {
      spyOn(component.addSetting, 'emit');
      component.settingForm.patchValue({
        plateNumber: '', // Invalid - required
        strictMatch: true,
        description: 'Test',
      });

      component.onAddSetting();

      expect(component.addSetting.emit).not.toHaveBeenCalled();
    });

    it('should trim plate number and description', () => {
      spyOn(component.addSetting, 'emit');
      component.settingForm.patchValue({
        plateNumber: '  ABC123  ',
        strictMatch: true,
        description: '  Test description  ',
      });

      component.onAddSetting();

      expect(component.addSetting.emit).toHaveBeenCalledWith(
        jasmine.objectContaining({
          plateNumber: 'ABC123',
          description: 'Test description',
        }),
      );
    });

    it('should handle null description', () => {
      spyOn(component.addSetting, 'emit');
      component.settingForm.patchValue({
        plateNumber: 'ABC123',
        strictMatch: true,
        description: null,
      });

      component.onAddSetting();

      expect(component.addSetting.emit).toHaveBeenCalledWith(
        jasmine.objectContaining({
          plateNumber: 'ABC123',
          description: null,
        }),
      );
    });
  });

  describe('Form Reset', () => {
    it('should reset form to default values', () => {
      // Set some values
      component.settingForm.patchValue({
        plateNumber: 'ABC123',
        strictMatch: false,
        description: 'Some description',
      });

      spyOn(component.resetForm, 'emit');

      component.onResetForm();

      expect(component.settingForm.get('plateNumber')?.value).toBe('');
      expect(component.settingForm.get('strictMatch')?.value).toBe(true);
      expect(component.settingForm.get('description')?.value).toBe('');
      expect(component.resetForm.emit).toHaveBeenCalled();
    });

    it('should clear validation errors on reset', () => {
      // Create validation errors
      component.settingForm.get('plateNumber')?.setErrors({ required: true, duplicate: true });

      component.onResetForm();

      // After reset, the form should be pristine but may still have required validation
      // since it's empty. Check that duplicate error is cleared.
      const plateNumberControl = component.settingForm.get('plateNumber');
      expect(plateNumberControl?.hasError('duplicate')).toBe(false);
    });
  });

  describe('Duplicate Detection', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('existingSettings', [
        new TestPlateSetting({ plateNumber: 'ABC123' }),
        new TestPlateSetting({ plateNumber: 'XYZ789' }),
        new TestPlateSetting({ plateNumber: 'test123' }),
      ]);
    });

    it('should detect exact matches', () => {
      expect(component['isDuplicatePlate']('ABC123')).toBe(true);
      expect(component['isDuplicatePlate']('XYZ789')).toBe(true);
    });

    it('should detect case-insensitive matches', () => {
      expect(component['isDuplicatePlate']('abc123')).toBe(true);
      expect(component['isDuplicatePlate']('xyz789')).toBe(true);
      expect(component['isDuplicatePlate']('TEST123')).toBe(true);
    });

    it('should return false for non-duplicates', () => {
      expect(component['isDuplicatePlate']('NEW123')).toBe(false);
      expect(component['isDuplicatePlate']('UNIQUE789')).toBe(false);
    });

    it('should handle empty/null plate numbers', () => {
      expect(component['isDuplicatePlate']('')).toBe(false);
      expect(component['isDuplicatePlate'](null as any)).toBe(false);
      expect(component['isDuplicatePlate'](undefined as any)).toBe(false);
    });
  });

  describe('Form Validation Getters', () => {
    it('should return correct plateNumberRequired state', () => {
      component.settingForm.get('plateNumber')?.setErrors({ required: true });
      expect(component.plateNumberRequired).toBe(true);

      component.settingForm.get('plateNumber')?.setErrors(null);
      expect(component.plateNumberRequired).toBe(false);
    });

    it('should return correct plateNumberMinLength state', () => {
      component.settingForm.get('plateNumber')?.setErrors({ minlength: true });
      expect(component.plateNumberMinLength).toBe(true);

      component.settingForm.get('plateNumber')?.setErrors(null);
      expect(component.plateNumberMinLength).toBe(false);
    });

    it('should return correct plateNumberDuplicate state', () => {
      component.settingForm.get('plateNumber')?.setErrors({ duplicate: true });
      expect(component.plateNumberDuplicate).toBe(true);

      component.settingForm.get('plateNumber')?.setErrors(null);
      expect(component.plateNumberDuplicate).toBe(false);
    });

    it('should return correct description length', () => {
      component.settingForm.get('description')?.setValue('Test description');
      expect(component.descriptionLength).toBe(16);

      component.settingForm.get('description')?.setValue('');
      expect(component.descriptionLength).toBe(0);

      component.settingForm.get('description')?.setValue(null);
      expect(component.descriptionLength).toBe(0);
    });
  });

  describe('Setting Creation', () => {
    it('should create new setting using config factory', () => {
      component.settingForm.patchValue({
        plateNumber: 'ABC123',
        strictMatch: false,
        description: 'Test setting',
      });

      const result = component['createNewSettingFromForm']();

      expect(result).toBeInstanceOf(TestPlateSetting);
      expect(result.plateNumber).toBe('ABC123');
      expect(result.strictMatch).toBe(false);
      expect(result.description).toBe('Test setting');
    });

    it('should handle form control null values', () => {
      // Set up form with potential null values
      component.settingForm.patchValue({
        plateNumber: null,
        strictMatch: null,
        description: null,
      });

      const result = component['createNewSettingFromForm']();

      expect(result).toBeInstanceOf(TestPlateSetting);
      // When form control value is null, null?.trim() returns undefined
      expect(result.plateNumber).toBeUndefined();
      expect(result.strictMatch).toBeNull();
      expect(result.description).toBeNull(); // null?.trim() ?? null = null
    });
  });

  describe('Integration Tests', () => {
    it('should handle complete add workflow', () => {
      spyOn(component.addSetting, 'emit');
      fixture.componentRef.setInput('existingSettings', [new TestPlateSetting({ plateNumber: 'EXISTING123' })]);

      // Fill form
      component.settingForm.patchValue({
        plateNumber: 'NEW123',
        strictMatch: true,
        description: 'New test setting',
      });

      // Submit
      component.onAddSetting();

      // Verify emission
      expect(component.addSetting.emit).toHaveBeenCalledWith(
        jasmine.objectContaining({
          plateNumber: 'NEW123',
          strictMatch: true,
          description: 'New test setting',
        }),
      );
    });

    it('should prevent duplicate submission workflow', () => {
      spyOn(component.addSetting, 'emit');
      fixture.componentRef.setInput('existingSettings', [new TestPlateSetting({ plateNumber: 'EXISTING123' })]);

      // Fill form with duplicate
      component.settingForm.patchValue({
        plateNumber: 'existing123', // Case insensitive duplicate
        strictMatch: true,
        description: 'Duplicate attempt',
      });

      // Submit
      component.onAddSetting();

      // Verify no emission and error state
      expect(component.addSetting.emit).not.toHaveBeenCalled();
      expect(component.settingForm.get('plateNumber')?.hasError('duplicate')).toBe(true);
    });

    it('should handle reset workflow', () => {
      spyOn(component.resetForm, 'emit');

      // Fill form
      component.settingForm.patchValue({
        plateNumber: 'ABC123',
        strictMatch: false,
        description: 'Will be reset',
      });

      // Add some errors
      component.settingForm.get('plateNumber')?.setErrors({ duplicate: true });

      // Reset
      component.onResetForm();

      // Verify clean state
      expect(component.settingForm.get('plateNumber')?.value).toBe('');
      expect(component.settingForm.get('strictMatch')?.value).toBe(true);
      expect(component.settingForm.get('description')?.value).toBe('');
      // After reset with empty value, required validation may be present
      expect(component.settingForm.get('plateNumber')?.hasError('duplicate')).toBe(false);
      expect(component.resetForm.emit).toHaveBeenCalled();
    });
  });
});
