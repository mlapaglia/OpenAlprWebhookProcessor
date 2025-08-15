import { Injectable, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { PlateService } from '../../plate.service';
import type { PlateData } from '../../plate-item/plate-item.component';
import type { PlateStatistics, PlateStatisticsData } from '../plateStatistics';
import { Plate } from '../plate';
import { Observable } from 'rxjs';

@Injectable()
export class PlateStatisticsStateService {
  private readonly plateService = inject(PlateService);
  private readonly datePipe = inject(DatePipe);

  private readonly _plateStatistics = signal<PlateStatisticsData[]>([]);
  private readonly _loadingStatistics = signal(false);
  private readonly _loadingStatisticsFailed = signal(false);
  private readonly _statisticsLoaded = signal(false);

  readonly plateStatistics = this._plateStatistics.asReadonly();
  readonly loadingStatistics = this._loadingStatistics.asReadonly();
  readonly loadingStatisticsFailed = this._loadingStatisticsFailed.asReadonly();
  readonly statisticsLoaded = this._statisticsLoaded.asReadonly();

  loadStatistics(plateData: PlateData): Observable<void> {
    if (this._statisticsLoaded() || this._loadingStatistics()) {
      return new Observable(subscriber => subscriber.complete());
    }

    this._loadingStatistics.set(true);
    this._loadingStatisticsFailed.set(false);

    return new Observable(subscriber => {
      this.plateService.getPlateStatistics(plateData.plateNumber).subscribe({
        next: (result) => {
          this._loadingStatistics.set(false);
          this._statisticsLoaded.set(true);
          this._plateStatistics.set(this.buildStatisticsArray(plateData, result));
          subscriber.next();
          subscriber.complete();
        },
        error: () => {
          this._loadingStatistics.set(false);
          this._loadingStatisticsFailed.set(true);
          subscriber.error();
        },
      });
    });
  }

  private buildStatisticsArray(plateData: PlateData, result: PlateStatistics): PlateStatisticsData[] {
    const plate = this.convertPlateDataToPlate(plateData);
    return [
      { key: 'Confidence', value: `${plateData.processedPlateConfidence}%` },
      { key: 'Seen past 90 days', value: result.last90Days.toString() },
      { key: 'Total Seen', value: result.totalSeen.toString() },
      { key: 'First seen', value: this.datePipe.transform(result.firstSeen, 'medium') ?? '' },
      { key: 'Last seen', value: this.datePipe.transform(result.lastSeen, 'medium') ?? '' },
      { key: 'Processing time', value: `${plate.openAlprProcessingTimeMs.toString()}ms` },
      { key: 'Possible plates', value: plate.possiblePlateNumbers },
      { key: 'Region', value: plate.region },
    ];
  }

  private convertPlateDataToPlate(plateData: PlateData): Plate {
    return new Plate({
      id: plateData.id,
      plateNumber: plateData.plateNumber,
      openAlprCameraId: Number(plateData.openAlprCameraId),
      vehicleDescription: plateData.vehicleDescription,
      direction: plateData.direction,
      receivedOn: plateData.receivedOn,
      isAlert: plateData.isAlert,
      isIgnore: plateData.isIgnore,
      isOpen: plateData.isOpen,
      imageUrl: plateData.imageUrl ?? '',
      cropImageUrl: plateData.cropImageUrl ?? '',
      processedPlateConfidence: plateData.processedPlateConfidence ?? 0,
      notes: plateData.notes ?? '',
      canBeEnriched: plateData.canBeEnriched ?? false,
      region: '',
      possiblePlateNumbers: '',
      openAlprProcessingTimeMs: 0,
      alertDescription: '',
    });
  }

  reset() {
    this._plateStatistics.set([]);
    this._loadingStatistics.set(false);
    this._loadingStatisticsFailed.set(false);
    this._statisticsLoaded.set(false);
  }
}
