import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Alert } from './alerts/alert';
import { Camera } from './cameras/camera';
import { Ignore } from './ignores/ignore';
import { Agent } from './openalpr-agent/agent';
import { AgentStatus } from './openalpr-agent/agentStatus';

@Injectable({
    providedIn: 'root'
})
export class SettingsService {
  
    constructor(private http: HttpClient) { }

    getCameras(): Observable<Camera[]> {
        return this.http.get<Camera[]>('/api/cameras');
    }

    deleteCamera(cameraId: string): Observable<void> {
        return this.http.post<void>(`/api/cameras/${cameraId}/delete`, null);
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

    startAgentScrape(): Observable<void> {
        return this.http.post<void>('/api/settings/agent/scrape', null);
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
}
