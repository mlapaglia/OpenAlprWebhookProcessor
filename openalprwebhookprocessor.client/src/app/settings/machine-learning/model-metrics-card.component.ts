import { Component, Input, type OnChanges, type SimpleChanges } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { CommonModule } from '@angular/common';
import { type TrainingStatus } from './machine-learning.service';

@Component({
  selector: 'app-model-metrics-card',
  templateUrl: './model-metrics-card.component.html',
  styleUrls: ['./model-metrics-card.component.less'],
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
  ],
})
export class ModelMetricsCardComponent implements OnChanges {
  @Input() trainingStatus: TrainingStatus | null = null;

  public metricsData = new MatTableDataSource<{ key: string; value: string }>();
  public displayedColumns: string[] = ['key', 'value'];

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['trainingStatus'] && this.trainingStatus) {
      this.updateMetricsTable();
    }
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
}
