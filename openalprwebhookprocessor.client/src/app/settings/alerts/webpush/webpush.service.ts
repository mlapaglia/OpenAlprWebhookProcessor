import { Injectable, inject } from '@angular/core'
import { Webpush } from './webpush'
import { Observable } from 'rxjs'
import { HttpClient } from '@angular/common/http'

@Injectable({
  providedIn: 'root',
})
export class WebpushService {
  private http = inject(HttpClient)

  public upsertWebpush(pushover: Webpush): Observable<null> {
    return this.http.post<null>('/api/alerts/webpush', pushover)
  }

  public getWebpush(): Observable<Webpush> {
    return this.http.get<Webpush>('/api/alerts/webpush')
  }

  public testWebpush(): Observable<null> {
    return this.http.post<null>('/api/alerts/webpush/test', null)
  }
}
