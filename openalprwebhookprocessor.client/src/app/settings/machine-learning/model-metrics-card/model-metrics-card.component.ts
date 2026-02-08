import { Component, type OnChanges, ChangeDetectionStrategy, input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';

import { type TrainingStatus } from '../machine-learning.service';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

@Component({
  selector: 'app-model-metrics-card',
  templateUrl: './model-metrics-card.component.html',
  styleUrls: ['./model-metrics-card.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MatCardModule,
    MatTableModule
],
})
export class ModelMetricsCardComponent extends OnPushBaseComponent implements OnChanges {
  readonly trainingStatus = input<TrainingStatus | null>();

  public metricsData = new MatTableDataSource<{ key: string; value: string }>();
  public displayedColumns: string[] = ['key', 'value'];

  ngOnChanges(): void {
    this.updateMetricsTable();
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  private updateMetricsTable(): void {
    const metrics = this.trainingStatus()?.modelMetrics;

    if (!metrics) {
      this.metricsData = new MatTableDataSource<{ key: string; value: string }>([]);
      this.markForCheck();
      return;
    }

    const data = [
      { key: 'R-Squared (R²)', value: metrics.rSquared.toFixed(4) },
      { key: 'Mean Absolute Error', value: `${metrics.meanAbsoluteError.toFixed(2)} hours` },
      { key: 'Root Mean Squared Error', value: `${metrics.rootMeanSquaredError.toFixed(2)} hours` },
    ];

    this.metricsData = new MatTableDataSource(data);
    this.markForCheck();
  }
}












