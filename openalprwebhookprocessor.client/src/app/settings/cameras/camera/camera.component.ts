import { Component, type OnInit, type OnDestroy, inject, ChangeDetectionStrategy, input, output } from '@angular/core';
import type { Camera } from '../camera';
import { CameraService } from '../camera.service';
import { AddCameraComponent } from './add-camera/add-camera.component';
import { CameraImageComponent } from './camera-image/camera-image.component';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatChipsModule } from '@angular/material/chips';
import { DatePipe, DecimalPipe } from '@angular/common';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

@Component({
  selector: 'app-camera',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './camera.component.html',
  styleUrls: ['./camera.component.less'],
  imports: [
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatListModule,
    MatChipsModule,
    DatePipe,
    DecimalPipe,
    AddCameraComponent,
    CameraImageComponent,
  ],
})
export class CameraComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  private readonly cameraService = inject(CameraService);

  readonly camera = input.required<Camera>();
  readonly add = output<void>();
  readonly edit = output<string>();
  readonly delete = output<string>();
  readonly test = output<string>();

  public isLoadingImage = true;
  public isLoadingFailed = false;
  public imageDataUrl: string | null = null;

  ngOnInit(): void {
    if (!this.camera().sampleImageUrl) {
      this.isLoadingImage = false;
    } else {
      this.loadImage();
    }
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  private loadImage(): void {
    this.isLoadingImage = true;
    this.isLoadingFailed = false;
    this.imageDataUrl = null;

    this.subscribeAndMarkForCheck(
      this.cameraService.loadSampleImage(this.camera().sampleImageUrl),
      (dataUrl: string) => {
        this.imageDataUrl = dataUrl;
        this.isLoadingImage = false;
      },
      _ => {
        this.isLoadingImage = false;
        this.isLoadingFailed = true;
        this.markForCheck();
      });
  }

  public editCamera() {
    this.edit.emit(this.camera().id);
  }

  public removeCamera() {
    this.delete.emit(this.camera().id);
  }

  public testCamera() {
    this.test.emit(this.camera().id);
  }

  // Helper methods to reduce template complexity
  public get hasOpenAlprIntegration(): boolean {
    return this.camera().updateOverlayEnabled;
  }

  public get hasDayNightMode(): boolean {
    return this.camera().dayNightModeEnabled;
  }

  public get hasAnyFeatures(): boolean {
    return this.hasOpenAlprIntegration || this.hasDayNightMode;
  }

  public get avatarIcon(): string {
    return this.hasOpenAlprIntegration ? 'car_tag' : 'videocam';
  }
}
