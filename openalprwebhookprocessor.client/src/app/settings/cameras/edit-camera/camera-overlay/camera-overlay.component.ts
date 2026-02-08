import { Component, inject, type OnInit, ChangeDetectionStrategy, type OnDestroy, output, input } from '@angular/core';

import { FormBuilder, Validators, ReactiveFormsModule, type FormGroup } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatButtonModule } from '@angular/material/button';
import { animate, style, transition, trigger } from '@angular/animations';
import type { Camera } from '../../camera';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

@Component({
  selector: 'app-camera-overlay',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './camera-overlay.component.html',
  styleUrls: ['./camera-overlay.component.less'],
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
