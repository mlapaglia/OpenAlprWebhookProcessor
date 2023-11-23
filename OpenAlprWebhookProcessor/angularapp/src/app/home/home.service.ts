import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { DayCounts } from './plateCountResponse';

@Injectable({ providedIn: 'root' })
export class HomeService {
    private plateCountsUrl = 'licenseplates/counts';

    constructor(private http: HttpClient) { }

    getPlatesCount(): Observable<DayCounts> {
        return this.http.get<DayCounts>(`/${this.plateCountsUrl}`);
    }
}
