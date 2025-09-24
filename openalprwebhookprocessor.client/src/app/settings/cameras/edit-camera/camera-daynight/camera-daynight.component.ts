import { Component, inject, type OnInit, type OnChanges, ChangeDetectionStrategy, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, Validators, ReactiveFormsModule, type FormGroup } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatButtonModule } from '@angular/material/button';
import type { Camera } from '../../camera';
import type { ZoomFocus } from '../zoomfocus';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

@Component({
  selector: 'app-camera-daynight',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './camera-daynight.component.html',
  styleUrls: ['./camera-daynight.component.less'],
  standalone: true,
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
export class CameraDayNightComponent extends OnPushBaseComponent implements OnInit, OnChanges {
  readonly camera = input<Camera>();
  readonly currentZoomFocus = input<ZoomFocus>();
  readonly cameraChange = output<Camera>();
  readonly currentZoomFocusChange = output<ZoomFocus>();
  readonly triggerDayMode = output<void>();
  readonly triggerNightMode = output<void>();
  readonly getZoomFocus = output<void>();
  readonly setZoomFocus = output<void>();
  readonly triggerAutofocus = output<void>();

  dayNightForm: FormGroup;
  private readonly fb = inject(FormBuilder);

  constructor() {
    super();
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
    this.initializeFormValues();
    this.setupFormSubscriptions();
    this.setupConditionalValidation();
  }

  private initializeFormValues() {
    const camera = this.camera();

    if (camera) {
      this.dayNightForm.patchValue({
        dayNightModeEnabled: camera.dayNightModeEnabled,
        latitude: camera.latitude,
        longitude: camera.longitude,
        sunsetOffset: camera.sunsetOffset,
        sunriseOffset: camera.sunriseOffset,
        timezoneOffset: camera.timezoneOffset,
        dayNightModeUrl: camera.dayNightModeUrl,
        dayZoom: camera.dayZoom,
        dayFocus: camera.dayFocus,
        nightZoom: camera.nightZoom,
        nightFocus: camera.nightFocus,
      });
    }
  }

  private setupFormSubscriptions() {
    this.subscribeAndMarkForCheck(
      this.dayNightForm.valueChanges,
      (values) => {
        this.cameraChange.emit({ ...this.camera(), ...values });
      },
    );
  }

  private setupConditionalValidation() {
    const dayNightModeControl = this.dayNightForm.get('dayNightModeEnabled');
    if (dayNightModeControl) {
      this.subscribeAndMarkForCheck(
        dayNightModeControl.valueChanges,
        (enabled) => {
          const urlControl = this.dayNightForm.get('dayNightModeUrl');
          if (enabled) {
            urlControl?.setValidators([Validators.required, Validators.pattern(/^https?:\/\/.+/)]);
          } else {
            urlControl?.clearValidators();
          }
          urlControl?.updateValueAndValidity();
        },
      );
    }
  }

  ngOnChanges() {
    this.updateCurrentZoomFocusDisplay();
    this.markForCheck();
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
    const current = this.currentZoomFocus() ?? { zoom: 0, focus: 0 };
    const updatedZoomFocus = {
      ...current,
      [field]: value,
    };
    this.currentZoomFocusChange.emit(updatedZoomFocus);
    this.markForCheck();
  }
}
