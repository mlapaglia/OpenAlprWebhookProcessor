import { HttpClient } from '@angular/common/http'
import { Injectable, inject } from '@angular/core'
import { Observable } from 'rxjs'
import { Forward } from './forward'

@Injectable({
  providedIn: 'root',
})
export class ForwardsService {
  private http = inject(HttpClient)

  getForwards(): Observable<Forward[]> {
    return this.http.get<Forward[]>('/api/settings/forwards')
  }

  deleteForward(forwardsId: string) {
    return this.http.delete(`/api/settings/forwards/${forwardsId}`)
  }

  upsertForwards(forwards: Forward[]) {
    return this.http.post('/api/settings/forwards', forwards)
  }
}
