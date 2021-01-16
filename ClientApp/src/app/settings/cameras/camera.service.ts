import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Camera } from './camera';

@Injectable({
  providedIn: 'root'
})
export class CameraService {
  
  constructor(private http: HttpClient) { }

  getCameras(): Observable<Camera[]> {
    return this.http.get<Camera[]>(`/settings/cameras`);
  }

  deleteCamera(cameraId: string): Observable<any> {
    return this.http.post(`/settings/cameras/${cameraId}/delete`, null);
  }

  upsertCamera(camera: Camera) {
    return this.http.post(`/settings/camera`, camera);
  }
}
