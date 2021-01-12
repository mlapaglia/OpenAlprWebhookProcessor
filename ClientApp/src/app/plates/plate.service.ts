import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { PlateResponse } from "./plateResponse";

@Injectable({ providedIn: 'root' })
export class PlateService {
    private recentPlatesUrl = 'licenseplates/recent';
    private hydrateDatabaseUrl = "hydration/start";

    constructor(private http: HttpClient) { }
        
    getRecentPlates(pageSize: number, pageNumber: number): Observable<PlateResponse> {
        return this.http.get<PlateResponse>(`/${this.recentPlatesUrl}?pageSize=${pageSize}&pageNumber=${pageNumber}`);
    }

    hydrateDatabase(): Observable<any> {
        return this.http.post(`/${this.hydrateDatabaseUrl}`, {});
    }
}