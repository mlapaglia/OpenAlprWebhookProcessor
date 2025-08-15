import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { provideNativeDateAdapter } from '@angular/material/core';
import { PlateFiltersComponent, type PlateFilters, type VehicleFilters } from './plate-filters.component';
import { LocalStorageService } from '../../_services/local-storage.service';

describe('PlateFiltersComponent', () => {
  let component: PlateFiltersComponent;
  let fixture: ComponentFixture<PlateFiltersComponent>;
  let mockLocalStorageService: jasmine.SpyObj<LocalStorageService>;

  const mockVehicleFilters: VehicleFilters = {
    vehicleMakes: ['Toyota', 'Honda', 'Ford'],
    vehicleModels: ['Camry', 'Civic', 'F-150'],
    vehicleTypes: ['Sedan', 'SUV', 'Truck'],
    vehicleColors: ['Red', 'Blue', 'White'],
  };

  const mockCameras = ['camera1', 'camera2', 'camera3'];

  beforeEach(async () => {
    const spy = jasmine.createSpyObj('LocalStorageService', ['getData', 'setData', 'removeData']);

    await TestBed.configureTestingModule({
      imports: [
        PlateFiltersComponent,
        BrowserAnimationsModule,
      ],
      providers: [
        { provide: LocalStorageService, useValue: spy },
        provideNativeDateAdapter(),
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    mockLocalStorageService = TestBed.inject(LocalStorageService) as jasmine.SpyObj<LocalStorageService>;
    fixture = TestBed.createComponent(PlateFiltersComponent);
    component = fixture.componentInstance;
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should have default values', () => {
      expect(component.todaysDate()).toBeInstanceOf(Date);
      expect(component.cameras()).toEqual([]);
      expect(component.vehicleFilters()).toEqual({
        vehicleMakes: [],
        vehicleModels: [],
        vehicleTypes: [],
        vehicleColors: [],
      });
      expect(component.showAdvancedFilters()).toBe(false);
    });

    it('should initialize filter properties with default values', () => {
      expect(component.filterStartOn).toBeNull();
      expect(component.filterEndOn).toBeNull();
      expect(component.filterPlateNumber).toBe('');
      expect(component.filterOpenAlprCameraId).toBe('');
      expect(component.filterVehicleMake).toBe('');
      expect(component.filterVehicleModel).toBe('');
      expect(component.filterVehicleType).toBe('');
      expect(component.filterVehicleColor).toBe('');
      expect(component.filterDirection).toBe('');
      expect(component.regexSearchEnabled).toBe(false);
      expect(component.filterIncludeIgnoredPlatesEnabled).toBe(false);
      expect(component.filterPlatesSeenLessThan).toBe(false);
      expect(component.filterPlateNumberIsValid).toBe(true);
    });
  });

  describe('Input Properties', () => {
    it('should accept todaysDate input', () => {
      const testDate = new Date('2023-06-15');
      fixture.componentRef.setInput('todaysDate', testDate);
      expect(component.todaysDate()).toEqual(testDate);
    });

    it('should accept cameras input', () => {
      fixture.componentRef.setInput('cameras', mockCameras);
      expect(component.cameras()).toEqual(mockCameras);
    });

    it('should accept vehicleFilters input', () => {
      fixture.componentRef.setInput('vehicleFilters', mockVehicleFilters);
      expect(component.vehicleFilters()).toEqual(mockVehicleFilters);
    });

    it('should accept showAdvancedFilters input', () => {
      fixture.componentRef.setInput('showAdvancedFilters', true);
      expect(component.showAdvancedFilters()).toBe(true);
    });
  });

  describe('ngOnInit', () => {
    it('should call loadFiltersFromStorage and emitFilters', () => {
      spyOn(component, 'loadFiltersFromStorage' as any);
      spyOn(component, 'emitFilters' as any);
      mockLocalStorageService.getData.and.returnValue('');

      component.ngOnInit();

      expect(component['loadFiltersFromStorage']).toHaveBeenCalled();
      expect(component['emitFilters']).toHaveBeenCalled();
    });

    it('should load filters from storage when available', () => {
      const savedFilters = {
        startDate: '2023-01-01T00:00:00.000Z',
        endDate: '2023-12-31T23:59:59.999Z',
        plateNumber: 'TEST123',
        cameraId: 'camera1',
        vehicleMake: 'Toyota',
        vehicleModel: 'Camry',
        vehicleType: 'Sedan',
        vehicleColor: 'Blue',
        direction: '90',
        regexSearchEnabled: true,
        includeIgnoredPlates: false,
        platesSeenLessThan: true,
      };
      mockLocalStorageService.getData.and.returnValue(JSON.stringify(savedFilters));

      component.ngOnInit();

      expect(component.filterPlateNumber).toBe('TEST123');
      expect(component.filterOpenAlprCameraId).toBe('camera1');
      expect(component.filterVehicleMake).toBe('Toyota');
      expect(component.filterVehicleModel).toBe('Camry');
      expect(component.filterVehicleType).toBe('Sedan');
      expect(component.filterVehicleColor).toBe('Blue');
      expect(component.filterDirection).toBe('90');
      expect(component.regexSearchEnabled).toBe(true);
      expect(component.filterIncludeIgnoredPlatesEnabled).toBe(false);
      expect(component.filterPlatesSeenLessThan).toBe(true);
    });

    it('should set default dates when no saved filters', () => {
      mockLocalStorageService.getData.and.returnValue('');

      component.ngOnInit();

      expect(component.filterStartOn).toBeInstanceOf(Date);
      expect(component.filterEndOn).toBeInstanceOf(Date);
      expect(component.filterStartOn?.getHours()).toBe(0);
      expect(component.filterEndOn?.getHours()).toBe(23);
    });

    it('should handle invalid JSON in saved filters', () => {
      mockLocalStorageService.getData.and.returnValue('invalid json');

      // Create today reference before calling ngOnInit to avoid timing issues
      const today = new Date();
      const sevenDaysAgo = new Date(today);
      sevenDaysAgo.setDate(today.getDate() - 7);

      component.ngOnInit();

      // Should fallback to default date range when JSON parsing fails
      expect(component.filterStartOn).toBeInstanceOf(Date);
      expect(component.filterEndOn).toBeInstanceOf(Date);
      // Verify it set the correct default date range (7 days ago to today)
      expect(component.filterStartOn?.toDateString()).toBe(sevenDaysAgo.toDateString());
      expect(component.filterEndOn?.toDateString()).toBe(today.toDateString());
    });
  });

  describe('Validation', () => {
    describe('validateSearchPlateNumber', () => {
      it('should validate empty plate number as valid', () => {
        component.filterPlateNumber = '';
        component.validateSearchPlateNumber();
        expect(component.filterPlateNumberIsValid).toBe(true);
      });

      it('should validate regular plate number by length', () => {
        component.regexSearchEnabled = false;
        component.filterPlateNumber = 'AB';
        component.validateSearchPlateNumber();
        expect(component.filterPlateNumberIsValid).toBe(true);

        component.filterPlateNumber = 'A';
        component.validateSearchPlateNumber();
        expect(component.filterPlateNumberIsValid).toBe(false);
      });

      it('should validate regex when regex search is enabled', () => {
        component.regexSearchEnabled = true;

        component.filterPlateNumber = '[A-Z]{3}\\d{3}';
        component.validateSearchPlateNumber();
        expect(component.filterPlateNumberIsValid).toBe(true);

        component.filterPlateNumber = '[A-Z';
        component.validateSearchPlateNumber();
        expect(component.filterPlateNumberIsValid).toBe(false);
      });
    });
  });

  describe('Event Handlers', () => {
    beforeEach(() => {
      component.filterPlateNumberIsValid = true;
    });

    describe('onSearch', () => {
      it('should save filters and emit searchTriggered when validation passes', () => {
        spyOn(component, 'saveFiltersToStorage' as any);
        spyOn(component.searchTriggered, 'emit');

        component.onSearch();

        expect(component['saveFiltersToStorage']).toHaveBeenCalled();
        expect(component.searchTriggered.emit).toHaveBeenCalled();
      });

      it('should not trigger search when validation fails', () => {
        component.filterPlateNumberIsValid = false;
        spyOn(component, 'saveFiltersToStorage' as any);
        spyOn(component.searchTriggered, 'emit');

        component.onSearch();

        expect(component['saveFiltersToStorage']).not.toHaveBeenCalled();
        expect(component.searchTriggered.emit).not.toHaveBeenCalled();
      });
    });

    describe('onClear', () => {
      it('should reset all filter properties to defaults', () => {
        // Set some values first
        component.filterPlateNumber = 'TEST123';
        component.filterOpenAlprCameraId = 'camera1';
        component.filterVehicleMake = 'Toyota';
        component.filterIncludeIgnoredPlatesEnabled = false;
        component.regexSearchEnabled = true;

        component.onClear();

        expect(component.filterPlateNumber).toBe('');
        expect(component.filterOpenAlprCameraId).toBe('');
        expect(component.filterVehicleMake).toBe('');
        expect(component.filterVehicleModel).toBe('');
        expect(component.filterVehicleType).toBe('');
        expect(component.filterVehicleColor).toBe('');
        expect(component.filterDirection).toBe('');
        expect(component.regexSearchEnabled).toBe(false);
        expect(component.filterIncludeIgnoredPlatesEnabled).toBe(false);
        expect(component.filterPlatesSeenLessThan).toBe(false);
        expect(component.filterPlateNumberIsValid).toBe(true);
      });

      it('should reset dates to today', () => {
        component.onClear();

        expect(component.filterStartOn).toBeInstanceOf(Date);
        expect(component.filterEndOn).toBeInstanceOf(Date);
        expect(component.filterStartOn?.getHours()).toBe(0);
        expect(component.filterEndOn?.getHours()).toBe(23);
      });

      it('should clear filters from storage and emit filtersCleared', () => {
        spyOn(component, 'clearFiltersFromStorage' as any);
        spyOn(component.filtersCleared, 'emit');

        component.onClear();

        expect(component['clearFiltersFromStorage']).toHaveBeenCalled();
        expect(component.filtersCleared.emit).toHaveBeenCalled();
      });
    });

    describe('onToggleAdvanced', () => {
      it('should emit advancedFiltersToggled', () => {
        spyOn(component.advancedFiltersToggled, 'emit');

        component.onToggleAdvanced();

        expect(component.advancedFiltersToggled.emit).toHaveBeenCalled();
      });
    });

    describe('onFilterChange', () => {
      it('should validate plate number and emit filters', () => {
        spyOn(component, 'validateSearchPlateNumber');
        spyOn(component, 'emitFilters' as any);

        component.onFilterChange();

        expect(component.validateSearchPlateNumber).toHaveBeenCalled();
        expect(component['emitFilters']).toHaveBeenCalled();
      });
    });

    describe('onStartDateChange', () => {
      it('should set start date to start of day and trigger filter change', () => {
        const testDate = new Date('2023-06-15T14:30:00.000Z');
        component.filterStartOn = testDate;
        spyOn(component, 'onFilterChange');

        component.onStartDateChange();

        expect(component.filterStartOn?.getHours()).toBe(0);
        expect(component.filterStartOn?.getMinutes()).toBe(0);
        expect(component.filterStartOn?.getSeconds()).toBe(0);
        expect(component.filterStartOn?.getMilliseconds()).toBe(0);
        expect(component.onFilterChange).toHaveBeenCalled();
      });

      it('should handle null start date', () => {
        component.filterStartOn = null;
        spyOn(component, 'onFilterChange');

        component.onStartDateChange();

        expect(component.filterStartOn).toBeNull();
        expect(component.onFilterChange).toHaveBeenCalled();
      });
    });

    describe('onEndDateChange', () => {
      it('should set end date to end of day and trigger filter change', () => {
        const testDate = new Date('2023-06-15T14:30:00.000Z');
        component.filterEndOn = testDate;
        spyOn(component, 'onFilterChange');

        component.onEndDateChange();

        expect(component.filterEndOn?.getHours()).toBe(23);
        expect(component.filterEndOn?.getMinutes()).toBe(59);
        expect(component.filterEndOn?.getSeconds()).toBe(59);
        expect(component.filterEndOn?.getMilliseconds()).toBe(999);
        expect(component.onFilterChange).toHaveBeenCalled();
      });

      it('should handle null end date', () => {
        component.filterEndOn = null;
        spyOn(component, 'onFilterChange');

        component.onEndDateChange();

        expect(component.filterEndOn).toBeNull();
        expect(component.onFilterChange).toHaveBeenCalled();
      });
    });
  });

  describe('Filter Emission', () => {
    it('should emit correct filters object', () => {
      component.filterStartOn = new Date('2023-01-01');
      component.filterEndOn = new Date('2023-12-31');
      component.filterPlateNumber = 'TEST123';
      component.filterOpenAlprCameraId = 'camera1';
      component.filterVehicleMake = 'Toyota';
      component.filterVehicleModel = 'Camry';
      component.filterVehicleType = 'Sedan';
      component.filterVehicleColor = 'Blue';
      component.filterDirection = '90';
      component.regexSearchEnabled = true;
      component.filterIncludeIgnoredPlatesEnabled = false;
      component.filterPlatesSeenLessThan = true;

      spyOn(component.filtersChanged, 'emit');

      component['emitFilters']();

      const expectedFilters: PlateFilters = {
        startDate: component.filterStartOn,
        endDate: component.filterEndOn,
        plateNumber: 'TEST123',
        cameraId: 'camera1',
        vehicleMake: 'Toyota',
        vehicleModel: 'Camry',
        vehicleType: 'Sedan',
        vehicleColor: 'Blue',
        direction: '90',
        regexSearchEnabled: true,
        includeIgnoredPlates: false,
        platesSeenLessThan: true,
      };

      expect(component.filtersChanged.emit).toHaveBeenCalledWith(expectedFilters);
    });
  });

  describe('Local Storage Integration', () => {
    describe('saveFiltersToStorage', () => {
      it('should save filters to localStorage', () => {
        component.filterPlateNumber = 'TEST123';
        component.filterVehicleMake = 'Toyota';

        component['saveFiltersToStorage']();

        expect(mockLocalStorageService.setData).toHaveBeenCalledWith(
          'plateFilters',
          jasmine.any(String),
        );
      });
    });

    describe('clearFiltersFromStorage', () => {
      it('should remove filters from localStorage', () => {
        component['clearFiltersFromStorage']();

        expect(mockLocalStorageService.removeData).toHaveBeenCalledWith('plateFilters');
      });
    });
  });

  describe('Date Utility Methods', () => {
    describe('setToStartOfDay', () => {
      it('should set time to start of day', () => {
        const inputDate = new Date('2023-06-15T14:30:45.500Z');
        const result = component['setToStartOfDay'](inputDate);

        expect(result.getHours()).toBe(0);
        expect(result.getMinutes()).toBe(0);
        expect(result.getSeconds()).toBe(0);
        expect(result.getMilliseconds()).toBe(0);
        expect(result.getDate()).toBe(inputDate.getDate());
        expect(result.getMonth()).toBe(inputDate.getMonth());
        expect(result.getFullYear()).toBe(inputDate.getFullYear());
      });

      it('should not modify the original date', () => {
        const inputDate = new Date('2023-06-15T14:30:45.500Z');
        const originalTime = inputDate.getTime();

        component['setToStartOfDay'](inputDate);

        expect(inputDate.getTime()).toBe(originalTime);
      });
    });

    describe('setToEndOfDay', () => {
      it('should set time to end of day', () => {
        const inputDate = new Date('2023-06-15T14:30:45.500Z');
        const result = component['setToEndOfDay'](inputDate);

        expect(result.getHours()).toBe(23);
        expect(result.getMinutes()).toBe(59);
        expect(result.getSeconds()).toBe(59);
        expect(result.getMilliseconds()).toBe(999);
        expect(result.getDate()).toBe(inputDate.getDate());
        expect(result.getMonth()).toBe(inputDate.getMonth());
        expect(result.getFullYear()).toBe(inputDate.getFullYear());
      });

      it('should not modify the original date', () => {
        const inputDate = new Date('2023-06-15T14:30:45.500Z');
        const originalTime = inputDate.getTime();

        component['setToEndOfDay'](inputDate);

        expect(inputDate.getTime()).toBe(originalTime);
      });
    });
  });

  describe('Edge Cases', () => {
    it('should handle includeIgnoredPlates null/undefined values correctly', () => {
      const savedFilters = {
        includeIgnoredPlates: null,
      };
      mockLocalStorageService.getData.and.returnValue(JSON.stringify(savedFilters));

      component.ngOnInit();

      expect(component.filterIncludeIgnoredPlatesEnabled).toBe(false);
    });

    it('should handle missing properties in saved filters', () => {
      const incompleteFilters = {
        plateNumber: 'TEST123',
        // Missing other properties
      };
      mockLocalStorageService.getData.and.returnValue(JSON.stringify(incompleteFilters));

      component.ngOnInit();

      expect(component.filterPlateNumber).toBe('TEST123');
      expect(component.filterVehicleMake).toBe('');
      expect(component.filterIncludeIgnoredPlatesEnabled).toBe(false);
    });
  });
});
