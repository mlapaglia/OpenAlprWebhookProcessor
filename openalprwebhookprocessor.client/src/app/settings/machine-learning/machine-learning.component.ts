import { Component, inject, type OnInit, type OnDestroy } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { CommonModule } from '@angular/common';
import { MachineLearningService, type ModelInfo, type TrainingStatus, type MachineLearningConfigDto } from './machine-learning.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { interval, type Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { ModelMetricsCardComponent } from './model-metrics-card.component';
import { ConfigurationCardComponent } from './configuration-card.component';
import { TrainingStatusCardComponent } from './training-status-card.component';

@Component({
  selector: 'app-machine-learning',
  templateUrl: './machine-learning.component.html',
  styleUrls: ['./machine-learning.component.less'],
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTableModule,
    ModelMetricsCardComponent,
    ConfigurationCardComponent,
    TrainingStatusCardComponent,
  ],
})
export class MachineLearningComponent implements OnInit, OnDestroy {
  private readonly mlService = inject(MachineLearningService);
  private readonly snackbarService = inject(SnackbarService);

  public modelInfo: ModelInfo | null = null;
  public trainingStatus: TrainingStatus | null = null;
  public configuration: MachineLearningConfigDto | null = null;

  public isLoadingModelInfo = false;
  public isLoadingTrainingStatus = false;
  public isLoadingConfiguration = false;
  public isTriggering = false;
  public isSavingConfiguration = false;
  public isEditingConfiguration = false;

  public featuresData = new MatTableDataSource<{ feature: string }>();

  public displayedColumns: string[] = ['key', 'value'];
  public featuresColumns: string[] = ['feature'];

  private refreshSubscription?: Subscription;

  ngOnInit(): void {
    this.loadData();

    // Auto-refresh every 30 seconds
    this.refreshSubscription = interval(30000).pipe(
      switchMap(() => this.mlService.getTrainingStatus()),
    ).subscribe((status) => {
      this.trainingStatus = status;
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
        this.isLoadingConfiguration = false;
      },
      error: (_error) => {
        this.snackbarService.create('Failed to load configuration', SnackBarType.Error);
        this.isLoadingConfiguration = false;
      },
    });
  }

  private updateFeaturesTable(): void {
    if (!this.modelInfo?.features) return;

    const data = this.modelInfo.features.map(feature => ({ feature }));
    this.featuresData = new MatTableDataSource(data);
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
  }

  public saveConfiguration(configDto: MachineLearningConfigDto): void {
    if (this.isSavingConfiguration) return;

    this.isSavingConfiguration = true;

    this.mlService.saveConfiguration(configDto).subscribe({
      next: (_response) => {
        this.snackbarService.create('Configuration saved successfully', SnackBarType.Successful);
        this.isSavingConfiguration = false;
        this.isEditingConfiguration = false;
        this.loadConfiguration();
      },
      error: (_error) => {
        this.snackbarService.create('Failed to save configuration', SnackBarType.Error);
        this.isSavingConfiguration = false;
      },
    });
  }
}
