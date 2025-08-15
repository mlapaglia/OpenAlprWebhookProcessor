import { TestBed } from '@angular/core/testing';
import { DatePipe } from '@angular/common';
import { Subject } from 'rxjs';
import { PlateStatisticsStateService } from './plate-statistics-state.service';
import { PlateService } from '../../plate.service';
import type { PlateData } from '../../plate-item/plate-item.component';

describe('PlateStatisticsStateService', () => {
  let service: PlateStatisticsStateService;
  let plateService: jasmine.SpyObj<PlateService>;

  const mockPlateData: PlateData = {
    id: '1',
    plateNumber: 'ABC123',
    processedPlateConfidence: 95.5,
    receivedOn: new Date('2025-01-01T12:00:00Z'),
    openAlprCameraId: 1,
    vehicleDescription: 'sedan',
    direction: 1,
    isAlert: false,
    isIgnore: false,
    isOpen: true,
    imageUrl: 'test.jpg',
    cropImageUrl: 'crop.jpg',
  };

  const mockStatisticsResult = {
    last90Days: 5,
    totalSeen: 25,
    firstSeen: '2024-06-01T10:00:00Z',
    lastSeen: '2025-01-01T12:00:00Z',
  };

  beforeEach(() => {
    const plateServiceSpy = jasmine.createSpyObj('PlateService', ['getPlateStatistics']);

    TestBed.configureTestingModule({
      providers: [
        PlateStatisticsStateService,
        { provide: PlateService, useValue: plateServiceSpy },
        DatePipe,
      ],
    });

    service = TestBed.inject(PlateStatisticsStateService);
    plateService = TestBed.inject(PlateService) as jasmine.SpyObj<PlateService>;
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should initialize with default values', () => {
    expect(service.plateStatistics()).toEqual([]);
    expect(service.loadingStatistics()).toBe(false);
    expect(service.loadingStatisticsFailed()).toBe(false);
    expect(service.statisticsLoaded()).toBe(false);
  });

  describe('loadStatistics', () => {
    it('should not load if already loaded', (done) => {
      service['_statisticsLoaded'].set(true);

      service.loadStatistics(mockPlateData).subscribe({
        complete: () => {
          expect(plateService.getPlateStatistics).not.toHaveBeenCalled();
          done();
        },
      });
    });

    it('should not load if already loading', (done) => {
      service['_loadingStatistics'].set(true);

      service.loadStatistics(mockPlateData).subscribe({
        complete: () => {
          expect(plateService.getPlateStatistics).not.toHaveBeenCalled();
          done();
        },
      });
    });

    it('should load statistics successfully', (done) => {
      const subject = new Subject<any>();
      plateService.getPlateStatistics.and.returnValue(subject.asObservable());

      expect(service.loadingStatistics()).toBe(false);
      expect(service.loadingStatisticsFailed()).toBe(false);

      service.loadStatistics(mockPlateData).subscribe({
        complete: () => {
          expect(service.loadingStatistics()).toBe(false);
          expect(service.statisticsLoaded()).toBe(true);
          expect(service.loadingStatisticsFailed()).toBe(false);

          const stats = service.plateStatistics();
          expect(stats.length).toBe(8);
          expect(stats[0].key).toBe('Confidence');
          expect(stats[0].value).toBe('95.5%');
          expect(stats[1].key).toBe('Seen past 90 days');
          expect(stats[1].value).toBe('5');
          expect(stats[2].key).toBe('Total Seen');
          expect(stats[2].value).toBe('25');
          expect(stats[3].key).toBe('First seen');
          expect(stats[4].key).toBe('Last seen');
          expect(stats[5].key).toBe('Processing time');
          expect(stats[6].key).toBe('Possible plates');
          expect(stats[7].key).toBe('Region');

          done();
        },
      });

      expect(service.loadingStatistics()).toBe(true);
      expect(plateService.getPlateStatistics).toHaveBeenCalledWith('ABC123');

      subject.next(mockStatisticsResult);
      subject.complete();
    });

    it('should handle loading errors', (done) => {
      const subject = new Subject<any>();
      plateService.getPlateStatistics.and.returnValue(subject.asObservable());

      expect(service.loadingStatistics()).toBe(false);
      expect(service.loadingStatisticsFailed()).toBe(false);

      service.loadStatistics(mockPlateData).subscribe({
        error: () => {
          expect(service.loadingStatistics()).toBe(false);
          expect(service.loadingStatisticsFailed()).toBe(true);
          expect(service.statisticsLoaded()).toBe(false);
          expect(service.plateStatistics()).toEqual([]);
          done();
        },
      });

      expect(service.loadingStatistics()).toBe(true);

      subject.error(new Error('Load failed'));
    });
  });

  describe('reset', () => {
    it('should reset all state to initial values', () => {
      service['_plateStatistics'].set([{ key: 'test', value: 'test' }]);
      service['_loadingStatistics'].set(true);
      service['_loadingStatisticsFailed'].set(true);
      service['_statisticsLoaded'].set(true);

      service.reset();

      expect(service.plateStatistics()).toEqual([]);
      expect(service.loadingStatistics()).toBe(false);
      expect(service.loadingStatisticsFailed()).toBe(false);
      expect(service.statisticsLoaded()).toBe(false);
    });
  });
});
