import { Injectable } from '@angular/core';
import * as signalR from "@microsoft/signalr";
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  private hubConnection: signalR.HubConnection;
  public connectionEstablished = new Subject<Boolean>();
  public licensePlateReceived = new Subject<string>();

  constructor() { }
  
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

      this.hubConnection.on('LicenePlateRecorded', (plateNumber) => {
        this.licensePlateReceived.next(plateNumber);
      });
  }

  public stopConnection() {
    this.hubConnection
      .stop()
      .then(() => console.log("Connection ended"));
  }

  public subscribe() {
    this.hubConnection.on('licenePlateRecorded', (data) => {
      console.log("saw a plate: " + data);
    });
  }
}
