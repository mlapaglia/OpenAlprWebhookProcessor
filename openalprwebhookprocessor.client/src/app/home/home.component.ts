import { Component, inject, type AfterViewInit, ChangeDetectionStrategy, viewChild } from '@angular/core';
import type { User } from 'app/_models';
import { AccountService } from 'app/_services';
import { CommonModule } from '@angular/common';
import { PredictionsSectionComponent } from './predictions-section/predictions-section.component';
import { QuickStatsComponent } from './quick-stats/quick-stats.component';
import { ChartsComponent } from './charts/charts.component';
import { MostSeenPlatesComponent } from './most-seen/most-seen-plates.component';
import { LayoutService, type LayoutState } from './layout.service';
import { HomeDataService, type HomeData } from './home-data.service';
import type { PredictionResult } from './prediction-response';
import type { QuickStats } from './home.service';
import { OnPushBaseComponent } from '../_helpers/onpush-base.component';

@Component({
  templateUrl: 'home.component.html',
  styleUrls: ['./home.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    PredictionsSectionComponent,
    QuickStatsComponent,
    ChartsComponent,
    MostSeenPlatesComponent,
  ],
})
export class HomeComponent extends OnPushBaseComponent implements AfterViewInit {
  readonly chartsComponent = viewChild<ChartsComponent>(ChartsComponent);

  private readonly accountService = inject(AccountService);
  private readonly layoutService = inject(LayoutService);
  private readonly homeDataService = inject(HomeDataService);

  user: User;

  public mostSeenCounts: { name: string, value: number }[] = [];
  public nextExpected: PredictionResult | null = null;
  public upcomingPredictions: PredictionResult[] = [];
  public predictablePlates: PredictionResult[] = [];
  public quickStats: QuickStats | null = null;

  public isLoadingPredictions = false;
  public isLoadingStats = false;
  public isLoadingCharts = false;

  public isMobile = false;
  public isTablet = false;
  public quickStatCols = 4;

  constructor() {
    super();
    this.user = this.accountService.userValue;
  }

  ngAfterViewInit() {
    // Setup responsive layout after view initialization to avoid FOUC
    this.setupResponsiveLayout();

    setTimeout(() => {
      this.loadAllData();
    }, 0);
  }

  override ngOnDestroy() {
    super.ngOnDestroy();
  }

  private setupResponsiveLayout() {
    this.subscribeAndMarkForCheck(
      this.layoutService.getLayoutState(),
      (layoutState: LayoutState) => {
        this.isMobile = layoutState.isMobile;
        this.isTablet = layoutState.isTablet;
        this.quickStatCols = layoutState.quickStatCols;
      },
    );
  }

  private loadAllData() {
    this.setLoadingStates(true);

    this.subscribeAndMarkForCheck(
      this.homeDataService.loadAllData(),
      (data: HomeData) => {
        this.quickStats = data.quickStats;
        this.nextExpected = data.nextExpected;
        this.upcomingPredictions = data.upcomingPredictions;
        this.predictablePlates = data.predictablePlates;
        this.mostSeenCounts = data.mostSeenCounts;

        const chartComponent = this.chartsComponent();
        if (chartComponent && data.hourlyStats.length > 0) {
          const labels = data.hourlyStats.map(x => x.displayHour);
          const chartData = data.hourlyStats.map(x => x.count);
          chartComponent.updateHourlyChart(labels, chartData);
        }

        if (chartComponent && data.dailyStats.length > 0) {
          const dailyLabels = data.dailyStats.map(x => x.date);
          const dailyData = data.dailyStats.map(x => x.count);
          chartComponent.updateDailyChart(dailyLabels, dailyData);
        }

        this.setLoadingStates(false);
      },
      () => {
        this.setLoadingStates(false);
      },
    );
  }

  private setLoadingStates(loading: boolean) {
    this.isLoadingStats = loading;
    this.isLoadingPredictions = loading;
    this.isLoadingCharts = loading;
    this.markForCheck();
  }
}
