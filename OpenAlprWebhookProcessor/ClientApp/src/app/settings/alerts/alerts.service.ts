import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Alert } from '@app/_models';
import { Observable } from 'rxjs';


@Injectable({
  providedIn: 'root'
})
export class AlertsService {
    constructor(private http: HttpClient) { }

    getAlerts(): Observable<Alert[]> {
        return this.http.get<Alert[]>(`/alerts`);
    }

    deleteAlert(alertId: string) {
        return this.http.delete(`/alerts/${alertId}`, null);
    }

    upsertAlerts(alerts: Alert[]) {
        return this.http.post(`/alerts`, alerts);
    }

    addAlert(alert: Alert) {
        return this.http.post(`/alerts/add`, alert);
    }

    testAlert() {
        return this.http.post(`/alerts/test`, null);
    }
}