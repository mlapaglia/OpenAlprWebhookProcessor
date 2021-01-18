import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Camera } from './cameras/camera';
import { Agent } from './openalpr-agent/agent'

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  
  constructor(private http: HttpClient) { }

  getCameras(): Observable<Camera[]> {
    return this.http.get<Camera[]>(`http://localhost:5000/settings/cameras`);
  }

  deleteCamera(cameraId: number): Observable<any> {
    return this.http.post(`http://localhost:5000/settings/cameras/${cameraId}/delete`, null);
  }

  upsertCamera(camera: Camera) {
    return this.http.post(`http://localhost:5000/settings/camera`, camera);
  }

  testCamera(cameraId: number) {
    return this.http.post(`http://localhost:5000/settings/cameras/${cameraId}/test`, null);
  }

  upsertAgent(agent: Agent) {
    return this.http.post(`http://localhost:5000/settings/agent`, agent);
  }

  getAgent(): Observable<Agent> {
    return this.http.get<Agent>(`http://localhost:5000/settings/agent`);
  }
}
