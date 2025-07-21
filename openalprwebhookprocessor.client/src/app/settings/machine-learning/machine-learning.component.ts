import { Component, OnInit, inject, OnDestroy } from '@angular/core'
import { MatCardModule } from '@angular/material/card'
import { MatButtonModule } from '@angular/material/button'
import { MatIconModule } from '@angular/material/icon'
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner'
import { MatTableModule, MatTableDataSource } from '@angular/material/table'
import { CommonModule, DatePipe } from '@angular/common'
import { MachineLearningService, ModelInfo, TrainingStatus } from './machine-learning.service'
import { SnackbarService } from 'app/snackbar/snackbar.service'
import { SnackBarType } from 'app/snackbar/snackbartype'
import { interval, Subscription } from 'rxjs'
import { switchMap } from 'rxjs/operators'

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
    DatePipe
  ],
})
export class MachineLearningComponent implements OnInit, OnDestroy {
  private mlService = inject(MachineLearningService)
  private snackbarService = inject(SnackbarService)

  public modelInfo: ModelInfo | null = null
  public trainingStatus: TrainingStatus | null = null
  public isLoadingModelInfo = false
  public isLoadingTrainingStatus = false
  public isTriggering = false

  public statusData: MatTableDataSource<{ key: string; value: string }> = new MatTableDataSource()
  public metricsData: MatTableDataSource<{ key: string; value: string }> = new MatTableDataSource()
  public configData: MatTableDataSource<{ key: string; value: string }> = new MatTableDataSource()
  public featuresData: MatTableDataSource<{ feature: string }> = new MatTableDataSource()

  public displayedColumns: string[] = ['key', 'value']
  public featuresColumns: string[] = ['feature']

  private refreshSubscription?: Subscription

  ngOnInit(): void {
    this.loadData()

    // Auto-refresh every 30 seconds
    this.refreshSubscription = interval(30000).pipe(
      switchMap(() => this.mlService.getTrainingStatus())
    ).subscribe((status) => {
      this.trainingStatus = status
      this.updateStatusTable()
    })
  }

  ngOnDestroy(): void {
    this.refreshSubscription?.unsubscribe()
  }

  private loadData(): void {
    this.loadModelInfo()
    this.loadTrainingStatus()
  }

  private loadModelInfo(): void {
    this.isLoadingModelInfo = true
    this.mlService.getModelInfo().subscribe(
      (info) => {
        this.modelInfo = info
        this.updateFeaturesTable()
        this.isLoadingModelInfo = false
      },
      (_error) => {
        this.snackbarService.create('Failed to load model information', SnackBarType.Error)
        this.isLoadingModelInfo = false
      }
    )
  }

  private loadTrainingStatus(): void {
    this.isLoadingTrainingStatus = true
    this.mlService.getTrainingStatus().subscribe(
      (status) => {
        this.trainingStatus = status
        this.updateStatusTable()
        this.updateMetricsTable()
        this.updateConfigTable()
        this.isLoadingTrainingStatus = false
      },
      (_error) => {
        this.snackbarService.create('Failed to load training status', SnackBarType.Error)
        this.isLoadingTrainingStatus = false
      }
    )
  }

  private updateStatusTable(): void {
    if (!this.trainingStatus) return

    const data = [
      { key: 'Training Status', value: this.trainingStatus.isTraining ? 'Training...' : 'Idle' },
      { key: 'Last Training Started', value: this.trainingStatus.lastTrainingStarted ? new Date(this.trainingStatus.lastTrainingStarted).toLocaleString() : 'Never' },
      { key: 'Last Training Completed', value: this.trainingStatus.lastTrainingCompleted ? new Date(this.trainingStatus.lastTrainingCompleted).toLocaleString() : 'Never' },
      { key: 'Last Training Result', value: this.getTrainingResult() },
      { key: 'Training Data Count', value: this.trainingStatus.trainingDataCount.toLocaleString() },
      { key: 'Model File Last Saved', value: this.trainingStatus.modelFile?.lastSaved ? new Date(this.trainingStatus.modelFile.lastSaved).toLocaleString() : 'Not saved' },
      { key: 'Model File Size', value: this.trainingStatus.modelFile?.fileSizeBytes ? this.formatFileSize(this.trainingStatus.modelFile.fileSizeBytes) : 'N/A' }
    ]

    this.statusData = new MatTableDataSource(data)
  }

  private updateMetricsTable(): void {
    if (!this.trainingStatus?.modelMetrics) return

    const data = [
      { key: 'R-Squared (R²)', value: this.trainingStatus.modelMetrics.rSquared.toFixed(4) },
      { key: 'Mean Absolute Error', value: `${this.trainingStatus.modelMetrics.meanAbsoluteError.toFixed(2)} hours` },
      { key: 'Root Mean Squared Error', value: `${this.trainingStatus.modelMetrics.rootMeanSquaredError.toFixed(2)} hours` }
    ]

    this.metricsData = new MatTableDataSource(data)
  }

  private updateConfigTable(): void {
    if (!this.trainingStatus?.configuration) return

    const data = [
      { key: 'Training Interval', value: this.trainingStatus.configuration.trainingInterval },
      { key: 'Minimum Training Data', value: this.trainingStatus.configuration.minimumTrainingData.toLocaleString() },
      { key: 'Minimum Model Quality (R²)', value: this.trainingStatus.configuration.minimumModelQuality.toString() },
      { key: 'Training Batch Size', value: this.trainingStatus.configuration.batchSize.toLocaleString() }
    ]

    this.configData = new MatTableDataSource(data)
  }

  private updateFeaturesTable(): void {
    if (!this.modelInfo?.features) return

    const data = this.modelInfo.features.map(feature => ({ feature }))
    this.featuresData = new MatTableDataSource(data)
  }

  private getTrainingResult(): string {
    if (!this.trainingStatus) return 'Unknown'

    if (this.trainingStatus.isTraining) return 'In Progress'
    if (this.trainingStatus.lastError) return `Failed: ${this.trainingStatus.lastError}`
    if (this.trainingStatus.lastTrainingSuccessful) return 'Success'

    return 'Not completed'
  }

  private formatFileSize(bytes: number): string {
    const sizes = ['Bytes', 'KB', 'MB', 'GB']
    if (bytes === 0) return '0 Bytes'
    const i = Math.floor(Math.log(bytes) / Math.log(1024))
    return Math.round(bytes / Math.pow(1024, i) * 100) / 100 + ' ' + sizes[i]
  }

  public getResultColor(value: string): string {
    if (value.includes('Success')) return '#32de84'
    if (value.includes('Failed')) return '#D2122E'
    if (value.includes('In Progress')) return '#ff9800'
    return '#666666'
  }

  public triggerTraining(): void {
    if (this.isTriggering) return

    this.isTriggering = true
    this.mlService.triggerTraining().subscribe(
      (_response) => {
        this.snackbarService.create('Training triggered successfully', SnackBarType.Successful)
        this.isTriggering = false
        setTimeout(() => this.loadTrainingStatus(), 2000) // Refresh after 2 seconds
      },
      (_error) => {
        this.snackbarService.create('Failed to trigger training', SnackBarType.Error)
        this.isTriggering = false
      }
    )
  }

  public refreshData(): void {
    this.loadData()
  }
}
