import { Component, inject, type OnInit, type OnDestroy, ChangeDetectionStrategy, input, output } from '@angular/core';

import { FormBuilder, Validators, ReactiveFormsModule, type FormGroup } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatButtonModule } from '@angular/material/button';
import { animate, style, transition, trigger } from '@angular/animations';
import { CameraMaskComponent } from '../camera-mask/camera-mask.component';
import type { Camera } from '../../camera';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

@Component({
  selector: 'app-camera-openalpr',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './camera-openalpr.component.html',
  styleUrls: ['./camera-openalpr.component.less'],
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
    CameraMaskComponent
],
  standalone: true,
})
export class CameraOpenAlprComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  readonly camera = input.required<Camera>();
  readonly cameraChange = output<Camera>();
  readonly editMask = output<void>();

  openAlprForm: FormGroup;
  isEditingMask = false;
  private readonly fb = inject(FormBuilder);

  constructor() {
    super();
    this.openAlprForm = this.fb.group({
      openAlprEnabled: [false],
      openAlprName: [''],
      openAlprCameraId: ['', Validators.min(1)],
    });
  }

  ngOnInit() {
    this.openAlprForm.patchValue({
      openAlprEnabled: this.camera().openAlprEnabled,
      openAlprName: this.camera().openAlprName,
      openAlprCameraId: this.camera().openAlprCameraId,
    });

    this.subscribeAndMarkForCheck(
      this.openAlprForm.valueChanges,
      (values) => {
        this.cameraChange.emit({ ...this.camera(), ...values });
      },
    );

    const openAlprEnabledControl = this.openAlprForm.get('openAlprEnabled');
    if (openAlprEnabledControl) {
      this.subscribeAndMarkForCheck(
        openAlprEnabledControl.valueChanges,
        (enabled) => {
          const nameControl = this.openAlprForm.get('openAlprName');
          const idControl = this.openAlprForm.get('openAlprCameraId');

          if (enabled) {
            nameControl?.setValidators([Validators.required]);
            idControl?.setValidators([Validators.required, Validators.min(1)]);
          } else {
            nameControl?.clearValidators();
            idControl?.clearValidators();
          }

          nameControl?.updateValueAndValidity();
          idControl?.updateValueAndValidity();
        },
      );
    }
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  onEditMask() {
    this.isEditingMask = !this.isEditingMask;
    this.editMask.emit();
    this.markForCheck();
  }
}
