import { Component, input, output, inject, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { VehicleLogoService } from '../vehicle-logo.service';
import { PlateComponent } from '../plate/plate.component';
import { RefreshButtonComponent } from '../../shared/refresh-button/refresh-button.component';

export interface PlateData {
  id: string;
  plateNumber: string;
  openAlprCameraId: number;
  vehicleDescription: string;
  direction: number;
  receivedOn: Date;
  isAlert: boolean;
  isIgnore: boolean;
  isOpen: boolean;
  imageUrl?: string;
  cropImageUrl?: string;
  processedPlateConfidence?: number;
  notes?: string;
  canBeEnriched?: boolean;
}

@Component({
  selector: 'app-plate-item',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    NgOptimizedImage,
    MatExpansionModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    PlateComponent,
    RefreshButtonComponent,
  ],
  templateUrl: './plate-item.component.html',
  styleUrl: './plate-item.component.less',
})
export class PlateItemComponent {
  private readonly vehicleLogoService = inject(VehicleLogoService);

  readonly plate = input.required<PlateData>();
  readonly plateOpened = output<string>();
  readonly plateClosed = output<string>();
  readonly enrichPlate = output<string>();
  readonly editPlate = output<string>();
  readonly alertPlate = output<string>();
  readonly ignorePlate = output<string>();
  readonly viewPlate = output<string>();

  isExpanded = false;

  onPlateOpened() {
    this.isExpanded = true;
    this.plateOpened.emit(this.plate().id);
  }

  onPlateClosed() {
    this.isExpanded = false;
    this.plateClosed.emit(this.plate().id);
  }

  onEnrichPlate() {
    this.enrichPlate.emit(this.plate().id);
  }

  onEditPlate() {
    this.editPlate.emit(this.plate().id);
  }

  onAlertPlate() {
    this.alertPlate.emit(this.plate().id);
  }

  onIgnorePlate() {
    this.ignorePlate.emit(this.plate().id);
  }

  onViewPlate() {
    this.viewPlate.emit(this.plate().id);
  }

  onPlateChanged(_: PlateData) {
    // do nothing
  }

  get vehicleMakeInfo() {
    const plate = this.plate() as PlateData | null;
    const vehicleDescription = (plate?.vehicleDescription as string | undefined) ?? '';
    return this.vehicleLogoService.getVehicleMakeInfo(vehicleDescription);
  }

  get formattedVehicleDescription() {
    const plate = this.plate() as PlateData | null;
    const vehicleDescription = (plate?.vehicleDescription as string | undefined) ?? '';
    return this.vehicleLogoService.formatVehicleDescription(vehicleDescription);
  }

  public onImageError(event: Event) {
    const target = event.target as HTMLImageElement | null;
    if (target?.style) {
      target.style.display = 'none';
    }
  }
}
