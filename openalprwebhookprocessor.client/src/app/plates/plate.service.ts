import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import type { Stream } from 'stream';
import type { Plate } from './plate/plate';
import type { PlateResponse } from './plate/plateResponse';
import type { PlateStatistics } from './plate/plateStatistics';
import type { VehicleFilters } from './vehicleFilters';
import type { GetPlateResponse } from './plate/getPlateResponse';

@Injectable({ providedIn: 'root' })
export class PlateService {
  private readonly http = inject(HttpClient);

  private readonly getRelayImageUrl = 'images';
  private readonly searchPlatesUrl = 'licenseplates/search';
  private readonly editPlateUrl = 'licenseplates/edit';
  private readonly singlePlateUrl = 'licenseplates';
  private readonly hydrateDatabaseUrl = 'hydration/start';
  private readonly getFiltersUrl = 'licenseplates/filters';
  private readonly getStatistics = 'licenseplates/statistics';
  private readonly enrichPlateUrl = 'licenseplates/enrich';

  searchPlates(plateRequest: PlateRequest): Observable<PlateResponse> {
    return this.http.post<PlateResponse>(`/api/${this.searchPlatesUrl}`, plateRequest);
  }

  upsertPlate(plate: Plate): Observable<null> {
    return this.http.post<null>(`/api/${this.editPlateUrl}`, plate);
  }

  deletePlate(plateId: string): Observable<null> {
    return this.http.delete<null>(`/api/${this.singlePlateUrl}/${plateId}`);
  }

  getRelayImage(imageId: string) {
    return this.http.get<Stream>(`/api/${this.getRelayImageUrl}/${imageId}`);
  }

  hydrateDatabase(): Observable<null> {
    return this.http.post<null>(`/api/${this.hydrateDatabaseUrl}`, {});
  }

  getFilters(): Observable<VehicleFilters> {
    return this.http.get<VehicleFilters>(`/api/${this.getFiltersUrl}`);
  }

  getPlate(plateId: string): Observable<GetPlateResponse> {
    return this.http.get<GetPlateResponse>(`/api/${this.singlePlateUrl}/${plateId}`);
  }

  getPlateStatistics(plateNumber: string): Observable<PlateStatistics> {
    return this.http.get<PlateStatistics>(`/api/${this.getStatistics}/${plateNumber}`);
  }

  enrichPlate(plateId: string): Observable<null> {
    return this.http.post<null>(`/api/${this.enrichPlateUrl}/${plateId}`, null);
  }
}

export class PlateRequest {
  plateNumber: string;
  startSearchOn: Date;
  endSearchOn: Date;
  strictMatch: boolean;
  filterIgnoredPlates: boolean;
  filterPlatesSeenLessThan: number;
  regexSearchEnabled: boolean;
  pageNumber: number;
  pageSize: number;
  vehicleMake: string;
  vehicleModel: string;
  vehicleColor: string;
  vehicleType: string;
  vehicleRegion: string;
}
