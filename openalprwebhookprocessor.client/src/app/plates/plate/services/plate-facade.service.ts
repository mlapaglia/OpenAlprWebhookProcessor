import { Injectable, inject, signal } from '@angular/core';
import { PlateTabStateService, type PlateTab } from './plate-tab-state.service';
import { PlateStatisticsStateService } from './plate-statistics-state.service';
import { PlateNotesStateService } from './plate-notes-state.service';
import { VehicleLogoService } from '../../vehicle-logo.service';
import type { PlateData } from '../../plate-item/plate-item.component';
import { Plate } from '../plate';

@Injectable()
export class PlateFacadeService {
  private readonly tabState = inject(PlateTabStateService);
  private readonly statisticsState = inject(PlateStatisticsStateService);
  private readonly notesState = inject(PlateNotesStateService);
  private readonly vehicleLogoService = inject(VehicleLogoService);

  readonly plateSignal = signal<Plate>(new Plate({}));

  readonly activeTab = this.tabState.activeTab;
  readonly plateStatistics = this.statisticsState.plateStatistics;
  readonly loadingStatistics = this.statisticsState.loadingStatistics;
  readonly loadingStatisticsFailed = this.statisticsState.loadingStatisticsFailed;
  readonly isSavingNotes = this.notesState.isSavingNotes;

  get shouldShowImages() {
    return this.tabState.shouldShowImages;
  }

  get shouldShowStatistics() {
    return this.tabState.shouldShowStatistics;
  }

  get shouldShowNotes() {
    return this.tabState.shouldShowNotes;
  }

  setPlate(plateData: PlateData) {
    const plate = this.convertPlateDataToPlate(plateData);
    this.plateSignal.set(plate);
  }

  setTab(tab: PlateTab, plateData: PlateData) {
    this.tabState.setActiveTab(tab);

    if (tab === 'stats' && this.tabState.shouldShowStatistics && !this.statisticsState.statisticsLoaded()) {
      this.statisticsState.loadStatistics(plateData).subscribe();
    }
  }

  saveNotes(plate: Plate, onSuccess: (updatedPlate: Plate) => void) {
    this.notesState.saveNotes(plate).subscribe({
      next: (updatedPlate) => onSuccess(updatedPlate),
      error: () => { /* do nothing */ },
    });
  }

  clearNotes(plate: Plate, onUpdate: (updatedPlate: Plate) => void) {
    const updatedPlate = this.notesState.clearNotes(plate);
    onUpdate(updatedPlate);
  }

  getVehicleMakeInfo(vehicleDescription: string | null) {
    return this.vehicleLogoService.getVehicleMakeInfo(vehicleDescription ?? '');
  }

  private convertPlateDataToPlate(plateData: PlateData): Plate {
    return new Plate({
      id: plateData.id,
      plateNumber: plateData.plateNumber,
      openAlprCameraId: Number(plateData.openAlprCameraId),
      vehicleDescription: plateData.vehicleDescription,
      direction: plateData.direction,
      receivedOn: plateData.receivedOn,
      isAlert: plateData.isAlert,
      isIgnore: plateData.isIgnore,
      isOpen: plateData.isOpen,
      imageUrl: plateData.imageUrl ?? '',
      cropImageUrl: plateData.cropImageUrl ?? '',
      processedPlateConfidence: plateData.processedPlateConfidence ?? 0,
      notes: plateData.notes ?? '',
      canBeEnriched: plateData.canBeEnriched ?? false,
      region: plateData.region ?? '',
      possiblePlateNumbers: plateData.possiblePlateNumbers ?? '',
      openAlprProcessingTimeMs: plateData.openAlprProcessingTimeMs ?? 0,
      alertDescription: '',
    });
  }

  reset() {
    this.tabState.reset();
    this.statisticsState.reset();
  }
}
