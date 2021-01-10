import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from "src/environments/environment";
import { Plate } from "./plate";

@Injectable({ providedIn: 'root' })
export class PlateService {
    private recentPlatesUrl = 'licenseplates/recent';

    constructor(
        private http: HttpClient) { }
        
    getRecentPlates(): Observable<Plate[]> {
        return this.http.get<Plate[]>(`/${this.recentPlatesUrl}`);
    }
}