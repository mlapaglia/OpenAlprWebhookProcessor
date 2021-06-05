import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Forward } from './forward';

@Injectable({
  providedIn: 'root'
})
export class ForwardsService {
  constructor(private http: HttpClient) { }

  getForwards(): Observable<Forward[]> {
    return this.http.get<Forward[]>(`settings/forwards`);
  }

  deleteForward(forwardsId: string) {
    return this.http.delete(`/settings/forwards/${forwardsId}`, null);
  }

  upsertForwards(forwards: Forward[]) {
      return this.http.post(`/settings/forwards`, forwards);
  }
}
