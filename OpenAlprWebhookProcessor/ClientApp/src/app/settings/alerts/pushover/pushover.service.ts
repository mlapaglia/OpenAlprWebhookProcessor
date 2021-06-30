import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Forward } from '@app/settings/forwards/forward';
import { Pushover } from './pushover';

@Injectable({
  providedIn: 'root'
})
export class PushoverService {

  constructor(private http: HttpClient) { }

  public upsertPushover(pushover: Pushover) {
    return this.http.post('/alerts/pushover', pushover);
  }

  public getPushover() {
    return this.http.get('/alerts/pushover');
  }
}
