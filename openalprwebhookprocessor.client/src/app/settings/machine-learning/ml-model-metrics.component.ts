import type { OnChanges } from '@angular/core';
import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import type { TrainingStatus } from './machine-learning.service';

interface MetricsData {
  key: string;
  value: string;
}

@Component({
  selector: 'app-ml-model-metrics',
  template: `
    @if (trainingStatus?.modelMetrics) {
      <mat-card>
        <mat-card-header>
          <mat-card-title i18n>Model Performance Metrics</mat-card-title>
          <mat-card-subtitle i18n>Quality metrics from the last successful training</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <table mat-table [dataSource]="metricsData" class="mat-elevation-z2" style="width: 100%;">
            <ng-container matColumnDef="key" i18n-matColumnDef>
              <th i18n mat-header-cell *matHeaderCellDef style="min-width: 200px;">Metric</th>
              <td mat-cell *matCellDef="let element"><strong>{{ element.key }}</strong></td>
            </ng-container>
            <ng-container matColumnDef="value" i18n-matColumnDef>
              <th i18n mat-header-cell *matHeaderCellDef>Value</th>
              <td mat-cell *matCellDef="let element">{{ element.value }}</td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
          </table>
        </mat-card-content>
      </mat-card>
    }
  `,
  styleUrls: ['./machine-learning.component.less'],
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MlModelMetricsComponent implements OnChanges {
  @Input() trainingStatus: TrainingStatus | null = null;

  public metricsData = new MatTableDataSource<MetricsData>([]);
  public displayedColumns: string[] = ['key', 'value'];

  ngOnChanges(): void {
    this.updateMetricsTable();
  }

  private updateMetricsTable(): void {
    if (!this.trainingStatus?.modelMetrics) {
      this.metricsData = new MatTableDataSource<MetricsData>([]);
      return;
    }

    const data: MetricsData[] = [
      { key: 'R-Squared (R²)', value: this.trainingStatus.modelMetrics.rSquared.toFixed(4) },
      { key: 'Mean Absolute Error', value: `${this.trainingStatus.modelMetrics.meanAbsoluteError.toFixed(2)} hours` },
      { key: 'Root Mean Squared Error', value: `${this.trainingStatus.modelMetrics.rootMeanSquaredError.toFixed(2)} hours` },
    ];

    this.metricsData = new MatTableDataSource(data);
  }
}
