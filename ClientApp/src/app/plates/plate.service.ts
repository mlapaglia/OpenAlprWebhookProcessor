import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { Plate } from "./plate";

@Injectable({ providedIn: 'root' })
export class PlateService {
    private apiKey = '';
    private agentUrl = 'http://192.168.1.164:4382';
    private recentPlatesUrl = 'licensePlates/recent';

    constructor(
        private http: HttpClient) { }
        
    getRecentPlates(): Observable<Plate[]> {
        return this.http.get<Plate[]>(this.recentPlatesUrl);
    }
}