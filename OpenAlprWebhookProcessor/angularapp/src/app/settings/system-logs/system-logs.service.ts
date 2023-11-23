import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class SystemLogsService {

    constructor(private http: HttpClient) { }

    getLogs(): Observable<string[]> {
        return this.http.get<string[]>('/logs');
    }

    getPlateGroups(onlyFailedPlateGroups: boolean): Observable<Blob> {
        return this.http.get<Blob>(`/settings/debug/plates?onlyFailedPlateGroups=${onlyFailedPlateGroups}`, { responseType: 'blob' as 'json' });
    }

    deletePlates() {
        return this.http.delete('/settings/debug/plates');
    }
}
