import { Component, inject, ViewChild, type AfterViewInit, type OnDestroy } from '@angular/core';
import type { User } from 'app/_models';
import { AccountService } from 'app/_services';
import { HomeService, type QuickStats } from './home.service';
import { CommonModule } from '@angular/common';
import type { PredictionResult } from './prediction-response';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import type { Subscription } from 'rxjs';
import { PredictionsSectionComponent } from './predictions-section.component';
import { QuickStatsComponent } from './quick-stats.component';
import { ChartsComponent } from './charts.component';
import { MostSeenPlatesComponent } from './most-seen-plates.component';

@Component({
  templateUrl: 'home.component.html',
  imports: [
    CommonModule,
    PredictionsSectionComponent,
    QuickStatsComponent,
    ChartsComponent,
    MostSeenPlatesComponent,
  ],
})
export class HomeComponent implements AfterViewInit, OnDestroy {
  @ViewChild(ChartsComponent) chartsComponent?: ChartsComponent;

  private readonly accountService = inject(AccountService);
  private readonly homeService = inject(HomeService);
  private readonly breakpointObserver = inject(BreakpointObserver);
  private breakpointSubscription?: Subscription;

  user: User;

  public mostSeenCounts: { name: string, value: number }[] = [];

  // ML Predictions
  public nextExpected: PredictionResult | null = null;
  public upcomingPredictions: PredictionResult[] = [];
  public predictablePlates: PredictionResult[] = [];

  public quickStats: QuickStats | null = null;

  // Loading states
  public isLoadingPredictions = false;
  public isLoadingStats = false;
  public isLoadingCharts = false;

  // Responsive layout
  public isMobile = false;
  public isTablet = false;

  // Grid layout
  public quickStatCols = 4;

  constructor() {
    this.user = this.accountService.userValue;
    this.setupResponsiveLayout();
  }

  ngAfterViewInit() {
    // Defer data loading to next tick to avoid ExpressionChangedAfterItHasBeenCheckedError
    setTimeout(() => {
      this.loadAllData();
    }, 0);
  }

  ngOnDestroy() {
    this.breakpointSubscription?.unsubscribe();
  }

  private setupResponsiveLayout() {
    this.breakpointSubscription = this.breakpointObserver.observe([
      Breakpoints.XSmall,
      Breakpoints.Small,
      Breakpoints.Medium,
    ]).subscribe(_ => {
      this.isMobile = this.breakpointObserver.isMatched(Breakpoints.XSmall);
      this.isTablet = this.breakpointObserver.isMatched(Breakpoints.Small);

      if (this.isMobile) {
        this.quickStatCols = 2;
      } else if (this.isTablet) {
        this.quickStatCols = 4;
      } else {
        this.quickStatCols = 4;
      }
    });
  }

  private loadAllData() {
    this.loadChartData();
    this.loadPredictions();
    this.loadStats();
  }

  private loadChartData() {
    this.isLoadingCharts = true;

    // Daily plates chart
    this.homeService.getPlatesCount().subscribe({
      next: (result) => {
        const labels = result.counts.map(x => new Date(x.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' }));
        const data = result.counts.map(x => x.count);

        if (this.chartsComponent) {
          this.chartsComponent.updateDailyChart(labels, data);
        }
      },
      error: _ => {
        this.isLoadingCharts = false;
      },
    });

    // Most seen plates
    this.homeService.getMostSeenPlates().subscribe({
      next: (result) => {
        this.mostSeenCounts = result.counts.map(x => ({
          name: x.plateNumber,
          value: x.count,
        }));
      },
      error: _ => {
        /* nothing */
      },
    });

    // Hourly distribution chart
    this.homeService.getHourlyStats().subscribe({
      next: (stats) => {
        const labels = stats.map(x => x.displayHour);
        const data = stats.map(x => x.count);

        if (this.chartsComponent) {
          this.chartsComponent.updateHourlyChart(labels, data);
        }
        this.isLoadingCharts = false;
      },
      error: _ => {
        // Create mock data if API doesn't exist yet
        this.createMockHourlyChart();
        this.isLoadingCharts = false;
      },
    });
  }

  private loadPredictions() {
    this.isLoadingPredictions = true;

    this.homeService.getNextExpectedPlate().subscribe({
      next: (prediction) => {
        this.nextExpected = prediction;
      },
      error: _ => {
        /* nothing */
      },
    });

    this.homeService.getUpcomingPredictions(6, 24).subscribe({
      next: (predictions) => {
        this.upcomingPredictions = predictions;
        this.isLoadingPredictions = false;
      },
      error: _ => {
        this.isLoadingPredictions = false;
      },
    });

    this.homeService.getMostPredictablePlates(6).subscribe({
      next: (predictions) => {
        this.predictablePlates = predictions.sort((a, b) => b.confidenceScore - a.confidenceScore);
      },
      error: _ => {
        /* nothing */
      },
    });
  }

  private loadStats() {
    this.isLoadingStats = true;

    this.homeService.getQuickStats().subscribe({
      next: (stats) => {
        this.quickStats = stats;
        this.isLoadingStats = false;
      },
      error: _ => {
        // Generate mock stats if API doesn't exist yet
        this.quickStats = this.generateMockStats();
        this.isLoadingStats = false;
      },
    });
  }



  private createMockHourlyChart() {
    const hours = ['12 AM', '3 AM', '6 AM', '9 AM', '12 PM', '3 PM', '6 PM', '9 PM'];
    const data = hours.map(() => Math.floor(Math.random() * 50) + 10);

    if (this.chartsComponent) {
      this.chartsComponent.updateHourlyChart(hours, data);
    }
  }

  private generateMockStats(): QuickStats {
    return {
      todayCount: Math.floor(Math.random() * 100) + 50,
      weekCount: Math.floor(Math.random() * 500) + 300,
      monthCount: Math.floor(Math.random() * 2000) + 1000,
      uniquePlatesThisWeek: Math.floor(Math.random() * 200) + 100,
      activeCameras: Math.floor(Math.random() * 10) + 5,
      averageDailyPlates: Math.floor(Math.random() * 80) + 40,
    };
  }
}
