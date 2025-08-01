import { Injectable, inject } from '@angular/core';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { AccountService } from 'app/_services';
import type { ApiLogLevel } from 'app/settings/system-logs/system-logs.service';

@Injectable({
  providedIn: 'root',
})
export class SignalrService {
  private readonly snackbarService = inject(SnackbarService);
  private readonly accountService = inject(AccountService);

  private hubConnection: signalR.HubConnection;

  public connectionEstablished = new Subject<boolean>();
  public licensePlateReceived = new Subject<string>();
  public licensePlateAlerted = new Subject<string>();
  public processInformationLogged = new Subject<{ logLevel: ApiLogLevel, logMessage: string }>();
  public openAlprAgentConnectionStatusChanged = new Subject<boolean>();
  public isConnected: boolean;
  public connectionStatusChanged: Subject<boolean> = new Subject<boolean>();
  public databaseCleanupCompleted: Subject<boolean> = new Subject<boolean>();
  public connectionStartTime: Date | null = null;

  public startConnection() {
    if (this.isConnected) {
      return;
    }

    const user = this.accountService.userValue;
    if (!user?.jwtToken) {
      return;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/api/processorHub', {
        accessTokenFactory: () => user.jwtToken,
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => {
        this.connectionStartTime = new Date();
        this.snackbarService.create('Connected to server!', SnackBarType.Connected);
        this.connectionEstablished.next(true);
        this.triggerConnectionStatusChange(true);
      })
      .catch(_ => {
        this.snackbarService.create('Connection lost', SnackBarType.Disconnected);
      });

    this.setupEventHandlers();
  }

  private setupEventHandlers() {
    this.hubConnection.on('ProcessInformationLogged', (logLevel: ApiLogLevel, logMessage: string) => {
      this.processInformationLogged.next({ logLevel, logMessage });
    });

    this.hubConnection.on('OpenAlprAgentConnected', (agentId, ipAddress) => {
      this.openAlprAgentConnectionStatusChanged.next(true);
      this.snackbarService.create(
        `OpenALPR Agent Connected: ${agentId}`,
        SnackBarType.Connected,
        `IP Address: ${ipAddress}`);
    });

    this.hubConnection.on('OpenAlprAgentDisconnected', (agentId, ipAddress) => {
      this.openAlprAgentConnectionStatusChanged.next(false);
      this.snackbarService.create(
        `OpenALPR Agent Disconnected: ${agentId}`,
        SnackBarType.Disconnected,
        `IP Address: ${ipAddress}`);
    });

    this.hubConnection.on('LicensePlateRecorded', (plateNumber) => {
      this.licensePlateReceived.next(plateNumber);
    });

    this.hubConnection.on('DatabaseCleanupCompleted', () => {
      this.snackbarService.create('Database Cleanup Completed!', SnackBarType.Successful);
    });

    this.hubConnection.on('LicensePlateAlerted', (plateNumber) => {
      this.snackbarService.create(`Alert! Plate Number: ${plateNumber}`, SnackBarType.Alert);
    });

    this.hubConnection.onreconnected(() => {
      this.connectionStartTime = new Date(); // Reset connection time on reconnect
      this.snackbarService.create('Reconnected to server!', SnackBarType.Connected);
      this.triggerConnectionStatusChange(true);
    });

    this.hubConnection.onreconnecting(() => {
      this.snackbarService.create('Reconnecting to server...', SnackBarType.Disconnected);
      this.triggerConnectionStatusChange(false);
    });

    this.hubConnection.onclose(() => {
      this.connectionStartTime = null;
      this.snackbarService.create('Connection lost', SnackBarType.Disconnected);
      this.triggerConnectionStatusChange(false);
    });

    this.hubConnection.on('ScrapeFinished', () => {
      this.snackbarService.create('Scrape finished!', SnackBarType.Info);
    });
  }

  public stopConnection() {
    if (!this.hubConnection) {
      return;
    }

    this.connectionStartTime = null;
    this.hubConnection
      .stop()
      .then(() => {
        this.snackbarService.create('Connection closed', SnackBarType.Disconnected);
        this.triggerConnectionStatusChange(false);
      })
      .catch(_ => {
        // do nothing
      });
  }

  private triggerConnectionStatusChange(isConnected: boolean): void {
    this.isConnected = isConnected;
    this.connectionStatusChanged.next(this.isConnected);
  }

  public getConnectionInfo() {
    if (!this.hubConnection) {
      return null;
    }

    const getStateString = (state: signalR.HubConnectionState): string => {
      switch (state) {
        case signalR.HubConnectionState.Connecting: return 'Connecting';
        case signalR.HubConnectionState.Connected: return 'Connected';
        case signalR.HubConnectionState.Reconnecting: return 'Reconnecting';
        case signalR.HubConnectionState.Disconnecting: return 'Disconnecting';
        case signalR.HubConnectionState.Disconnected: return 'Disconnected';
        default: return 'Unknown';
      }
    };

    return {
      state: getStateString(this.hubConnection.state),
      connectionId: this.hubConnection.connectionId,
      transport: (this.hubConnection as any).transport?.name || 'Unknown',
      startTime: this.connectionStartTime,
      durationSeconds: this.connectionStartTime ?
        Math.floor((new Date().getTime() - this.connectionStartTime.getTime()) / 1000) : 0,
    };
  }
}
