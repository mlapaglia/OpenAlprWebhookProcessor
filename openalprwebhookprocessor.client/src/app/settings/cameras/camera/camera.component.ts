import { Component, EventEmitter, Input, type OnInit, type OnDestroy, Output, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subscription } from 'rxjs';
import type { Camera } from '../camera';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-camera',
  templateUrl: './camera.component.html',
  styleUrls: ['./camera.component.less'],
  imports: [MatCardModule, MatProgressSpinnerModule, MatIconModule, MatTooltipModule, MatButtonModule, DatePipe],
})
export class CameraComponent implements OnInit, OnDestroy {
  private readonly http = inject(HttpClient);
  private readonly subscriptions = new Subscription();

  @Input() camera: Camera;
  @Output() add = new EventEmitter<number>();
  @Output() edit = new EventEmitter<string>();
  @Output() delete = new EventEmitter<string>();
  @Output() test = new EventEmitter<string>();

  public isLoadingImage = true;
  public isLoadingFailed = false;
  public imageDataUrl: string | null = null;

  ngOnInit(): void {
    if (!this.camera.sampleImageUrl) {
      this.isLoadingImage = false;
    } else {
      this.loadImage();
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  private loadImage(): void {
    this.isLoadingImage = true;
    this.isLoadingFailed = false;
    this.imageDataUrl = null;

    this.subscriptions.add(
      this.http.get(this.camera.sampleImageUrl, { responseType: 'blob' }).subscribe({
        next: (blob: Blob) => {
          const reader = new FileReader();
          reader.onload = () => {
            this.imageDataUrl = reader.result as string;
            this.isLoadingImage = false;
          };
          reader.onerror = () => {
            this.isLoadingImage = false;
            this.isLoadingFailed = true;
          };
          reader.readAsDataURL(blob);
        },
        error: () => {
          this.isLoadingImage = false;
          this.isLoadingFailed = true;
        },
      }),
    );
  }

  public addCamera() {
    this.add.emit(undefined);
  }

  public editCamera() {
    this.edit.emit(this.camera.id);
  }

  public removeCamera() {
    this.delete.emit(this.camera.id);
  }

  public testCamera() {
    this.test.emit(this.camera.id);
  }
}
