import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

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
}