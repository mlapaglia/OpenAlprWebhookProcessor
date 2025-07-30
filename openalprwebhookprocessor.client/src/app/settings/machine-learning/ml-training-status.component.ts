import type { OnChanges } from '@angular/core';
import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import type { TrainingStatus } from './machine-learning.service';

interface StatusData {
  key: string;
  value: string;
}

@Component({
  selector: 'app-ml-training-status',
  template: `
    <mat-card>
      <mat-card-header>
        <mat-card-title i18n>
          Training Status
          @if (isLoadingTrainingStatus) {
            <mat-progress-spinner style="margin-left: 16px;" color="primary" mode="indeterminate" i18n-mode diameter="20" />
          }
        </mat-card-title>
        <mat-card-subtitle i18n>Current training service status and history</mat-card-subtitle>
      </mat-card-header>
      @if (trainingStatus) {
        <mat-card-content>
          <table mat-table [dataSource]="statusData" class="mat-elevation-z2" style="width: 100%;">
            <ng-container matColumnDef="key" i18n-matColumnDef>
              <th i18n mat-header-cell *matHeaderCellDef style="min-width: 200px;">Property</th>
              <td mat-cell *matCellDef="let element"><strong>{{ element.key }}</strong></td>
            </ng-container>
            <ng-container matColumnDef="value" i18n-matColumnDef>
              <th i18n mat-header-cell *matHeaderCellDef>Value</th>
              <td mat-cell *matCellDef="let element">
                @if (element.key === 'Training Status') {
                  <span [style.color]="trainingStatusColor">
                    {{ element.value }}
                  </span>
                }
                @else if (element.key === 'Last Training Result') {
                  <span [style.color]="getResultColor(element.value)">
                    {{ element.value }}
                  </span>
                }
                @else {
                  {{ element.value }}
                }
              </td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
          </table>
        </mat-card-content>
      }
      <mat-card-actions>
        <button mat-raised-button color="primary"
                (click)="triggerTraining.emit()"
                [disabled]="isTriggering || trainingStatus?.isTraining">
          @if (isTriggering) {
            <mat-progress-spinner color="primary" mode="indeterminate" i18n-mode diameter="20" style="margin-right: 8px;" />
          }
          {{ triggerButtonText }}
        </button>
        <button mat-button (click)="refreshData.emit()" [disabled]="isLoadingTrainingStatus">
          <mat-icon i18n>refresh</mat-icon>
          Refresh
        </button>
      </mat-card-actions>
    </mat-card>
  `,
  styleUrls: ['./machine-learning.component.less'],
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTableModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MlTrainingStatusComponent implements OnChanges {
  @Input() trainingStatus: TrainingStatus | null = null;
  @Input() isLoadingTrainingStatus = false;
  @Input() isTriggering = false;

  @Output() triggerTraining = new EventEmitter<void>();
  @Output() refreshData = new EventEmitter<void>();

  public statusData = new MatTableDataSource<StatusData>([]);
  public displayedColumns: string[] = ['key', 'value'];

  public get trainingStatusColor(): string {
    return this.trainingStatus?.isTraining ? '#ff9800' : '#32de84';
  }

  public get triggerButtonText(): string {
    return this.isTriggering ? 'Training...' : 'Trigger Training';
  }

  ngOnChanges(): void {
    this.updateStatusTable();
  }

  public getResultColor(value: string): string {
    if (value.includes('Success')) return '#32de84';
    if (value.includes('Failed')) return '#D2122E';
    if (value.includes('In Progress')) return '#ff9800';
    return '#666666';
  }

  private updateStatusTable(): void {
    if (!this.trainingStatus) {
      this.statusData = new MatTableDataSource<StatusData>([]);
      return;
    }

    const data: StatusData[] = [
      { key: 'Training Status', value: this.trainingStatus.isTraining ? 'Training...' : 'Idle' },
      { key: 'Last Training Started', value: this.formatDate(this.trainingStatus?.lastTrainingStarted) },
      { key: 'Last Training Completed', value: this.formatDate(this.trainingStatus?.lastTrainingCompleted) },
      { key: 'Last Training Result', value: this.getTrainingResult() },
      { key: 'Training Data Count', value: this.trainingStatus.trainingDataCount.toLocaleString() },
      { key: 'Model File Last Saved', value: this.formatModelFileDate() },
      { key: 'Model File Size', value: this.formatModelFileSize() },
    ];

    this.statusData = new MatTableDataSource(data);
  }

  private formatDate(dateString: string | null | undefined): string {
    return dateString ? new Date(dateString).toLocaleString() : 'Never';
  }

  private formatModelFileDate(): string {
    return this.trainingStatus?.modelFile?.lastSaved
      ? new Date(this.trainingStatus.modelFile.lastSaved).toLocaleString()
      : 'Not saved';
  }

  private formatModelFileSize(): string {
    const bytes = this.trainingStatus?.modelFile?.fileSizeBytes;
    if (!bytes) return 'N/A';

    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    if (bytes === 0) return '0 Bytes';
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return `${Math.round(bytes / Math.pow(1024, i) * 100) / 100} ${sizes[i]}`;
  }

  private getTrainingResult(): string {
    if (!this.trainingStatus) return 'Unknown';

    if (this.trainingStatus.isTraining) return 'In Progress';
    if (this.trainingStatus.lastError) return `Failed: ${this.trainingStatus.lastError}`;
    if (this.trainingStatus.lastTrainingSuccessful) return 'Success';

    return 'Not completed';
  }
}
