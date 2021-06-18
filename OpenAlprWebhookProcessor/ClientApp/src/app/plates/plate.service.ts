import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { Stream } from "stream";
import { PlateResponse } from "./plateResponse";
import { VehicleFilters } from "./VehicleFilters";

@Injectable({ providedIn: 'root' })
export class PlateService {
    private getRelayImageUrl = 'images';
    private searchPlatesUrl = 'licenseplates/search';
    private deletePlateUrl = 'licenseplates';
    private hydrateDatabaseUrl = "hydration/start";
    private getFiltersUrl = 'licenseplates/filters';

    constructor(private http: HttpClient) { }

    searchPlates(plateRequest: PlateRequest): Observable<PlateResponse> {
        return this.http.post<PlateResponse>(`/${this.searchPlatesUrl}`, plateRequest);
    }

    deletePlate(plateId: string): Observable<any> {
        return this.http.delete(`/${this.deletePlateUrl}/${plateId}`);
    }

    getRelayImage(imageId: string) {
        return this.http.get<Stream>(`/${this.getRelayImageUrl}/${imageId}`)
    }

    hydrateDatabase(): Observable<any> {
        return this.http.post(`/${this.hydrateDatabaseUrl}`, {});
    }

    getFilters(): Observable<VehicleFilters> {
        return this.http.get<VehicleFilters>(`/${this.getFiltersUrl}`);
    }
}

export class PlateRequest {
    plateNumber: string;
    startSearchOn: Date;
    endSearchOn: Date;
    strictMatch: boolean;
    filterIgnoredPlates: boolean;
    regexSearchEnabled: boolean
    pageNumber: number;
    pageSize: number;
    vehicleMake: string;
    vehicleModel: string;
    vehicleColor: string;
    vehicleType: string;
}
