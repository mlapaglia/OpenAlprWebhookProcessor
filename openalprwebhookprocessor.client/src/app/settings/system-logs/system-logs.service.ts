import { HttpClient } from '@angular/common/http'
import { Injectable, inject } from '@angular/core'
import { Observable } from 'rxjs'

@Injectable({
  providedIn: 'root',
})
export class SystemLogsService {
  private http = inject(HttpClient)

  getLogs(): Observable<string[]> {
    return this.http.get<string[]>('/api/logs')
  }

  getPlateGroups(onlyFailedPlateGroups: boolean): Observable<Blob> {
    return this.http.get<Blob>(`/api/settings/debug/plates?onlyFailedPlateGroups=${onlyFailedPlateGroups}`, { responseType: 'blob' as 'json' })
  }

  deletePlates() {
    return this.http.delete('/settings/debug/plates')
  }
}
