import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Camera } from './cameras/camera';
import { Ignore } from './ignores/ignore/ignore';
import { Agent } from './openalpr-agent/agent'

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  
  constructor(private http: HttpClient) { }

  getCameras(): Observable<Camera[]> {
    return this.http.get<Camera[]>(`/settings/cameras`);
  }

  deleteCamera(cameraId: number): Observable<any> {
    return this.http.delete(`/settings/cameras/${cameraId}`, null);
  }

  upsertCamera(camera: Camera) {
    return this.http.post(`/settings/camera`, camera);
  }

  testCamera(cameraId: number) {
    return this.http.post(`/settings/cameras/${cameraId}/test`, null);
  }

  upsertAgent(agent: Agent) {
    return this.http.post(`/settings/agent`, agent);
  }

  getAgent(): Observable<Agent> {
    return this.http.get<Agent>(`/settings/agent`);
  }

  getIgnores(): Observable<Ignore[]> {
    return this.http.get<Ignore[]>(`/settings/ignores`);
  }

  deleteIgnore(ignoreId: string) {
    return this.http.delete(`/settings/ignores/${ignoreId}`, null);
  }

  upsertIgnores(ignores: Ignore[]) {
    return this.http.post(`/settings/ignores`, ignores);
  }
}
