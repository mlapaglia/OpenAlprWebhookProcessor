import { ChangeDetectionStrategy, Component, inject, type OnDestroy, type OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';
import { SettingsService } from '../../settings.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { type ScheduledJob, type ScheduledJobsResponse, ScheduledJobType, SunriseSunsetType } from './scheduled-job';

@Component({
  selector: 'app-scheduled-jobs',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './scheduled-jobs.component.html',
  styleUrls: ['./scheduled-jobs.component.less'],
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatTableModule,
    MatChipsModule,
  ],
})
export class ScheduledJobsComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  private readonly settingsService = inject(SettingsService);
  private readonly snackBarService = inject(SnackbarService);

  public scheduledJobsData: ScheduledJobsResponse | null = null;
  public isLoading = false;
  public displayedColumns: string[] = ['jobType', 'camera', 'scheduledTime', 'timeUntil', 'status'];

  // Computed properties
  public get totalJobs(): number {
    return this.scheduledJobsData?.scheduledJobs.length ?? 0;
  }

  public get overdueJobs(): number {
    return this.scheduledJobsData?.scheduledJobs.filter(job => this.isJobOverdue(job)).length ?? 0;
  }

  public get jobsInNext24Hours(): number {
    const now = new Date();
    const next24Hours = new Date(now.getTime() + 24 * 60 * 60 * 1000);
    return this.scheduledJobsData?.scheduledJobs.filter(job => {
      const executionTime = new Date(job.scheduledExecutionTime);
      return executionTime >= now && executionTime <= next24Hours;
    }).length ?? 0;
  }

  ngOnInit(): void {
    this.loadScheduledJobs();
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  public loadScheduledJobs(): void {
    this.isLoading = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.settingsService.getScheduledJobs(),
      (data) => {
        this.scheduledJobsData = data;
        this.isLoading = false;
      },
      _ => {
        this.snackBarService.create(
          'Failed to load scheduled jobs information',
          SnackBarType.Error,
        );
        this.isLoading = false;
      },
    );
  }

  public getJobTypeDisplay(jobType: ScheduledJobType): string {
    switch (jobType) {
      case ScheduledJobType.ClearOverlay:
        return 'Clear Overlay';
      case ScheduledJobType.SunriseSunset:
        return 'Sunrise/Sunset';
      default:
        return 'Unknown';
    }
  }

  public getJobTypeIcon(jobType: ScheduledJobType): string {
    switch (jobType) {
      case ScheduledJobType.ClearOverlay:
        return 'clear';
      case ScheduledJobType.SunriseSunset:
        return 'wb_sunny';
      default:
        return 'help_outline';
    }
  }

  public getSunriseSunsetDisplay(sunriseSunsetType?: SunriseSunsetType): string {
    if (sunriseSunsetType === undefined) return '';

    switch (sunriseSunsetType) {
      case SunriseSunsetType.Sunrise:
        return 'Sunrise';
      case SunriseSunsetType.Sunset:
        return 'Sunset';
      default:
        return 'Unknown';
    }
  }

  public formatDateTime(dateTimeString: string): string {
    try {
      const date = new Date(dateTimeString);
      return date.toLocaleString();
    } catch {
      return dateTimeString;
    }
  }

  public formatTimeUntil(job: ScheduledJob): string {
    try {
      const now = new Date();
      const executionTime = new Date(job.scheduledExecutionTime);

      // Check if the date is valid
      if (isNaN(executionTime.getTime())) {
        return 'Unknown';
      }

      const diffMs = executionTime.getTime() - now.getTime();

      const absDiffMs = Math.abs(diffMs);
      const hours = Math.floor(absDiffMs / (1000 * 60 * 60));
      const minutes = Math.floor((absDiffMs % (1000 * 60 * 60)) / (1000 * 60));

      if (diffMs < 0) {
        return `${hours}h ${minutes}m ago`;
      } else {
        return `${hours}h ${minutes}m`;
      }
    } catch {
      return 'Unknown';
    }
  }

  public isJobOverdue(job: ScheduledJob): boolean {
    try {
      const now = new Date();
      const executionTime = new Date(job.scheduledExecutionTime);
      return executionTime < now;
    } catch {
      return false;
    }
  }

  public getStatusChipClass(job: ScheduledJob): string {
    if (this.isJobOverdue(job)) {
      return 'overdue-chip';
    }
    return 'scheduled-chip';
  }

  public getStatusDisplay(job: ScheduledJob): string {
    if (this.isJobOverdue(job)) {
      return 'Overdue';
    }
    return 'Scheduled';
  }

  public getCameraDisplay(job: ScheduledJob): string {
    if (job.cameraId) {
      return `Camera ${job.cameraId.substring(0, 8)}...`;
    }
    return 'N/A';
  }

  public onRefreshClick(event: Event): void {
    event.preventDefault();
    this.loadScheduledJobs();
  }
}
