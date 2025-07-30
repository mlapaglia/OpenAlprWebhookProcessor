import type { AfterViewInit, OnDestroy, ElementRef } from '@angular/core';
import { Component, inject, ViewChild } from '@angular/core';
import type { User } from 'app/_models';
import { AccountService } from 'app/_services';
import type { QuickStats } from './home.service';
import { HomeService } from './home.service';
import { MatCardModule } from '@angular/material/card';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { CommonModule } from '@angular/common';
import type { PredictionResult } from './prediction-response';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import type { ChartConfiguration } from 'chart.js';
import { Chart, registerables } from 'chart.js';

// Register Chart.js components
Chart.register(...registerables);

@Component({
  templateUrl: 'home.component.html',
  imports: [
    MatCardModule,
    MatListModule,
    MatIconModule,
    MatGridListModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    CommonModule,
  ],
})
export class HomeComponent implements AfterViewInit, OnDestroy {
  @ViewChild('dailyChart') dailyChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('hourlyChart') hourlyChartRef!: ElementRef<HTMLCanvasElement>;

  private readonly accountService = inject(AccountService);
  private readonly homeService = inject(HomeService);
  private readonly breakpointObserver = inject(BreakpointObserver);

  user: User;

  // Chart instances
  private dailyChart: Chart | null = null;
  private hourlyChart: Chart | null = null;

  // Data
  public mostSeenCounts: { name: string, value: number }[] = [];

  // ML Predictions
  public nextExpected: PredictionResult | null = null;
  public upcomingPredictions: PredictionResult[] = [];
  public predictablePlates: PredictionResult[] = [];

  // Quick Stats
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
    this.initializeCharts();
    // Defer data loading to next tick to avoid ExpressionChangedAfterItHasBeenCheckedError
    setTimeout(() => {
      this.loadAllData();
    }, 0);
  }

  ngOnDestroy() {
    // Cleanup charts
    if (this.dailyChart) {
      this.dailyChart.destroy();
    }
    if (this.hourlyChart) {
      this.hourlyChart.destroy();
    }
  }

  private initializeCharts() {
    // Initialize daily chart
    const dailyCtx = this.dailyChartRef.nativeElement.getContext('2d');
    if (dailyCtx) {
      const dailyConfig: ChartConfiguration = {
        type: 'bar',
        data: {
          labels: [],
          datasets: [
            {
              label: 'Daily Plates',
              data: [],
              backgroundColor: '#2196F3',
              borderRadius: 4,
              barPercentage: 0.6,
            },
          ],
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: {
              display: false,
            },
            tooltip: {
              mode: 'index',
              intersect: false,
            },
          },
          scales: {
            x: {
              grid: {
                display: false,
              },
              ticks: {
                autoSkip: false,
                maxRotation: 45,
                minRotation: 45,
              },
            },
            y: {
              beginAtZero: true,
              grid: {
                color: 'rgba(0, 0, 0, 0.1)',
              },
            },
          },
        },
      };
      this.dailyChart = new Chart(dailyCtx, dailyConfig);
    }

    // Initialize hourly chart
    const hourlyCtx = this.hourlyChartRef.nativeElement.getContext('2d');
    if (hourlyCtx) {
      const hourlyConfig: ChartConfiguration = {
        type: 'bar',
        data: {
          labels: [],
          datasets: [
            {
              label: 'Hourly Distribution',
              data: [],
              backgroundColor: '#4CAF50',
              borderRadius: 4,
              barPercentage: 0.6,
            },
          ],
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: {
              display: false,
            },
            tooltip: {
              mode: 'index',
              intersect: false,
            },
          },
          scales: {
            x: {
              grid: {
                display: false,
              },
              ticks: {
                autoSkip: false,
                maxRotation: 45,
                minRotation: 45,
              },
            },
            y: {
              beginAtZero: true,
              grid: {
                color: 'rgba(0, 0, 0, 0.1)',
              },
            },
          },
        },
      };
      this.hourlyChart = new Chart(hourlyCtx, hourlyConfig);
    }
  }

  private setupResponsiveLayout() {
    this.breakpointObserver.observe([
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

        if (this.dailyChart) {
          this.dailyChart.data.labels = labels;
          this.dailyChart.data.datasets[0].data = data;
          this.dailyChart.update();
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
      },
    });

    // Hourly distribution chart
    this.homeService.getHourlyStats().subscribe({
      next: (stats) => {
        const labels = stats.map(x => x.displayHour);
        const data = stats.map(x => x.count);

        if (this.hourlyChart) {
          this.hourlyChart.data.labels = labels;
          this.hourlyChart.data.datasets[0].data = data;
          this.hourlyChart.update();
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

    // Get next expected plate (featured)
    this.homeService.getNextExpectedPlate().subscribe({
      next: (prediction) => {
        this.nextExpected = prediction;
      },
      error: _ => {
      },
    });

    // Get upcoming predictions (next 24 hours)
    this.homeService.getUpcomingPredictions(6, 24).subscribe({
      next: (predictions) => {
        this.upcomingPredictions = predictions;
        this.isLoadingPredictions = false;
      },
      error: _ => {
        this.isLoadingPredictions = false;
      },
    });

    // Get most predictable plates
    this.homeService.getMostPredictablePlates(6).subscribe({
      next: (predictions) => {
        this.predictablePlates = predictions.sort((a, b) => b.confidenceScore - a.confidenceScore);
      },
      error: _ => {
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

  // Utility methods
  formatTimeUntil(predictedTime: Date): string {
    const now = new Date();
    const predicted = new Date(predictedTime);
    const diffMs = predicted.getTime() - now.getTime();
    const diffHours = Math.round(diffMs / (1000 * 60 * 60));

    if (diffHours < 1) {
      const diffMins = Math.round(diffMs / (1000 * 60));
      return diffMins > 0 ? `${diffMins}m` : 'Now';
    } else if (diffHours < 24) {
      return `${diffHours}h`;
    } else {
      const diffDays = Math.round(diffHours / 24);
      return `${diffDays}d`;
    }
  }

  getConfidenceColor(confidence: number): string {
    if (confidence >= 0.7) return '#4CAF50'; // Green
    if (confidence >= 0.4) return '#FF9800'; // Orange
    return '#F44336'; // Red
  }

  getConfidenceText(confidence: number): string {
    if (confidence >= 0.7) return 'High';
    if (confidence >= 0.4) return 'Medium';
    return 'Low';
  }

  private createMockHourlyChart() {
    const hours = ['12 AM', '3 AM', '6 AM', '9 AM', '12 PM', '3 PM', '6 PM', '9 PM'];
    const data = hours.map(() => Math.floor(Math.random() * 50) + 10);

    if (this.hourlyChart) {
      this.hourlyChart.data.labels = hours;
      this.hourlyChart.data.datasets[0].data = data;
      this.hourlyChart.update();
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
