import { Component, inject, type OnInit, type OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';

import { interval } from 'rxjs';
import { MachineLearningService, type ModelInfo, type TrainingStatus, type MachineLearningConfigDto } from '../machine-learning.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { ModelMetricsCardComponent } from '../model-metrics-card/model-metrics-card.component';
import { ConfigurationCardComponent } from '../configuration-card/configuration-card.component';
import { TrainingStatusCardComponent } from '../training-status-card/training-status-card.component';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

@Component({
  selector: 'app-machine-learning',
  templateUrl: './machine-learning.component.html',
  styleUrls: ['./machine-learning.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTableModule,
    ModelMetricsCardComponent,
    ConfigurationCardComponent,
    TrainingStatusCardComponent
],
})
export class MachineLearningComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
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

  ngOnInit(): void {
    this.loadData();
    this.setupAutoRefresh();
  }

  private setupAutoRefresh(): void {
    // Auto-refresh training status every 30 seconds
    this.subscribeAndMarkForCheck(
      interval(30000),
      () => {
        this.loadTrainingStatus();
      },
    );
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  private loadData(): void {
    this.loadModelInfo();
    this.loadTrainingStatus();
    this.loadConfiguration();
  }

  private loadModelInfo(): void {
    this.isLoadingModelInfo = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.mlService.getModelInfo(),
      (info) => {
        this.modelInfo = info;
        this.updateFeaturesTable();
        this.isLoadingModelInfo = false;
      },
      _ => {
        this.snackbarService.create('Failed to load model information', SnackBarType.Error);
        this.isLoadingModelInfo = false;
      },
    );
  }

  private loadTrainingStatus(): void {
    this.isLoadingTrainingStatus = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.mlService.getTrainingStatus(),
      (status) => {
        this.trainingStatus = status;
        this.isLoadingTrainingStatus = false;
      },
      _ => {
        this.snackbarService.create('Failed to load training status', SnackBarType.Error);
        this.isLoadingTrainingStatus = false;
      },
    );
  }

  private loadConfiguration(): void {
    this.isLoadingConfiguration = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.mlService.getConfiguration(),
      (config) => {
        this.configuration = config;
        this.isLoadingConfiguration = false;
      },
      _ => {
        this.snackbarService.create('Failed to load configuration', SnackBarType.Error);
        this.isLoadingConfiguration = false;
      },
    );
  }

  private updateFeaturesTable(): void {
    if (!this.modelInfo?.features) return;

    const data = this.modelInfo.features.map(feature => ({ feature }));
    this.featuresData = new MatTableDataSource(data);
    this.markForCheck();
  }

  public triggerTraining(): void {
    if (this.isTriggering) return;

    this.isTriggering = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.mlService.triggerTraining(),
      () => {
        this.snackbarService.create('Training triggered successfully', SnackBarType.Successful);
        this.isTriggering = false;
        setTimeout(() => this.loadTrainingStatus(), 2000); // Refresh after 2 seconds
      },
      _ => {
        this.snackbarService.create('Failed to trigger training', SnackBarType.Error);
        this.isTriggering = false;
      },
    );
  }

  public refreshData(): void {
    this.loadData();
  }

  public editConfiguration(): void {
    this.isEditingConfiguration = true;
    this.markForCheck();
  }

  public cancelConfigurationEdit(): void {
    this.isEditingConfiguration = false;
    this.markForCheck();
  }

  public saveConfiguration(configDto: MachineLearningConfigDto): void {
    if (this.isSavingConfiguration) return;

    this.isSavingConfiguration = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.mlService.saveConfiguration(configDto),
      () => {
        this.snackbarService.create('Configuration saved successfully', SnackBarType.Successful);
        this.isSavingConfiguration = false;
        this.isEditingConfiguration = false;
        this.loadConfiguration();
      },
      _ => {
        this.snackbarService.create('Failed to save configuration', SnackBarType.Error);
        this.isSavingConfiguration = false;
      },
    );
  }
}
