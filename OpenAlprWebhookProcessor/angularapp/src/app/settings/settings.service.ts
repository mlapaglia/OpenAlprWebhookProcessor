import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Alert } from './alerts/alert/alert';
import { Camera } from './cameras/camera';
import { Ignore } from './ignores/ignore/ignore';
import { Agent } from './openalpr-agent/agent'

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  
  constructor(private http: HttpClient) { }

  getCameras(): Observable<Camera[]> {
    return this.http.get<Camera[]>(`cameras`);
  }

  deleteCamera(cameraId: string): Observable<any> {
    return this.http.post(`cameras/${cameraId}/delete`, null);
  }

  upsertCamera(camera: Camera) {
    return this.http.post(`cameras`, camera);
  }

  upsertAgent(agent: Agent) {
    return this.http.post(`/settings/agent`, agent);
  }

  getAgent(): Observable<Agent> {
    return this.http.get<Agent>(`/settings/agent`);
  }

  startAgentScrape(): Observable<any> {
    return this.http.post(`/settings/agent/scrape`, null);
  }

  getIgnores(): Observable<Ignore[]> {
    return this.http.get<Ignore[]>(`/settings/ignores`);
  }

  deleteIgnore(ignoreId: string) {
    return this.http.delete(`/settings/ignores/${ignoreId}`);
  }

  upsertIgnores(ignores: Ignore[]) {
    return this.http.post(`/settings/ignores`, ignores);
  }

  addIgnore(ignore: Ignore) {
    return this.http.post(`/settings/ignores/add`, ignore);
  }

  addAlert(alert: Alert) {
    return this.http.post(`/settings/alerts/add`, alert);
  }
}
