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
    return this.http.get<Enricher>('/settings/enrichers');
  }
}
