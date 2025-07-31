import { Component, Input } from '@angular/core';
import type { Plate } from './plate';
import { CommonModule, DatePipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { PlateStatisticsComponent } from './plate-statistics.component';
import { PlateImagesComponent } from './plate-images.component';
import { PlateNotesComponent } from './plate-notes.component';

@Component({
  selector: 'app-plate',
  templateUrl: './plate.component.html',
  styleUrls: ['./plate.component.less'],
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    PlateStatisticsComponent,
    PlateImagesComponent,
    PlateNotesComponent,
    DatePipe,
  ],
})
export class PlateComponent {
  @Input() plate!: Plate;
  @Input() isVisible = false;

  activeTab: 'overview' | 'images' | 'stats' | 'notes' = 'overview';

  onPlateChange(updatedPlate: Plate) {
    this.plate = updatedPlate;
  }

  setTab(tab: 'overview' | 'images' | 'stats' | 'notes') {
    this.activeTab = tab;
  }

  getVehicleImageUrl(): string {
    return this.plate?.imageUrl?.toString() || '';
  }

  getPlateImageUrl(): string {
    return this.plate?.cropImageUrl?.toString() || '';
  }
}
