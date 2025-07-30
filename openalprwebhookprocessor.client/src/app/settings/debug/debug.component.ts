import { Component, inject, OnInit } from '@angular/core';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { SettingsService } from '../settings.service';
import { Version } from '../version';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';

@Component({
  selector: 'app-debug',
  templateUrl: './debug.component.html',
  styleUrls: ['./debug.component.less'],
  imports: [
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatDividerModule,
  ],
})
export class DebugComponent implements OnInit {
  private readonly settingsService = inject(SettingsService);
  private readonly snackBarService = inject(SnackbarService);

  public isCleaningDatabase = false;
  public version: Version | null = null;
  public isLoadingVersion = false;

  ngOnInit(): void {
    void this.loadVersion();
  }

  private async loadVersion(): Promise<void> {
    try {
      this.isLoadingVersion = true;
      this.version = await this.settingsService.getVersion().toPromise() ?? null;
    } catch (error) {
      console.error('Failed to load version:', error);
      this.snackBarService.create(
        'Failed to load version information',
        SnackBarType.Error,
      );
    } finally {
      this.isLoadingVersion = false;
    }
  }

  async cleanupDatabase(): Promise<void> {
    if (this.isCleaningDatabase) {
      return;
    }

    const confirmed = confirm(
      'This will permanently:\n\n' +
      '• Remove all webhook forwards\n' +
      '• Remove all web push subscriptions\n' +
      '• Remove all pushover clients\n' +
      '• Clear all stored images\n' +
      '• Obfuscate all license plate numbers\n\n' +
      'This action cannot be undone. Are you sure?',
    );

    if (!confirmed) {
      return;
    }

    try {
      this.isCleaningDatabase = true;

      await this.settingsService.cleanupDatabase().toPromise();

      this.snackBarService.create(
        'Database cleanup completed successfully',
        SnackBarType.Successful,
      );
    } catch (error) {
      console.error('Database cleanup failed:', error);
      this.snackBarService.create(
        'Database cleanup failed. Check logs for details.',
        SnackBarType.Error,
      );
    } finally {
      this.isCleaningDatabase = false;
    }
  }
}
