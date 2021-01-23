import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { Stream } from "stream";
import { PlateResponse } from "./plateResponse";

@Injectable({ providedIn: 'root' })
export class PlateService {
    private getRelayImageUrl = 'images';
    private searchPlatesUrl = 'licenseplates/search'
    private hydrateDatabaseUrl = "hydration/start";

    constructor(private http: HttpClient) { }

    searchPlates(plateRequest: PlateRequest): Observable<PlateResponse> {
        return this.http.post<PlateResponse>(`/${this.searchPlatesUrl}`, plateRequest);
    }

    getRelayImage(imageId: string) {
        return this.http.get<Stream>(`/${this.getRelayImageUrl}/${imageId}`)
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