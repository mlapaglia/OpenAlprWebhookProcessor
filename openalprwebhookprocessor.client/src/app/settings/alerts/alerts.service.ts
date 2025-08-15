import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import type { Alert } from './alert';

@Injectable({
  providedIn: 'root',
})
export class AlertsService {
  private readonly http = inject(HttpClient);

  getAlerts(): Observable<Alert[]> {
    return this.http.get<Alert[]>('/api/alerts');
  }

  deleteAlert(alertId: string) {
    return this.http.delete(`/api/alerts/${alertId}`);
  }

  addAlert(alert: Alert) {
    return this.http.post('/api/alerts/add', alert);
  }

  updateAlert(alert: Alert) {
    return this.http.put(`/api/alerts/${alert.id}`, alert);
  }

  testAlert() {
    return this.http.post('/api/alerts/test', null);
  }
}
