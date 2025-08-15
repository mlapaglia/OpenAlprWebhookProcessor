import { Component, inject, ChangeDetectionStrategy, type OnInit, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators, type FormGroup, type AbstractControl } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import type { IPlateSetting } from './plate-setting.interface';

export interface FormConfig {
  entityName: string;
  addButtonText: string;
}

@Component({
  selector: 'app-plate-settings-form',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-card class="add-setting-card">
      <mat-card-header>
        <mat-card-title i18n class="mat-headline-6">Add New {{config()?.entityName | titlecase}}</mat-card-title>
        <mat-card-subtitle i18n>Create a new rule to {{config()?.entityName === 'ignore rule' ? 'ignore' : 'monitor'}} specific license plates</mat-card-subtitle>
      </mat-card-header>

      <mat-card-content>
        <form [formGroup]="settingForm" (ngSubmit)="onSubmit()" class="setting-form">
          <div class="form-row">
            <mat-form-field appearance="outline" i18n-appearance class="plate-field">
              <mat-label i18n>License Plate Number</mat-label>
              <input
                matInput
                formControlName="plateNumber"
                placeholder="e.g., ABC123" i18n-placeholder
                maxlength="20"
                autocomplete="off">
              <mat-icon i18n matSuffix>local_police</mat-icon>
              @if (hasPlateNumberRequiredError) {
                <mat-error i18n>
                  Plate number is required
                </mat-error>
              }
              @if (hasPlateNumberMinLengthError) {
                <mat-error i18n>
                  Plate number must be at least 2 characters
                </mat-error>
              }
              @if (hasPlateNumberDuplicateError) {
                <mat-error i18n>
                  This plate number already exists
                </mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline" i18n-appearance class="match-type-field">
              <mat-label i18n>Match Type</mat-label>
              <mat-select formControlName="strictMatch">
                <mat-option [value]="true">
                  <div class="option-content">
                    <span i18n class="option-title">Strict Match</span>
                    <span i18n class="option-subtitle">Exact character match required</span>
                  </div>
                </mat-option>
                <mat-option [value]="false">
                  <div class="option-content">
                    <span i18n class="option-title">Lenient Match</span>
                    <span i18n class="option-subtitle">Allows for OCR variations</span>
                  </div>
                </mat-option>
              </mat-select>
              <mat-icon i18n matSuffix>tune</mat-icon>
            </mat-form-field>
          </div>

          <mat-form-field appearance="outline" i18n-appearance class="description-field">
            <mat-label i18n>Description (Optional)</mat-label>
            <textarea
              matInput
              formControlName="description"
              [placeholder]="'Add a note about this ' + (config()?.entityName || '') + '...'"
              rows="2"
              maxlength="200">
            </textarea>
            <mat-icon i18n matSuffix>notes</mat-icon>
            <mat-hint align="end" i18n-align>{{descriptionLength}}/200</mat-hint>
          </mat-form-field>

          <div class="form-actions">
            <button
              mat-raised-button
              color="primary"
              type="submit"
              [disabled]="settingForm.invalid || isSubmitting()">
              @if (!isSubmitting()) {
                <mat-icon i18n>add</mat-icon>
              }
              @if (isSubmitting()) {
                <mat-spinner diameter="20" />
              }
              {{ addButtonText }}
            </button>

            <button i18n
              mat-button
              type="button"
              (click)="resetForm()"
              [disabled]="isSubmitting()">
              Reset
            </button>
          </div>
        </form>
      </mat-card-content>
    </mat-card>
  `,
  styleUrls: ['./plate-settings-table.component.less'],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatOptionModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
})
export class PlateSettingsFormComponent<T extends IPlateSetting> implements OnInit {
  readonly config = input<FormConfig>();
  readonly isSubmitting = input<boolean>(false);
  readonly existingPlateNumbers = input<string[]>([]);
  readonly addSetting = output<Partial<T>>();

  private readonly formBuilder = inject(FormBuilder);

  public settingForm: FormGroup;

  constructor() {
    this.settingForm = this.formBuilder.group({
      plateNumber: ['', [Validators.required, Validators.minLength(2)]],
      strictMatch: [true, [Validators.required]],
      description: [''],
    });
  }

  ngOnInit(): void {
    this.settingForm.get('plateNumber')?.addValidators(this.duplicateValidator.bind(this));
  }

  public get plateNumberControl() {
    return this.settingForm.get('plateNumber');
  }

  public get hasPlateNumberRequiredError(): boolean {
    return this.plateNumberControl?.hasError('required') ?? false;
  }

  public get hasPlateNumberMinLengthError(): boolean {
    return this.plateNumberControl?.hasError('minlength') ?? false;
  }

  public get hasPlateNumberDuplicateError(): boolean {
    return this.plateNumberControl?.hasError('duplicate') ?? false;
  }

  public get descriptionControl() {
    return this.settingForm.get('description');
  }

  public get descriptionLength(): number {
    return this.descriptionControl?.value?.length ?? 0;
  }

  public get addButtonText(): string {
    return this.isSubmitting() ? 'Adding...' : this.config()?.addButtonText ?? '';
  }

  public onSubmit(): void {
    if (this.settingForm.valid && !this.isSubmitting()) {
      const formValue = this.settingForm.value;
      this.addSetting.emit({
        plateNumber: formValue.plateNumber?.trim(),
        strictMatch: formValue.strictMatch,
        description: formValue.description?.trim() ?? '',
      } as Partial<T>);
    }
  }

  public resetForm(): void {
    this.settingForm.reset({
      plateNumber: '',
      strictMatch: true,
      description: '',
    });
    this.settingForm.get('plateNumber')?.setErrors(null);
  }

  private duplicateValidator(control: AbstractControl) {
    if (!control.value) return null;

    const plateNumber = control.value.toLowerCase();
    const existingNumbers = (this.existingPlateNumbers() as string[] | undefined) ?? [];
    const isDuplicate = existingNumbers.some(
      existing => existing.toLowerCase() === plateNumber,
    );

    return isDuplicate ? { duplicate: true } : null;
  }
}
