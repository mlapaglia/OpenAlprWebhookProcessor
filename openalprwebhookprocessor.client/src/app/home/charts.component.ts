import type { AfterViewInit, OnDestroy, ElementRef } from '@angular/core';
import { Component, Input, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import type { ChartConfiguration } from 'chart.js';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-charts',
  templateUrl: './charts.component.html',
  styleUrls: ['./charts.component.css'],
  imports: [
    CommonModule,
    MatCardModule,
    MatProgressSpinnerModule,
  ],
})
export class ChartsComponent implements AfterViewInit, OnDestroy {
  @ViewChild('dailyChart') dailyChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('hourlyChart') hourlyChartRef!: ElementRef<HTMLCanvasElement>;

  @Input() isLoadingCharts = false;

  private dailyChart: Chart | null = null;
  private hourlyChart: Chart | null = null;

  ngAfterViewInit() {
    this.initializeCharts();
  }

  ngOnDestroy() {
    if (this.dailyChart) {
      this.dailyChart.destroy();
    }
    if (this.hourlyChart) {
      this.hourlyChart.destroy();
    }
  }

  public updateDailyChart(labels: string[], data: number[]) {
    if (this.dailyChart) {
      this.dailyChart.data.labels = labels;
      this.dailyChart.data.datasets[0].data = data;
      this.dailyChart.update();
    }
  }

  public updateHourlyChart(labels: string[], data: number[]) {
    if (this.hourlyChart) {
      this.hourlyChart.data.labels = labels;
      this.hourlyChart.data.datasets[0].data = data;
      this.hourlyChart.update();
    }
  }

  private initializeCharts() {
    this.initializeDailyChart();
    this.initializeHourlyChart();
  }

  private initializeDailyChart() {
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
  }

  private initializeHourlyChart() {
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
}