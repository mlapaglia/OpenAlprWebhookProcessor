import type { OnInit, OnChanges } from '@angular/core';
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Validators, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatButtonModule } from '@angular/material/button';
import { animate, style, transition, trigger } from '@angular/animations';
import type { Camera } from '../../camera';
import type { ZoomFocus } from '../zoomfocus';

@Component({
  selector: 'app-camera-daynight',
  templateUrl: './camera-daynight.component.html',
  styleUrls: ['./camera-daynight.component.less'],
  standalone: true,
  animations: [
    trigger('inOutAnimation', [
      transition(':enter', [
        style({ height: 0, opacity: 0 }),
        animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)', style({ height: '*', opacity: 1 })),
      ]),
      transition(':leave', [
        style({ height: '*', opacity: 1 }),
        animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)', style({ height: 0, opacity: 0 })),
      ]),
    ]),
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatCardModule,
    MatSlideToggleModule,
    MatButtonModule,
  ],
})
export class CameraDayNightComponent implements OnInit, OnChanges {
  @Input() camera!: Camera;
  @Input() currentZoomFocus!: ZoomFocus;
  @Output() cameraChange = new EventEmitter<Camera>();
  @Output() currentZoomFocusChange = new EventEmitter<ZoomFocus>();
  @Output() triggerDayMode = new EventEmitter<void>();
  @Output() triggerNightMode = new EventEmitter<void>();
  @Output() getZoomFocus = new EventEmitter<void>();
  @Output() setZoomFocus = new EventEmitter<void>();
  @Output() triggerAutofocus = new EventEmitter<void>();

  dayNightForm: FormGroup;

  constructor(private readonly fb: FormBuilder) {
    this.dayNightForm = this.fb.group({
      dayNightModeEnabled: [false],
      latitude: [''],
      longitude: [''],
      sunsetOffset: [''],
      sunriseOffset: [''],
      timezoneOffset: [''],
      dayNightModeUrl: [''],
      dayZoom: [''],
      dayFocus: [''],
      nightZoom: [''],
      nightFocus: [''],
    });
  }

  ngOnInit() {
    if (this.camera) {
      this.dayNightForm.patchValue({
        dayNightModeEnabled: this.camera.dayNightModeEnabled,
        latitude: this.camera.latitude,
        longitude: this.camera.longitude,
        sunsetOffset: this.camera.sunsetOffset,
        sunriseOffset: this.camera.sunriseOffset,
        timezoneOffset: this.camera.timezoneOffset,
        dayNightModeUrl: this.camera.dayNightModeUrl,
        dayZoom: this.camera.dayZoom,
        dayFocus: this.camera.dayFocus,
        nightZoom: this.camera.nightZoom,
        nightFocus: this.camera.nightFocus,
      });

      this.dayNightForm.valueChanges.subscribe(values => {
        this.camera = { ...this.camera, ...values };
        this.cameraChange.emit(this.camera);
      });

      this.dayNightForm.get('dayNightModeEnabled')?.valueChanges.subscribe(enabled => {
        const urlControl = this.dayNightForm.get('dayNightModeUrl');

        if (enabled) {
          urlControl?.setValidators([Validators.required, Validators.pattern(/^https?:\/\/.+/)]);
        } else {
          urlControl?.clearValidators();
        }

        urlControl?.updateValueAndValidity();
      });
    }
  }

  ngOnChanges() {
    if (this.currentZoomFocus) {
      this.updateCurrentZoomFocusDisplay();
    }
  }

  private updateCurrentZoomFocusDisplay() {
    // Update the display for current zoom/focus values
  }

  onTriggerDayMode() {
    this.triggerDayMode.emit();
  }

  onTriggerNightMode() {
    this.triggerNightMode.emit();
  }

  onGetZoomFocus() {
    this.getZoomFocus.emit();
  }

  onSetZoomFocus() {
    this.setZoomFocus.emit();
  }

  onTriggerAutofocus() {
    this.triggerAutofocus.emit();
  }

  onZoomChange(event: Event) {
    const value = +(event.target as HTMLInputElement).value;
    this.onCurrentZoomFocusChange('zoom', value);
  }

  onFocusChange(event: Event) {
    const value = +(event.target as HTMLInputElement).value;
    this.onCurrentZoomFocusChange('focus', value);
  }

  onCurrentZoomFocusChange(field: 'zoom' | 'focus', value: number) {
    this.currentZoomFocus = {
      ...this.currentZoomFocus,
      [field]: value,
    };
    this.currentZoomFocusChange.emit(this.currentZoomFocus);
  }
}
