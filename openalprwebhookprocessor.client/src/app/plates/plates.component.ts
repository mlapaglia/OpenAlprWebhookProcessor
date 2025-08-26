import { Component, inject, type OnDestroy, type OnInit, ChangeDetectionStrategy, input, model } from '@angular/core';
import { SignalrService } from 'app/signalr/signalr.service';
import { type Plate } from './plate/plate';
import { type PlateRequest, PlateService } from './plate.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { type Ignore } from 'app/settings/ignores/ignore';
import { IgnoresService } from 'app/settings/ignores/ignores.service';
import { type Alert } from 'app/settings/alerts/alert';
import { AlertsService } from 'app/settings/alerts/alerts.service';
import { MatDialog } from '@angular/material/dialog';
import { EditPlateComponent } from './edit-plate/edit-plate.component';
import { LocalStorageService } from 'app/_services/local-storage.service';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { type PageEvent } from '@angular/material/paginator';
import { PlateFiltersComponent, type PlateFilters, type VehicleFilters as FilterVehicleFilters } from './plate-filters/plate-filters.component';
import { PlateListComponent } from './plate-list/plate-list.component';
import { type PlateData } from './plate-item/plate-item.component';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

@Component({
  selector: 'app-plates',
  templateUrl: './plates.component.html',
  styleUrls: ['./plates.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    CommonModule,
    PlateFiltersComponent,
    PlateListComponent,
  ],
})
export class PlatesComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  private readonly plateService = inject(PlateService);
  private readonly signalRHub = inject(SignalrService);
  private readonly snackbarService = inject(SnackbarService);
  private readonly alertsService = inject(AlertsService);
  private readonly ignoresService = inject(IgnoresService);
  private readonly localStorageService = inject(LocalStorageService);
  private readonly route = inject(ActivatedRoute);
  private readonly dialog = inject(MatDialog);

  readonly id = input.required<string>();
  readonly plateFilters = model<PlateFilters>({
    startDate: this.getSevenDaysAgo(),
    endDate: this.setToEndOfDay(new Date()),
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
  });

  // Core data
  public plates: PlateData[] = [];
  public totalNumberOfPlates = 0;
  public isLoading = false;

  // Filter and pagination state
  public pageSize = 25;
  private pageNumber = 0;
  private currentRequestId = 0;

  // Data for filters
  public todaysDate = new Date();
  public vehicleFilters: FilterVehicleFilters = {
    cameras: [],
    makes: [],
    models: [],
    types: [],
    colors: [],
    regions: [],
  };

  // Loading states
  public isDeletingPlate = false;
  public isEnrichingPlate = false;
  public isAddingToIgnoreList = false;
  public isAddingToAlertList = false;

  private readonly pageSizeCacheKey = 'platePageSize';

  ngOnInit(): void {
    this.initializePageSize();
    this.populateFilters();
    this.subscribeAndMarkForCheck(
      this.route.params,
      ({ id }) => {
        if (id !== undefined) {
          this.getPlate(id as string);
        }
      },
    );
    this.subscribeForUpdates();
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  private getSevenDaysAgo(): Date {
    const date = new Date();
    date.setDate(date.getDate() - 7);
    return this.setToStartOfDay(date);
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

  private initializePageSize(): void {
    const savedPageSize = this.localStorageService.getData(this.pageSizeCacheKey);
    if (savedPageSize) {
      try {
        const parsedSize = JSON.parse(savedPageSize);
        const size = typeof parsedSize === 'number' ? parsedSize : parseInt(parsedSize, 10);
        if ([10, 25, 75, 100].includes(size)) {
          this.pageSize = size;
        }
      } catch {
        this.pageSize = 25;
      }
    }
  }

  get hasVehicleFiltersData(): boolean {
    return this.vehicleFilters.makes.length > 0 ||
           this.vehicleFilters.models.length > 0 ||
           this.vehicleFilters.types.length > 0 ||
           this.vehicleFilters.colors.length > 0 ||
           this.vehicleFilters.regions.length > 0;
  }

  private populateFilters() {
    this.subscribeAndMarkForCheck(
      this.plateService.getFilters(),
      (vehicleFilters) => {
        this.vehicleFilters.makes = vehicleFilters.vehicleMakes ?? [];
        this.vehicleFilters.models = vehicleFilters.vehicleModels ?? [];
        this.vehicleFilters.vehicleMakeModelMap = vehicleFilters.vehicleMakeModelMap ?? {};
        this.vehicleFilters.types = vehicleFilters.vehicleTypes ?? [];
        this.vehicleFilters.colors = vehicleFilters.vehicleColors ?? [];
        this.vehicleFilters.regions = vehicleFilters.vehicleRegions ?? [];
      },
    );
  }

  private subscribeForUpdates() {
    this.subscribeAndMarkForCheck(
      this.signalRHub.licensePlateReceived,
      () => {
        if (!this.isLoading) {
          this.searchPlates();
        }
      },
    );
  }

  private getPlate(id: string) {
    this.currentRequestId++;
    const requestId = this.currentRequestId;

    this.isLoading = true;
    this.markForCheck();

    this.plateService.getPlate(id).subscribe({
      next: (result) => {
        if (requestId === this.currentRequestId) {
          this.plates = [this.mapToPlateData(result.plate)];
          this.totalNumberOfPlates = 1;
          this.isLoading = false;
          this.markForCheck();
        }
      },
      error: (_error) => {
        if (requestId === this.currentRequestId) {
          this.isLoading = false;
          this.markForCheck();
        }
      },
    });
  }

  private searchPlates(plateNumber: string | null = null, resetPage: boolean = true) {
    this.currentRequestId++;
    const requestId = this.currentRequestId;

    if (resetPage) {
      this.pageNumber = 0;
    }
    this.isLoading = true;
    this.markForCheck();

    const request = this.buildPlateRequest(plateNumber);

    this.plateService.searchPlates(request).subscribe({
      next: (result) => {
        if (requestId === this.currentRequestId) {
          this.plates = result.plates.map(plate => this.mapToPlateData(plate));
          this.totalNumberOfPlates = result.totalCount;
          this.isLoading = false;
          this.markForCheck();
        }
      },
      error: (_error) => {
        if (requestId === this.currentRequestId) {
          this.isLoading = false;
          this.snackbarService.create('Search failed. Please try again.', SnackBarType.Error);
          this.markForCheck();
        }
      },
    });
  }

  private buildPlateRequest(plateNumber: string | null = null): PlateRequest {
    const filters = this.plateFilters();

    return {
      pageSize: this.pageSize,
      pageNumber: this.pageNumber,
      ...this.buildDateFilters(filters),
      ...this.buildVehicleFilters(filters),
      plateNumber: plateNumber ?? filters?.plateNumber ?? '',
      strictMatch: false,
      ...this.buildBooleanFilters(filters),
    };
  }

  private buildDateFilters(filters: PlateFilters | null) {
    return {
      startSearchOn: this.setToStartOfDay(filters?.startDate ?? new Date()),
      endSearchOn: this.setToEndOfDay(filters?.endDate ?? new Date()),
    };
  }

  private buildVehicleFilters(filters: PlateFilters | null) {
    const getStringValue = (value: string | undefined) => value ?? '';

    return {
      vehicleMake: getStringValue(filters?.vehicleMake),
      vehicleModel: getStringValue(filters?.vehicleModel),
      vehicleType: getStringValue(filters?.vehicleType),
      vehicleColor: getStringValue(filters?.vehicleColor),
      vehicleRegion: getStringValue(filters?.vehicleRegion),
    };
  }

  private buildBooleanFilters(filters: PlateFilters | null) {
    return {
      includeIgnoredPlates: filters?.includeIgnoredPlates ?? true,
      filterPlatesSeenLessThan: filters?.platesSeenLessThan ? 10 : 0,
      regexSearchEnabled: filters?.regexSearchEnabled ?? false,
    };
  }

  private mapToPlateData(plate: Plate): PlateData {
    return {
      id: plate.id,
      plateNumber: plate.plateNumber,
      openAlprCameraId: plate.openAlprCameraId,
      vehicleDescription: plate.vehicleDescription,
      direction: plate.direction,
      receivedOn: new Date(plate.receivedOn),
      isAlert: plate.isAlert,
      isIgnore: plate.isIgnore,
      isOpen: false,
      imageUrl: plate.imageUrl,
      cropImageUrl: plate.cropImageUrl,
      processedPlateConfidence: plate.processedPlateConfidence,
      notes: plate.notes,
      canBeEnriched: plate.canBeEnriched,
    };
  }

  onSearchTriggered() {
    this.searchPlates();
  }

  onPlateOpened(plateId: string) {
    const plate = this.plates.find(p => p.id === plateId);
    if (plate) {
      plate.isOpen = true;
      this.markForCheck();
    }
  }

  onPlateClosed(plateId: string) {
    const plate = this.plates.find(p => p.id === plateId);
    if (plate) {
      plate.isOpen = false;
      this.markForCheck();
    }
  }

  onPaginatorChange(event: PageEvent) {
    if (event.pageSize && event.pageSize > 0) {
      this.localStorageService.setData(this.pageSizeCacheKey, JSON.stringify(event.pageSize));
      this.pageSize = event.pageSize;
    }
    this.pageNumber = event.pageIndex;
    this.searchPlates(null, false);
  }

  onEnrichPlate(plateId: string) {
    this.isEnrichingPlate = true;
    this.markForCheck();

    this.plateService.enrichPlate(plateId.toString()).subscribe({
      next: () => {
        this.isEnrichingPlate = false;
        this.snackbarService.create('Plate enriched successfully', SnackBarType.Successful);
        this.searchPlates();
      },
      error: () => {
        this.isEnrichingPlate = false;
        this.snackbarService.create('Failed to enrich plate', SnackBarType.Error);
        this.markForCheck();
      },
    });
  }

  onEditPlate(plateId: string) {
    const plateData = this.plates.find(p => p.id === plateId);
    if (plateData) {
      const dialogRef = this.dialog.open(EditPlateComponent, {
        data: {
          plateId: plateData.id.toString(),
          currentPlateNumber: plateData.plateNumber,
        },
      });

      dialogRef.afterClosed().subscribe((result) => {
        if (result) {
          this.searchPlates();
        }
      });
    }
  }

  onAlertPlate(plateId: string) {
    const plate = this.plates.find(p => p.id === plateId);
    if (plate) {
      this.addToAlertList(plate.id, plate.plateNumber);
    }
  }

  onIgnorePlate(plateId: string) {
    const plate = this.plates.find(p => p.id === plateId);
    if (plate) {
      this.addToIgnoreList(plate.id, plate.plateNumber);
    }
  }

  onSearchForPlate(plateNumber: string) {
    this.plateFilters().plateNumber = plateNumber;
    this.searchPlates(plateNumber);
  }

  private addToAlertList(plateId: string, plateNumber: string) {
    this.isAddingToAlertList = true;
    this.markForCheck();

    const alert: Alert = {
      id: plateId,
      plateNumber,
      description: '',
      strictMatch: false,
    };

    this.alertsService.addAlert(alert).subscribe({
      next: () => {
        this.isAddingToAlertList = false;
        this.snackbarService.create('Added to alert list', SnackBarType.Successful);
        this.searchPlates();
      },
      error: () => {
        this.isAddingToAlertList = false;
        this.snackbarService.create('Failed to add to alert list', SnackBarType.Error);
        this.markForCheck();
      },
    });
  }

  private addToIgnoreList(plateId: string, plateNumber: string) {
    this.isAddingToIgnoreList = true;
    this.markForCheck();

    const ignore: Ignore = {
      id: plateId,
      plateNumber,
      description: '',
      strictMatch: false,
    };

    this.ignoresService.addIgnore(ignore).subscribe({
      next: () => {
        this.isAddingToIgnoreList = false;
        this.snackbarService.create('Added to ignore list', SnackBarType.Successful);
        this.searchPlates();
      },
      error: () => {
        this.isAddingToIgnoreList = false;
        this.snackbarService.create('Failed to add to ignore list', SnackBarType.Error);
        this.markForCheck();
      },
    });
  }
}
