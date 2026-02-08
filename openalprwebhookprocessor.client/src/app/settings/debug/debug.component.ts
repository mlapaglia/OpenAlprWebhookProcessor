import { ChangeDetectionStrategy, Component, inject, type OnDestroy, type OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { SettingsService } from '../settings.service';
import type { Version } from '../version';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';

import { SignalrConnectionMonitorComponent } from './signalr-connection-monitor/signalr-connection-monitor.component';
import { ScheduledJobsComponent } from './scheduled-jobs/scheduled-jobs.component';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';
import { ConfirmationDialogComponent, type ConfirmationDialogData } from 'app/shared/confirmation-dialog/confirmation-dialog.component';

@Component({
  selector: 'app-debug',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './debug.component.html',
  styleUrls: ['./debug.component.less'],
  imports: [
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatDividerModule,
    SignalrConnectionMonitorComponent,
    ScheduledJobsComponent,
  ],
})
export class DebugComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  private readonly settingsService = inject(SettingsService);
  private readonly snackBarService = inject(SnackbarService);
  private readonly dialog = inject(MatDialog);

  public isCleaningDatabase = false;
  public version: Version | null = null;
  public isLoadingVersion = false;

  ngOnInit(): void {
    this.loadVersion();
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  private loadVersion(): void {
    this.isLoadingVersion = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.settingsService.getVersion(),
      (version) => {
        this.version = version;
        this.isLoadingVersion = false;
      },
      (error) => {
        console.error('Failed to load version:', error);
        this.snackBarService.create(
          'Failed to load version information',
          SnackBarType.Error,
        );
        this.isLoadingVersion = false;
      },
    );
  }

  cleanupDatabase(): void {
    if (this.isCleaningDatabase) {
      return;
    }

    const dialogData: ConfirmationDialogData = {
      title: 'Confirm Database Cleanup',
      message: 'This will permanently:\n\n' +
        '• Remove all webhook forwards\n' +
        '• Remove all web push subscriptions\n' +
        '• Remove all pushover clients\n' +
        '• Clear all stored images\n' +
        '• Obfuscate all license plate numbers\n\n' +
        'This action cannot be undone. Are you sure?',
      confirmText: 'Yes, Clean Database',
      cancelText: 'Cancel',
      icon: 'warning',
      color: 'warn',
    };

    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      data: dialogData,
      width: '500px',
      disableClose: true,
    });

    this.subscribeAndMarkForCheck(
      dialogRef.afterClosed(),
      (confirmed) => {
        if (confirmed) {
          this.performDatabaseCleanup();
        }
      },
    );
  }

  private performDatabaseCleanup(): void {
    this.isCleaningDatabase = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.settingsService.cleanupDatabase(),
      () => {
        this.snackBarService.create(
          'Database cleanup completed successfully',
          SnackBarType.Successful,
        );
        this.isCleaningDatabase = false;
      },
      (error) => {
        console.error('Database cleanup failed:', error);
        this.snackBarService.create(
          'Database cleanup failed. Check logs for details.',
          SnackBarType.Error,
        );
        this.isCleaningDatabase = false;
      },
    );
  }
}
