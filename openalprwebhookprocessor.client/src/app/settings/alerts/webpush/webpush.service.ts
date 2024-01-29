import { Injectable } from '@angular/core';
import { Webpush } from './webpush';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({
    providedIn: 'root'
})
export class WebpushService {

    constructor(private http: HttpClient) { }

    public upsertWebpush(pushover: Webpush): Observable<void> {
        return this.http.post<void>('/api/alerts/webpush', pushover);
    }

    public getWebpush(): Observable<Webpush> {
        return this.http.get<Webpush>('/api/alerts/webpush');
    }

    public testWebpush(): Observable<void> {
        return this.http.post<void>('/api/alerts/webpush/test', null);
    }
}
