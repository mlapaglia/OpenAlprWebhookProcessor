import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CameraMask } from './camera-mask';
import { Coordinate } from './coordinate';

@Injectable({
    providedIn: 'root'
})
export class CameraMaskService {
  
    constructor(private http: HttpClient) { }

    getCameraSnapshot(cameraId: string): Observable<Blob> {
        return this.http.get<Blob>(`/api/images/${cameraId}/snapshot`);
    }

    getPlateCaptures(cameraId: string): Observable<string[]> {
        return this.http.get<string[]>(`/api/cameras/${cameraId}/plateCaptures`);
    }

    getPlateCapture(imageUrl: string): Observable<Blob> {
        return this.http.get<Blob>(imageUrl, { responseType: 'blob' as 'json' });
    }

    getCameraMaskCoordinates(cameraId: string): Observable<Coordinate[]> {
        return this.http.get<Coordinate[]>(`/api/cameras/${cameraId}/mask/coordinates`);
    }

    upsertImageMask(imageMask: CameraMask) {
        return this.http.post(`/api/cameras/${imageMask.cameraId}/mask`, imageMask);
    }
}
