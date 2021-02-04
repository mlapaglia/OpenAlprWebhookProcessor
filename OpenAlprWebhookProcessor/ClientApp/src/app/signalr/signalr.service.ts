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

  constructor(
    private snackbarService: SnackbarService
    ) { }
  
  public startConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/processorhub')
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

      this.hubConnection.on('LicensePlateRecorded', (plateNumber) => {
        this.licensePlateReceived.next(plateNumber);
      });

      this.hubConnection.on('LicensePlateAlerted', (plateNumber) => {
        this.snackbarService.create(`Alert! Plate Number: ${plateNumber}`, SnackBarType.Alert);
      });
  }

  public stopConnection() {
    this.hubConnection
      .stop()
      .then(() => {
        console.log("Connection ended");
        this.snackbarService.create(`Connection lost`, SnackBarType.Disconnected);
      });
  }
}
