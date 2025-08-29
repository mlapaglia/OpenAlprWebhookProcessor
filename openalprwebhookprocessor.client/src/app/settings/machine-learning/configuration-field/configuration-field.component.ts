import { Component, ChangeDetectionStrategy, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, type FormGroup } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-configuration-field',
  templateUrl: './configuration-field.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
  ],
})
export class ConfigurationFieldComponent {
  readonly fieldKey = input.required<string>();
  readonly fieldValue = input.required<string>();
  readonly isEditing = input.required<boolean>();
  readonly configForm = input.required<FormGroup>();

  // Map field keys to form control names and input properties
  private readonly fieldMapping: Record<string, {
    controlName: string;
    inputType: string;
    placeholder?: string;
    step?: string;
    min?: string;
    max?: string;
  }> = {
    'Minimum Model Quality (R²)': {
      controlName: 'minimumModelQuality',
      inputType: 'number',
      step: '0.001',
      min: '0.001',
      max: '1.0',
    },
    'Minimum Training Data': {
      controlName: 'minimumTrainingData',
      inputType: 'number',
      min: '10',
      max: '1000000',
    },
    'Training Batch Size': {
      controlName: 'trainingBatchSize',
      inputType: 'number',
      min: '1000',
      max: '1000000',
    },
    'Training Interval': {
      controlName: 'trainingInterval',
      inputType: 'text',
      placeholder: '06:00:00',
    },
    'Model File Name': {
      controlName: 'modelFileName',
      inputType: 'text',
    },
    'Config Folder': {
      controlName: 'configFolderName',
      inputType: 'text',
    },
    'ML Models Folder': {
      controlName: 'mlModelsFolderName',
      inputType: 'text',
    },
  };

  public get fieldConfig() {
    return this.fieldMapping[this.fieldKey()];
  }

  public get formControl() {
    const config = this.fieldConfig;
    // eslint-disable-next-line @typescript-eslint/no-unnecessary-condition
    return config ? this.configForm().get(config.controlName) : null;
  }

  public get hasError(): boolean {
    return !!(this.formControl?.errors && this.formControl.touched);
  }

  public get errorMessage(): string {
    const control = this.formControl;
    if (control?.errors && control.touched) {
      const { fieldConfig } = this;
      // eslint-disable-next-line @typescript-eslint/no-unnecessary-condition
      const fieldName = fieldConfig?.controlName ?? this.fieldKey();
      if (control.errors['required']) return `${fieldName} is required`;
      if (control.errors['min']) return `${fieldName} must be at least ${control.errors['min'].min}`;
      if (control.errors['max']) return `${fieldName} must be at most ${control.errors['max'].max}`;
      if (control.errors['maxlength']) return `${fieldName} must be at most ${control.errors['maxlength'].requiredLength} characters`;
    }
    return '';
  }
}





