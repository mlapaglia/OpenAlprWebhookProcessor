import { Injectable } from '@angular/core';
import { Webpush } from './webpush';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class WebpushService {

  constructor(private http: HttpClient) { }

  public upsertWebpush(pushover: Webpush): Observable<any> {
    return this.http.post('/alerts/webpush', pushover);
  }

  public getWebpush(): Observable<Webpush> {
    return this.http.get<Webpush>('/alerts/webpush');
  }

  public testWebpush(): Observable<any> {
    return this.http.post('/alerts/webpush/test', null);
  }
}
