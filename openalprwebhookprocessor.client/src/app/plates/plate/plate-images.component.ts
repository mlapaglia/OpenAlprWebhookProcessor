import type { OnInit, OnDestroy, OnChanges } from '@angular/core';
import { Component, Input, inject } from '@angular/core';
import { Lightbox } from 'ngx-lightbox';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CommonModule } from '@angular/common';
import type { Plate } from './plate';

@Component({
  selector: 'app-plate-images',
  templateUrl: './plate-images.component.html',
  styleUrls: ['./plate-images.component.css'],
  imports: [
    CommonModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatTooltipModule,
  ],
})
export class PlateImagesComponent implements OnInit, OnChanges, OnDestroy {
  private readonly lightbox = inject(Lightbox);

  @Input() plate!: Plate;
  @Input() isVisible = false;

  public loadingVehicleImage = false;
  public vehicleImageUrl = '';
  public loadingVehicleImageFailed = false;

  public loadingPlateImage = false;
  public loadingPlateImageFailed = false;
  public plateImageUrl = '';

  private isInitialized = false;

  ngOnInit(): void {
    this.loadingVehicleImage = true;
    this.loadingPlateImage = true;
    this.loadPlateImages();
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
        this.loadPlateImages();
      }
    }
  }

  ngOnDestroy(): void {
    this.vehicleImageUrl = '';
    this.plateImageUrl = '';
  }

  private loadPlateImages() {
    if (!this.vehicleImageUrl && this.isVisible) {
      this.loadingVehicleImage = true;
      this.loadingVehicleImageFailed = false;
      this.vehicleImageUrl = this.plate.imageUrl.toString();
    }

    if (!this.plateImageUrl && this.isVisible) {
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
}
