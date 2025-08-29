import { ChangeDetectionStrategy, Component, input, output, inject, type OnDestroy, type OnInit, model, HostListener } from '@angular/core';
import { FormsModule, ReactiveFormsModule, FormControl } from '@angular/forms';
import type { Observable } from 'rxjs';
import { map, startWith, combineLatest } from 'rxjs';
import { CommonModule } from '@angular/common';
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
import { LocalStorageService } from '../../_services/local-storage.service';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';
import { RefreshButtonComponent } from 'app/shared/refresh-button/refresh-button.component';

export interface PlateFilters {
  startDate: Date;
  endDate: Date;
  plateNumber: string;
  cameraId: string;
  vehicleMake: string;
  vehicleModel: string;
  vehicleType: string;
  vehicleColor: string;
  vehicleRegion: string;
  regexSearchEnabled: boolean;
  includeIgnoredPlates: boolean;
  platesSeenLessThan: boolean;
}

export interface VehicleFilters {
  cameras: string[],
  makes: string[];
  models: string[];
  vehicleMakeModelMap?: Record<string, string[]>;
  types: string[];
  colors: string[];
  regions: string[];
}

@Component({
  selector: 'app-plate-filters',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatTooltipModule,
    MatAutocompleteModule,
    RefreshButtonComponent,
  ],
  templateUrl: './plate-filters.component.html',
  styleUrl: './plate-filters.component.less',
})
export class PlateFiltersComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  private readonly localStorageService = inject(LocalStorageService);

  readonly todaysDate = input(new Date());
  readonly isSearching = input(false);
  readonly vehicleFilters = input.required<VehicleFilters>();

  readonly searchTriggered = output<void>();

  readonly plateFilters = model.required<PlateFilters>();

  // Validation
  filterPlateNumberIsValid = true;
  filterDateRangeIsValid = true;

  protected showAdvancedFilters = false;
  protected isMobile = false;

  @HostListener('window:resize', ['$event'])
  onResize(event: Event) {
    const target = event.target as Window;
    this.updateMobileState(target.innerWidth);
  }

  // Form controls for autocomplete
  cameraControl = new FormControl('');
  vehicleMakeControl = new FormControl('');
  vehicleModelControl = new FormControl('');
  vehicleTypeControl = new FormControl('');
  vehicleColorControl = new FormControl('');
  vehicleRegionControl = new FormControl('');

  // Filtered options for autocomplete
  filteredCameras$: Observable<string[]>;
  filteredMakes$: Observable<string[]>;
  filteredModels$: Observable<string[]>;
  filteredTypes$: Observable<string[]>;
  filteredColors$: Observable<string[]>;
  filteredRegions$: Observable<string[]>;

  ngOnInit() {
    this.updateMobileState(window.innerWidth);
    this.loadFiltersFromStorage();
    this.validateDateRange();
    this.setupFilteredOptions();
    this.syncFormControlsWithModel();
    this.setupFormControlSubscriptions();
    this.searchTriggered.emit();
    this.markForCheck();
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  private updateMobileState(width: number) {
    const wasMobile = this.isMobile;
    this.isMobile = width <= 768;

    if (wasMobile !== this.isMobile) {
      this.markForCheck();
    }
  }

  protected validateSearchPlateNumber() {
    if (!this.plateFilters().plateNumber) {
      this.filterPlateNumberIsValid = true;
      this.markForCheck();
      return;
    }

    if (this.plateFilters().regexSearchEnabled) {
      try {
        new RegExp(this.plateFilters().plateNumber);
        this.filterPlateNumberIsValid = true;
      } catch {
        this.filterPlateNumberIsValid = false;
      }
    } else {
      this.filterPlateNumberIsValid = this.plateFilters().plateNumber.length >= 2;
    }
    this.markForCheck();
  }

  protected validateDateRange() {
    const filters = this.plateFilters();

    this.filterDateRangeIsValid = filters.startDate <= filters.endDate;
    this.markForCheck();
  }

  protected onSearch() {
    if (this.filterPlateNumberIsValid && this.filterDateRangeIsValid) {
      this.saveFiltersToStorage();
      this.searchTriggered.emit();
    }
  }

  protected onClear() {
    this.setDefaultDateRange();
    this.plateFilters().plateNumber = '';
    this.plateFilters().cameraId = '';
    this.plateFilters().vehicleMake = '';
    this.plateFilters().vehicleModel = '';
    this.plateFilters().vehicleType = '';
    this.plateFilters().vehicleColor = '';
    this.plateFilters().vehicleRegion = '';
    this.plateFilters().regexSearchEnabled = false;
    this.plateFilters().includeIgnoredPlates = false;
    this.plateFilters().platesSeenLessThan = false;
    this.filterPlateNumberIsValid = true;
    this.filterDateRangeIsValid = true;

    // Clear form controls for autocomplete inputs
    this.cameraControl.setValue('');
    this.vehicleMakeControl.setValue('');
    this.vehicleModelControl.setValue('');
    this.vehicleTypeControl.setValue('');
    this.vehicleColorControl.setValue('');
    this.vehicleRegionControl.setValue('');

    this.clearFiltersFromStorage();
    this.searchTriggered.emit();
    this.markForCheck();
  }

  protected onFilterChange() {
    this.validateSearchPlateNumber();
    this.validateDateRange();
  }

  protected onToggleAdvanced() {
    this.showAdvancedFilters = !this.showAdvancedFilters;
    this.markForCheck();
  }

  protected onStartDateChange() {
    this.validateDateRange();
    this.onFilterChange();
    this.markForCheck();
  }

  protected onEndDateChange() {
    this.validateDateRange();
    this.onFilterChange();
    this.markForCheck();
  }

  private loadFiltersFromStorage() {
    const savedFilters = this.localStorageService.getData('plateFilters');
    if (savedFilters) {
      try {
        const filters = JSON.parse(savedFilters) as Partial<PlateFilters>;
        this.applyFiltersFromStorage(filters);
      } catch {
        this.setDefaultDateRange();
      }
    } else {
      this.setDefaultDateRange();
    }
    this.markForCheck();
  }

  private applyFiltersFromStorage(filters: Partial<PlateFilters>) {
    this.setDateRangeFromStorage(filters);
    this.setStringFiltersFromStorage(filters);
    this.setBooleanFiltersFromStorage(filters);
  }

  private setDateRangeFromStorage(filters: Partial<PlateFilters>) {
    const { startDate, endDate } = this.getDefaultDateRange();
    this.plateFilters().startDate = filters.startDate ? new Date(filters.startDate) : startDate;
    this.plateFilters().endDate = filters.endDate ? new Date(filters.endDate) : endDate;
  }

  private setStringFiltersFromStorage(filters: Partial<PlateFilters>) {
    this.plateFilters().plateNumber = filters.plateNumber ?? '';
    this.plateFilters().cameraId = filters.cameraId ?? '';
    this.plateFilters().vehicleMake = filters.vehicleMake ?? '';
    this.plateFilters().vehicleModel = filters.vehicleModel ?? '';
    this.plateFilters().vehicleType = filters.vehicleType ?? '';
    this.plateFilters().vehicleColor = filters.vehicleColor ?? '';
    this.plateFilters().vehicleRegion = filters.vehicleRegion ?? '';
  }

  private setBooleanFiltersFromStorage(filters: Partial<PlateFilters>) {
    this.plateFilters().regexSearchEnabled = filters.regexSearchEnabled ?? false;
    this.plateFilters().includeIgnoredPlates = filters.includeIgnoredPlates ?? false;
    this.plateFilters().platesSeenLessThan = filters.platesSeenLessThan ?? false;
  }

  private setDefaultDateRange() {
    const { startDate, endDate } = this.getDefaultDateRange();
    this.plateFilters().startDate = startDate;
    this.plateFilters().endDate = endDate;
  }

  private getDefaultDateRange() {
    const today = new Date();
    const sevenDaysAgo = new Date(today);
    sevenDaysAgo.setDate(today.getDate() - 7);
    return {
      startDate: sevenDaysAgo,
      endDate: new Date(today),
    };
  }

  private saveFiltersToStorage() {
    const filters: PlateFilters = {
      startDate: this.plateFilters().startDate,
      endDate: this.plateFilters().endDate,
      plateNumber: this.plateFilters().plateNumber,
      cameraId: this.plateFilters().cameraId,
      vehicleMake: this.plateFilters().vehicleMake,
      vehicleModel: this.plateFilters().vehicleModel,
      vehicleType: this.plateFilters().vehicleType,
      vehicleColor: this.plateFilters().vehicleColor,
      vehicleRegion: this.plateFilters().vehicleRegion,
      regexSearchEnabled: this.plateFilters().regexSearchEnabled,
      includeIgnoredPlates: this.plateFilters().includeIgnoredPlates,
      platesSeenLessThan: this.plateFilters().platesSeenLessThan,
    };
    this.localStorageService.setData('plateFilters', JSON.stringify(filters));
  }

  private clearFiltersFromStorage() {
    this.localStorageService.removeData('plateFilters');
  }

  get showRegexError(): boolean {
    return !this.filterPlateNumberIsValid && this.plateFilters().regexSearchEnabled;
  }

  get showSearchError(): boolean {
    return !this.filterPlateNumberIsValid && !this.plateFilters().regexSearchEnabled;
  }

  get showDateRangeError(): boolean {
    return !this.filterDateRangeIsValid;
  }

  get isSearchDisabled(): boolean {
    return !this.filterPlateNumberIsValid || !this.filterDateRangeIsValid;
  }



  private filterOptions(value: string, options: string[]): string[] {
    const filterValue = value.toLowerCase();
    return options.filter(option => option.toLowerCase().includes(filterValue));
  }

  private syncFormControlsWithModel(): void {
    const filters = this.plateFilters();
    this.cameraControl.setValue(filters.cameraId || '');
    this.vehicleMakeControl.setValue(filters.vehicleMake || '');
    this.vehicleModelControl.setValue(filters.vehicleModel || '');
    this.vehicleTypeControl.setValue(filters.vehicleType || '');
    this.vehicleColorControl.setValue(filters.vehicleColor || '');
    this.vehicleRegionControl.setValue(filters.vehicleRegion || '');

    if (filters.vehicleMake) {
      this.vehicleModelControl.enable();
    } else {
      this.vehicleModelControl.disable();
    }
  }

  private setupFilteredOptions(): void {
    this.filteredCameras$ = this.cameraControl.valueChanges.pipe(
      startWith(''),
      map(value => this.filterOptions(value ?? '', this.vehicleFilters().cameras)),
    );

    this.filteredMakes$ = this.vehicleMakeControl.valueChanges.pipe(
      startWith(''),
      map(value => this.filterOptions(value ?? '', this.vehicleFilters().makes)),
    );

    this.filteredModels$ = combineLatest([
      this.vehicleMakeControl.valueChanges.pipe(startWith(this.vehicleMakeControl.value ?? '')),
      this.vehicleModelControl.valueChanges.pipe(startWith(this.vehicleModelControl.value ?? '')),
    ]).pipe(
      map(([makeValue, modelValue]) => {
        const selectedMake = makeValue ?? this.plateFilters().vehicleMake;
        const makeModelMap = this.vehicleFilters().vehicleMakeModelMap;
        if (!selectedMake || !makeModelMap) {
          return [];
        }
        // Find the make in the map (case-insensitive)
        const makeKey = Object.keys(makeModelMap).find(key =>
          key.toLowerCase() === selectedMake.toLowerCase(),
        );
        if (!makeKey) {
          return [];
        }
        const availableModels = makeModelMap[makeKey] ?? [];
        return this.filterOptions(modelValue ?? '', availableModels);
      }),
    );

    this.filteredTypes$ = this.vehicleTypeControl.valueChanges.pipe(
      startWith(''),
      map(value => this.filterOptions(value ?? '', this.vehicleFilters().types)),
    );

    this.filteredColors$ = this.vehicleColorControl.valueChanges.pipe(
      startWith(''),
      map(value => this.filterOptions(value ?? '', this.vehicleFilters().colors)),
    );

    this.filteredRegions$ = this.vehicleRegionControl.valueChanges.pipe(
      startWith(''),
      map(value => this.filterOptions(value ?? '', this.vehicleFilters().regions)),
    );
  }

  private setupFormControlSubscriptions(): void {
    this.cameraControl.valueChanges.subscribe(value => {
      this.plateFilters().cameraId = value ?? '';
      this.onFilterChange();
    });

    this.vehicleMakeControl.valueChanges.subscribe(value => {
      this.plateFilters().vehicleMake = value ?? '';
      // Clear model when make changes
      if (this.plateFilters().vehicleModel) {
        this.plateFilters().vehicleModel = '';
        this.vehicleModelControl.setValue('');
      }
      // Enable/disable model control based on make selection
      if (value) {
        this.vehicleModelControl.enable();
      } else {
        this.vehicleModelControl.disable();
      }
      this.onFilterChange();
    });

    this.vehicleModelControl.valueChanges.subscribe(value => {
      this.plateFilters().vehicleModel = value ?? '';
      this.onFilterChange();
    });

    this.vehicleTypeControl.valueChanges.subscribe(value => {
      this.plateFilters().vehicleType = value ?? '';
      this.onFilterChange();
    });

    this.vehicleColorControl.valueChanges.subscribe(value => {
      this.plateFilters().vehicleColor = value ?? '';
      this.onFilterChange();
    });

    this.vehicleRegionControl.valueChanges.subscribe(value => {
      this.plateFilters().vehicleRegion = value ?? '';
      this.onFilterChange();
    });
  }
}
