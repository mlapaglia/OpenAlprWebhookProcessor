import type { OnDestroy, ElementRef } from '@angular/core';
import { Component, inject, type AfterViewInit, ChangeDetectionStrategy, viewChild, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ChartService, type ChartData } from './charts.service';
import { OnPushBaseComponent } from '../../_helpers/onpush-base.component';
import type { Chart } from 'chart.js';

@Component({
  selector: 'app-charts',
  templateUrl: './charts.component.html',
  styleUrls: ['./charts.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    MatCardModule,
    MatProgressSpinnerModule,
  ],
})
export class ChartsComponent extends OnPushBaseComponent implements AfterViewInit, OnDestroy {
  readonly dailyChartRef = viewChild<ElementRef<HTMLCanvasElement>>('dailyChart');
  readonly hourlyChartRef = viewChild<ElementRef<HTMLCanvasElement>>('hourlyChart');

  readonly isLoadingCharts = input(false);

  private readonly chartService = inject(ChartService);
  private dailyChart: Chart | null = null;
  private hourlyChart: Chart | null = null;

  ngAfterViewInit() {
    this.initializeCharts();
  }

  override ngOnDestroy() {
    this.chartService.destroyChart(this.dailyChart);
    this.chartService.destroyChart(this.hourlyChart);
    super.ngOnDestroy();
  }

  public updateDailyChart(labels: string[], data: number[]) {
    const chartData: ChartData = { labels, data };
    this.chartService.updateChart(this.dailyChart, chartData);
  }

  public updateHourlyChart(labels: string[], data: number[]) {
    const chartData: ChartData = { labels, data };
    this.chartService.updateChart(this.hourlyChart, chartData);
  }

  private initializeCharts() {
    const dailyElement = this.dailyChartRef()?.nativeElement;
    const hourlyElement = this.hourlyChartRef()?.nativeElement;

    if (dailyElement) {
      this.dailyChart = this.chartService.createDailyChart(dailyElement);
    }

    if (hourlyElement) {
      this.hourlyChart = this.chartService.createHourlyChart(hourlyElement);
    }
  }
}
