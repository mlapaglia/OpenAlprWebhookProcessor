import { Component, type OnChanges, ChangeDetectionStrategy, input, output } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';

import { RefreshButtonComponent } from 'app/shared/refresh-button/refresh-button.component';
import { type TrainingStatus } from '../machine-learning.service';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

@Component({
  selector: 'app-training-status-card',
  templateUrl: './training-status-card.component.html',
  styleUrls: ['./training-status-card.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTableModule,
    RefreshButtonComponent
],
})
export class TrainingStatusCardComponent extends OnPushBaseComponent implements OnChanges {
  readonly trainingStatus = input<TrainingStatus | null>(null);
  readonly isLoadingTrainingStatus = input<boolean>();
  readonly isTriggering = input<boolean>();

  readonly triggerTraining = output<void>();
  readonly refreshData = output<void>();

  public statusData = new MatTableDataSource<{ key: string; value: string }>();
  public displayedColumns: string[] = ['key', 'value'];

  ngOnChanges(): void {
    this.updateStatusTable();
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  private updateStatusTable(): void {
    const status = this.trainingStatus();
    if (!status) return;

    const data = [
      { key: 'Training Status', value: status.isTraining ? 'Training...' : 'Idle' },
      { key: 'Last Training Started', value: status.lastTrainingStarted ? new Date(status.lastTrainingStarted).toLocaleString() : 'Never' },
      { key: 'Last Training Completed', value: status.lastTrainingCompleted ? new Date(status.lastTrainingCompleted).toLocaleString() : 'Never' },
      { key: 'Last Training Result', value: this.getTrainingResult() },
      { key: 'Training Data Count', value: status.trainingDataCount.toLocaleString() },
      { key: 'Model File Last Saved', value: status.modelFile?.lastSaved ? new Date(status.modelFile.lastSaved).toLocaleString() : 'Not saved' },
      { key: 'Model File Size', value: status.modelFile?.fileSizeBytes ? this.formatFileSize(status.modelFile.fileSizeBytes) : 'N/A' },
    ];

    this.statusData = new MatTableDataSource(data);
    this.markForCheck();
  }

  private getTrainingResult(): string {
    const status = this.trainingStatus();
    if (!status) return 'Unknown';

    if (status.isTraining) return 'In Progress';
    if (status.lastError) return `Failed: ${status.lastError}`;
    if (status.lastTrainingSuccessful) return 'Success';

    return 'Not completed';
  }

  private formatFileSize(bytes: number): string {
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    if (bytes === 0) return '0 Bytes';
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return `${Math.round(bytes / Math.pow(1024, i) * 100) / 100} ${sizes[i]}`;
  }

  public getResultColor(value: string): string {
    if (value.includes('Success')) return 'var(--mat-app-primary)';
    if (value.includes('Failed')) return 'var(--mat-app-error)';
    if (value.includes('In Progress')) return 'var(--mat-app-tertiary)';
    return 'var(--mat-app-on-surface-variant)';
  }

  public onTriggerTraining(): void {
    this.triggerTraining.emit();
  }

  public onRefreshData(): void {
    this.refreshData.emit();
  }
}
