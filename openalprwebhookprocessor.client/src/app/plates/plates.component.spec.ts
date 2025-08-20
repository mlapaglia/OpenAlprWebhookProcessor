import type { ComponentFixture } from '@angular/core/testing';
import { TestBed } from '@angular/core/testing';
import { PlatesComponent } from './plates.component';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { of, Subject, throwError, defer } from 'rxjs';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { provideNativeDateAdapter } from '@angular/material/core';

import { PlateService } from './plate.service';
import { SignalrService } from 'app/signalr/signalr.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { AlertsService } from 'app/settings/alerts/alerts.service';
import { SettingsService } from 'app/settings/settings.service';
import { IgnoresService } from 'app/settings/ignores/ignores.service';
import { LocalStorageService } from 'app/_services/local-storage.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { type PageEvent } from '@angular/material/paginator';

describe(PlatesComponent.name, () => {
  let component: PlatesComponent;
  let fixture: ComponentFixture<PlatesComponent>;
  let mockPlateService: jasmine.SpyObj<PlateService>;
  let mockSignalrService: jasmine.SpyObj<SignalrService>;
  let mockSnackbarService: jasmine.SpyObj<SnackbarService>;
  let mockAlertsService: jasmine.SpyObj<AlertsService>;
  let mockSettingsService: jasmine.SpyObj<SettingsService>;
  let mockIgnoresService: jasmine.SpyObj<IgnoresService>;
  let mockLocalStorageService: jasmine.SpyObj<LocalStorageService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockDialog: jasmine.SpyObj<MatDialog>;
  let mockActivatedRoute: any;
  let licensePlateReceivedSubject: Subject<any>;

  const mockPlate = {
    id: '1',
    plateNumber: 'ABC123',
    openAlprCameraId: 1,
    vehicleDescription: 'White Toyota',
    direction: 1,
    receivedOn: new Date('2023-01-01T12:00:00Z'),
    isAlert: false,
    isIgnore: false,
    isOpen: false,
    imageUrl: 'image.jpg',
    cropImageUrl: 'crop.jpg',
    processedPlateConfidence: 0.95,
    notes: 'Test note',
    canBeEnriched: true,
    region: 'us',
    possiblePlateNumbers: 'ABC123',
    openAlprProcessingTimeMs: 100,
    alertDescription: '',
  };

  const mockVehicleFilters = {
    vehicleMakes: ['Toyota', 'Honda'],
    vehicleModels: ['Camry', 'Civic'],
    vehicleTypes: ['Sedan', 'SUV'],
    vehicleColors: ['White', 'Black'],
  };

  const mockCameras = [
    {
      id: '1',
      latitude: 0,
      longitude: 0,
      ipAddress: '192.168.1.1',
      manufacturer: 'Hikvision' as any,
      modelNumber: 'DS-2CD2T42WD-I5',
      openAlprName: 'Camera1',
      openAlprCameraId: 1,
      cameraPassword: 'password',
      cameraUsername: 'admin',
      updateOverlayEnabled: false,
      updateOverlayTextUrl: '',
      nightZoom: '1.0',
      nightFocus: '1.0',
      dayZoom: '1.0',
      dayFocus: '1.0',
      dayNightModeEnabled: false,
      dayNightModeUrl: '',
      dayNightNextScheduledCommand: new Date(),
      openAlprEnabled: true,
      sunriseOffset: 0,
      sunsetOffset: 0,
      platesSeen: 0,
      sampleImageUrl: '',
      timezoneOffset: 0,
    },
    {
      id: '2',
      latitude: 0,
      longitude: 0,
      ipAddress: '192.168.1.2',
      manufacturer: 'Hikvision' as any,
      modelNumber: 'DS-2CD2T42WD-I5',
      openAlprName: 'Camera2',
      openAlprCameraId: 2,
      cameraPassword: 'password',
      cameraUsername: 'admin',
      updateOverlayEnabled: false,
      updateOverlayTextUrl: '',
      nightZoom: '1.0',
      nightFocus: '1.0',
      dayZoom: '1.0',
      dayFocus: '1.0',
      dayNightModeEnabled: false,
      dayNightModeUrl: '',
      dayNightNextScheduledCommand: new Date(),
      openAlprEnabled: true,
      sunriseOffset: 0,
      sunsetOffset: 0,
      platesSeen: 0,
      sampleImageUrl: '',
      timezoneOffset: 0,
    },
  ];

  beforeEach(async () => {
    licensePlateReceivedSubject = new Subject();

    mockPlateService = jasmine.createSpyObj('PlateService', [
      'getFilters',
      'searchPlates',
      'getPlate',
      'enrichPlate',
    ]);
    mockSignalrService = jasmine.createSpyObj('SignalrService', [], {
      licensePlateReceived: licensePlateReceivedSubject.asObservable(),
    });
    mockSnackbarService = jasmine.createSpyObj('SnackbarService', ['create']);
    mockAlertsService = jasmine.createSpyObj('AlertsService', ['addAlert']);
    mockSettingsService = jasmine.createSpyObj('SettingsService', ['getCameras']);
    mockIgnoresService = jasmine.createSpyObj('IgnoresService', ['addIgnore']);
    mockLocalStorageService = jasmine.createSpyObj('LocalStorageService', ['getData', 'setData']);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    mockDialog = jasmine.createSpyObj('MatDialog', ['open']);

    mockActivatedRoute = {
      params: of({ id: undefined }),
    };

    await TestBed.configureTestingModule({
      imports: [
        PlatesComponent,
        RouterTestingModule,
        BrowserAnimationsModule,
      ],
      providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        provideNativeDateAdapter(),
        { provide: PlateService, useValue: mockPlateService },
        { provide: SignalrService, useValue: mockSignalrService },
        { provide: SnackbarService, useValue: mockSnackbarService },
        { provide: AlertsService, useValue: mockAlertsService },
        { provide: SettingsService, useValue: mockSettingsService },
        { provide: IgnoresService, useValue: mockIgnoresService },
        { provide: LocalStorageService, useValue: mockLocalStorageService },
        { provide: Router, useValue: mockRouter },
        { provide: MatDialog, useValue: mockDialog },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    mockPlateService.getFilters.and.returnValue(of(mockVehicleFilters));
    mockSettingsService.getCameras.and.returnValue(of(mockCameras));
    mockPlateService.searchPlates.and.returnValue(
      of({ plates: [mockPlate], totalCount: 1 }),
    );
    mockLocalStorageService.getData.and.returnValue('');

    fixture = TestBed.createComponent(PlatesComponent);
    component = fixture.componentInstance;
  });

  it('should be defined', () => {
    expect(PlatesComponent).toBeDefined();
  });

  it('should create component successfully', () => {
    expect(component).toBeTruthy();
  });

  describe('Component Initialization', () => {
    it('should initialize with default values', () => {
      expect(component.plates).toEqual([]);
      expect(component.totalNumberOfPlates).toBe(0);
      expect(component.isLoading).toBe(false);
      expect(component.showAdvancedFilters).toBe(false);
      expect(component.currentFilters).toBeNull();
      expect(component.pageSize).toBe(25);
      expect(component.isDeletingPlate).toBe(false);
      expect(component.isEnrichingPlate).toBe(false);
      expect(component.isAddingToIgnoreList).toBe(false);
      expect(component.isAddingToAlertList).toBe(false);
    });

    it('should initialize page size from local storage', () => {
      mockLocalStorageService.getData.and.returnValue('75');

      const newComponent = TestBed.createComponent(PlatesComponent).componentInstance;
      newComponent.ngOnInit();

      expect(newComponent.pageSize).toBe(75);
    });

    it('should use default page size when local storage has invalid value', () => {
      mockLocalStorageService.getData.and.returnValue('invalid');

      component.ngOnInit();

      expect(component.pageSize).toBe(25);
    });

    it('should populate filters on init', () => {
      component.ngOnInit();

      expect(mockPlateService.getFilters).toHaveBeenCalled();
      expect(mockSettingsService.getCameras).toHaveBeenCalled();
      expect(component.vehicleFilters).toEqual(mockVehicleFilters);
      expect(component.cameras).toEqual(['Camera1', 'Camera2']);
    });
  });

  describe('Route Parameter Handling', () => {
    it('should get single plate when id is provided in route params', () => {
      mockActivatedRoute.params = of({ id: '123' });
      mockPlateService.getPlate.and.returnValue(
        of({ plate: mockPlate }),
      );

      component.ngOnInit();

      expect(mockPlateService.getPlate).toHaveBeenCalledWith('123');
      expect(component.plates.length).toBe(1);
      expect(component.totalNumberOfPlates).toBe(1);
    });

    it('should handle error when getting single plate', () => {
      mockActivatedRoute.params = of({ id: '123' });
      mockPlateService.getPlate.and.returnValue(throwError('Error'));

      component.ngOnInit();

      expect(component.isLoading).toBe(false);
    });
  });

  describe('SignalR Integration', () => {
    it('should subscribe to license plate updates', () => {
      spyOn(component, 'searchPlates' as any);
      component.ngOnInit();

      licensePlateReceivedSubject.next({});

      expect((component as any).searchPlates).toHaveBeenCalled();
    });

    it('should not search plates when loading', () => {
      spyOn(component, 'searchPlates' as any);
      component.isLoading = true;
      component.ngOnInit();

      licensePlateReceivedSubject.next({});

      expect((component as any).searchPlates).not.toHaveBeenCalled();
    });
  });

  describe('Filter Event Handlers', () => {
    it('should handle filters changed event', () => {
      const filters = { plateNumber: 'ABC123' } as any;

      component.onFiltersChanged(filters);

      expect(component.currentFilters).toBe(filters);
    });

    it('should handle search triggered event', () => {
      spyOn(component, 'searchPlates' as any);

      component.onSearchTriggered();

      expect((component as any).searchPlates).toHaveBeenCalled();
    });

    it('should handle filters cleared event', () => {
      spyOn(component, 'searchPlates' as any);
      component.currentFilters = { plateNumber: 'ABC123' } as any;

      component.onFiltersCleared();

      // The component doesn't clear currentFilters - the plate-filters component
      // emits updated filters via onFiltersChanged first, then this triggers search
      expect(component.currentFilters).toEqual({ plateNumber: 'ABC123' } as any);
      expect((component as any).searchPlates).toHaveBeenCalled();
    });

    it('should toggle advanced filters', () => {
      expect(component.showAdvancedFilters).toBe(false);

      component.onAdvancedFiltersToggled();

      expect(component.showAdvancedFilters).toBe(true);
    });
  });

  describe('Plate State Management', () => {
    beforeEach(() => {
      component.plates = [
        { id: '1', isOpen: false } as any,
        { id: '2', isOpen: false } as any,
      ];
    });

    it('should open plate', () => {
      component.onPlateOpened('1');

      expect(component.plates[0].isOpen).toBe(true);
      expect(component.plates[1].isOpen).toBe(false);
    });

    it('should close plate', () => {
      component.plates[0].isOpen = true;

      component.onPlateClosed('1');

      expect(component.plates[0].isOpen).toBe(false);
    });
  });

  describe('Pagination', () => {
    it('should handle paginator change event', () => {
      spyOn(component, 'searchPlates' as any);
      const pageEvent: PageEvent = {
        pageIndex: 1,
        pageSize: 75,
        length: 100,
      };

      component.onPaginatorChange(pageEvent);

      expect(mockLocalStorageService.setData).toHaveBeenCalledWith('platePageSize', '75');
      expect(component.pageSize).toBe(75);
      expect((component as any).searchPlates).toHaveBeenCalled();
    });

    it('should handle paginator change event with valid page size only', () => {
      spyOn(component, 'searchPlates' as any);
      const pageEvent: PageEvent = {
        pageIndex: 2,
        pageSize: 100,
        length: 200,
      };

      component.onPaginatorChange(pageEvent);

      expect(mockLocalStorageService.setData).toHaveBeenCalledWith('platePageSize', '100');
      expect(component.pageSize).toBe(100);
      expect((component as any).searchPlates).toHaveBeenCalled();
    });
  });

  describe('Plate Operations', () => {
    beforeEach(() => {
      component.plates =
      [{ id: '1', plateNumber: 'ABC123' } as any];
    });

    it('should enrich plate successfully', () => {
      spyOn(component, 'searchPlates' as any);
      mockPlateService.enrichPlate.and.returnValue(of(null));

      component.onEnrichPlate('1');

      expect(component.isEnrichingPlate).toBe(false);
      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Plate enriched successfully',
        SnackBarType.Successful,
      );
      expect((component as any).searchPlates).toHaveBeenCalled();
    });

    it('should handle enrich plate error', () => {
      mockPlateService.enrichPlate.and.returnValue(throwError('Error'));

      component.onEnrichPlate('1');

      expect(component.isEnrichingPlate).toBe(false);
      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Failed to enrich plate',
        SnackBarType.Error,
      );
    });

    it('should open edit plate dialog', () => {
      const mockDialogRef = { afterClosed: () => of(true) };
      mockDialog.open.and.returnValue(mockDialogRef as any);
      spyOn(component, 'searchPlates' as any);

      component.onEditPlate('1');

      expect(mockDialog.open).toHaveBeenCalled();
      expect((component as any).searchPlates).toHaveBeenCalled();
    });

    it('should add plate to alert list', () => {
      spyOn(component, 'searchPlates' as any);
      mockAlertsService.addAlert.and.returnValue(of({}));

      component.onAlertPlate('1');

      expect(mockAlertsService.addAlert).toHaveBeenCalledWith(
        jasmine.objectContaining({ plateNumber: 'ABC123' }));

      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Added to alert list',
        SnackBarType.Successful,
      );
    });

    it('should handle add to alert list error', () => {
      mockAlertsService.addAlert.and.returnValue(throwError('Error'));

      component.onAlertPlate('1');

      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Failed to add to alert list',
        SnackBarType.Error,
      );
    });

    it('should add plate to ignore list', () => {
      spyOn(component, 'searchPlates' as any);
      mockIgnoresService.addIgnore.and.returnValue(of({}));

      component.onIgnorePlate('1');

      expect(mockIgnoresService.addIgnore).toHaveBeenCalledWith(
        jasmine.objectContaining({ plateNumber: 'ABC123' }));

      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Added to ignore list',
        SnackBarType.Successful,
      );
    });

    it('should search for plate', () => {
      spyOn(component, 'searchPlates' as any);

      component.onSearchForPlate('ABC123');

      expect((component as any).searchPlates).toHaveBeenCalledWith('ABC123');
    });
  });

  describe('Search Plates Method', () => {
    beforeEach(() => {
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should search plates successfully and update component state', () => {
      const mockSearchResult = {
        plates: [mockPlate],
        totalCount: 5,
      };
      mockPlateService.searchPlates.and.returnValue(of(mockSearchResult));

      component.onSearchTriggered();

      expect(component.isLoading).toBe(false);
      expect(component.plates.length).toBe(1);
      expect(component.totalNumberOfPlates).toBe(5);
      expect(component.plates[0].plateNumber).toBe('ABC123');
      expect(mockPlateService.searchPlates).toHaveBeenCalled();
    });

    it('should show error notification when search fails', () => {
      mockPlateService.searchPlates.and.returnValue(throwError('Search failed'));

      component.onSearchTriggered();

      expect(component.isLoading).toBe(false);
      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Search failed. Please try again.',
        SnackBarType.Error,
      );
    });

    it('should handle concurrent search requests correctly', () => {
      const firstSearchResult = {
        plates: [{ ...mockPlate, plateNumber: 'FIRST' }],
        totalCount: 1,
      };
      const secondSearchResult = {
        plates: [{ ...mockPlate, plateNumber: 'SECOND' }],
        totalCount: 1,
      };

      // First search takes longer (using defer to create observable from promise)
      let firstSearchResolve: any;
      const firstSearchObservable = defer(() => new Promise<any>(resolve => {
        firstSearchResolve = resolve;
      }));

      // Second search resolves immediately
      mockPlateService.searchPlates.and.returnValues(
        firstSearchObservable,
        of(secondSearchResult),
      );

      // Trigger first search
      component.onSearchTriggered();
      expect(component.isLoading).toBe(true);

      // Trigger second search
      component.onSearchTriggered();

      // Resolve first search after second one completes
      firstSearchResolve(firstSearchResult);

      // Only second search result should be displayed
      expect(component.plates[0].plateNumber).toBe('SECOND');
      expect(component.totalNumberOfPlates).toBe(1);
      expect(component.isLoading).toBe(false);
    });

    it('should handle concurrent search requests with error correctly', () => {
      // First search will error (using defer to create observable from promise)
      let firstSearchReject: any;
      const firstSearchObservable = defer(() => new Promise<any>((resolve, reject) => {
        firstSearchReject = reject;
      }));

      // Second search succeeds
      const secondSearchResult = {
        plates: [mockPlate],
        totalCount: 1,
      };

      mockPlateService.searchPlates.and.returnValues(
        firstSearchObservable,
        of(secondSearchResult),
      );

      // Trigger first search
      component.onSearchTriggered();
      expect(component.isLoading).toBe(true);

      // Trigger second search
      component.onSearchTriggered();

      // Reject first search after second one completes
      firstSearchReject('First search failed');

      // Should not show error notification for outdated request
      expect(mockSnackbarService.create).not.toHaveBeenCalledWith(
        'Search failed. Please try again.',
        SnackBarType.Error,
      );
      expect(component.isLoading).toBe(false);
    });

    it('should search with plate number parameter', () => {
      const plateNumber = 'TEST123';
      mockPlateService.searchPlates.and.returnValue(of({ plates: [], totalCount: 0 }));

      // Access private method for testing
      (component as any).searchPlates(plateNumber);

      expect(mockPlateService.searchPlates).toHaveBeenCalledWith(
        jasmine.objectContaining({
          plateNumber,
        }),
      );
    });

    it('should reset page number when searching', () => {
      // Set page number to something other than 0
      (component as any).pageNumber = 5;
      mockPlateService.searchPlates.and.returnValue(of({ plates: [], totalCount: 0 }));

      component.onSearchTriggered();

      expect((component as any).pageNumber).toBe(0);
    });

    it('should set loading state during search', () => {
      let searchResolve: any;
      const searchObservable = defer(() => new Promise<any>(resolve => {
        searchResolve = resolve;
      }));
      mockPlateService.searchPlates.and.returnValue(searchObservable);

      component.onSearchTriggered();

      expect(component.isLoading).toBe(true);

      searchResolve({ plates: [], totalCount: 0 });

      // Need to wait for promise resolution
      setTimeout(() => {
        expect(component.isLoading).toBe(false);
      });
    });
  });

  describe('Component Cleanup', () => {
    it('should call super.ngOnDestroy for cleanup', () => {
      component.ngOnInit();
      spyOn(Object.getPrototypeOf(Object.getPrototypeOf(component)), 'ngOnDestroy');

      component.ngOnDestroy();

      expect(Object.getPrototypeOf(Object.getPrototypeOf(component)).ngOnDestroy).toHaveBeenCalled();
    });
  });
});
