import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import type { Alert } from './alerts/alert';
import type { Camera } from './cameras/camera';
import type { Ignore } from './ignores/ignore';
import type { Agent } from './openalpr-agent/agent';
import type { AgentStatus } from './openalpr-agent/agentStatus';

@Injectable({
  providedIn: 'root',
})
export class SettingsService {
  private readonly http = inject(HttpClient);

  getCameras(): Observable<Camera[]> {
    return this.http.get<Camera[]>('/api/cameras');
  }

  deleteCamera(cameraId: string): Observable<null> {
    return this.http.post<null>(`/api/cameras/${cameraId}/delete`, null);
  }

  upsertCamera(camera: Camera) {
    return this.http.post('/api/cameras', camera);
  }

  upsertAgent(agent: Agent) {
    return this.http.post('/api/settings/agent', agent);
  }

  getAgent(): Observable<Agent> {
    return this.http.get<Agent>('/api/settings/agent');
  }

  getAgentStatus(): Observable<AgentStatus> {
    return this.http.get<AgentStatus>('/api/settings/agent/status');
  }

  disableAgent(agentId: string): Observable<boolean> {
    return this.http.post<boolean>('/api/settings/agent/disable', agentId);
  }

  enableAgent(agentId: string): Observable<boolean> {
    return this.http.post<boolean>('/api/settings/agent/enable', agentId);
  }

  startAgentScrape(): Observable<null> {
    return this.http.post<null>('/api/settings/agent/scrape', null);
  }

  getIgnores(): Observable<Ignore[]> {
    return this.http.get<Ignore[]>('/api/settings/ignores');
  }

  deleteIgnore(ignoreId: string) {
    return this.http.delete(`/settings/ignores/${ignoreId}`);
  }

  upsertIgnores(ignores: Ignore[]) {
    return this.http.post('/api/settings/ignores', ignores);
  }

  addIgnore(ignore: Ignore) {
    return this.http.post('/api/settings/ignores/add', ignore);
  }

  addAlert(alert: Alert) {
    return this.http.post('/api/settings/alerts/add', alert);
  }

  cleanupDatabase(): Observable<null> {
    return this.http.post<null>('/api/settings/cleanup/database', null);
  }
}
