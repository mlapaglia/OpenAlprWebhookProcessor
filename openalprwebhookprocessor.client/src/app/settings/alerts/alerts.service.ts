import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Alert } from './alert';

@Injectable({
    providedIn: 'root'
})
export class AlertsService {
    constructor(private http: HttpClient) { }

    getAlerts(): Observable<Alert[]> {
        return this.http.get<Alert[]>('/api/alerts');
    }

    deleteAlert(alertId: string) {
        return this.http.delete(`/alerts/${alertId}`);
    }

    upsertAlerts(alerts: Alert[]) {
        return this.http.post('/api/alerts', alerts);
    }

    addAlert(alert: Alert) {
        return this.http.post('/api/alerts/add', alert);
    }

    testAlert() {
        return this.http.post('/api/alerts/test', null);
    }
}