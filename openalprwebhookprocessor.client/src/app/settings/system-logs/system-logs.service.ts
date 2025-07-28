import { HttpClient } from '@angular/common/http'
import { Injectable, inject } from '@angular/core'
import { Observable } from 'rxjs'

export enum ApiLogLevel {
  Verbose = 0,
  Debug = 1,
  Information = 2,
  Warning = 3,
  Error = 4,
  Critical = 5,
}

@Injectable({
  providedIn: 'root',
})
export class SystemLogsService {
  private http = inject(HttpClient)

  getLogs(logLevel: ApiLogLevel = ApiLogLevel.Information, search?: string): Observable<string[]> {
    let url = `/api/logs?logLevel=${logLevel}`
    if (search && search.trim()) {
      url += `&search=${encodeURIComponent(search.trim())}`
    }
    return this.http.get<string[]>(url)
  }

  getPlateGroups(onlyFailedPlateGroups: boolean): Observable<Blob> {
    return this.http.get<Blob>(`/api/settings/debug/plates?onlyFailedPlateGroups=${onlyFailedPlateGroups}`, { responseType: 'blob' as 'json' })
  }

  deletePlates() {
    return this.http.delete('/settings/debug/plates')
  }
}
