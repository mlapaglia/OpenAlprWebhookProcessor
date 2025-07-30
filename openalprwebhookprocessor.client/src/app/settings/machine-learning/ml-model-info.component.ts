import { Component, Input, ChangeDetectionStrategy, type OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import type { ModelInfo } from './machine-learning.service';

interface FeatureData {
  feature: string;
}

@Component({
  selector: 'app-ml-model-info',
  template: `
    @if (modelInfo) {
      <mat-card>
        <mat-card-header>
          <mat-card-title i18n>Model Information</mat-card-title>
          <mat-card-subtitle>{{ modelInfo.description }}</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <div style="display: flex; flex-direction: row; gap: 16px; flex-wrap: wrap;">
            <div style="flex: 1; min-width: 300px;">
              <p><strong i18n>Model Type:</strong> {{ modelInfo.modelType }}</p>
              <p><strong i18n>Status:</strong>
                <span [style.color]="statusColor">
                  {{ statusText }}
                </span>
              </p>
              <p><strong i18n>Training Schedule:</strong> {{ modelInfo.trainingSchedule }}</p>
              <p><strong i18n>Last Updated:</strong> {{ modelInfo.lastUpdated }}</p>
            </div>
            <div style="flex: 1; min-width: 300px;">
              <p><strong i18n>Features Used ({{ featuresCount }}):</strong></p>
              <div style="max-height: 200px; overflow-y: auto;">
                <table mat-table [dataSource]="featuresData" class="mat-elevation-z1">
                  <ng-container matColumnDef="feature" i18n-matColumnDef>
                    <th i18n mat-header-cell *matHeaderCellDef>Feature</th>
                    <td mat-cell *matCellDef="let element">{{ element.feature }}</td>
                  </ng-container>
                  <tr mat-header-row *matHeaderRowDef="featuresColumns"></tr>
                  <tr mat-row *matRowDef="let row; columns: featuresColumns;"></tr>
                </table>
              </div>
            </div>
          </div>
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
export class MlModelInfoComponent implements OnChanges {
  @Input() modelInfo: ModelInfo | null = null;

  public featuresData = new MatTableDataSource<FeatureData>([]);
  public featuresColumns: string[] = ['feature'];

  public get statusColor(): string {
    return this.modelInfo?.modelAvailable ? '#32de84' : '#D2122E';
  }

  public get statusText(): string {
    return this.modelInfo?.modelAvailable ? 'Available' : 'Not Available';
  }

  public get featuresCount(): number {
    return this.featuresData.data.length;
  }

  ngOnChanges(): void {
    this.updateFeaturesTable();
  }

  private updateFeaturesTable(): void {
    if (!this.modelInfo?.features) {
      this.featuresData = new MatTableDataSource<FeatureData>([]);
      return;
    }

    const data = this.modelInfo.features.map(feature => ({ feature }));
    this.featuresData = new MatTableDataSource(data);
  }
}
