import { Component, Input, Output, EventEmitter, inject, type OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators, type FormGroup } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { MatCardModule } from '@angular/material/card';
import type { IPlateSetting } from './plate-setting.interface';
import type { PlateSettingsConfig } from './plate-settings-table.component';

@Component({
  selector: 'app-add-new-setting-form',
  templateUrl: './add-new-setting-form.component.html',
  imports: [
    CommonModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
    ReactiveFormsModule,
    MatSelectModule,
    MatOptionModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatCardModule,
  ],
})
export class AddNewSettingFormComponent<T extends IPlateSetting> implements OnInit {
  @Input() config!: PlateSettingsConfig<T>;
  @Input() existingSettings: T[] = [];
  @Input() isAddingSetting = false;
  @Output() addSetting = new EventEmitter<T>();
  @Output() resetForm = new EventEmitter<void>();

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
    if (!this.config) {
      throw new Error('AddNewSettingFormComponent requires a config input');
    }
  }

  public onAddSetting(): void {
    if (!this.validateForm()) {
      return;
    }

    const newSetting = this.createNewSettingFromForm();
    this.addSetting.emit(newSetting);
  }

  public onResetForm(): void {
    this.settingForm.reset({
      plateNumber: '',
      strictMatch: true,
      description: '',
    });
    this.resetForm.emit();
  }

  private validateForm(): boolean {
    if (this.settingForm.invalid || this.isAddingSetting) {
      return false;
    }

    const plateNumber = this.settingForm.get('plateNumber')?.value?.trim();
    if (this.isDuplicatePlate(plateNumber)) {
      this.settingForm.get('plateNumber')?.setErrors({ duplicate: true });
      return false;
    }

    return true;
  }

  private createNewSettingFromForm(): T {
    const newSetting = this.config.createNew();
    Object.assign(newSetting, {
      plateNumber: this.settingForm.get('plateNumber')?.value?.trim(),
      strictMatch: this.settingForm.get('strictMatch')?.value,
      description: this.settingForm.get('description')?.value?.trim() || null,
    });
    return newSetting;
  }

  private isDuplicatePlate(plateNumber: string): boolean {
    if (!plateNumber) return false;
    return this.existingSettings.some(
      setting => setting.plateNumber.toLowerCase() === plateNumber.toLowerCase(),
    );
  }

  // Form validation getters to avoid method calls in template
  public get plateNumberRequired(): boolean {
    return this.settingForm.get('plateNumber')?.hasError('required') ?? false;
  }

  public get plateNumberMinLength(): boolean {
    return this.settingForm.get('plateNumber')?.hasError('minlength') ?? false;
  }

  public get plateNumberDuplicate(): boolean {
    return this.settingForm.get('plateNumber')?.hasError('duplicate') ?? false;
  }

  public get descriptionLength(): number {
    return this.settingForm.get('description')?.value?.length || 0;
  }
}
