import { Component, inject, type OnInit, type OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { SignalrService } from 'app/signalr/signalr.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';

interface SignalRConnection {
  connectionId: string;
  userId: string;
  connectedAt: string;
  durationSeconds: number;
  transport: string;
  userAgent: string;
  ipAddress: string;
}

interface SignalRConnectionSummary {
  totalConnections: number;
  connections: SignalRConnection[];
}

@Component({
  selector: 'app-signalr-connection-monitor',
  templateUrl: './signalr-connection-monitor.component.html',
  styleUrls: ['./signalr-connection-monitor.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatDividerModule,
    CommonModule,
  ],
})
export class SignalrConnectionMonitorComponent implements OnInit, OnDestroy {
  private readonly signalrService = inject(SignalrService);
  private readonly snackBarService = inject(SnackbarService);
  private readonly http = inject(HttpClient);
  private readonly cdr = inject(ChangeDetectorRef);

  public signalrConnections: SignalRConnectionSummary | null = null;
  public isLoadingConnections = false;
  public isManualRefresh = false;
  public clientConnectionInfo: any = null;
  private refreshInterval: any;

  ngOnInit(): void {
    void this.loadSignalRConnections(false);
    this.updateClientConnectionInfo();

    this.refreshInterval = setInterval(() => {
      this.updateClientConnectionInfo();
      void this.loadSignalRConnections(false);
    }, 5000);
  }

  ngOnDestroy(): void {
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
    }
  }

  private async loadSignalRConnections(showLoading: boolean = true): Promise<void> {
    try {
      if (showLoading) {
        this.isLoadingConnections = true;
        this.isManualRefresh = true;
      }

      const newData = await this.http.get<SignalRConnectionSummary>('/api/settings/debug/signalr-connections').toPromise() ?? null;

      // Only update if data actually changed to prevent unnecessary re-renders
      if (!this.signalrConnections ||
          this.signalrConnections.totalConnections !== newData?.totalConnections ||
          JSON.stringify(this.signalrConnections.connections) !== JSON.stringify(newData?.connections)) {
        this.signalrConnections = newData;
        if (!showLoading) {
          this.cdr.markForCheck(); // Only trigger change detection for background updates
        }
      }
    } catch (error) {
      // Only show error messages for manual refreshes to avoid spam during background updates
      if (showLoading) {
        console.error('Failed to load SignalR connections:', error);
        this.snackBarService.create(
          'Failed to load SignalR connection information',
          SnackBarType.Error,
        );
      }
    } finally {
      if (showLoading) {
        this.isLoadingConnections = false;
        this.isManualRefresh = false;
      }
    }
  }

  private updateClientConnectionInfo(): void {
    const newInfo = this.signalrService.getConnectionInfo();

    // Only update if the connection info has actually changed to prevent unnecessary re-renders
    if (!this.clientConnectionInfo ||
        this.clientConnectionInfo.state !== newInfo?.state ||
        this.clientConnectionInfo.connectionId !== newInfo?.connectionId ||
        this.clientConnectionInfo.transport !== newInfo?.transport ||
        Math.abs((this.clientConnectionInfo.durationSeconds || 0) - (newInfo?.durationSeconds || 0)) > 1) {
      this.clientConnectionInfo = newInfo;
      this.cdr.markForCheck(); // Trigger change detection only when data actually changes
    }
  }

  async refreshConnections(): Promise<void> {
    await this.loadSignalRConnections(true);
    this.updateClientConnectionInfo();
  }

  formatDuration(seconds: number): string {
    if (seconds < 60) {
      return `${seconds}s`;
    } else if (seconds < 3600) {
      const minutes = Math.floor(seconds / 60);
      const remainingSeconds = seconds % 60;
      return `${minutes}m ${remainingSeconds}s`;
    } else {
      const hours = Math.floor(seconds / 3600);
      const minutes = Math.floor((seconds % 3600) / 60);
      return `${hours}h ${minutes}m`;
    }
  }
}
