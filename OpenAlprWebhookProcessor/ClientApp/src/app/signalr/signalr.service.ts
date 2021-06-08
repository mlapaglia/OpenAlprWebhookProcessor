import { Injectable } from '@angular/core';
import { SnackbarService } from '@app/snackbar/snackbar.service';
import { SnackBarType } from '@app/snackbar/snackbartype';
import * as signalR from "@microsoft/signalr";
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  private hubConnection: signalR.HubConnection;

  public connectionEstablished = new Subject<Boolean>();
  public licensePlateReceived = new Subject<string>();
  public licensePlateAlerted = new Subject<string>();
  public processInformationLogged = new Subject<string>();

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
        console.log('Connection started');
        this.snackbarService.create(`Connected to server!`, SnackBarType.Connected);
        this.connectionEstablished.next(true);
      })
      .catch(err => {
        console.log('Error while starting connection: ' + err)
        this.snackbarService.create(`Connection lost`, SnackBarType.Disconnected);
      });

      this.hubConnection.on('ProcessInformationLogged', (logMessage) => {
        this.processInformationLogged.next(logMessage);
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
      });

      this.hubConnection.onreconnecting(() => {
        console.log("Connection reconnecting");
        this.snackbarService.create(`Reconnecting to server...`, SnackBarType.Disconnected);
      })

      this.hubConnection.onclose(() => {
        console.log("Connection ended");
        this.snackbarService.create(`Connection lost`, SnackBarType.Disconnected);
      });
  }

  public stopConnection() {
    this.hubConnection
      .stop()
      .then(() => {
        this.snackbarService.create(`Connection closed`, SnackBarType.Disconnected);
      });
  }
}
