import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SystemLogsService {

  constructor(private http: HttpClient) { }

  getPlateGroups(onlyFailedPlateGroups: Boolean): Observable<Blob> {
    return this.http.get<Blob>(`/settings/debug/plates?onlyFailedPlateGroups=${onlyFailedPlateGroups}`, { responseType: 'blob' as 'json' });
  }
}
