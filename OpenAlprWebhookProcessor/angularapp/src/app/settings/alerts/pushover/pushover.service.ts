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
        return this.http.post<void>('/alerts/pushover', pushover);
    }

    public getPushover(): Observable<Pushover> {
        return this.http.get<Pushover>('/alerts/pushover');
    }

    public testPushover(): Observable<void> {
        return this.http.post<void>('/alerts/pushover/test', null);
    }
}