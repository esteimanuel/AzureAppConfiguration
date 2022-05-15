import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AppConfigService {
  hubConnection: HubConnection;

  constructor() {
    const hubUrl = 'https://azfun-este-kn-app-config.azurewebsites.net/api';

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(hubUrl)
      .configureLogging(LogLevel.Information)
      .build();
  }

  startConnection() {
    this.hubConnection
      .start()
      .then(() => console.log('Connected'))
      .catch((err:any) => console.log('Error while starting connection', err));
  }

  closeConnection() {
    this.hubConnection
      .stop()
      .then(() => console.log('Disconnected'));
  }

  onConfigChanged() {
    const subject = new Subject();
    this.hubConnection.on('configChanges', (msg:any) => subject.next(msg));
    return subject.asObservable();
  }
}
