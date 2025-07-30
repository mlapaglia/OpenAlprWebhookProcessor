import type { OnInit, OnDestroy } from '@angular/core';
import { Component, inject } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDividerModule } from '@angular/material/divider';
import { CommonModule } from '@angular/common';
import type { FormGroup } from '@angular/forms';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import type { ModelInfo, TrainingStatus, MachineLearningConfigDto } from './machine-learning.service';
import { MachineLearningService } from './machine-learning.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import type { Subscription } from 'rxjs';
import { interval } from 'rxjs';
import { switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-machine-learning',
  templateUrl: './machine-learning.component.html',
  styleUrls: ['./machine-learning.component.less'],
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
    MatDividerModule,
  ],
})
export class MachineLearningComponent implements OnInit, OnDestroy {
  private readonly mlService = inject(MachineLearningService);
  private readonly snackbarService = inject(SnackbarService);
  private readonly fb = inject(FormBuilder);

  public modelInfo: ModelInfo | null = null;
  public trainingStatus: TrainingStatus | null = null;
  public configuration: MachineLearningConfigDto | null = null;
  public configForm: FormGroup;

  public isLoadingModelInfo = false;
  public isLoadingTrainingStatus = false;
  public isLoadingConfiguration = false;
  public isTriggering = false;
  public isSavingConfiguration = false;
  public isEditingConfiguration = false;

  public statusData = new MatTableDataSource<{ key: string; value: string }>();
  public metricsData = new MatTableDataSource<{ key: string; value: string }>();
  public configData = new MatTableDataSource<{ key: string; value: string }>();
  public featuresData = new MatTableDataSource<{ feature: string }>();

  public displayedColumns: string[] = ['key', 'value'];
  public featuresColumns: string[] = ['feature'];

  private refreshSubscription?: Subscription;

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

  ngOnInit(): void {
    this.loadData();

    // Auto-refresh every 30 seconds
    this.refreshSubscription = interval(30000).pipe(
      switchMap(() => this.mlService.getTrainingStatus()),
    ).subscribe((status) => {
      this.trainingStatus = status;
      this.updateStatusTable();
    });
  }

  ngOnDestroy(): void {
    this.refreshSubscription?.unsubscribe();
  }

  private loadData(): void {
    this.loadModelInfo();
    this.loadTrainingStatus();
    this.loadConfiguration();
  }

  private loadModelInfo(): void {
    this.isLoadingModelInfo = true;
    this.mlService.getModelInfo().subscribe({
      next: (info) => {
        this.modelInfo = info;
        this.updateFeaturesTable();
        this.isLoadingModelInfo = false;
      },
      error: (_error) => {
        this.snackbarService.create('Failed to load model information', SnackBarType.Error);
        this.isLoadingModelInfo = false;
      },
    });
  }

  private loadTrainingStatus(): void {
    this.isLoadingTrainingStatus = true;
    this.mlService.getTrainingStatus().subscribe({
      next: (status) => {
        this.trainingStatus = status;
        this.updateStatusTable();
        this.updateMetricsTable();
        this.isLoadingTrainingStatus = false;
      },
      error: (_error) => {
        this.snackbarService.create('Failed to load training status', SnackBarType.Error);
        this.isLoadingTrainingStatus = false;
      },
    });
  }

