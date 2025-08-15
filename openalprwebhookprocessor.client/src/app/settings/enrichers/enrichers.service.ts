import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import type { Enricher } from './enricher';

@Injectable({
  providedIn: 'root',
})
export class EnrichersService {
  private readonly http = inject(HttpClient);

  public getEnricher(): Observable<Enricher> {
    return this.http.get<Enricher>('/api/settings/enrichers');
  }

  public upsertEnricher(enricher: Enricher): Observable<null> {
    return this.http.post<null>('/api/settings/enrichers', enricher);
  }

  public testEnricher(enricherId: string): Observable<boolean> {
    return this.http.post<boolean>(`/api/settings/enrichers/${enricherId}/test`, null);
  }
}
