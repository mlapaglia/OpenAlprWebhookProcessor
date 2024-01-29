import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Enricher } from './enricher';

@Injectable({
    providedIn: 'root'
})
export class EnrichersService {

    constructor(private http: HttpClient) { }

    public getEnricher(): Observable<Enricher> {
        return this.http.get<Enricher>('/api/settings/enrichers');
    }

    public upsertEnricher(enricher: Enricher): Observable<void> {
        return this.http.post<void>('/api/settings/enrichers', enricher);
    }

    public testEnricher(enricherId: string): Observable<boolean> {
        return this.http.post<boolean>(`/api/settings/enrichers/${enricherId}/test`, null);
    }
}
