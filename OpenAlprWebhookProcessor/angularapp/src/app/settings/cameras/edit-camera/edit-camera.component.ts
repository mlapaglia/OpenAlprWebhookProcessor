import { animate, style, transition, trigger } from '@angular/animations';
import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { Camera } from '../camera';
import { EditCameraService } from './edit-camera.service';
import { ZoomFocus } from './zoomfocus';
import { CameraMaskComponent } from './camera-mask/camera-mask.component';
import { MatButtonModule } from '@angular/material/button';
import { NgIf } from '@angular/common';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatOptionModule } from '@angular/material/core';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';

@Component({
    selector: 'app-edit-camera',
    templateUrl: './edit-camera.component.html',
    styleUrls: ['./edit-camera.component.less'],
    animations: [
        trigger('inOutAnimation', [
            transition(':enter', [
                style({ height: 0, opacity: 0 }),
                animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)', style({ height: '*', opacity: 1 }))
            ]),
            transition(':leave', [
                style({ height: '*', opacity: 1 }),
                animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)', style({ height: 0, opacity: 0 }))
            ])
        ])
    ],
    standalone: true,
    imports: [MatDialogModule, MatFormFieldModule, MatSelectModule, ReactiveFormsModule, FormsModule, MatOptionModule, MatInputModule, MatIconModule, MatSlideToggleModule, NgIf, MatButtonModule, CameraMaskComponent]
})
export class EditCameraComponent implements OnInit {
  public camera: Camera;
  public hidePassword: boolean = true;
  public currentZoomFocus: ZoomFocus = new ZoomFocus();
  public isEditingMask: boolean = false;

  constructor(
    public dialogRef: MatDialogRef<EditCameraComponent>,
    private snackBarService: SnackbarService,
    private editCameraService: EditCameraService,

    @Inject(MAT_DIALOG_DATA) public data: Camera) { }

  ngOnInit(): void {
    this.camera = this.data;
    this.getZoomFocus();
  }

  public triggerDayMode() {
    this.editCameraService.triggerDayMode(this.camera.id).subscribe(() => {
      this.snackBarService.create("day mode test sent successfully", SnackBarType.Info);
    });
  }

  public triggerNightMode() {
    this.editCameraService.triggerNightMode(this.camera.id).subscribe(() => {
      this.snackBarService.create("night mode test sent successfully", SnackBarType.Info);
    });
  }

  public testOverlay() {
    this.editCameraService.triggerTestOverlay(this.camera.id).subscribe(() => {
      this.snackBarService.create("overlay test sent successfully", SnackBarType.Info);
    });
  }

  public getZoomFocus() {
    this.editCameraService.getZoomAndFocus(this.camera.id).subscribe(result => {
      this.currentZoomFocus = result;
    });
  }

  public setZoomFocus() {
    this.editCameraService.setZoomAndFocus(this.camera.id, this.currentZoomFocus).subscribe(() => {
      this.getZoomFocus();
    });
  }

  public triggerAutofocus() {
    this.editCameraService.triggerAutofocus(this.camera.id).subscribe(() => {
      this.getZoomFocus();
    },
    () => {
      this.snackBarService.create("auto focus failed", SnackBarType.Error);
    });
  }

  public editMask() {
      this.isEditingMask = !this.isEditingMask;
  }
}