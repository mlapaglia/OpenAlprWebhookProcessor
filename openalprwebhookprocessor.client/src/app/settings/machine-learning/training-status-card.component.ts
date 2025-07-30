import { Component, Input, Output, EventEmitter, type OnChanges, type SimpleChanges } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { CommonModule } from '@angular/common';
import { type TrainingStatus } from './machine-learning.service';

@Component({
  selector: 'app-training-status-card',
  templateUrl: './training-status-card.component.html',
  styleUrls: ['./training-status-card.component.less'],
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTableModule,
  ],
})
export class TrainingStatusCardComponent implements OnChanges {
  @Input() trainingStatus: TrainingStatus | null = null;
  @Input() isLoadingTrainingStatus = false;
  @Input() isTriggering = false;

  @Output() triggerTraining = new EventEmitter<void>();
  @Output() refreshData = new EventEmitter<void>();

  public statusData = new MatTableDataSource<{ key: string; value: string }>();
  public displayedColumns: string[] = ['key', 'value'];

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['trainingStatus'] && this.trainingStatus) {
      this.updateStatusTable();
    }
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

  public onTriggerTraining(): void {
    this.triggerTraining.emit();
  }

  public onRefreshData(): void {
    this.refreshData.emit();
  }
}
