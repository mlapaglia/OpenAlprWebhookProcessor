import { Component, input, inject, type OnInit, type OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import { Lightbox } from 'ngx-lightbox';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CommonModule } from '@angular/common';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';
import type { Plate } from '../plate';

@Component({
  selector: 'app-plate-images',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatTooltipModule,
  ],
  templateUrl: './plate-images.component.html',
  styleUrls: ['./plate-images.component.css'],
})
export class PlateImagesComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  private readonly lightbox = inject(Lightbox);

  readonly plate = input.required<Plate>();

  public loadingVehicleImage = false;
  public vehicleImageUrl = '';
  public loadingVehicleImageFailed = false;

  public loadingPlateImage = false;
  public loadingPlateImageFailed = false;
  public plateImageUrl = '';

  ngOnInit(): void {
    this.loadingVehicleImage = true;
    this.loadingPlateImage = true;
    this.loadPlateImages();
  }

  override ngOnDestroy(): void {
    this.vehicleImageUrl = '';
    this.plateImageUrl = '';
    super.ngOnDestroy();
  }

  private loadPlateImages() {
    if (!this.vehicleImageUrl) {
      this.loadingVehicleImage = true;
      this.loadingVehicleImageFailed = false;
      this.vehicleImageUrl = this.plate().imageUrl;
    }

    if (!this.plateImageUrl) {
      this.loadingPlateImage = true;
      this.loadingPlateImageFailed = false;
      this.plateImageUrl = this.plate().cropImageUrl;
    }
  }

  public openLightbox(url: string, plateNumber: string) {
    const albums = [
      {
        src: url,
        caption: plateNumber,
        thumb: url,
      },
    ];

    this.lightbox.open(albums, 0);
  }

  public vehicleImageLoaded() {
    this.loadingVehicleImage = false;
    this.markForCheck();
  }

  public vehicleImageFailedToLoad() {
    this.loadingVehicleImage = false;
    this.loadingVehicleImageFailed = true;
    this.markForCheck();
  }

  public plateImageLoaded() {
    this.loadingPlateImage = false;
    this.markForCheck();
  }

  public plateImageFailedToLoad() {
    this.loadingPlateImage = false;
    this.loadingPlateImageFailed = true;
    this.markForCheck();
  }
}
