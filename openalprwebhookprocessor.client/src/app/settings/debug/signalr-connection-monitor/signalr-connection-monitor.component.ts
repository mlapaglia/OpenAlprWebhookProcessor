import { Component, inject, type OnInit, type OnDestroy, ChangeDetectionStrategy } from '@angular/core';
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
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

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

interface ClientConnectionInfo {
  state: string;
  connectionId: string | null;
  transport: string;
  startTime: Date | null;
  durationSeconds: number;
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
export class SignalrConnectionMonitorComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  private readonly signalrService = inject(SignalrService);
  private readonly snackBarService = inject(SnackbarService);
  private readonly http = inject(HttpClient);

  public signalrConnections: SignalRConnectionSummary | null = null;
  public isLoadingConnections = false;
  public isManualRefresh = false;
  public clientConnectionInfo: ClientConnectionInfo | null = null;
  private refreshInterval: NodeJS.Timeout | null = null;

  ngOnInit(): void {
    this.loadSignalRConnections(false);
    this.updateClientConnectionInfo();

    this.refreshInterval = setInterval(() => {
      this.updateClientConnectionInfo();
      this.loadSignalRConnections(false);
    }, 5000);
  }

  override ngOnDestroy(): void {
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
    }

    super.ngOnDestroy();
  }

  private loadSignalRConnections(showLoading: boolean = true): void {
    this.setLoadingState(showLoading, true);

    this.subscribeAndMarkForCheck(
      this.http.get<SignalRConnectionSummary>('/api/settings/debug/signalr-connections'),
      (newData) => {
        this.updateSignalRData(newData, showLoading);
        this.setLoadingState(showLoading, false);
      },
      (error) => {
        this.handleLoadError(error, showLoading);
        this.setLoadingState(showLoading, false);
      },
    );
  }

  private setLoadingState(showLoading: boolean, isLoading: boolean): void {
    if (showLoading) {
      this.isLoadingConnections = isLoading;
      this.isManualRefresh = isLoading;
      this.markForCheck();
    }
  }

  private updateSignalRData(newData: SignalRConnectionSummary | null, showLoading: boolean): void {
    if (!this.signalrConnections ||
        this.signalrConnections.totalConnections !== newData?.totalConnections ||
        JSON.stringify(this.signalrConnections.connections) !== JSON.stringify(newData.connections)) {
      this.signalrConnections = newData;
      if (!showLoading) {
        this.markForCheck();
      }
    }
  }

  private handleLoadError(error: unknown, showLoading: boolean): void {
    if (showLoading) {
      console.error('Failed to load SignalR connections:', error);
      this.snackBarService.create(
        'Failed to load SignalR connection information',
        SnackBarType.Error,
      );
    }
  }

  private updateClientConnectionInfo(): void {
    const newInfo = this.signalrService.getConnectionInfo();

    if (this.hasConnectionInfoChanged(newInfo)) {
      this.clientConnectionInfo = newInfo;
      this.markForCheck();
    }
  }

  private hasConnectionInfoChanged(newInfo: ClientConnectionInfo | null): boolean {
    if (!this.clientConnectionInfo) {
      return true;
    }

    if (!newInfo) {
      return true;
    }

    if (this.clientConnectionInfo.state !== newInfo.state) {
      return true;
    }

    if (this.clientConnectionInfo.connectionId !== newInfo.connectionId) {
      return true;
    }

    if (this.clientConnectionInfo.transport !== newInfo.transport) {
      return true;
    }

    const currentDuration = this.clientConnectionInfo.durationSeconds;
    const newDuration = newInfo.durationSeconds;

    return Math.abs(currentDuration - newDuration) > 1;
  }

  refreshConnections(): void {
    this.loadSignalRConnections(true);
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
