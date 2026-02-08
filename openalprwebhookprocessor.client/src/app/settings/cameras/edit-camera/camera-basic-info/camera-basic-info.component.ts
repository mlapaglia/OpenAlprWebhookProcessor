import { Component, inject, input, output, type OnInit, ChangeDetectionStrategy } from '@angular/core';

import { FormBuilder, Validators, ReactiveFormsModule, type FormGroup } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import type { Camera } from '../../camera';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

@Component({
  selector: 'app-camera-basic-info',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './camera-basic-info.component.html',
  styleUrls: ['./camera-basic-info.component.less'],
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatOptionModule,
    MatIconModule,
    MatCardModule,
  ],
})
export class CameraBasicInfoComponent extends OnPushBaseComponent implements OnInit {
  readonly camera = input.required<Camera>();
  readonly cameraChange = output<Camera>();

  basicInfoForm: FormGroup;
  hidePassword = true;

  private readonly fb = inject(FormBuilder);

  constructor() {
    super();
    this.basicInfoForm = this.fb.group({
      manufacturer: ['', Validators.required],
      modelNumber: ['', Validators.required],
      ipAddress: ['', [Validators.required, Validators.pattern(/^(\d{1,3}\.){3}\d{1,3}$/)]],
      cameraUsername: ['', Validators.required],
      cameraPassword: ['', Validators.required],
    });
  }

  ngOnInit() {
    // Initial form patching
    const camera = this.camera();
    this.basicInfoForm.patchValue({
      manufacturer: camera.manufacturer,
      modelNumber: camera.modelNumber,
      ipAddress: camera.ipAddress,
      cameraUsername: camera.cameraUsername,
      cameraPassword: camera.cameraPassword,
    });

    this.subscribeAndMarkForCheck(
      this.basicInfoForm.valueChanges, values => {
        const updatedCamera = { ...this.camera(), ...values };
        this.cameraChange.emit(updatedCamera);
      });
  }

  togglePasswordVisibility() {
    this.hidePassword = !this.hidePassword;
    this.markForCheck();
  }

  onPasswordToggleKeyDown(event: KeyboardEvent) {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      this.togglePasswordVisibility();
    }
  }
}
