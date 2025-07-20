import { HttpClient } from '@angular/common/http'
import { Injectable, inject } from '@angular/core'
import { Observable } from 'rxjs'
import { map } from 'rxjs/operators'
import { DayCounts } from './plateCountResponse'
import { MostSeenCounts } from './mostSeenResponse'
import { PredictionResult } from './prediction-response'

export interface HourlyStats {
  hour: number;
  count: number;
  displayHour: string; // "9 AM", "2 PM", etc.
}

export interface QuickStats {
  todayCount: number;
  weekCount: number;
  monthCount: number;
  uniquePlatesThisWeek: number;
  activeCameras: number;
  averageDailyPlates: number;
}

export interface GetHourlyStatsResponse {
  counts: HourlyCount[];
}

export interface HourlyCount {
  hour: number;
  count: number;
}

@Injectable({ providedIn: 'root' })
export class HomeService {
  private http = inject(HttpClient)

  private plateCountsUrl = 'licensePlates/counts'
  private mostSeenUrl = 'licensePlates/most-seen'
  private predictionsUrl = 'machinelearning/predict/top'
  private hourlyStatsUrl = 'licensePlates/stats/hourly'
  private quickStatsUrl = 'licensePlates/stats/quick'

  getPlatesCount(): Observable<DayCounts> {
    return this.http.get<DayCounts>(`/api/${this.plateCountsUrl}`)
  }

  getMostSeenPlates(): Observable<MostSeenCounts> {
    return this.http.get<MostSeenCounts>(`/api/${this.mostSeenUrl}`)
  }

  getUpcomingPredictions(count: number = 10, withinHours: number = 24): Observable<PredictionResult[]> {
    return this.http.get<PredictionResult[]>(`/api/${this.predictionsUrl}?count=${count}&withinHours=${withinHours}`)
  }

  getMostPredictablePlates(count: number = 10): Observable<PredictionResult[]> {
    // Get predictions for the most predictable plates (highest confidence scores)
    return this.http.get<PredictionResult[]>(`/api/${this.predictionsUrl}?count=${count}&withinHours=168`) // Within a week
  }

  getNextExpectedPlate(): Observable<PredictionResult | null> {
    return this.http.get<PredictionResult[]>(`/api/${this.predictionsUrl}?count=1&withinHours=24`)
      .pipe(
        map((results: PredictionResult[]) => results && results.length > 0 ? results[0] : null)
      )
  }

  getHourlyStats(): Observable<HourlyStats[]> {
    return this.http.get<GetHourlyStatsResponse>(`/api/${this.hourlyStatsUrl}`)
      .pipe(
        map(response => response.counts.map(item => ({
          ...item,
          displayHour: this.formatHour(item.hour)
        })))
      )
  }

  getQuickStats(): Observable<QuickStats> {
    return this.http.get<QuickStats>(`/api/${this.quickStatsUrl}`)
  }

  private formatHour(hour: number): string {
    if (hour === 0) return '12 AM'
    if (hour === 12) return '12 PM'
    if (hour < 12) return `${hour} AM`
    return `${hour - 12} PM`
  }
}
