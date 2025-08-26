import type { ComponentFixture } from '@angular/core/testing';
import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ActivatedRoute } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { of, throwError, Subject } from 'rxjs';
import { ChangeDetectorRef } from '@angular/core';

import { PlatesComponent } from './plates.component';
import { PlateService, type PlateRequest } from './plate.service';
import { SignalrService } from 'app/signalr/signalr.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { IgnoresService } from 'app/settings/ignores/ignores.service';
import { AlertsService } from 'app/settings/alerts/alerts.service';
import { LocalStorageService } from 'app/_services/local-storage.service';
import { type Plate } from './plate/plate';
import { type PlateResponse } from './plate/plateResponse';
import { type VehicleFilters } from './vehicleFilters';
import { type GetPlateResponse } from './plate/getPlateResponse';
import { type PageEvent } from '@angular/material/paginator';
import { type PlateData } from './plate-item/plate-item.component';
import { type PlateFilters } from './plate-filters/plate-filters.component';

describe('PlatesComponent', () => {
  let component: PlatesComponent;
  let fixture: ComponentFixture<PlatesComponent>;
  let mockPlateService: jasmine.SpyObj<PlateService>;
  let mockSignalrService: jasmine.SpyObj<SignalrService>;
  let mockSnackbarService: jasmine.SpyObj<SnackbarService>;
  let mockIgnoresService: jasmine.SpyObj<IgnoresService>;
  let mockAlertsService: jasmine.SpyObj<AlertsService>;
  let mockLocalStorageService: jasmine.SpyObj<LocalStorageService>;
  let mockDialog: jasmine.SpyObj<MatDialog>;
  let mockActivatedRoute: any;

  const mockPlate: Plate = {
    id: '123',
    plateNumber: 'ABC123',
    vehicleDescription: 'Toyota Camry Blue',
    openAlprCameraId: 1,
    direction: 90,
    receivedOn: new Date('2023-01-01T10:00:00Z'),
    isAlert: false,
    isIgnore: false,
    isOpen: false,
    cropImageUrl: 'http://example.com/crop.jpg',
    imageUrl: 'http://example.com/image.jpg',
    processedPlateConfidence: 95,
    canBeEnriched: true,
    region: 'US-CA',
    possiblePlateNumbers: 'ABC123,ABC124',
    openAlprProcessingTimeMs: 150,
    alertDescription: '',
    notes: 'Test notes',
  };

  const mockVehicleFilters: VehicleFilters = {
    vehicleMakes: ['Toyota', 'Honda'],
    vehicleModels: ['Camry', 'Civic'],
    vehicleColors: ['Blue', 'Red'],
    vehicleTypes: ['Sedan', 'SUV'],
    vehicleRegions: ['US-CA', 'US-TX'],
    vehicleMakeModelMap: {
      Toyota: ['Camry', 'Corolla'],
      Honda: ['Civic', 'Accord'],
    },
  };

  const mockPlateResponse: PlateResponse = {
    plates: [mockPlate],
    totalCount: 1,
  };

  beforeEach(async () => {
    const plateServiceSpy = jasmine.createSpyObj('PlateService', [
      'searchPlates',
      'getPlate',
      'getFilters',
      'enrichPlate',
      'upsertPlate',
      'deletePlate',
    ]);
    const signalrServiceSpy = jasmine.createSpyObj('SignalrService', [], {
      licensePlateReceived: new Subject<string>(),
    });
    const snackbarServiceSpy = jasmine.createSpyObj('SnackbarService', ['create']);
    const ignoresServiceSpy = jasmine.createSpyObj('IgnoresService', ['addIgnore']);
    const alertsServiceSpy = jasmine.createSpyObj('AlertsService', ['addAlert']);
    const localStorageServiceSpy = jasmine.createSpyObj('LocalStorageService', ['getData', 'setData']);
    const dialogSpy = jasmine.createSpyObj('MatDialog', ['open']);
    const changeDetectorRefSpy = jasmine.createSpyObj('ChangeDetectorRef', ['markForCheck', 'detectChanges']);

    mockActivatedRoute = {
      params: of({ id: '123' }),
    };

    await TestBed.configureTestingModule({
      imports: [PlatesComponent, NoopAnimationsModule],
      providers: [
        { provide: PlateService, useValue: plateServiceSpy },
        { provide: SignalrService, useValue: signalrServiceSpy },
        { provide: SnackbarService, useValue: snackbarServiceSpy },
        { provide: IgnoresService, useValue: ignoresServiceSpy },
        { provide: AlertsService, useValue: alertsServiceSpy },
        { provide: LocalStorageService, useValue: localStorageServiceSpy },
        { provide: MatDialog, useValue: dialogSpy },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: ChangeDetectorRef, useValue: changeDetectorRefSpy },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PlatesComponent);
    component = fixture.componentInstance;

    mockPlateService = TestBed.inject(PlateService) as jasmine.SpyObj<PlateService>;
    mockSignalrService = TestBed.inject(SignalrService) as jasmine.SpyObj<SignalrService>;
    mockSnackbarService = TestBed.inject(SnackbarService) as jasmine.SpyObj<SnackbarService>;
    mockIgnoresService = TestBed.inject(IgnoresService) as jasmine.SpyObj<IgnoresService>;
    mockAlertsService = TestBed.inject(AlertsService) as jasmine.SpyObj<AlertsService>;
    mockLocalStorageService = TestBed.inject(LocalStorageService) as jasmine.SpyObj<LocalStorageService>;
    mockDialog = TestBed.inject(MatDialog) as jasmine.SpyObj<MatDialog>;

    // Set up default mock returns
    mockPlateService.getFilters.and.returnValue(of(mockVehicleFilters));
    mockPlateService.searchPlates.and.returnValue(of(mockPlateResponse));
    mockPlateService.getPlate.and.returnValue(of({ plate: mockPlate }));
    mockLocalStorageService.getData.and.returnValue('');

    // Set required input
    fixture.componentRef.setInput('id', '123');
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.plates).toEqual([]);
      expect(component.totalNumberOfPlates).toBe(0);
      expect(component.isLoading).toBe(false);
      expect(component.pageSize).toBe(25);
      expect(component.isDeletingPlate).toBe(false);
      expect(component.isEnrichingPlate).toBe(false);
      expect(component.isAddingToIgnoreList).toBe(false);
      expect(component.isAddingToAlertList).toBe(false);
    });

    it('should initialize plateFilters with correct default values', () => {
      const filters = component.plateFilters();
      expect(filters.startDate).toBeDefined();
      expect(filters.endDate).toBeDefined();
      expect(filters.plateNumber).toBe('');
      expect(filters.cameraId).toBe('');
      expect(filters.vehicleMake).toBe('');
      expect(filters.vehicleModel).toBe('');
      expect(filters.vehicleType).toBe('');
      expect(filters.vehicleColor).toBe('');
      expect(filters.vehicleRegion).toBe('');
      expect(filters.regexSearchEnabled).toBe(false);
      expect(filters.includeIgnoredPlates).toBe(false);
      expect(filters.platesSeenLessThan).toBe(false);
    });

    it('should set startDate to 7 days ago', () => {
      const sevenDaysAgo = new Date();
      sevenDaysAgo.setDate(sevenDaysAgo.getDate() - 7);
      sevenDaysAgo.setHours(0, 0, 0, 0);

      const filters = component.plateFilters();
      const startDate = new Date(filters.startDate);
      startDate.setHours(0, 0, 0, 0);

      expect(startDate.getTime()).toBeCloseTo(sevenDaysAgo.getTime(), -1);
    });

    it('should set endDate to end of today', () => {
      const endOfToday = new Date();
      endOfToday.setHours(23, 59, 59, 999);

      const filters = component.plateFilters();
      const endDate = new Date(filters.endDate);

      expect(endDate.getHours()).toBe(23);
      expect(endDate.getMinutes()).toBe(59);
      expect(endDate.getSeconds()).toBe(59);
    });
  });

  describe('Page Size Initialization', () => {
    it('should use default page size when no cached value exists', () => {
      mockLocalStorageService.getData.and.returnValue('');

      component.ngOnInit();

      expect(component.pageSize).toBe(25);
    });

    it('should use cached page size when valid value exists', () => {
      mockLocalStorageService.getData.and.returnValue('75');

      component.ngOnInit();

      expect(component.pageSize).toBe(75);
    });

    it('should use default page size when cached value is invalid', () => {
      mockLocalStorageService.getData.and.returnValue('999');

      component.ngOnInit();

      expect(component.pageSize).toBe(25);
    });

    it('should handle JSON parsed cached page size', () => {
      mockLocalStorageService.getData.and.returnValue(JSON.stringify(100));

      component.ngOnInit();

      expect(component.pageSize).toBe(100);
    });

    it('should handle malformed cached page size gracefully', () => {
      mockLocalStorageService.getData.and.returnValue('invalid-json');

      component.ngOnInit();

      expect(component.pageSize).toBe(25);
    });
  });

  describe('Filter Population', () => {
    it('should populate vehicle filters on init', () => {
      component.ngOnInit();

      expect(mockPlateService.getFilters).toHaveBeenCalled();
      expect(component.vehicleFilters.makes).toEqual(['Toyota', 'Honda']);
      expect(component.vehicleFilters.models).toEqual(['Camry', 'Civic']);
      expect(component.vehicleFilters.colors).toEqual(['Blue', 'Red']);
      expect(component.vehicleFilters.types).toEqual(['Sedan', 'SUV']);
      expect(component.vehicleFilters.regions).toEqual(['US-CA', 'US-TX']);
    });

    it('should handle missing vehicle filter data gracefully', () => {
      const incompleteFilters: Partial<VehicleFilters> = {
        vehicleMakes: ['Toyota'],
      };
      mockPlateService.getFilters.and.returnValue(of(incompleteFilters as VehicleFilters));

      component.ngOnInit();

      expect(component.vehicleFilters.makes).toEqual(['Toyota']);
      expect(component.vehicleFilters.models).toEqual([]);
      expect(component.vehicleFilters.colors).toEqual([]);
      expect(component.vehicleFilters.types).toEqual([]);
      expect(component.vehicleFilters.regions).toEqual([]);
    });

    it('should correctly identify when vehicle filters have data', () => {
      component.vehicleFilters = {
        cameras: [],
        makes: ['Toyota'],
        models: [],
        types: [],
        colors: [],
        regions: [],
      };

      expect(component.hasVehicleFiltersData).toBe(true);
    });

    it('should correctly identify when vehicle filters have no data', () => {
      component.vehicleFilters = {
        cameras: [],
        makes: [],
        models: [],
        types: [],
        colors: [],
        regions: [],
      };

      expect(component.hasVehicleFiltersData).toBe(false);
    });
  });

  describe('Plate Loading', () => {
    it('should load single plate when id is provided in route params', fakeAsync(() => {
      const getPlateResponse: GetPlateResponse = { plate: mockPlate };
      mockPlateService.getPlate.and.returnValue(of(getPlateResponse));

      component.ngOnInit();
      tick();

      expect(mockPlateService.getPlate).toHaveBeenCalledWith('123');
      expect(component.plates.length).toBe(1);
      expect(component.plates[0].id).toBe('123');
      expect(component.totalNumberOfPlates).toBe(1);
      expect(component.isLoading).toBe(false);
    }));

    it('should handle error when loading single plate', fakeAsync(() => {
      mockPlateService.getPlate.and.returnValue(throwError(() => new Error('Load error')));

      component.ngOnInit();
      tick();

      expect(component.isLoading).toBe(false);
      expect(component.plates.length).toBe(0);
    }));

    it('should ignore stale requests when loading single plate', fakeAsync(() => {
      const firstResponse: GetPlateResponse = {
        plate: { ...mockPlate, id: 'first' },
      };
      const secondResponse: GetPlateResponse = {
        plate: { ...mockPlate, id: 'second' },
      };

      // Create a Subject to control when the observables emit
      const firstSubject = new Subject<GetPlateResponse>();
      const secondSubject = new Subject<GetPlateResponse>();

      mockPlateService.getPlate.and.returnValues(
        firstSubject.asObservable(),
        secondSubject.asObservable(),
      );

      // Start first request
      component.ngOnInit();
      tick();

      // Start second request (simulating route change)
      mockActivatedRoute.params = of({ id: '456' });
      component.ngOnInit();
      tick();

      // Resolve second request first (newer)
      secondSubject.next(secondResponse);
      secondSubject.complete();
      tick();

      // Resolve first request (older, should be ignored)
      firstSubject.next(firstResponse);
      firstSubject.complete();
      tick();

      expect(component.plates[0].id).toBe('second');
    }));
  });

  describe('Plate Search', () => {
    it('should search plates with correct request parameters', () => {
      const filters: PlateFilters = {
        startDate: new Date('2023-01-01'),
        endDate: new Date('2023-12-31'),
        plateNumber: 'ABC123',
        cameraId: 'cam1',
        vehicleMake: 'Toyota',
        vehicleModel: 'Camry',
        vehicleType: 'Sedan',
        vehicleColor: 'Blue',
        vehicleRegion: 'US-CA',
        regexSearchEnabled: true,
        includeIgnoredPlates: true,
        platesSeenLessThan: true,
      };

      component.plateFilters.set(filters);
      component.onSearchTriggered();

      const expectedRequest = jasmine.objectContaining({
        pageSize: 25,
        pageNumber: 0,
        plateNumber: 'ABC123',
        strictMatch: false,
        vehicleMake: 'Toyota',
        vehicleModel: 'Camry',
        vehicleType: 'Sedan',
        vehicleColor: 'Blue',
        vehicleRegion: 'US-CA',
        includeIgnoredPlates: true,
        filterPlatesSeenLessThan: 10,
        regexSearchEnabled: true,
      });

      expect(mockPlateService.searchPlates).toHaveBeenCalledWith(expectedRequest);
    });

    it('should handle search error gracefully', () => {
      mockPlateService.searchPlates.and.returnValue(throwError(() => new Error('Search failed')));

      component.onSearchTriggered();

      expect(component.isLoading).toBe(false);
      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Search failed. Please try again.',
        SnackBarType.Error,
      );
    });

    it('should reset page number when search is triggered', () => {
      component['pageNumber'] = 5;

      component.onSearchTriggered();

      expect(mockPlateService.searchPlates).toHaveBeenCalledWith(
        jasmine.objectContaining({ pageNumber: 0 }),
      );
    });

    it('should not reset page number when resetPage is false', () => {
      component['pageNumber'] = 5;

      component.onPaginatorChange({ pageIndex: 5, pageSize: 25, length: 100 });

      expect(mockPlateService.searchPlates).toHaveBeenCalledWith(
        jasmine.objectContaining({ pageNumber: 5 }),
      );
    });
  });

  describe('SignalR Integration', () => {
    it('should search plates when SignalR receives new plate and not loading', fakeAsync(() => {
      component.isLoading = false;

      // Create a spy for the private searchPlates method instead
      spyOn<any>(component, 'searchPlates');

      component.ngOnInit();
      tick();

      mockSignalrService.licensePlateReceived.next('new-plate');
      tick();

      expect(component['searchPlates']).toHaveBeenCalled();
    }));

    it('should not search plates when SignalR receives new plate but component is loading', fakeAsync(() => {
      spyOn<any>(component, 'searchPlates');

      component.ngOnInit();
      tick();

      // Set loading to true after initialization
      component.isLoading = true;

      mockSignalrService.licensePlateReceived.next('new-plate');
      tick();

      expect(component['searchPlates']).not.toHaveBeenCalled();
    }));
  });

  describe('Pagination', () => {
    it('should handle paginator change event', () => {
      const pageEvent: PageEvent = {
        pageIndex: 2,
        pageSize: 50,
        length: 200,
      };

      component.onPaginatorChange(pageEvent);

      expect(component.pageSize).toBe(50);
      expect(mockLocalStorageService.setData).toHaveBeenCalledWith(
        'platePageSize',
        JSON.stringify(50),
      );
      expect(mockPlateService.searchPlates).toHaveBeenCalledWith(
        jasmine.objectContaining({
          pageNumber: 2,
          pageSize: 50,
        }),
      );
    });

    it('should not update page size if pageSize is invalid', () => {
      const pageEvent: PageEvent = {
        pageIndex: 1,
        pageSize: 0,
        length: 100,
      };

      const originalPageSize = component.pageSize;
      component.onPaginatorChange(pageEvent);

      expect(component.pageSize).toBe(originalPageSize);
      expect(mockLocalStorageService.setData).not.toHaveBeenCalled();
    });
  });

  describe('Plate Actions', () => {
    beforeEach(() => {
      component.plates = [
        {
          id: '123',
          plateNumber: 'ABC123',
          vehicleDescription: 'Toyota Camry',
          openAlprCameraId: 1,
          direction: 90,
          receivedOn: new Date(),
          isAlert: false,
          isIgnore: false,
          isOpen: false,
          imageUrl: 'http://example.com/image.jpg',
          cropImageUrl: 'http://example.com/crop.jpg',
          processedPlateConfidence: 95,
          notes: 'Test notes',
          canBeEnriched: true,
          region: 'US-CA',
          possiblePlateNumbers: 'ABC123',
          openAlprProcessingTimeMs: 150,
        },
      ];

      // Reset loading states
      component.isEnrichingPlate = false;
      component.isAddingToAlertList = false;
      component.isAddingToIgnoreList = false;
    });

    describe('Plate Opening/Closing', () => {
      it('should open plate when onPlateOpened is called', () => {
        component.onPlateOpened('123');

        expect(component.plates[0].isOpen).toBe(true);
      });

      it('should close plate when onPlateClosed is called', () => {
        component.plates[0].isOpen = true;

        component.onPlateClosed('123');

        expect(component.plates[0].isOpen).toBe(false);
      });

      it('should handle opening non-existent plate gracefully', () => {
        expect(() => component.onPlateOpened('non-existent')).not.toThrow();
      });
    });

    describe('Component State', () => {
      it('should allow setting and getting loading states', () => {
        expect(component.isEnrichingPlate).toBe(false);
        component.isEnrichingPlate = true;
        expect(component.isEnrichingPlate).toBe(true);
        component.isEnrichingPlate = false;
        expect(component.isEnrichingPlate).toBe(false);
      });
    });

    describe('Plate Enrichment', () => {
      it('should enrich plate successfully', fakeAsync(() => {
        mockPlateService.enrichPlate.and.returnValue(of(null));
        spyOn<any>(component, 'searchPlates');

        // Verify initial state
        expect(component.isEnrichingPlate).toBe(false);

        // Manually set the loading state to verify it can be set
        component.isEnrichingPlate = true;
        expect(component.isEnrichingPlate).toBe(true);

        // Reset to false
        component.isEnrichingPlate = false;
        expect(component.isEnrichingPlate).toBe(false);

        // Call the method
        component.onEnrichPlate('123');

        // Process async operations first
        tick();

        // Verify service call and final state
        expect(mockPlateService.enrichPlate).toHaveBeenCalledWith('123');
        expect(component.isEnrichingPlate).toBe(false);
        expect(mockSnackbarService.create).toHaveBeenCalledWith(
          'Plate enriched successfully',
          SnackBarType.Successful,
        );
        expect(component['searchPlates']).toHaveBeenCalled();
      }));

      it('should handle enrich plate error', fakeAsync(() => {
        mockPlateService.enrichPlate.and.returnValue(throwError(() => new Error('Enrich failed')));

        component.onEnrichPlate('123');
        tick();

        expect(component.isEnrichingPlate).toBe(false);
        expect(mockSnackbarService.create).toHaveBeenCalledWith(
          'Failed to enrich plate',
          SnackBarType.Error,
        );
      }));
    });

    describe('Plate Editing', () => {
      it('should open edit dialog for existing plate', () => {
        const mockDialogRef = {
          afterClosed: () => of(true),
        };
        mockDialog.open.and.returnValue(mockDialogRef as any);
        spyOn<any>(component, 'searchPlates');

        component.onEditPlate('123');

        expect(mockDialog.open).toHaveBeenCalled();
        expect(component['searchPlates']).toHaveBeenCalled();
      });

      it('should not refresh if dialog is cancelled', () => {
        const mockDialogRef = {
          afterClosed: () => of(false),
        };
        mockDialog.open.and.returnValue(mockDialogRef as any);
        spyOn<any>(component, 'searchPlates');

        component.onEditPlate('123');

        expect(component['searchPlates']).not.toHaveBeenCalled();
      });

      it('should handle editing non-existent plate gracefully', () => {
        expect(() => component.onEditPlate('non-existent')).not.toThrow();
        expect(mockDialog.open).not.toHaveBeenCalled();
      });
    });

    describe('Alert List Management', () => {
      it('should add plate to alert list successfully', fakeAsync(() => {
        mockAlertsService.addAlert.and.returnValue(of({}));
        spyOn<any>(component, 'searchPlates');

        expect(component.isAddingToAlertList).toBe(false);

        component.onAlertPlate('123');

        tick();

        expect(mockAlertsService.addAlert).toHaveBeenCalledWith(
          jasmine.objectContaining({
            id: '123',
            plateNumber: 'ABC123',
            description: '',
            strictMatch: false,
          }),
        );
        expect(component.isAddingToAlertList).toBe(false);
        expect(mockSnackbarService.create).toHaveBeenCalledWith(
          'Added to alert list',
          SnackBarType.Successful,
        );
        expect(component['searchPlates']).toHaveBeenCalled();
      }));

      it('should handle add to alert list error', fakeAsync(() => {
        mockAlertsService.addAlert.and.returnValue(throwError(() => new Error('Add failed')));

        component.onAlertPlate('123');
        tick();

        expect(component.isAddingToAlertList).toBe(false);
        expect(mockSnackbarService.create).toHaveBeenCalledWith(
          'Failed to add to alert list',
          SnackBarType.Error,
        );
      }));

      it('should handle alerting non-existent plate gracefully', () => {
        expect(() => component.onAlertPlate('non-existent')).not.toThrow();
        expect(mockAlertsService.addAlert).not.toHaveBeenCalled();
      });
    });

    describe('Ignore List Management', () => {
      it('should add plate to ignore list successfully', fakeAsync(() => {
        mockIgnoresService.addIgnore.and.returnValue(of({}));
        spyOn<any>(component, 'searchPlates');

        expect(component.isAddingToIgnoreList).toBe(false);

        component.onIgnorePlate('123');

        tick();

        expect(mockIgnoresService.addIgnore).toHaveBeenCalledWith(
          jasmine.objectContaining({
            id: '123',
            plateNumber: 'ABC123',
            description: '',
            strictMatch: false,
          }),
        );
        expect(component.isAddingToIgnoreList).toBe(false);
        expect(mockSnackbarService.create).toHaveBeenCalledWith(
          'Added to ignore list',
          SnackBarType.Successful,
        );
        expect(component['searchPlates']).toHaveBeenCalled();
      }));

      it('should handle add to ignore list error', fakeAsync(() => {
        mockIgnoresService.addIgnore.and.returnValue(throwError(() => new Error('Add failed')));

        component.onIgnorePlate('123');
        tick();

        expect(component.isAddingToIgnoreList).toBe(false);
        expect(mockSnackbarService.create).toHaveBeenCalledWith(
          'Failed to add to ignore list',
          SnackBarType.Error,
        );
      }));

      it('should handle ignoring non-existent plate gracefully', () => {
        expect(() => component.onIgnorePlate('non-existent')).not.toThrow();
        expect(mockIgnoresService.addIgnore).not.toHaveBeenCalled();
      });
    });

    describe('Plate Search', () => {
      it('should search for specific plate number', () => {
        spyOn<any>(component, 'searchPlates');

        component.onSearchForPlate('XYZ789');

        expect(component.plateFilters().plateNumber).toBe('XYZ789');
        expect(component['searchPlates']).toHaveBeenCalled();
      });
    });
  });

  describe('Data Mapping', () => {
    it('should correctly map Plate to PlateData', () => {
      const plateData: PlateData[] = [
        {
          id: '123',
          plateNumber: 'ABC123',
          vehicleDescription: 'Toyota Camry Blue',
          openAlprCameraId: 1,
          direction: 90,
          receivedOn: new Date('2023-01-01T10:00:00Z'),
          isAlert: false,
          isIgnore: false,
          isOpen: false,
          imageUrl: 'http://example.com/image.jpg',
          cropImageUrl: 'http://example.com/crop.jpg',
          processedPlateConfidence: 95,
          notes: 'Test notes',
          canBeEnriched: true,
          region: 'US-CA',
          possiblePlateNumbers: 'ABC123,ABC124',
          openAlprProcessingTimeMs: 150,
        },
      ];

      mockPlateService.searchPlates.and.returnValue(of(mockPlateResponse));

      component.onSearchTriggered();

      expect(component.plates).toEqual(plateData);
    });
  });

  describe('Request Building', () => {
    it('should build correct date filters', () => {
      const startDate = new Date('2023-01-01T15:30:00Z');
      const endDate = new Date('2023-12-31T10:30:00Z');

      component.plateFilters.set({
        ...component.plateFilters(),
        startDate,
        endDate,
      });

      component.onSearchTriggered();

      const call = mockPlateService.searchPlates.calls.mostRecent();
      const request = call.args[0] as PlateRequest;

      expect(request.startSearchOn.getHours()).toBe(0);
      expect(request.startSearchOn.getMinutes()).toBe(0);
      expect(request.startSearchOn.getSeconds()).toBe(0);
      expect(request.startSearchOn.getMilliseconds()).toBe(0);

      expect(request.endSearchOn.getHours()).toBe(23);
      expect(request.endSearchOn.getMinutes()).toBe(59);
      expect(request.endSearchOn.getSeconds()).toBe(59);
      expect(request.endSearchOn.getMilliseconds()).toBe(999);
    });

    it('should build correct vehicle filters with empty strings for undefined values', () => {
      component.plateFilters.set({
        ...component.plateFilters(),
        vehicleMake: undefined as any,
        vehicleModel: 'Camry',
        vehicleType: '',
        vehicleColor: undefined as any,
        vehicleRegion: 'US-CA',
      });

      component.onSearchTriggered();

      const call = mockPlateService.searchPlates.calls.mostRecent();
      const request = call.args[0] as PlateRequest;

      expect(request.vehicleMake).toBe('');
      expect(request.vehicleModel).toBe('Camry');
      expect(request.vehicleType).toBe('');
      expect(request.vehicleColor).toBe('');
      expect(request.vehicleRegion).toBe('US-CA');
    });

    it('should build correct boolean filters', () => {
      component.plateFilters.set({
        ...component.plateFilters(),
        includeIgnoredPlates: true,
        platesSeenLessThan: true,
        regexSearchEnabled: false,
      });

      component.onSearchTriggered();

      const call = mockPlateService.searchPlates.calls.mostRecent();
      const request = call.args[0] as PlateRequest;

      expect(request.includeIgnoredPlates).toBe(true);
      expect(request.filterPlatesSeenLessThan).toBe(10);
      expect(request.regexSearchEnabled).toBe(false);
    });

    it('should set filterPlatesSeenLessThan to 0 when platesSeenLessThan is false', () => {
      component.plateFilters.set({
        ...component.plateFilters(),
        platesSeenLessThan: false,
      });

      component.onSearchTriggered();

      const call = mockPlateService.searchPlates.calls.mostRecent();
      const request = call.args[0] as PlateRequest;

      expect(request.filterPlatesSeenLessThan).toBe(0);
    });
  });

  describe('Component Cleanup', () => {
    it('should call parent ngOnDestroy', () => {
      spyOn(Object.getPrototypeOf(Object.getPrototypeOf(component)), 'ngOnDestroy');

      component.ngOnDestroy();

      expect(Object.getPrototypeOf(Object.getPrototypeOf(component)).ngOnDestroy).toHaveBeenCalled();
    });
  });
});
