import { DatePipe } from '@angular/common';
import { Component, Input, inject, type OnChanges, type OnDestroy } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { Subscription } from 'rxjs';
import { PlateService } from '../plate.service';
import type { Plate } from './plate';
import type { PlateStatisticsData } from './plateStatistics';

@Component({
  selector: 'app-plate-statistics',
  templateUrl: './plate-statistics.component.html',
  imports:
  [MatCardModule, MatProgressSpinnerModule, MatIconModule, MatTableModule],
})
export class PlateStatisticsComponent implements OnChanges, OnDestroy {
  private readonly plateService = inject(PlateService);
  private readonly datePipe = inject(DatePipe);
  private readonly statisticsSubscription = new Subscription();

  @Input() plate: Plate;
  @Input() isVisible: boolean;

  public loadingStatistics: boolean;
  public loadingStatisticsFailed: boolean;
  public plateStatistics: PlateStatisticsData[] = [];
  public displayedColumns: string[] = ['key', 'value'];

  ngOnChanges() {
    if (this.isVisible && this.plateStatistics.length === 0) {
      this.getPlateStatistics();
    }
  }

  ngOnDestroy() {
    this.statisticsSubscription.unsubscribe();
  }

  private getPlateStatistics() {
    this.loadingStatistics = true;
    this.statisticsSubscription.closed = false;
    this.statisticsSubscription.add(this.plateService.getPlateStatistics(this.plate.plateNumber).subscribe((result) => {
      this.loadingStatistics = false;
      this.loadingStatisticsFailed = false;

      this.plateStatistics.push({
        key: 'Confidence',
        value: `${this.plate.processedPlateConfidence}%`,
      });

      this.plateStatistics.push({
        key: 'Seen past 90 days',
        value: result.last90Days.toString(),
      });

      this.plateStatistics.push({
        key: 'Total Seen',
        value: result.totalSeen.toString(),
      });

      this.plateStatistics.push({
        key: 'First seen',
        value: this.datePipe.transform(result.firstSeen, 'medium') ?? '',
      });

      this.plateStatistics.push({
        key: 'Last seen',
        value: this.datePipe.transform(result.lastSeen, 'medium') ?? '',
      });

      this.plateStatistics.push({
        key: 'Processing time',
        value: `${this.plate.openAlprProcessingTimeMs.toString()}ms`,
      });

      this.plateStatistics.push({
        key: 'Possible plates',
        value: this.plate.possiblePlateNumbers,
      });

      this.plateStatistics.push({
        key: 'Region',
        value: this.plate.region,
      });
    },
    () => {
      this.loadingStatistics = false;
      this.loadingStatisticsFailed = true;
    }));
  }
}
