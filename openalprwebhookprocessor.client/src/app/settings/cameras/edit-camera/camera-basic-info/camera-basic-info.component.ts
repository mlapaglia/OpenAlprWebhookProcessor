import type { OnInit } from '@angular/core';
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Validators, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import type { Camera } from '../../camera';

@Component({
  selector: 'app-camera-basic-info',
  templateUrl: './camera-basic-info.component.html',
  styleUrls: ['./camera-basic-info.component.less'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatOptionModule,
    MatIconModule,
    MatCardModule,
  ],
})
export class CameraBasicInfoComponent implements OnInit {
  @Input() camera!: Camera;
  @Output() cameraChange = new EventEmitter<Camera>();

  basicInfoForm: FormGroup;
  hidePassword = true;

  constructor(private readonly fb: FormBuilder) {
    this.basicInfoForm = this.fb.group({
      manufacturer: ['', Validators.required],
      modelNumber: ['', Validators.required],
      ipAddress: ['', [Validators.required, Validators.pattern(/^(\d{1,3}\.){3}\d{1,3}$/)]],
      cameraUsername: ['', Validators.required],
      cameraPassword: ['', Validators.required],
    });
  }

  ngOnInit() {
    if (this.camera) {
      this.basicInfoForm.patchValue({
        manufacturer: this.camera.manufacturer,
        modelNumber: this.camera.modelNumber,
        ipAddress: this.camera.ipAddress,
        cameraUsername: this.camera.cameraUsername,
        cameraPassword: this.camera.cameraPassword,
      });

      this.basicInfoForm.valueChanges.subscribe(values => {
        this.camera = { ...this.camera, ...values };
        this.cameraChange.emit(this.camera);
      });
    }
  }

  togglePasswordVisibility() {
    this.hidePassword = !this.hidePassword;
  }
}
