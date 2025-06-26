import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Pushover } from './pushover';

@Injectable({
    providedIn: 'root'
})
export class PushoverService {
    private http = inject(HttpClient);


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