import { Component, input, output, inject, ChangeDetectionStrategy, type OnInit } from '@angular/core';
import { Plate } from './plate';
import type { PlateData } from '../plate-item/plate-item.component';
import { CommonModule, DatePipe, NgOptimizedImage } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { PlateStatisticsComponent } from './plate-statistics/plate-statistics.component';
import { PlateImagesComponent } from './plate-images/plate-images.component';
import { PlateNotesComponent } from './plate-notes/plate-notes.component';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';
import { PlateFacadeService } from './services/plate-facade.service';
import { PlateTabStateService } from './services/plate-tab-state.service';
import { PlateStatisticsStateService } from './services/plate-statistics-state.service';
import { PlateNotesStateService } from './services/plate-notes-state.service';

@Component({
  selector: 'app-plate',
  templateUrl: './plate.component.html',
  styleUrls: ['./plate.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    PlateFacadeService,
    PlateTabStateService,
    PlateStatisticsStateService,
    PlateNotesStateService,
  ],
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    PlateStatisticsComponent,
    PlateImagesComponent,
    PlateNotesComponent,
    DatePipe,
    NgOptimizedImage,
  ],
})
export class PlateComponent extends OnPushBaseComponent implements OnInit {
  private readonly facade = inject(PlateFacadeService);

  readonly plate = input.required<PlateData>();
  readonly isVisible = input(false);
  readonly isExpanded = input(false);
  readonly plateChanged = output<PlateData>();

  activeTab: 'overview' | 'images' | 'stats' | 'notes' = 'overview';

  get plateStatistics() {
    return this.facade.plateStatistics();
  }

  get loadingStatistics() {
    return this.facade.loadingStatistics();
  }

  get loadingStatisticsFailed() {
    return this.facade.loadingStatisticsFailed();
  }

  get isSavingNotes() {
    return this.facade.isSavingNotes();
  }

  readonly plateAsPlateSignal = this.facade.plateSignal;

  ngOnInit() {
    this.facade.setPlate(this.plate());
  }

  onPlateChange(updatedPlate: PlateData) {
    this.plateChanged.emit(updatedPlate);
    this.facade.setPlate(updatedPlate);
    this.markForCheck();
  }

  onPlateChangeFromChild(updatedPlate: Plate) {
    const plateData: PlateData = {
      id: updatedPlate.id,
      plateNumber: updatedPlate.plateNumber,
      openAlprCameraId: updatedPlate.openAlprCameraId,
      vehicleDescription: updatedPlate.vehicleDescription,
      direction: updatedPlate.direction,
      receivedOn: updatedPlate.receivedOn,
      isAlert: updatedPlate.isAlert,
      isIgnore: updatedPlate.isIgnore,
      isOpen: updatedPlate.isOpen,
      imageUrl: updatedPlate.imageUrl,
      cropImageUrl: updatedPlate.cropImageUrl,
      processedPlateConfidence: updatedPlate.processedPlateConfidence,
      notes: updatedPlate.notes,
      canBeEnriched: updatedPlate.canBeEnriched,
    };
    this.plateChanged.emit(plateData);
    this.facade.setPlate(plateData);
    this.markForCheck();
  }

  setTab(tab: 'overview' | 'images' | 'stats' | 'notes') {
    this.activeTab = tab;
    this.facade.setTab(tab, this.plate());
    this.markForCheck();
  }

  get vehicleImageUrl(): string {
    return this.plate().imageUrl ?? '';
  }

  get plateImageUrl(): string {
    return this.plate().cropImageUrl ?? '';
  }

  get vehicleMakeInfo() {
    return this.facade.getVehicleMakeInfo(this.plate().vehicleDescription);
  }

  get shouldShowImages() {
    return this.facade.shouldShowImages;
  }

  get shouldShowNotes() {
    return this.facade.shouldShowNotes;
  }

  get shouldShowStatistics() {
    return this.facade.shouldShowStatistics;
  }

  get plateAsPlate(): Plate {
    return this.convertPlateDataToPlate(this.plate());
  }

  private convertPlateDataToPlate(plateData: PlateData): Plate {
    return new Plate({
      ...this.mapBasicProperties(plateData),
      ...this.mapImageUrls(plateData),
      ...this.mapDefaults(),
    });
  }

  private mapBasicProperties(plateData: PlateData) {
    return {
      ...this.mapIdentificationProps(plateData),
      ...this.mapVehicleProps(plateData),
      ...this.mapStatusProps(plateData),
      ...this.mapMetadataProps(plateData),
    };
  }

  private mapIdentificationProps(plateData: PlateData) {
    return {
      id: plateData.id,
      plateNumber: plateData.plateNumber,
      openAlprCameraId: plateData.openAlprCameraId,
    };
  }

  private mapVehicleProps(plateData: PlateData) {
    return {
      vehicleDescription: plateData.vehicleDescription,
      direction: plateData.direction,
    };
  }

  private mapStatusProps(plateData: PlateData) {
    return {
      isAlert: plateData.isAlert,
      isIgnore: plateData.isIgnore,
      isOpen: plateData.isOpen,
    };
  }

  private mapMetadataProps(plateData: PlateData) {
    return {
      receivedOn: plateData.receivedOn,
      processedPlateConfidence: plateData.processedPlateConfidence ?? 0,
      notes: plateData.notes ?? '',
      canBeEnriched: plateData.canBeEnriched ?? false,
    };
  }

  private mapImageUrls(plateData: PlateData) {
    return {
      imageUrl: plateData.imageUrl ?? '',
      cropImageUrl: plateData.cropImageUrl ?? '',
    };
  }

  private mapDefaults() {
    return {
      region: '',
      possiblePlateNumbers: '',
      openAlprProcessingTimeMs: 0,
      alertDescription: '',
    };
  }

  public saveNotes = (updatedPlate: Plate) => {
    this.facade.saveNotes(updatedPlate, (plate) => {
      this.onPlateChangeFromChild(plate);
    });
  };

  public clearNotes = (updatedPlate: Plate) => {
    this.facade.clearNotes(updatedPlate, (plate) => {
      this.onPlateChangeFromChild(plate);
    });
  };

  onImageError(event: Event): void {
    const target = event.target as HTMLImageElement;
    target.style.display = 'none';
  }
}
