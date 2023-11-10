import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CameraMask } from './camera-mask';
import { Coordinate } from './coordinate';

@Injectable({
  providedIn: 'root'
})
export class CameraMaskService {
  
  constructor(private http: HttpClient) { }

  getCameraSnapshot(imageUrl: string): Observable<Blob> {
    let headers = new HttpHeaders({
        'Accept': 'image/jpeg',
      });

    return this.http.get<Blob>(imageUrl, { responseType: 'blob' as 'json' });
  }

  getCameraMaskCoordinates(cameraId: string): Observable<Coordinate[]> {
    return this.http.get<Coordinate[]>(`/cameras/${cameraId}/mask/coordinates`);
  }

  upsertImageMask(imageMask: CameraMask) {
    return this.http.post(`/cameras/${imageMask.cameraId}/mask`, imageMask);
  }
}