import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ConfigurationFieldComponent } from './configuration-field.component';

describe('ConfigurationFieldComponent', () => {
  let component: ConfigurationFieldComponent;
  let fixture: ComponentFixture<ConfigurationFieldComponent>;
  let mockForm: FormGroup;

  beforeEach(async () => {
    mockForm = new FormGroup({
      minimumModelQuality: new FormControl(0.5, [Validators.required, Validators.min(0.001), Validators.max(1.0)]),
      minimumTrainingData: new FormControl(100, [Validators.required, Validators.min(10), Validators.max(1000000)]),
      trainingBatchSize: new FormControl(5000, [Validators.required, Validators.min(1000), Validators.max(1000000)]),
      trainingInterval: new FormControl('06:00:00', [Validators.required]),
      modelFileName: new FormControl('model.json', [Validators.required]),
      configFolderName: new FormControl('config', [Validators.required]),
      mlModelsFolderName: new FormControl('ml-models', [Validators.required]),
    });

    await TestBed.configureTestingModule({
      imports: [ConfigurationFieldComponent, ReactiveFormsModule, NoopAnimationsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(ConfigurationFieldComponent);
    component = fixture.componentInstance;
  });

  describe('component initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with required inputs', () => {
      fixture.componentRef.setInput('fieldKey', 'Minimum Model Quality (R²)');
      fixture.componentRef.setInput('fieldValue', '0.500');
      fixture.componentRef.setInput('isEditing', true);
      fixture.componentRef.setInput('configForm', mockForm);

      expect(component.fieldKey()).toBe('Minimum Model Quality (R²)');
      expect(component.fieldValue()).toBe('0.500');
      expect(component.isEditing()).toBe(true);
      expect(component.configForm()).toBe(mockForm);
    });
  });

  describe('fieldConfig getter', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('configForm', mockForm);
    });

    it('should return correct config for Minimum Model Quality', () => {
      fixture.componentRef.setInput('fieldKey', 'Minimum Model Quality (R²)');

      const config = component.fieldConfig;

      expect(config.controlName).toBe('minimumModelQuality');
      expect(config.inputType).toBe('number');
      expect(config.step).toBe('0.001');
      expect(config.min).toBe('0.001');
      expect(config.max).toBe('1.0');
    });

    it('should return correct config for Minimum Training Data', () => {
      fixture.componentRef.setInput('fieldKey', 'Minimum Training Data');

      const config = component.fieldConfig;

      expect(config.controlName).toBe('minimumTrainingData');
      expect(config.inputType).toBe('number');
      expect(config.min).toBe('10');
      expect(config.max).toBe('1000000');
    });

    it('should return correct config for Training Batch Size', () => {
      fixture.componentRef.setInput('fieldKey', 'Training Batch Size');

      const config = component.fieldConfig;

      expect(config.controlName).toBe('trainingBatchSize');
      expect(config.inputType).toBe('number');
      expect(config.min).toBe('1000');
      expect(config.max).toBe('1000000');
    });

    it('should return correct config for Training Interval', () => {
      fixture.componentRef.setInput('fieldKey', 'Training Interval');

      const config = component.fieldConfig;

      expect(config.controlName).toBe('trainingInterval');
      expect(config.inputType).toBe('text');
      expect(config.placeholder).toBe('06:00:00');
    });

    it('should return correct config for text fields', () => {
      fixture.componentRef.setInput('fieldKey', 'Model File Name');

      const config = component.fieldConfig;

      expect(config.controlName).toBe('modelFileName');
      expect(config.inputType).toBe('text');
    });
  });

  describe('formControl getter', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('configForm', mockForm);
    });

    it('should return correct form control', () => {
      fixture.componentRef.setInput('fieldKey', 'Minimum Model Quality (R²)');

      const control = component.formControl;

      expect(control).toBe(mockForm.get('minimumModelQuality'));
    });

    it('should return null for invalid field key', () => {
      fixture.componentRef.setInput('fieldKey', 'Invalid Field');

      const control = component.formControl;

      expect(control).toBeNull();
    });
  });

  describe('hasError getter', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('fieldKey', 'Minimum Model Quality (R²)');
      fixture.componentRef.setInput('configForm', mockForm);
    });

    it('should return false when control has no errors', () => {
      const control = mockForm.get('minimumModelQuality');
      control?.setValue(0.5);
      control?.markAsUntouched();

      expect(component.hasError).toBe(false);
    });

    it('should return true when control has errors and is touched', () => {
      const control = mockForm.get('minimumModelQuality');
      control?.setValue(null);
      control?.markAsTouched();

      expect(component.hasError).toBe(true);
    });

    it('should return false when control has errors but is not touched', () => {
      const control = mockForm.get('minimumModelQuality');
      control?.setValue(null);
      control?.markAsUntouched();

      expect(component.hasError).toBe(false);
    });
  });

  describe('errorMessage getter', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('fieldKey', 'Minimum Model Quality (R²)');
      fixture.componentRef.setInput('configForm', mockForm);
    });

    it('should return required error message', () => {
      const control = mockForm.get('minimumModelQuality');
      control?.setValue(null);
      control?.markAsTouched();

      expect(component.errorMessage).toBe('minimumModelQuality is required');
    });

    it('should return min error message', () => {
      const control = mockForm.get('minimumModelQuality');
      control?.setValue(0.0005);
      control?.markAsTouched();

      expect(component.errorMessage).toBe('minimumModelQuality must be at least 0.001');
    });

    it('should return max error message', () => {
      const control = mockForm.get('minimumModelQuality');
      control?.setValue(1.5);
      control?.markAsTouched();

      expect(component.errorMessage).toBe('minimumModelQuality must be at most 1');
    });

    it('should return empty string when no errors', () => {
      const control = mockForm.get('minimumModelQuality');
      control?.setValue(0.5);
      control?.markAsTouched();

      expect(component.errorMessage).toBe('');
    });

    it('should return empty string when control is not touched', () => {
      const control = mockForm.get('minimumModelQuality');
      control?.setValue(null);
      control?.markAsUntouched();

      expect(component.errorMessage).toBe('');
    });

    it('should handle maxlength error', () => {
      // Create a control with maxlength validation
      const testForm = new FormGroup({
        testField: new FormControl('', [Validators.maxLength(5)]),
      });

      fixture.componentRef.setInput('fieldKey', 'Test Field');
      fixture.componentRef.setInput('configForm', testForm);

      // Mock the field mapping for the test
      (component as any).fieldMapping['Test Field'] = {
        controlName: 'testField',
        inputType: 'text',
      };

      const control = testForm.get('testField');
      control?.setValue('toolongvalue');
      control?.markAsTouched();

      expect(component.errorMessage).toBe('testField must be at most 5 characters');
    });

    it('should fallback to field key when controlName not available', () => {
      fixture.componentRef.setInput('fieldKey', 'Unknown Field');

      // Mock a form with an unknown field that has errors
      const testForm = new FormGroup({
        unknownField: new FormControl(null, [Validators.required]),
      });

      // Mock the field mapping and form control
      (component as any).fieldMapping['Unknown Field'] = undefined;
      spyOnProperty(component, 'formControl', 'get').and.returnValue(testForm.get('unknownField'));
      fixture.componentRef.setInput('configForm', testForm);

      const control = testForm.get('unknownField');
      control?.markAsTouched();

      expect(component.errorMessage).toBe('Unknown Field is required');
    });
  });
});
