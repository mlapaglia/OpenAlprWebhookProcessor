import { Component, inject, type OnInit, ChangeDetectionStrategy, type OnDestroy, output, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, Validators, ReactiveFormsModule, type FormGroup } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatButtonModule } from '@angular/material/button';
import type { Camera } from '../../camera';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

@Component({
  selector: 'app-camera-overlay',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './camera-overlay.component.html',
  styleUrls: ['./camera-overlay.component.less'],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatCardModule,
    MatSlideToggleModule,
    MatButtonModule,
  ],
  standalone: true,
})
export class CameraOverlayComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  readonly camera = input.required<Camera>();
  readonly cameraChange = output<Camera>();
  readonly testOverlay = output<void>();

  overlayForm: FormGroup;
  private readonly fb = inject(FormBuilder);

  constructor() {
    super();
    this.overlayForm = this.fb.group({
      updateOverlayEnabled: [false],
      updateOverlayTextUrl: [''],
    });
  }

  ngOnInit() {
    this.overlayForm.patchValue({
      updateOverlayEnabled: this.camera().updateOverlayEnabled,
      updateOverlayTextUrl: this.camera().updateOverlayTextUrl,
    });

    this.subscribeAndMarkForCheck(
      this.overlayForm.valueChanges,
      (values) => {
        this.cameraChange.emit({ ...this.camera(), ...values });
      },
    );

    const updateOverlayEnabled = this.overlayForm.get('updateOverlayEnabled');

    if (updateOverlayEnabled) {
      this.subscribeAndMarkForCheck(
        updateOverlayEnabled.valueChanges,
        (enabled) => {
          const urlControl = this.overlayForm.get('updateOverlayTextUrl');

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

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  onTestOverlay() {
    this.testOverlay.emit();
    this.markForCheck();
  }
}
