import { Injectable } from '@angular/core';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import * as signalR from "@microsoft/signalr";
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  private hubConnection: signalR.HubConnection;

  public connectionEstablished = new Subject<boolean>();
  public licensePlateReceived = new Subject<string>();
  public licensePlateAlerted = new Subject<string>();
  public processInformationLogged = new Subject<string>();
  public openAlprAgentConnectionStatusChanged = new Subject<boolean>();
  public isConnected: boolean;
  public connectionStatusChanged: Subject<boolean> = new Subject<boolean>();

  constructor(
    private snackbarService: SnackbarService
    ) { }
  
  public startConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/processorhub')
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => {
        this.snackbarService.create(`Connected to server!`, SnackBarType.Connected);
        this.connectionEstablished.next(true);
        this.triggerConnectionStatusChange(true);
      })
      .catch(err => {
        console.log('Error while starting connection: ' + err)
        this.snackbarService.create(`Connection lost`, SnackBarType.Disconnected);
      });

      this.hubConnection.on('ProcessInformationLogged', (logMessage) => {
        this.processInformationLogged.next(logMessage);
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
          "IP Address: " + ipAddress);
      });
      
      this.hubConnection.on('LicensePlateRecorded', (plateNumber) => {
        this.licensePlateReceived.next(plateNumber);
      });

      this.hubConnection.on('LicensePlateAlerted', (plateNumber) => {
        this.snackbarService.create(`Alert! Plate Number: ${plateNumber}`, SnackBarType.Alert);
      });

      this.hubConnection.onreconnected(() => {
        console.log("Connection reconnected");
        this.snackbarService.create(`Reconnected to server!`, SnackBarType.Connected);
        this.triggerConnectionStatusChange(true);
      });

      this.hubConnection.onreconnecting(() => {
        console.log("Connection reconnecting");
        this.snackbarService.create(`Reconnecting to server...`, SnackBarType.Disconnected);
        this.triggerConnectionStatusChange(false);
      })

      this.hubConnection.onclose(() => {
        console.log("Connection ended");
        this.snackbarService.create(`Connection lost`, SnackBarType.Disconnected);
        this.triggerConnectionStatusChange(false);
      });

      this.hubConnection.on('ScrapeFinished', () => {
        console.log("Scrape finished");
        this.snackbarService.create(`Scrape finished!`, SnackBarType.Info);
      });
  }

  public stopConnection() {
    this.hubConnection
      .stop()
      .then(() => {
        this.snackbarService.create(`Connection closed`, SnackBarType.Disconnected);
        this.triggerConnectionStatusChange(false);
      });
  }

  public triggerConnectionStatusChange(isConencted: boolean) {
    this.isConnected = isConencted;
    this.connectionStatusChanged.next(this.isConnected);
  }
}
