import { Injectable, inject } from '@angular/core';
import { forkJoin, of, type Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { HomeService, type QuickStats } from './home.service';
import type { PredictionResult } from './prediction-response';
import type { MostSeenCounts } from './most-seen/mostSeenResponse';

export interface HomeData {
  quickStats: QuickStats | null;
  nextExpected: PredictionResult | null;
  upcomingPredictions: PredictionResult[];
  predictablePlates: PredictionResult[];
  mostSeenCounts: { name: string, value: number }[];
  hourlyStats: { displayHour: string, count: number }[];
  dailyStats: { date: string, count: number }[];
}

export interface LoadingStates {
  isLoadingStats: boolean;
  isLoadingPredictions: boolean;
  isLoadingCharts: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class HomeDataService {
  private readonly homeService = inject(HomeService);

  loadAllData(): Observable<HomeData> {
    return forkJoin({
      quickStats: this.homeService.getQuickStats().pipe(
        catchError(() => of(null)),
      ),
      nextExpected: this.homeService.getNextExpectedPlate().pipe(
        catchError(() => of(null)),
      ),
      upcomingPredictions: this.homeService.getUpcomingPredictions(6, 24).pipe(
        catchError(() => of([])),
      ),
      predictablePlates: this.homeService.getMostPredictablePlates(6).pipe(
        map(predictions => predictions.sort((a, b) => b.confidenceScore - a.confidenceScore)),
        catchError(() => of([])),
      ),
      mostSeenCounts: this.homeService.getMostSeenPlates().pipe(
        map((response: MostSeenCounts) =>
          response.counts.map(count => ({ name: count.plateNumber, value: count.count })),
        ),
        catchError(() => of([])),
      ),
      hourlyStats: this.homeService.getHourlyStats().pipe(
        catchError(() => of([])),
      ),
      dailyStats: this.homeService.getPlatesCount().pipe(
        map(response => response.counts.map(count => ({
          date: new Date(count.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' }),
          count: count.count,
        }))),
        catchError(() => of([])),
      ),
    });
  }
}
