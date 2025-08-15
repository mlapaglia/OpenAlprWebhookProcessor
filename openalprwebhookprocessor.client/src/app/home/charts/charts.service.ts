import { Injectable } from '@angular/core';
import { Chart, type ChartConfiguration, registerables } from 'chart.js';

Chart.register(...registerables);

export interface ChartData {
  labels: string[];
  data: number[];
}

@Injectable({
  providedIn: 'root',
})
export class ChartService {

  createDailyChart(canvas: HTMLCanvasElement): Chart | null {
    const ctx = canvas.getContext('2d');
    if (!ctx) return null;

    const config: ChartConfiguration = {
      type: 'bar',
      data: this.createChartData('Daily Plates', '#2196F3'),
      options: this.getChartOptions(),
    };

    return new Chart(ctx, config);
  }

  createHourlyChart(canvas: HTMLCanvasElement): Chart | null {
    const ctx = canvas.getContext('2d');
    if (!ctx) return null;

    const config: ChartConfiguration = {
      type: 'bar',
      data: this.createChartData('Hourly Distribution', '#4CAF50'),
      options: this.getChartOptions(),
    };

    return new Chart(ctx, config);
  }

  private createChartData(label: string, backgroundColor: string) {
    return {
      labels: [],
      datasets: [
        {
          label,
          data: [],
          backgroundColor,
          borderRadius: 4,
          barPercentage: 0.6,
        },
      ],
    };
  }

  private getChartOptions() {
    return {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: false,
        },
        tooltip: {
          mode: 'index' as const,
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
    };
  }

  updateChart(chart: Chart | null, chartData: ChartData): void {
    if (!chart) return;

    chart.data.labels = chartData.labels;
    chart.data.datasets[0].data = chartData.data;
    chart.update();
  }

  destroyChart(chart: Chart | null): void {
    if (chart) {
      chart.destroy();
    }
  }
}
