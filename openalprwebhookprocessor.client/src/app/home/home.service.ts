import { HttpClient } from '@angular/common/http'
import { Injectable, inject } from '@angular/core'
import { Observable } from 'rxjs'
import { DayCounts } from './plateCountResponse'
import { MostSeenCounts } from './mostSeenResponse'

@Injectable({ providedIn: 'root' })
export class HomeService {
  private http = inject(HttpClient)

  private plateCountsUrl = 'licenseplates/counts'

  private mostSeenUrl = 'licenseplates/mostseen'

  getPlatesCount(): Observable<DayCounts> {
    return this.http.get<DayCounts>(`/api/${this.plateCountsUrl}`)
  }

  getMostSeenPlates(): Observable<MostSeenCounts> {
    return this.http.get<MostSeenCounts>(`/api/${this.mostSeenUrl}`)
  }
}
