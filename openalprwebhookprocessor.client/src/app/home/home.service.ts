import { HttpClient } from '@angular/common/http'
import { Injectable, inject } from '@angular/core'
import { Observable } from 'rxjs'
import { DayCounts } from './plateCountResponse'

@Injectable({ providedIn: 'root' })
export class HomeService {
  private http = inject(HttpClient)

  private plateCountsUrl = 'licenseplates/counts'

  getPlatesCount(): Observable<DayCounts> {
    return this.http.get<DayCounts>(`/api/${this.plateCountsUrl}`)
  }
}
