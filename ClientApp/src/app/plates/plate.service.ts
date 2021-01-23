import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { PlateResponse } from "./plateResponse";

@Injectable({ providedIn: 'root' })
export class PlateService {
    private searchPlatesUrl = 'licenseplates/search'
    private hydrateDatabaseUrl = "hydration/start";

    constructor(private http: HttpClient) { }

    searchPlates(plateRequest: PlateRequest): Observable<PlateResponse> {
        return this.http.post<PlateResponse>(`/${this.searchPlatesUrl}`, plateRequest);
    }

    hydrateDatabase(): Observable<any> {
        return this.http.post(`/${this.hydrateDatabaseUrl}`, {});
    }
}

export class PlateRequest {
    plateNumber: string;
    startSearchOn: Date;
    endSearchOn: Date;
    strictMatch: boolean;
    filterIgnoredPlates: boolean;
    pageNumber: number;
    pageSize: number;
}