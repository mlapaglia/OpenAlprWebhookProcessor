import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, type FormControl } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

export interface FieldConfig {
  key: string;
  type: 'number' | 'text';
  placeholder?: string;
  min?: number;
  max?: number;
  step?: number;
}

@Component({
  selector: 'app-ml-config-field',
  template: `
    <mat-form-field appearance="outline" i18n-appearance style="width: 100%;" subscriptSizing="dynamic" i18n-subscriptSizing>
      <input matInput
        [type]="fieldConfig.type"
        [formControl]="control"
        [placeholder]="fieldConfig.placeholder || ''"
        [min]="fieldConfig.min"
        [max]="fieldConfig.max"
        [step]="fieldConfig.step">
      @if (errorMessage) {
        <mat-error>{{ errorMessage }}</mat-error>
      }
    </mat-form-field>
  `,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MlConfigFieldComponent {
  @Input() fieldConfig!: FieldConfig;
  @Input() control!: FormControl;

  public get errorMessage(): string {
    if (this.control.errors && this.control.touched) {
      if (this.control.errors['required']) return `${this.fieldConfig.key} is required`;
      if (this.control.errors['min']) return `${this.fieldConfig.key} must be at least ${this.control.errors['min'].min}`;
      if (this.control.errors['max']) return `${this.fieldConfig.key} must be at most ${this.control.errors['max'].max}`;
      if (this.control.errors['maxlength']) return `${this.fieldConfig.key} must be at most ${this.control.errors['maxlength'].requiredLength} characters`;
    }
    return '';
  }
}
