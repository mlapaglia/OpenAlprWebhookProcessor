import { Component, input, ChangeDetectionStrategy } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import type { PlateStatisticsData } from '../plateStatistics';

@Component({
  selector: 'app-plate-statistics',
  templateUrl: './plate-statistics.component.html',
  styleUrls: ['./plate-statistics.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports:
  [MatCardModule, MatProgressSpinnerModule, MatIconModule, MatTableModule],
})
export class PlateStatisticsComponent {
  readonly plateStatistics = input<PlateStatisticsData[]>([]);
  readonly loadingStatistics = input<boolean>(false);
  readonly loadingStatisticsFailed = input<boolean>(false);

  public displayedColumns: string[] = ['key', 'value'];
}
