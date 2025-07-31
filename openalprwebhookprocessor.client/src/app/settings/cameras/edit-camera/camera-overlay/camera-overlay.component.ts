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
import type { Camera } from '../../camera';

@Component({
  standalone: true,
  selector: 'app-camera-overlay',
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
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatCardModule,
    MatSlideToggleModule,
    MatButtonModule,
  ],
})
export class CameraOverlayComponent implements OnInit {
  @Input() camera!: Camera;
  @Output() cameraChange = new EventEmitter<Camera>();
  @Output() testOverlay = new EventEmitter<void>();

  overlayForm: FormGroup;

  constructor(private readonly fb: FormBuilder) {
    this.overlayForm = this.fb.group({
      updateOverlayEnabled: [false],
      updateOverlayTextUrl: [''],
    });
  }

  ngOnInit() {
    if (this.camera) {
      this.overlayForm.patchValue({
        updateOverlayEnabled: this.camera.updateOverlayEnabled,
        updateOverlayTextUrl: this.camera.updateOverlayTextUrl,
      });

      this.overlayForm.valueChanges.subscribe(values => {
        this.camera = { ...this.camera, ...values };
        this.cameraChange.emit(this.camera);
      });

      this.overlayForm.get('updateOverlayEnabled')?.valueChanges.subscribe(enabled => {
        const urlControl = this.overlayForm.get('updateOverlayTextUrl');

        if (enabled) {
          urlControl?.setValidators([Validators.required, Validators.pattern(/^https?:\/\/.+/)]);
        } else {
          urlControl?.clearValidators();
        }

        urlControl?.updateValueAndValidity();
      });
    }
  }

  onTestOverlay() {
    this.testOverlay.emit();
  }
}
