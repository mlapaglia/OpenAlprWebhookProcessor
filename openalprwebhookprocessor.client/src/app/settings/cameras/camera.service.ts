import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { switchMap } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class CameraService {
  private readonly http = inject(HttpClient);

  loadSampleImage(imageUrl: string): Observable<string> {
    return this.http.get(imageUrl, { responseType: 'blob' }).pipe(
      switchMap((blob: Blob) => {
        return new Observable<string>((observer) => {
          const reader = new FileReader();
          reader.onload = () => {
            observer.next(reader.result as string);
            observer.complete();
          };
          reader.onerror = () => {
            observer.error(new Error('Failed to read image blob'));
          };
          reader.readAsDataURL(blob);
        });
      }),
    );
  }
}
