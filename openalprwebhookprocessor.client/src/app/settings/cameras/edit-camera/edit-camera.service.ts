import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ZoomFocus } from './zoomfocus';

@Injectable({
    providedIn: 'root'
})
export class EditCameraService {
  
    constructor(private http: HttpClient) { }

    triggerDayMode(cameraId: string) {
        return this.http.post(`/api/cameras/${cameraId}/test/day`, null);
    }

    triggerNightMode(cameraId: string) {
        return this.http.post(`/api/cameras/${cameraId}/test/night`, null);
    }

    triggerTestOverlay(cameraId: string) {
        return this.http.post(`/api/cameras/${cameraId}/test/overlay`, null);
    }

    getZoomAndFocus(cameraId: string): Observable<ZoomFocus> {
        return this.http.get<ZoomFocus>(`/api/cameras/${cameraId}/zoomAndFocus`);
    }

    setZoomAndFocus(cameraId: string, zoomFocus: ZoomFocus) {
        return this.http.post(`/api/cameras/${cameraId}/zoomAndFocus`, zoomFocus);
    }
  
    triggerAutofocus(cameraId: string) { 
        return this.http.post(`/api/cameras/${cameraId}/triggerAutofocus`, null);
    }
}
