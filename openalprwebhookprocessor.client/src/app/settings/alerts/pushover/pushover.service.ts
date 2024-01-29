import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Pushover } from './pushover';

@Injectable({
    providedIn: 'root'
})
export class PushoverService {
    constructor(private http: HttpClient) { }

    public upsertPushover(pushover: Pushover): Observable<void> {
        return this.http.post<void>('/api/alerts/pushover', pushover);
    }

    public getPushover(): Observable<Pushover> {
        return this.http.get<Pushover>('/api/alerts/pushover');
    }

    public testPushover(): Observable<void> {
        return this.http.post<void>('/api/alerts/pushover/test', null);
    }
}