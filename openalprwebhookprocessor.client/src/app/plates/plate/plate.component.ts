import { Component, Input, inject, type OnChanges, type OnDestroy, type OnInit } from '@angular/core';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { Lightbox } from 'ngx-lightbox';
import { PlateService } from '../plate.service';
import type { Plate } from './plate';
import { MatButtonModule } from '@angular/material/button';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { TextFieldModule } from '@angular/cdk/text-field';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { PlateStatisticsComponent } from './plate-statistics.component';

@Component({
  selector: 'app-plate',
  templateUrl: './plate.component.html',
  styleUrls: ['./plate.component.less'],
  imports: [
    MatCardModule, MatProgressSpinnerModule, MatIconModule,
    MatFormFieldModule, MatInputModule, TextFieldModule, ReactiveFormsModule,
    FormsModule, MatButtonModule, PlateStatisticsComponent,
  ],
})
export class PlateComponent implements OnInit, OnChanges, OnDestroy {
  private readonly lightbox = inject(Lightbox);
  private readonly plateService = inject(PlateService);
  private readonly snackbarService = inject(SnackbarService);

  @Input() plate: Plate;
  @Input() isVisible: boolean;

  public isInitialized: boolean;

  public loadingVehicleImage: boolean;
  public vehicleImageUrl: string;
  public loadingVehicleImageFailed: boolean;

  public loadingPlateImage: boolean;
  public loadingPlateImageFailed: boolean;
  public plateImageUrl: string;

  public isSavingNotes: boolean;

  ngOnInit(): void {
    this.loadingVehicleImage = true;
    this.loadingPlateImage = true;
    this.getPlateImages();
    this.isInitialized = true;
  }

  ngOnChanges(): void {
    if (this.isInitialized) {
      if (!this.isVisible) {
        if (this.loadingVehicleImage) {
          this.vehicleImageUrl = '';
        }

        if (this.loadingPlateImage) {
          this.plateImageUrl = '';
        }
      } else {
        this.getPlateImages();
      }
    }
  }

  ngOnDestroy(): void {
    this.vehicleImageUrl = '';
    this.plateImageUrl = '';
  }

  private getPlateImages() {
    if (!this.vehicleImageUrl) {
      this.loadingVehicleImage = true;
      this.loadingVehicleImageFailed = false;
      this.vehicleImageUrl = this.plate.imageUrl.toString();
    }

    if (!this.plateImageUrl) {
      this.loadingPlateImage = true;
      this.loadingPlateImageFailed = false;
      this.plateImageUrl = this.plate.cropImageUrl.toString();
    }
  }

  public openLightbox(url: URL, plateNumber: string) {
    const albums = [
      {
        src: url.toString(),
        caption: plateNumber,
        thumb: url.toString(),
      },
    ];

    this.lightbox.open(albums, 0);
  }

  public vehicleImageLoaded() {
    this.loadingVehicleImage = false;
  }

  public vehicleImageFailedToLoad() {
    this.loadingVehicleImage = false;
    this.loadingVehicleImageFailed = true;
  }

  public plateImageLoaded() {
    this.loadingPlateImage = false;
  }

  public plateImageFailedToLoad() {
    this.loadingPlateImage = false;
    this.loadingPlateImageFailed = true;
  }

  public saveNotes() {
    this.isSavingNotes = true;
    this.plateService.upsertPlate(this.plate).subscribe(() => {
      this.isSavingNotes = false;
      this.snackbarService.create(`Notes saved for: ${this.plate.plateNumber}`, SnackBarType.Saved);
    },
    () => {
      this.isSavingNotes = false;
      this.snackbarService.create(`Failed to save notes for: ${this.plate.plateNumber}`, SnackBarType.Error);
    });
  }

  public clearNotes() {
    this.plate.notes = '';
  }
}
