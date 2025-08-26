import type { ComponentFixture } from '@angular/core/testing';
import { TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatNativeDateModule } from '@angular/material/core';
import type { WritableSignal } from '@angular/core';
import { signal } from '@angular/core';

import type { PlateFilters, VehicleFilters } from './plate-filters.component';
import { PlateFiltersComponent } from './plate-filters.component';
import { LocalStorageService } from '../../_services/local-storage.service';
import { RefreshButtonComponent } from 'app/shared/refresh-button/refresh-button.component';

describe('PlateFiltersComponent', () => {
  let component: PlateFiltersComponent;
  let fixture: ComponentFixture<PlateFiltersComponent>;
  let localStorageService: jasmine.SpyObj<LocalStorageService>;
  let plateFiltersSignal: WritableSignal<PlateFilters>;

  const mockVehicleFilters: VehicleFilters = {
    cameras: ['Camera1', 'Camera2', 'Camera3'],
    makes: ['Toyota', 'Honda', 'Ford'],
    models: ['Camry', 'Civic', 'F-150'],
    vehicleMakeModelMap: {
      'Toyota': ['Camry', 'Corolla', 'Prius'],
      'Honda': ['Civic', 'Accord', 'CR-V'],
      'Ford': ['F-150', 'Mustang', 'Explorer'],
    },
    types: ['Sedan', 'SUV', 'Truck'],
    colors: ['Red', 'Blue', 'White', 'Black'],
    regions: ['US', 'CA', 'EU'],
  };

  const mockPlateFilters: PlateFilters = {
    startDate: new Date('2023-01-01'),
    endDate: new Date('2023-01-07'),
    plateNumber: '',
    cameraId: '',
    vehicleMake: '',
    vehicleModel: '',
    vehicleType: '',
    vehicleColor: '',
    vehicleRegion: '',
    regexSearchEnabled: false,
    includeIgnoredPlates: false,
    platesSeenLessThan: false,
  };

  beforeEach(async () => {
    const localStorageSpy = jasmine.createSpyObj('LocalStorageService', ['getData', 'setData', 'removeData']);

    await TestBed.configureTestingModule({
      imports: [
        PlateFiltersComponent,
        ReactiveFormsModule,
        NoopAnimationsModule,
        MatCardModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatDatepickerModule,
        MatNativeDateModule,
        MatButtonModule,
        MatIconModule,
        MatCheckboxModule,
        MatTooltipModule,
        MatAutocompleteModule,
        RefreshButtonComponent,
      ],
      providers: [{ provide: LocalStorageService, useValue: localStorageSpy }],
    }).compileComponents();

    localStorageService = TestBed.inject(LocalStorageService) as jasmine.SpyObj<LocalStorageService>;
    fixture = TestBed.createComponent(PlateFiltersComponent);
    component = fixture.componentInstance;

    // Create a writable signal for plate filters
    plateFiltersSignal = signal({ ...mockPlateFilters });

    // Set up component inputs using the new signal-based API
    fixture.componentRef.setInput('vehicleFilters', mockVehicleFilters);
    fixture.componentRef.setInput('plateFilters', plateFiltersSignal);
    fixture.componentRef.setInput('todaysDate', new Date('2023-01-07'));
    fixture.componentRef.setInput('isSearching', false);
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Initialization', () => {
    it('should initialize with default values', () => {
      localStorageService.getData.and.returnValue(null as any);
      fixture.detectChanges();

      expect(component.filterPlateNumberIsValid).toBe(true);
      expect(component.filterDateRangeIsValid).toBe(true);
      expect((component as any).showAdvancedFilters).toBe(false);
    });

    it('should load filters from storage on init', () => {
      const savedFilters = JSON.stringify({
        plateNumber: 'ABC123',
        vehicleMake: 'Toyota',
        vehicleModel: 'Camry',
        startDate: '2023-01-01',
        endDate: '2023-01-07',
      });
      localStorageService.getData.and.returnValue(savedFilters);

      fixture.detectChanges();

      expect(localStorageService.getData).toHaveBeenCalledWith('plateFilters');
      // Note: The component modifies the signal internally, so we check the component's plateFilters() method
      expect(component.plateFilters().plateNumber).toBe('ABC123');
      expect(component.plateFilters().vehicleMake).toBe('Toyota');
      expect(component.plateFilters().vehicleModel).toBe('Camry');
    });

    it('should set default date range when no storage data exists', () => {
      localStorageService.getData.and.returnValue(null as any);

      fixture.detectChanges();

      const filters = component.plateFilters();
      expect(filters.startDate).toBeDefined();
      expect(filters.endDate).toBeDefined();
      expect(filters.startDate <= filters.endDate).toBe(true);
    });
  });

  describe('Basic Validation', () => {
    beforeEach(() => {
      localStorageService.getData.and.returnValue(null as any);
      fixture.detectChanges();
    });

    it('should have validation properties', () => {
      expect(component.filterPlateNumberIsValid).toBeDefined();
      expect(component.filterDateRangeIsValid).toBeDefined();
    });

    it('should have getter properties for error states', () => {
      expect(component.showRegexError).toBeDefined();
      expect(component.showSearchError).toBeDefined();
      expect(component.showDateRangeError).toBeDefined();
      expect(component.isSearchDisabled).toBeDefined();
      expect(component.isModelDisabled).toBeDefined();
    });
  });

  describe('Form Controls', () => {
    beforeEach(() => {
      localStorageService.getData.and.returnValue(null as any);
      fixture.detectChanges();
    });

    it('should have all required form controls', () => {
      expect(component.cameraControl).toBeDefined();
      expect(component.vehicleMakeControl).toBeDefined();
      expect(component.vehicleModelControl).toBeDefined();
      expect(component.vehicleTypeControl).toBeDefined();
      expect(component.vehicleColorControl).toBeDefined();
      expect(component.vehicleRegionControl).toBeDefined();
    });

    it('should initialize form controls with empty values', () => {
      expect(component.cameraControl.value).toBe('');
      expect(component.vehicleMakeControl.value).toBe('');
      expect(component.vehicleModelControl.value).toBe('');
      expect(component.vehicleTypeControl.value).toBe('');
      expect(component.vehicleColorControl.value).toBe('');
      expect(component.vehicleRegionControl.value).toBe('');
    });
  });

  describe('Autocomplete Setup', () => {
    beforeEach(() => {
      localStorageService.getData.and.returnValue(null as any);
      fixture.detectChanges();
    });

    it('should initialize filtered observables', () => {
      expect(component.filteredCameras$).toBeDefined();
      expect(component.filteredMakes$).toBeDefined();
      expect(component.filteredModels$).toBeDefined();
      expect(component.filteredTypes$).toBeDefined();
      expect(component.filteredColors$).toBeDefined();
      expect(component.filteredRegions$).toBeDefined();
    });
  });

  describe('Actions', () => {
    beforeEach(() => {
      localStorageService.getData.and.returnValue(null as any);
      fixture.detectChanges();
    });

    describe('Search', () => {
      it('should emit search triggered when validation passes', () => {
        spyOn(component.searchTriggered, 'emit');
        component.filterPlateNumberIsValid = true;
        component.filterDateRangeIsValid = true;

        (component as any).onSearch();

        expect(component.searchTriggered.emit).toHaveBeenCalled();
        expect(localStorageService.setData).toHaveBeenCalled();
      });

      it('should not emit search triggered when validation fails', () => {
        spyOn(component.searchTriggered, 'emit');
        component.filterPlateNumberIsValid = false;
        component.filterDateRangeIsValid = true;

        (component as any).onSearch();

        expect(component.searchTriggered.emit).not.toHaveBeenCalled();
      });
    });

    describe('Clear', () => {
      it('should clear filters and trigger search', () => {
        spyOn(component.searchTriggered, 'emit');

        (component as any).onClear();

        // Check validation states are reset
        expect(component.filterPlateNumberIsValid).toBe(true);
        expect(component.filterDateRangeIsValid).toBe(true);

        // Check storage is cleared and search is triggered
        expect(localStorageService.removeData).toHaveBeenCalledWith('plateFilters');
        expect(component.searchTriggered.emit).toHaveBeenCalled();
      });

      it('should set default date range when clearing', () => {
        (component as any).onClear();

        expect(component.plateFilters().startDate).toBeDefined();
        expect(component.plateFilters().endDate).toBeDefined();
        expect(component.plateFilters().startDate <= component.plateFilters().endDate).toBe(true);
      });
    });

    describe('Toggle Advanced', () => {
      it('should toggle advanced filters visibility', () => {
        expect((component as any).showAdvancedFilters).toBe(false);

        (component as any).onToggleAdvanced();
        expect((component as any).showAdvancedFilters).toBe(true);

        (component as any).onToggleAdvanced();
        expect((component as any).showAdvancedFilters).toBe(false);
      });
    });
  });

  describe('Local Storage Integration', () => {
    beforeEach(() => {
      localStorageService.getData.and.returnValue(null as any);
      fixture.detectChanges();
    });

    it('should save filters to storage on search', () => {
      component.filterPlateNumberIsValid = true;
      component.filterDateRangeIsValid = true;

      (component as any).onSearch();

      expect(localStorageService.setData).toHaveBeenCalledWith('plateFilters', jasmine.any(String));
    });

    it('should handle invalid JSON in storage gracefully', () => {
      localStorageService.getData.and.returnValue('invalid json');

      expect(() => {
        (component as any).loadFiltersFromStorage();
      }).not.toThrow();
    });
  });

  describe('Mobile Responsiveness', () => {
    beforeEach(() => {
      localStorageService.getData.and.returnValue(null as any);
      fixture.detectChanges();
    });

    it('should update mobile state on window resize', () => {
      // Simulate mobile width
      const mobileEvent = { target: { innerWidth: 400 } } as any;
      component.onResize(mobileEvent);

      expect((component as any).isMobile).toBe(true);

      // Simulate desktop width
      const desktopEvent = { target: { innerWidth: 1200 } } as any;
      component.onResize(desktopEvent);

      expect((component as any).isMobile).toBe(false);
    });
  });

  describe('Component Lifecycle', () => {
    beforeEach(() => {
      localStorageService.getData.and.returnValue(null as any);
    });

    it('should call ngOnInit without errors', () => {
      expect(() => {
        fixture.detectChanges();
      }).not.toThrow();
    });

    it('should emit search on initialization', () => {
      spyOn(component.searchTriggered, 'emit');

      fixture.detectChanges();

      expect(component.searchTriggered.emit).toHaveBeenCalled();
    });
  });
});
