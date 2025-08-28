import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { HomeService, type HourlyStats, type QuickStats } from './home.service';
import type { DayCounts } from './plateCountResponse';
import type { MostSeenCounts } from './most-seen/mostSeenResponse';
import type { PredictionResult } from './prediction-response';

describe('HomeService', () => {
  let service: HomeService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [HomeService],
    });
    service = TestBed.inject(HomeService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('getPlatesCount', () => {
    it('should fetch plate counts', () => {
      const mockDayCounts: DayCounts = {
        counts: [
          { date: new Date('2023-01-01'), count: 10 },
          { date: new Date('2023-01-02'), count: 15 },
        ],
        weeklyUniqueCounts: [],
      };

      service.getPlatesCount().subscribe(result => {
        expect(result).toEqual(mockDayCounts);
      });

      const req = httpMock.expectOne('/api/licensePlates/counts');
      expect(req.request.method).toBe('GET');
      req.flush(mockDayCounts);
    });
  });

  describe('getMostSeenPlates', () => {
    it('should fetch most seen plates', () => {
      const mockMostSeenCounts: MostSeenCounts = {
        counts: [
          { plateNumber: 'ABC123', count: 25 },
          { plateNumber: 'XYZ789', count: 20 },
        ],
      };

      service.getMostSeenPlates().subscribe(result => {
        expect(result).toEqual(mockMostSeenCounts);
      });

      const req = httpMock.expectOne('/api/licensePlates/most-seen');
      expect(req.request.method).toBe('GET');
      req.flush(mockMostSeenCounts);
    });
  });

  describe('getUpcomingPredictions', () => {
    it('should fetch upcoming predictions with default parameters', () => {
      const mockPredictions: PredictionResult[] = [
        {
          licensePlate: 'ABC123',
          confidenceScore: 0.95,
          predictedNextSeen: new Date(),
          predictedHours: 24,
          totalHistoricalVisits: 10,
          averageTimeBetweenVisits: 2.5,
          lastSeen: new Date(),
          modelVersion: '1.0',
          predictionMadeAt: new Date(),
        },
        {
          licensePlate: 'XYZ789',
          confidenceScore: 0.87,
          predictedNextSeen: new Date(),
          predictedHours: 24,
          totalHistoricalVisits: 8,
          averageTimeBetweenVisits: 3.0,
          lastSeen: new Date(),
          modelVersion: '1.0',
          predictionMadeAt: new Date(),
        },
      ];

      service.getUpcomingPredictions().subscribe(result => {
        expect(result).toEqual(mockPredictions);
      });

      const req = httpMock.expectOne('/api/machinelearning/predict/top?count=10&withinHours=24');
      expect(req.request.method).toBe('GET');
      req.flush(mockPredictions);
    });

    it('should fetch upcoming predictions with custom parameters', () => {
      const mockPredictions: PredictionResult[] = [
        {
          licensePlate: 'ABC123',
          confidenceScore: 0.95,
          predictedNextSeen: new Date(),
          predictedHours: 24,
          totalHistoricalVisits: 10,
          averageTimeBetweenVisits: 2.5,
          lastSeen: new Date(),
          modelVersion: '1.0',
          predictionMadeAt: new Date(),
        },
      ];

      service.getUpcomingPredictions(5, 12).subscribe(result => {
        expect(result).toEqual(mockPredictions);
      });

      const req = httpMock.expectOne('/api/machinelearning/predict/top?count=5&withinHours=12');
      expect(req.request.method).toBe('GET');
      req.flush(mockPredictions);
    });
  });

  describe('getMostPredictablePlates', () => {
    it('should fetch most predictable plates', () => {
      const mockPredictions: PredictionResult[] = [
        {
          licensePlate: 'ABC123',
          confidenceScore: 0.95,
          predictedNextSeen: new Date(),
          predictedHours: 24,
          totalHistoricalVisits: 10,
          averageTimeBetweenVisits: 2.5,
          lastSeen: new Date(),
          modelVersion: '1.0',
          predictionMadeAt: new Date(),
        },
        {
          licensePlate: 'XYZ789',
          confidenceScore: 0.87,
          predictedNextSeen: new Date(),
          predictedHours: 24,
          totalHistoricalVisits: 8,
          averageTimeBetweenVisits: 3.0,
          lastSeen: new Date(),
          modelVersion: '1.0',
          predictionMadeAt: new Date(),
        },
      ];

      service.getMostPredictablePlates(5).subscribe(result => {
        expect(result).toEqual(mockPredictions);
      });

      const req = httpMock.expectOne('/api/machinelearning/predict/top?count=5&withinHours=168');
      expect(req.request.method).toBe('GET');
      req.flush(mockPredictions);
    });
  });

  describe('getNextExpectedPlate', () => {
    it('should return first prediction when results available', () => {
      const mockPredictions: PredictionResult[] = [
        {
          licensePlate: 'ABC123',
          confidenceScore: 0.95,
          predictedNextSeen: new Date(),
          predictedHours: 24,
          totalHistoricalVisits: 10,
          averageTimeBetweenVisits: 2.5,
          lastSeen: new Date(),
          modelVersion: '1.0',
          predictionMadeAt: new Date(),
        },
        {
          licensePlate: 'XYZ789',
          confidenceScore: 0.87,
          predictedNextSeen: new Date(),
          predictedHours: 24,
          totalHistoricalVisits: 8,
          averageTimeBetweenVisits: 3.0,
          lastSeen: new Date(),
          modelVersion: '1.0',
          predictionMadeAt: new Date(),
        },
      ];

      service.getNextExpectedPlate().subscribe(result => {
        expect(result).toEqual(mockPredictions[0]);
      });

      const req = httpMock.expectOne('/api/machinelearning/predict/top?count=1&withinHours=24');
      expect(req.request.method).toBe('GET');
      req.flush(mockPredictions);
    });

    it('should return null when no predictions available', () => {
      service.getNextExpectedPlate().subscribe(result => {
        expect(result).toBeNull();
      });

      const req = httpMock.expectOne('/api/machinelearning/predict/top?count=1&withinHours=24');
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });
  });

  describe('getHourlyStats', () => {
    it('should fetch and format hourly stats', () => {
      const mockResponse = {
        counts: [
          { hour: 0, count: 5 },
          { hour: 9, count: 15 },
          { hour: 12, count: 20 },
          { hour: 15, count: 25 },
          { hour: 23, count: 8 },
        ],
      };

      const expectedStats: HourlyStats[] = [
        { hour: 0, count: 5, displayHour: '12 AM' },
        { hour: 9, count: 15, displayHour: '9 AM' },
        { hour: 12, count: 20, displayHour: '12 PM' },
        { hour: 15, count: 25, displayHour: '3 PM' },
        { hour: 23, count: 8, displayHour: '11 PM' },
      ];

      service.getHourlyStats().subscribe(result => {
        expect(result).toEqual(expectedStats);
      });

      const req = httpMock.expectOne('/api/licensePlates/stats/hourly');
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });
  });

  describe('getQuickStats', () => {
    it('should fetch quick stats', () => {
      const mockQuickStats: QuickStats = {
        todayCount: 50,
        weekCount: 300,
        monthCount: 1200,
        uniquePlatesThisWeek: 150,
        activeCameras: 5,
        averageDailyPlates: 40,
      };

      service.getQuickStats().subscribe(result => {
        expect(result).toEqual(mockQuickStats);
      });

      const req = httpMock.expectOne('/api/licensePlates/stats/quick');
      expect(req.request.method).toBe('GET');
      req.flush(mockQuickStats);
    });
  });

  describe('formatHour', () => {
    it('should format midnight correctly', () => {
      const result = (service as any).formatHour(0);
      expect(result).toBe('12 AM');
    });

    it('should format noon correctly', () => {
      const result = (service as any).formatHour(12);
      expect(result).toBe('12 PM');
    });

    it('should format morning hours correctly', () => {
      expect((service as any).formatHour(1)).toBe('1 AM');
      expect((service as any).formatHour(9)).toBe('9 AM');
      expect((service as any).formatHour(11)).toBe('11 AM');
    });

    it('should format afternoon/evening hours correctly', () => {
      expect((service as any).formatHour(13)).toBe('1 PM');
      expect((service as any).formatHour(18)).toBe('6 PM');
      expect((service as any).formatHour(23)).toBe('11 PM');
    });
  });
});
