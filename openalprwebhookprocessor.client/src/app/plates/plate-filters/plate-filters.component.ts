import { ChangeDetectionStrategy, Component, input, output, inject, type OnDestroy, type OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
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
import { LocalStorageService } from '../../_services/local-storage.service';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';
import { RefreshButtonComponent } from 'app/shared/refresh-button/refresh-button.component';

export interface PlateFilters {
  startDate: Date | null;
  endDate: Date | null;
  plateNumber: string;
  cameraId: string;
  vehicleMake: string;
  vehicleModel: string;
  vehicleType: string;
  vehicleColor: string;
  direction: string;
  regexSearchEnabled: boolean;
  includeIgnoredPlates: boolean;
  platesSeenLessThan: boolean;
}

export interface VehicleFilters {
  vehicleMakes: string[];
  vehicleModels: string[];
  vehicleTypes: string[];
  vehicleColors: string[];
}

@Component({
  selector: 'app-plate-filters',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatTooltipModule,
    RefreshButtonComponent,
  ],
  templateUrl: './plate-filters.component.html',
  styleUrl: './plate-filters.component.less',
})
export class PlateFiltersComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  private readonly localStorageService = inject(LocalStorageService);

  readonly todaysDate = input(new Date());
  readonly cameras = input<string[]>([]);
  readonly vehicleFilters = input<VehicleFilters>({
    vehicleMakes: [],
    vehicleModels: [],
    vehicleTypes: [],
    vehicleColors: [],
  });
  readonly showAdvancedFilters = input(false);
  readonly isSearching = input(false);

  readonly filtersChanged = output<PlateFilters>();
  readonly searchTriggered = output<void>();
  readonly filtersCleared = output<void>();
  readonly advancedFiltersToggled = output<void>();

  // Filter properties
  filterStartOn: Date | null = null;
  filterEndOn: Date | null = null;
  filterPlateNumber = '';
  filterOpenAlprCameraId = '';
  filterVehicleMake = '';
  filterVehicleModel = '';
  filterVehicleType = '';
  filterVehicleColor = '';
  filterDirection = '';
  filterIncludeIgnoredPlatesEnabled = false;
  regexSearchEnabled = false;
  filterPlatesSeenLessThan = false;

  // Validation
  filterPlateNumberIsValid = true;
  filterDateRangeIsValid = true;

  ngOnInit() {
    this.loadFiltersFromStorage();
    this.validateDateRange();
    this.emitFilters();
    this.searchTriggered.emit();
    this.markForCheck();
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  validateSearchPlateNumber() {
    if (!this.filterPlateNumber) {
      this.filterPlateNumberIsValid = true;
      this.markForCheck();
      return;
    }

    if (this.regexSearchEnabled) {
      try {
        new RegExp(this.filterPlateNumber);
        this.filterPlateNumberIsValid = true;
      } catch {
        this.filterPlateNumberIsValid = false;
      }
    } else {
      this.filterPlateNumberIsValid = this.filterPlateNumber.length >= 2;
    }
    this.markForCheck();
  }

  validateDateRange() {
    if (!this.filterStartOn || !this.filterEndOn) {
      this.filterDateRangeIsValid = true;
      this.markForCheck();
      return;
    }

    this.filterDateRangeIsValid = this.filterStartOn <= this.filterEndOn;
    this.markForCheck();
  }

  onSearch() {
    if (this.filterPlateNumberIsValid && this.filterDateRangeIsValid) {
      this.saveFiltersToStorage();
      this.searchTriggered.emit();
    }
  }

  onClear() {
    this.setDefaultDateRange();
    this.filterPlateNumber = '';
    this.filterOpenAlprCameraId = '';
    this.filterVehicleMake = '';
    this.filterVehicleModel = '';
    this.filterVehicleType = '';
    this.filterVehicleColor = '';
    this.filterDirection = '';
    this.regexSearchEnabled = false;
    this.filterIncludeIgnoredPlatesEnabled = false;
    this.filterPlatesSeenLessThan = false;
    this.filterPlateNumberIsValid = true;
    this.filterDateRangeIsValid = true;

    this.clearFiltersFromStorage();
    this.markForCheck();

    this.emitFilters();
    this.filtersCleared.emit();
  }

  onToggleAdvanced() {
    this.advancedFiltersToggled.emit();
  }

  onFilterChange() {
    this.validateSearchPlateNumber();
    this.validateDateRange();
    this.emitFilters();
  }

  onStartDateChange() {
    if (this.filterStartOn) {
      this.filterStartOn = this.setToStartOfDay(this.filterStartOn);
    }
    this.validateDateRange();
    this.onFilterChange();
    this.markForCheck();
  }

  onEndDateChange() {
    if (this.filterEndOn) {
      this.filterEndOn = this.setToEndOfDay(this.filterEndOn);
    }
    this.validateDateRange();
    this.onFilterChange();
    this.markForCheck();
  }

  private emitFilters() {
    const filters: PlateFilters = {
      startDate: this.filterStartOn,
      endDate: this.filterEndOn,
      plateNumber: this.filterPlateNumber,
      cameraId: this.filterOpenAlprCameraId,
      vehicleMake: this.filterVehicleMake,
      vehicleModel: this.filterVehicleModel,
      vehicleType: this.filterVehicleType,
      vehicleColor: this.filterVehicleColor,
      direction: this.filterDirection,
      regexSearchEnabled: this.regexSearchEnabled,
      includeIgnoredPlates: this.filterIncludeIgnoredPlatesEnabled,
      platesSeenLessThan: this.filterPlatesSeenLessThan,
    };
    this.filtersChanged.emit(filters);
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
    this.filterStartOn = filters.startDate ? this.setToStartOfDay(new Date(filters.startDate)) : startDate;
    this.filterEndOn = filters.endDate ? this.setToEndOfDay(new Date(filters.endDate)) : endDate;
  }

  private setStringFiltersFromStorage(filters: Partial<PlateFilters>) {
    this.filterPlateNumber = filters.plateNumber ?? '';
    this.filterOpenAlprCameraId = filters.cameraId ?? '';
    this.filterVehicleMake = filters.vehicleMake ?? '';
    this.filterVehicleModel = filters.vehicleModel ?? '';
    this.filterVehicleType = filters.vehicleType ?? '';
    this.filterVehicleColor = filters.vehicleColor ?? '';
    this.filterDirection = filters.direction ?? '';
  }

  private setBooleanFiltersFromStorage(filters: Partial<PlateFilters>) {
    this.regexSearchEnabled = filters.regexSearchEnabled ?? false;
    this.filterIncludeIgnoredPlatesEnabled = filters.includeIgnoredPlates ?? false;
    this.filterPlatesSeenLessThan = filters.platesSeenLessThan ?? false;
  }

  private setDefaultDateRange() {
    const { startDate, endDate } = this.getDefaultDateRange();
    this.filterStartOn = startDate;
    this.filterEndOn = endDate;
  }

  private getDefaultDateRange() {
    const today = new Date();
    const sevenDaysAgo = new Date(today);
    sevenDaysAgo.setDate(today.getDate() - 7);
    return {
      startDate: this.setToStartOfDay(sevenDaysAgo),
      endDate: this.setToEndOfDay(new Date(today)),
    };
  }

  private setToStartOfDay(date: Date): Date {
    const newDate = new Date(date);
    newDate.setHours(0, 0, 0, 0);
    return newDate;
  }

  private setToEndOfDay(date: Date): Date {
    const newDate = new Date(date);
    newDate.setHours(23, 59, 59, 999);
    return newDate;
  }

  private saveFiltersToStorage() {
    const filters: PlateFilters = {
      startDate: this.filterStartOn,
      endDate: this.filterEndOn,
      plateNumber: this.filterPlateNumber,
      cameraId: this.filterOpenAlprCameraId,
      vehicleMake: this.filterVehicleMake,
      vehicleModel: this.filterVehicleModel,
      vehicleType: this.filterVehicleType,
      vehicleColor: this.filterVehicleColor,
      direction: this.filterDirection,
      regexSearchEnabled: this.regexSearchEnabled,
      includeIgnoredPlates: this.filterIncludeIgnoredPlatesEnabled,
      platesSeenLessThan: this.filterPlatesSeenLessThan,
    };
    this.localStorageService.setData('plateFilters', JSON.stringify(filters));
  }

  private clearFiltersFromStorage() {
    this.localStorageService.removeData('plateFilters');
  }

  get showRegexError(): boolean {
    return !this.filterPlateNumberIsValid && this.regexSearchEnabled;
  }

  get showSearchError(): boolean {
    return !this.filterPlateNumberIsValid && !this.regexSearchEnabled;
  }

  get showDateRangeError(): boolean {
    return !this.filterDateRangeIsValid;
  }

  get isSearchDisabled(): boolean {
    return !this.filterPlateNumberIsValid || !this.filterDateRangeIsValid;
  }
}
