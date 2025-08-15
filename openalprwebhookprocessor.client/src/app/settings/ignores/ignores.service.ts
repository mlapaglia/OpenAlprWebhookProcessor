import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import type { Ignore } from './ignore';

@Injectable({
  providedIn: 'root',
})
export class IgnoresService {
  private readonly http = inject(HttpClient);

  getIgnores(): Observable<Ignore[]> {
    return this.http.get<Ignore[]>('/api/ignores');
  }

  deleteIgnore(ignoreId: string) {
    return this.http.delete(`/api/ignores/${ignoreId}`);
  }

  addIgnore(ignore: Ignore) {
    return this.http.post('/api/ignores/add', ignore);
  }

  updateIgnore(ignore: Ignore) {
    return this.http.put(`/api/ignores/${ignore.id}`, ignore);
  }
}
