import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { HomeDataService } from './home-data.service';
import { HomeService } from './home.service';

describe('HomeDataService', () => {
  let service: HomeDataService;
  let mockHomeService: jasmine.SpyObj<HomeService>;

  beforeEach(() => {
    const homeServiceSpy = jasmine.createSpyObj('HomeService', [
      'getQuickStats',
      'getNextExpectedPlate',
      'getUpcomingPredictions',
      'getMostPredictablePlates',
      'getMostSeenPlates',
      'getHourlyStats',
      'getPlatesCount',
    ]);

    TestBed.configureTestingModule({
      providers: [
        HomeDataService,
        { provide: HomeService, useValue: homeServiceSpy },
      ],
    });

    service = TestBed.inject(HomeDataService);
    mockHomeService = TestBed.inject(HomeService) as jasmine.SpyObj<HomeService>;
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should load all data successfully', (done) => {
    const mockQuickStats = {
      todayCount: 10,
      weekCount: 50,
      monthCount: 200,
      uniquePlatesThisWeek: 25,
      activeCameras: 5,
      averageDailyPlates: 15,
    };

    const mockPredictions = [
      {
        licensePlate: 'ABC123',
        confidenceScore: 0.95,
        predictedNextSeen: new Date(),
        predictedHours: 24,
        totalHistoricalVisits: 10,
        averageTimeBetweenVisits: 2.5,
        lastSeen: new Date(),
        modelVersion: 'v1.0',
        predictionMadeAt: new Date(),
      },
      {
        licensePlate: 'XYZ789',
        confidenceScore: 0.88,
        predictedNextSeen: new Date(),
        predictedHours: 48,
        totalHistoricalVisits: 5,
        averageTimeBetweenVisits: 3.0,
        lastSeen: new Date(),
        modelVersion: 'v1.0',
        predictionMadeAt: new Date(),
      },
    ];

    const mockMostSeen = {
      counts: [
        { plateNumber: 'ABC123', count: 5 },
        { plateNumber: 'DEF456', count: 3 },
      ],
    };

    const mockHourlyStats = [
      { hour: 0, displayHour: '12 AM', count: 10 },
      { hour: 1, displayHour: '1 AM', count: 5 },
    ];

    const mockDailyStats = {
      counts: [
        { date: new Date('2024-01-01T12:00:00'), count: 15 },
        { date: new Date('2024-01-02T12:00:00'), count: 20 },
      ],
      weeklyUniqueCounts: [
        { date: new Date('2024-01-01T12:00:00'), count: 10 },
        { date: new Date('2024-01-02T12:00:00'), count: 12 },
      ],
    };

    mockHomeService.getQuickStats.and.returnValue(of(mockQuickStats));
    mockHomeService.getNextExpectedPlate.and.returnValue(of(null));
    mockHomeService.getUpcomingPredictions.and.returnValue(of([]));
    mockHomeService.getMostPredictablePlates.and.returnValue(of(mockPredictions));
    mockHomeService.getMostSeenPlates.and.returnValue(of(mockMostSeen));
    mockHomeService.getHourlyStats.and.returnValue(of(mockHourlyStats));
    mockHomeService.getPlatesCount.and.returnValue(of(mockDailyStats));

    service.loadAllData().subscribe(data => {
      expect(data.quickStats).toEqual(mockQuickStats);
      expect(data.nextExpected).toBeNull();
      expect(data.upcomingPredictions).toEqual([]);
      expect(data.predictablePlates).toEqual(mockPredictions.sort((a, b) => b.confidenceScore - a.confidenceScore));
      expect(data.mostSeenCounts).toEqual([
        { name: 'ABC123', value: 5 },
        { name: 'DEF456', value: 3 },
      ]);
      expect(data.hourlyStats).toEqual(mockHourlyStats);
      expect(data.dailyStats).toEqual([
        { date: 'Jan 1', count: 15 },
        { date: 'Jan 2', count: 20 },
      ]);
      done();
    });
  });

  it('should sort predictable plates by confidence score descending', (done) => {
    const mockPredictions = [
      {
        licensePlate: 'ABC123',
        confidenceScore: 0.75,
        predictedNextSeen: new Date(),
        predictedHours: 24,
        totalHistoricalVisits: 10,
        averageTimeBetweenVisits: 2.5,
        lastSeen: new Date(),
        modelVersion: 'v1.0',
        predictionMadeAt: new Date(),
      },
      {
        licensePlate: 'XYZ789',
        confidenceScore: 0.95,
        predictedNextSeen: new Date(),
        predictedHours: 48,
        totalHistoricalVisits: 5,
        averageTimeBetweenVisits: 3.0,
        lastSeen: new Date(),
        modelVersion: 'v1.0',
        predictionMadeAt: new Date(),
      },
      {
        licensePlate: 'DEF456',
        confidenceScore: 0.85,
        predictedNextSeen: new Date(),
        predictedHours: 36,
        totalHistoricalVisits: 8,
        averageTimeBetweenVisits: 2.8,
        lastSeen: new Date(),
        modelVersion: 'v1.0',
        predictionMadeAt: new Date(),
      },
    ];

    mockHomeService.getQuickStats.and.returnValue(of({} as any));
    mockHomeService.getNextExpectedPlate.and.returnValue(of(null));
    mockHomeService.getUpcomingPredictions.and.returnValue(of([]));
    mockHomeService.getMostPredictablePlates.and.returnValue(of(mockPredictions));
    mockHomeService.getMostSeenPlates.and.returnValue(of({ counts: [] }));
    mockHomeService.getHourlyStats.and.returnValue(of([]));
    mockHomeService.getPlatesCount.and.returnValue(of({ counts: [], weeklyUniqueCounts: [] }));

    service.loadAllData().subscribe(data => {
      expect(data.predictablePlates[0].confidenceScore).toBe(0.95);
      expect(data.predictablePlates[1].confidenceScore).toBe(0.85);
      expect(data.predictablePlates[2].confidenceScore).toBe(0.75);
      done();
    });
  });
});
