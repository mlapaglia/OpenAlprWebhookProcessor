import { TestBed } from '@angular/core/testing'
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing'
import { provideHttpClient } from '@angular/common/http'
import { HomeService, HourlyStats, QuickStats, GetHourlyStatsResponse } from './home.service'
import { DayCounts } from './plateCountResponse'
import { MostSeenCounts } from './mostSeenResponse'
import { PredictionResult } from './prediction-response'

describe('HomeService', () => {
  let service: HomeService
  let httpMock: HttpTestingController

  // Helper function to create a valid PredictionResult
  const createPredictionResult = (overrides: Partial<PredictionResult> = {}): PredictionResult => {
    const baseDate = new Date('2024-01-01T09:00:00')
    return {
      licensePlate: 'ABC123',
      predictedNextSeen: new Date('2024-01-01T11:00:00'),
      predictedHours: 2,
      confidenceScore: 0.85,
      totalHistoricalVisits: 25,
      averageTimeBetweenVisits: 48,
      lastSeen: baseDate,
      modelVersion: '1.0',
      predictionMadeAt: new Date('2024-01-01T09:30:00'),
      ...overrides
    }
  }

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        HomeService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    })
    service = TestBed.inject(HomeService)
    httpMock = TestBed.inject(HttpTestingController)
  })

  afterEach(() => {
    httpMock.verify() // Ensure no outstanding HTTP requests
  })

  it('should be created', () => {
    expect(service).toBeTruthy()
  })

  describe('getPlatesCount', () => {
    it('should return daily plate counts', () => {
      const mockResponse: DayCounts = {
        counts: [
          { date: new Date('2024-01-01'), count: 150 },
          { date: new Date('2024-01-02'), count: 175 },
          { date: new Date('2024-01-03'), count: 200 }
        ],
        weeklyUniqueCounts: [
          { date: new Date('2024-01-01'), count: 45 },
          { date: new Date('2024-01-02'), count: 52 }
        ]
      }

      service.getPlatesCount().subscribe(result => {
        expect(result).toEqual(mockResponse)
        expect(result.counts.length).toBe(3)
        expect(result.counts[0].count).toBe(150)
      })

      const req = httpMock.expectOne('/api/licensePlates/counts')
      expect(req.request.method).toBe('GET')
      req.flush(mockResponse)
    })

    it('should handle empty response', () => {
      const mockResponse: DayCounts = {
        counts: [],
        weeklyUniqueCounts: []
      }

      service.getPlatesCount().subscribe(result => {
        expect(result.counts).toEqual([])
      })

      const req = httpMock.expectOne('/api/licensePlates/counts')
      req.flush(mockResponse)
    })
  })

  describe('getMostSeenPlates', () => {
    it('should return most seen plates', () => {
      const mockResponse: MostSeenCounts = {
        counts: [
          { plateNumber: 'ABC123', count: 45 },
          { plateNumber: 'XYZ789', count: 38 },
          { plateNumber: 'DEF456', count: 32 }
        ]
      }

      service.getMostSeenPlates().subscribe(result => {
        expect(result).toEqual(mockResponse)
        expect(result.counts.length).toBe(3)
        expect(result.counts[0].plateNumber).toBe('ABC123')
      })

      const req = httpMock.expectOne('/api/licensePlates/most-seen')
      expect(req.request.method).toBe('GET')
      req.flush(mockResponse)
    })
  })

  describe('getUpcomingPredictions', () => {
    it('should return upcoming predictions with default parameters', () => {
      const mockResponse: PredictionResult[] = [
        createPredictionResult({ licensePlate: 'ABC123' }),
        createPredictionResult({ licensePlate: 'XYZ789', confidenceScore: 0.72 })
      ]

      service.getUpcomingPredictions().subscribe(result => {
        expect(result).toEqual(mockResponse)
        expect(result.length).toBe(2)
      })

      const req = httpMock.expectOne('/api/machinelearning/predict/top?count=10&withinHours=24')
      expect(req.request.method).toBe('GET')
      req.flush(mockResponse)
    })

    it('should return upcoming predictions with custom parameters', () => {
      const mockResponse: PredictionResult[] = []

      service.getUpcomingPredictions(5, 48).subscribe(result => {
        expect(result).toEqual(mockResponse)
      })

      const req = httpMock.expectOne('/api/machinelearning/predict/top?count=5&withinHours=48')
      expect(req.request.method).toBe('GET')
      req.flush(mockResponse)
    })
  })

  describe('getMostPredictablePlates', () => {
    it('should return most predictable plates', () => {
      const mockResponse: PredictionResult[] = [
        createPredictionResult({
          licensePlate: 'REG123',
          confidenceScore: 0.95,
          totalHistoricalVisits: 100
        })
      ]

      service.getMostPredictablePlates(5).subscribe(result => {
        expect(result).toEqual(mockResponse)
      })

      const req = httpMock.expectOne('/api/machinelearning/predict/top?count=5&withinHours=168')
      expect(req.request.method).toBe('GET')
      req.flush(mockResponse)
    })
  })

  describe('getNextExpectedPlate', () => {
    it('should return the next expected plate', () => {
      const mockResponse: PredictionResult[] = [
        createPredictionResult({
          licensePlate: 'NEXT123',
          confidenceScore: 0.88,
          totalHistoricalVisits: 45
        })
      ]

      service.getNextExpectedPlate().subscribe(result => {
        expect(result).toEqual(mockResponse[0])
        expect(result?.licensePlate).toBe('NEXT123')
      })

      const req = httpMock.expectOne('/api/machinelearning/predict/top?count=1&withinHours=24')
      expect(req.request.method).toBe('GET')
      req.flush(mockResponse)
    })

    it('should return null when no predictions available', () => {
      const mockResponse: PredictionResult[] = []

      service.getNextExpectedPlate().subscribe(result => {
        expect(result).toBeNull()
      })

      const req = httpMock.expectOne('/api/machinelearning/predict/top?count=1&withinHours=24')
      req.flush(mockResponse)
    })
  })

  describe('getHourlyStats', () => {
    it('should return hourly stats with formatted display hours', () => {
      const mockResponse: GetHourlyStatsResponse = {
        counts: [
          { hour: 0, count: 12 },
          { hour: 9, count: 45 },
          { hour: 12, count: 38 },
          { hour: 15, count: 52 },
          { hour: 23, count: 8 }
        ]
      }

      service.getHourlyStats().subscribe(result => {
        expect(result.length).toBe(5)
        expect(result[0]).toEqual({ hour: 0, count: 12, displayHour: '12 AM' })
        expect(result[1]).toEqual({ hour: 9, count: 45, displayHour: '9 AM' })
        expect(result[2]).toEqual({ hour: 12, count: 38, displayHour: '12 PM' })
        expect(result[3]).toEqual({ hour: 15, count: 52, displayHour: '3 PM' })
        expect(result[4]).toEqual({ hour: 23, count: 8, displayHour: '11 PM' })
      })

      const req = httpMock.expectOne('/api/licensePlates/stats/hourly')
      expect(req.request.method).toBe('GET')
      req.flush(mockResponse)
    })

    it('should handle empty hourly stats', () => {
      const mockResponse: GetHourlyStatsResponse = { counts: [] }

      service.getHourlyStats().subscribe(result => {
        expect(result).toEqual([])
      })

      const req = httpMock.expectOne('/api/licensePlates/stats/hourly')
      req.flush(mockResponse)
    })
  })

  describe('getQuickStats', () => {
    it('should return quick stats', () => {
      const mockResponse: QuickStats = {
        todayCount: 156,
        weekCount: 892,
        monthCount: 3421,
        uniquePlatesThisWeek: 234,
        activeCameras: 8,
        averageDailyPlates: 127
      }

      service.getQuickStats().subscribe(result => {
        expect(result).toEqual(mockResponse)
        expect(result.todayCount).toBe(156)
        expect(result.activeCameras).toBe(8)
      })

      const req = httpMock.expectOne('/api/licensePlates/stats/quick')
      expect(req.request.method).toBe('GET')
      req.flush(mockResponse)
    })
  })

  describe('formatHour (private method)', () => {
    it('should format hours correctly', () => {
      // Test the formatHour method indirectly through getHourlyStats
      const testCases = [
        { hour: 0, expected: '12 AM' },
        { hour: 1, expected: '1 AM' },
        { hour: 11, expected: '11 AM' },
        { hour: 12, expected: '12 PM' },
        { hour: 13, expected: '1 PM' },
        { hour: 23, expected: '11 PM' }
      ]

      testCases.forEach(testCase => {
        const mockResponse: GetHourlyStatsResponse = {
          counts: [{ hour: testCase.hour, count: 10 }]
        }

        service.getHourlyStats().subscribe(result => {
          expect(result[0].displayHour).toBe(testCase.expected)
        })

        const req = httpMock.expectOne('/api/licensePlates/stats/hourly')
        req.flush(mockResponse)
      })
    })
  })

  describe('Error handling', () => {
    it('should handle HTTP errors for getPlatesCount', () => {
      const errorMessage = 'Server error'

      service.getPlatesCount().subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error.status).toBe(500)
          expect(error.error).toBe(errorMessage)
        }
      })

      const req = httpMock.expectOne('/api/licensePlates/counts')
      req.flush(errorMessage, { status: 500, statusText: 'Server Error' })
    })

    it('should handle network errors', () => {
      service.getMostSeenPlates().subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error.error.type).toBe('NetworkError')
        }
      })

      const req = httpMock.expectOne('/api/licensePlates/most-seen')
      req.error(new ProgressEvent('NetworkError'))
    })
  })
})
