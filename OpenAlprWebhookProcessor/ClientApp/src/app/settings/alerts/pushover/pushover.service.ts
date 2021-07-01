import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Pushover } from './pushover';

@Injectable({
  providedIn: 'root'
})
export class PushoverService {

  constructor(private http: HttpClient) { }

  public upsertPushover(pushover: Pushover): Observable<any> {
    return this.http.post('/alerts/pushover', pushover);
  }

  public getPushover(): Observable<Pushover> {
    return this.http.get<Pushover>('/alerts/pushover');
  }

  public testPushover(): Observable<any> {
    return this.http.post('/alerts/pushover/test', null);
  }
}
