import { Component, Input, Output, EventEmitter, type OnChanges, type SimpleChanges, inject } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { CommonModule } from '@angular/common';
import { FormBuilder, Validators, ReactiveFormsModule, type FormGroup } from '@angular/forms';
import { type MachineLearningConfigDto } from './machine-learning.service';
import { ConfigurationFieldComponent } from './configuration-field.component';

@Component({
  selector: 'app-configuration-card',
  templateUrl: './configuration-card.component.html',
  styleUrls: ['./configuration-card.component.less'],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    ConfigurationFieldComponent,
  ],
})
export class ConfigurationCardComponent implements OnChanges {
  private readonly fb = inject(FormBuilder);

  @Input() configuration: MachineLearningConfigDto | null = null;
  @Input() isLoadingConfiguration = false;
  @Input() isEditingConfiguration = false;
  @Input() isSavingConfiguration = false;

  @Output() saveConfiguration = new EventEmitter<MachineLearningConfigDto>();
  @Output() cancelConfigurationEdit = new EventEmitter<void>();
  @Output() editConfiguration = new EventEmitter<void>();

  public configForm: FormGroup;
  public configData = new MatTableDataSource<{ key: string; value: string }>();
  public displayedColumns: string[] = ['key', 'value'];

  constructor() {
    this.configForm = this.fb.group({
      minimumModelQuality: [0.05, [Validators.required, Validators.min(0.001), Validators.max(1.0)]],
      minimumTrainingData: [100, [Validators.required, Validators.min(10), Validators.max(1000000)]],
      trainingBatchSize: [50000, [Validators.required, Validators.min(1000), Validators.max(1000000)]],
      trainingInterval: ['06:00:00', [Validators.required]],
      modelFileName: ['license-plate-prediction-model.zip', [Validators.required, Validators.maxLength(255)]],
      configFolderName: ['config', [Validators.required, Validators.maxLength(255)]],
      mlModelsFolderName: ['ml-models', [Validators.required, Validators.maxLength(255)]],
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['configuration'] && this.configuration) {
      this.updateConfigForm(this.configuration);
    }
  }

  private updateConfigForm(config: MachineLearningConfigDto): void {
    this.configForm.patchValue({
      minimumModelQuality: config.minimumModelQuality,
      minimumTrainingData: config.minimumTrainingData,
      trainingBatchSize: config.trainingBatchSize,
      trainingInterval: config.trainingInterval,
      modelFileName: config.modelFileName,
      configFolderName: config.configFolderName,
      mlModelsFolderName: config.mlModelsFolderName,
    });
    this.updateConfigTable();
  }

  private updateConfigTable(): void {
    if (!this.configuration) return;

    const data = [
      { key: 'Minimum Model Quality (R²)', value: this.configuration.minimumModelQuality.toString() },
      { key: 'Minimum Training Data', value: this.configuration.minimumTrainingData.toLocaleString() },
      { key: 'Training Batch Size', value: this.configuration.trainingBatchSize.toLocaleString() },
      { key: 'Training Interval', value: this.formatTrainingInterval(this.configuration.trainingInterval) },
      { key: 'Model File Name', value: this.configuration.modelFileName },
      { key: 'Config Folder', value: this.configuration.configFolderName },
      { key: 'ML Models Folder', value: this.configuration.mlModelsFolderName },
    ];

    this.configData = new MatTableDataSource(data);
  }

  public onSaveConfiguration(): void {
    if (this.configForm.invalid || this.isSavingConfiguration) return;

    const formValue = this.configForm.value;
    const configDto: MachineLearningConfigDto = {
      minimumModelQuality: formValue.minimumModelQuality,
      minimumTrainingData: formValue.minimumTrainingData,
      trainingBatchSize: formValue.trainingBatchSize,
      trainingInterval: formValue.trainingInterval,
      modelFileName: formValue.modelFileName,
      configFolderName: formValue.configFolderName,
      mlModelsFolderName: formValue.mlModelsFolderName,
    };

    this.saveConfiguration.emit(configDto);
  }

  public onCancelConfigurationEdit(): void {
    this.cancelConfigurationEdit.emit();
  }

  public onEditConfiguration(): void {
    this.editConfiguration.emit();
  }

  public getFieldError(fieldName: string): string {
    const field = this.configForm.get(fieldName);
    if (field?.errors && field.touched) {
      if (field.errors['required']) return `${fieldName} is required`;
      if (field.errors['min']) return `${fieldName} must be at least ${field.errors['min'].min}`;
      if (field.errors['max']) return `${fieldName} must be at most ${field.errors['max'].max}`;
      if (field.errors['maxlength']) return `${fieldName} must be at most ${field.errors['maxlength'].requiredLength} characters`;
    }
    return '';
  }

  public formatTrainingInterval(interval: string): string {
    const parts = interval.split(':');
    if (parts.length >= 2) {
      const hours = parseInt(parts[0]);
      const minutes = parseInt(parts[1]);
      return `${hours}h ${minutes}m`;
    }
    return interval;
  }
}