  private loadConfiguration(): void {
    this.isLoadingConfiguration = true;
    this.mlService.getConfiguration().subscribe({
      next: (config) => {
        this.configuration = config;
        this.updateConfigForm(config);
        this.isLoadingConfiguration = false;
      },
      error: (_error) => {
        this.snackbarService.create('Failed to load configuration', SnackBarType.Error);
        this.isLoadingConfiguration = false;
      },
    });
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

  private updateStatusTable(): void {
    if (!this.trainingStatus) return;

    const data = [
      { key: 'Training Status', value: this.trainingStatus.isTraining ? 'Training...' : 'Idle' },
      { key: 'Last Training Started', value: this.trainingStatus.lastTrainingStarted ? new Date(this.trainingStatus.lastTrainingStarted).toLocaleString() : 'Never' },
      { key: 'Last Training Completed', value: this.trainingStatus.lastTrainingCompleted ? new Date(this.trainingStatus.lastTrainingCompleted).toLocaleString() : 'Never' },
      { key: 'Last Training Result', value: this.getTrainingResult() },
      { key: 'Training Data Count', value: this.trainingStatus.trainingDataCount.toLocaleString() },
      { key: 'Model File Last Saved', value: this.trainingStatus.modelFile?.lastSaved ? new Date(this.trainingStatus.modelFile.lastSaved).toLocaleString() : 'Not saved' },
      { key: 'Model File Size', value: this.trainingStatus.modelFile?.fileSizeBytes ? this.formatFileSize(this.trainingStatus.modelFile.fileSizeBytes) : 'N/A' },
    ];

    this.statusData = new MatTableDataSource(data);
  }

  private updateMetricsTable(): void {
    if (!this.trainingStatus?.modelMetrics) {
      this.metricsData = new MatTableDataSource<{ key: string; value: string }>([]);
      return;
    }

    const data = [
      { key: 'R-Squared (R²)', value: this.trainingStatus.modelMetrics.rSquared.toFixed(4) },
      { key: 'Mean Absolute Error', value: `${this.trainingStatus.modelMetrics.meanAbsoluteError.toFixed(2)} hours` },
      { key: 'Root Mean Squared Error', value: `${this.trainingStatus.modelMetrics.rootMeanSquaredError.toFixed(2)} hours` },
    ];

    this.metricsData = new MatTableDataSource(data);
  }



  private updateFeaturesTable(): void {
    if (!this.modelInfo?.features) return;

    const data = this.modelInfo.features.map(feature => ({ feature }));
    this.featuresData = new MatTableDataSource(data);
  }

  private getTrainingResult(): string {
    if (!this.trainingStatus) return 'Unknown';

    if (this.trainingStatus.isTraining) return 'In Progress';
    if (this.trainingStatus.lastError) return `Failed: ${this.trainingStatus.lastError}`;
    if (this.trainingStatus.lastTrainingSuccessful) return 'Success';

    return 'Not completed';
  }

  private formatFileSize(bytes: number): string {
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    if (bytes === 0) return '0 Bytes';
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return `${Math.round(bytes / Math.pow(1024, i) * 100) / 100} ${sizes[i]}`;
  }

  public getResultColor(value: string): string {
    if (value.includes('Success')) return '#32de84';
    if (value.includes('Failed')) return '#D2122E';
    if (value.includes('In Progress')) return '#ff9800';
    return '#666666';
  }

  public triggerTraining(): void {
    if (this.isTriggering) return;

    this.isTriggering = true;
    this.mlService.triggerTraining().subscribe({
      next: (_response) => {
        this.snackbarService.create('Training triggered successfully', SnackBarType.Successful);
        this.isTriggering = false;
        setTimeout(() => this.loadTrainingStatus(), 2000); // Refresh after 2 seconds
      },
      error: (_error) => {
        this.snackbarService.create('Failed to trigger training', SnackBarType.Error);
        this.isTriggering = false;
      },
    });
  }

  public refreshData(): void {
    this.loadData();
  }

  public editConfiguration(): void {
    this.isEditingConfiguration = true;
  }

  public cancelConfigurationEdit(): void {
    this.isEditingConfiguration = false;
    if (this.configuration) {
      this.updateConfigForm(this.configuration);
    }
  }

  public saveConfiguration(): void {
    if (this.configForm.invalid || this.isSavingConfiguration) return;

    this.isSavingConfiguration = true;
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

    this.mlService.saveConfiguration(configDto).subscribe({
      next: (_response) => {
        this.snackbarService.create('Configuration saved successfully', SnackBarType.Successful);
        this.isSavingConfiguration = false;
        this.isEditingConfiguration = false;
        this.loadConfiguration(); // Reload to get updated timestamps
      },
      error: (_error) => {
        this.snackbarService.create('Failed to save configuration', SnackBarType.Error);
        this.isSavingConfiguration = false;
      },
    });
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
    // Convert from TimeSpan format to hours for display
    const parts = interval.split(':');
    if (parts.length >= 2) {
      const hours = parseInt(parts[0]);
      const minutes = parseInt(parts[1]);
      return `${hours}h ${minutes}m`;
    }
    return interval;
  }
}
