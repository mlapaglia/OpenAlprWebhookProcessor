import type { OnInit } from '@angular/core';
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
import { CameraMaskComponent } from '../camera-mask/camera-mask.component';
import type { Camera } from '../../camera';

@Component({
  standalone: true,
  selector: 'app-camera-openalpr',
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
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatCardModule,
    MatSlideToggleModule,
    MatButtonModule,
    CameraMaskComponent,
  ],
})
export class CameraOpenAlprComponent implements OnInit {
  @Input() camera!: Camera;
  @Output() cameraChange = new EventEmitter<Camera>();
  @Output() editMask = new EventEmitter<void>();

  openAlprForm: FormGroup;
  isEditingMask = false;

  constructor(private readonly fb: FormBuilder) {
    this.openAlprForm = this.fb.group({
      openAlprEnabled: [false],
      openAlprName: [''],
      openAlprCameraId: ['', Validators.min(1)],
    });
  }

  ngOnInit() {
    if (this.camera) {
      this.openAlprForm.patchValue({
        openAlprEnabled: this.camera.openAlprEnabled,
        openAlprName: this.camera.openAlprName,
        openAlprCameraId: this.camera.openAlprCameraId,
      });

      this.openAlprForm.valueChanges.subscribe(values => {
        this.camera = { ...this.camera, ...values };
        this.cameraChange.emit(this.camera);
      });

      this.openAlprForm.get('openAlprEnabled')?.valueChanges.subscribe(enabled => {
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
      });
    }
  }

  onEditMask() {
    this.isEditingMask = !this.isEditingMask;
    this.editMask.emit();
  }
}
