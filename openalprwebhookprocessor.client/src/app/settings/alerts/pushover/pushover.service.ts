import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import type { Pushover } from './pushover';

@Injectable({
  providedIn: 'root',
})
export class PushoverService {
  private readonly http = inject(HttpClient);

  public upsertPushover(pushover: Pushover): Observable<null> {
    return this.http.post<null>('/api/alerts/pushover', pushover);
  }

  public getPushover(): Observable<Pushover> {
    return this.http.get<Pushover>('/api/alerts/pushover');
  }

  public testPushover(): Observable<null> {
    return this.http.post<null>('/api/alerts/pushover/test', null);
  }
}
