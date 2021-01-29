import { Injectable } from '@angular/core';
import { AlertService } from '@app/_services';
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

  constructor(private alertService: AlertService) { }
  
  public startConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/processorhub')
      .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('Connection started');
        this.connectionEstablished.next(true);
      })
      .catch(err => console.log('Error while starting connection: ' + err));

      this.hubConnection.on('LicensePlateRecorded', (plateNumber) => {
        this.licensePlateReceived.next(plateNumber);
      });

      this.hubConnection.on('LicensePlateAlerted', (plateNumber) => {
        this.alertService.info(`Alert! Plate Number: ${plateNumber}`)
      });
  }

  public stopConnection() {
    this.hubConnection
      .stop()
      .then(() => console.log("Connection ended"));
  }
}
