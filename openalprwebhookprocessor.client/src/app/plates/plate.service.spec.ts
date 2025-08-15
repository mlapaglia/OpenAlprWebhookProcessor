import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { PlateService, PlateRequest } from './plate.service';
import { type Plate } from './plate/plate';
import { type PlateResponse } from './plate/plateResponse';
import { type VehicleFilters } from './vehicleFilters';
import { type GetPlateResponse } from './plate/getPlateResponse';
import { type PlateStatistics } from './plate/plateStatistics';

describe('PlateService', () => {
  let service: PlateService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [PlateService],
    });
    service = TestBed.inject(PlateService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('searchPlates', () => {
    it('should search plates with correct HTTP call', () => {
      const mockPlateRequest: PlateRequest = {
        plateNumber: 'ABC123',
        startSearchOn: new Date('2023-01-01'),
        endSearchOn: new Date('2023-12-31'),
        strictMatch: true,
        includeIgnoredPlates: false,
        filterPlatesSeenLessThan: 5,
        regexSearchEnabled: false,
        pageNumber: 1,
        pageSize: 10,
        vehicleMake: 'Toyota',
        vehicleModel: 'Camry',
        vehicleColor: 'Blue',
        vehicleType: 'Sedan',
        vehicleRegion: 'US',
      };

      const mockResponse: PlateResponse = {
        plates: [],
        totalCount: 0,
      };

      service.searchPlates(mockPlateRequest).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne('/api/licenseplates/search');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockPlateRequest);
      req.flush(mockResponse);
    });
  });

  describe('upsertPlate', () => {
    it('should upsert plate with correct HTTP call', () => {
      const mockPlate: Plate = {
        id: '123',
        plateNumber: 'ABC123',
        vehicleDescription: 'Toyota Camry',
        openAlprCameraId: 1,
        direction: 90,
        receivedOn: new Date(),
        isAlert: false,
        isIgnore: false,
        isOpen: false,
        cropImageUrl: 'http://example.com/crop.jpg',
        imageUrl: 'http://example.com/image.jpg',
        processedPlateConfidence: 95,
        canBeEnriched: false,
        region: 'US',
        possiblePlateNumbers: 'ABC123',
        openAlprProcessingTimeMs: 100,
        alertDescription: 'Test alert',
        notes: 'Test notes',
      };

      service.upsertPlate(mockPlate).subscribe(response => {
        expect(response).toBeNull();
      });

      const req = httpMock.expectOne('/api/licenseplates/edit');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockPlate);
      req.flush(null);
    });
  });

  describe('deletePlate', () => {
    it('should delete plate with correct HTTP call', () => {
      const plateId = '123';

      service.deletePlate(plateId).subscribe(response => {
        expect(response).toBeNull();
      });

      const req = httpMock.expectOne('/api/licenseplates/123');
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });

  describe('getRelayImage', () => {
    it('should get relay image with correct HTTP call', () => {
      const imageId = 'image123';

      service.getRelayImage(imageId).subscribe();

      const req = httpMock.expectOne('/api/images/image123');
      expect(req.request.method).toBe('GET');
      req.flush({});
    });
  });

  describe('hydrateDatabase', () => {
    it('should hydrate database with correct HTTP call', () => {
      service.hydrateDatabase().subscribe(response => {
        expect(response).toBeNull();
      });

      const req = httpMock.expectOne('/api/hydration/start');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({});
      req.flush(null);
    });
  });

  describe('getFilters', () => {
    it('should get filters with correct HTTP call', () => {
      const mockFilters: VehicleFilters = {
        vehicleMakes: ['Toyota', 'Honda'],
        vehicleModels: ['Camry', 'Civic'],
        vehicleColors: ['Blue', 'Red'],
        vehicleTypes: ['Sedan', 'SUV'],
        vehicleRegions: ['US', 'CA'],
      };

      service.getFilters().subscribe(response => {
        expect(response).toEqual(mockFilters);
      });

      const req = httpMock.expectOne('/api/licenseplates/filters');
      expect(req.request.method).toBe('GET');
      req.flush(mockFilters);
    });
  });

  describe('getPlate', () => {
    it('should get single plate with correct HTTP call', () => {
      const plateId = '123';
      const mockGetPlateResponse: GetPlateResponse = {
        plate: {
          id: '123',
          plateNumber: 'ABC123',
          vehicleDescription: 'Toyota Camry',
          openAlprCameraId: 1,
          direction: 90,
          receivedOn: new Date(),
          isAlert: false,
          isIgnore: false,
          isOpen: false,
          cropImageUrl: 'http://example.com/crop.jpg',
          imageUrl: 'http://example.com/image.jpg',
          processedPlateConfidence: 95,
          canBeEnriched: false,
          region: 'US',
          possiblePlateNumbers: 'ABC123',
          openAlprProcessingTimeMs: 100,
          alertDescription: 'Test alert',
          notes: 'Test notes',
        },
      };

      service.getPlate(plateId).subscribe(response => {
        expect(response).toEqual(mockGetPlateResponse);
      });

      const req = httpMock.expectOne('/api/licenseplates/123');
      expect(req.request.method).toBe('GET');
      req.flush(mockGetPlateResponse);
    });
  });

  describe('getPlateStatistics', () => {
    it('should get plate statistics with correct HTTP call', () => {
      const plateNumber = 'ABC123';
      const mockStatistics: PlateStatistics = {
        firstSeen: new Date('2023-01-01'),
        lastSeen: new Date('2023-12-31'),
        last90Days: 5,
        totalSeen: 10,
      };

      service.getPlateStatistics(plateNumber).subscribe(response => {
        expect(response).toEqual(mockStatistics);
      });

      const req = httpMock.expectOne('/api/licenseplates/statistics/ABC123');
      expect(req.request.method).toBe('GET');
      req.flush(mockStatistics);
    });
  });

  describe('enrichPlate', () => {
    it('should enrich plate with correct HTTP call', () => {
      const plateId = '123';

      service.enrichPlate(plateId).subscribe(response => {
        expect(response).toBeNull();
      });

      const req = httpMock.expectOne('/api/licenseplates/enrich/123');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toBeNull();
      req.flush(null);
    });
  });

  describe('PlateRequest', () => {
    it('should create PlateRequest with all properties', () => {
      const plateRequest = new PlateRequest();

      expect(plateRequest.plateNumber).toBeUndefined();
      expect(plateRequest.startSearchOn).toBeUndefined();
      expect(plateRequest.endSearchOn).toBeUndefined();
      expect(plateRequest.strictMatch).toBeUndefined();
      expect(plateRequest.includeIgnoredPlates).toBeUndefined();
      expect(plateRequest.filterPlatesSeenLessThan).toBeUndefined();
      expect(plateRequest.regexSearchEnabled).toBeUndefined();
      expect(plateRequest.pageNumber).toBeUndefined();
      expect(plateRequest.pageSize).toBeUndefined();
      expect(plateRequest.vehicleMake).toBeUndefined();
      expect(plateRequest.vehicleModel).toBeUndefined();
      expect(plateRequest.vehicleColor).toBeUndefined();
      expect(plateRequest.vehicleType).toBeUndefined();
      expect(plateRequest.vehicleRegion).toBeUndefined();
    });

    it('should allow setting PlateRequest properties', () => {
      const plateRequest = new PlateRequest();
      plateRequest.plateNumber = 'TEST123';
      plateRequest.pageNumber = 1;
      plateRequest.pageSize = 10;
      plateRequest.strictMatch = true;

      expect(plateRequest.plateNumber).toBe('TEST123');
      expect(plateRequest.pageNumber).toBe(1);
      expect(plateRequest.pageSize).toBe(10);
      expect(plateRequest.strictMatch).toBe(true);
    });
  });
});
