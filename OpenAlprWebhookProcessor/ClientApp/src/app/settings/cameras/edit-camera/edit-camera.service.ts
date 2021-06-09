import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class EditCameraService {
  
  constructor(private http: HttpClient) { }

  triggerDayMode(cameraId: string) {
    return this.http.post(`/cameras/${cameraId}/test/day`, null);
  }

  triggerNightMode(cameraId: string) {
    return this.http.post(`/cameras/${cameraId}/test/night`, null);
  }

  triggerTestOverlay(cameraId: string) {
    return this.http.post(`/cameras/${cameraId}/test/overlay`, null);
  }

  getZoomAndFocus(cameraId: string): Observable<any> {
    return this.http.get<any>(`/cameras/${cameraId}/zoomAndFocus`);
  }

  setZoomAndFocus(cameraId: string) {
    return this.http.post(`/cameras/${cameraId}/zoomAndFocus`, null);
  }
}