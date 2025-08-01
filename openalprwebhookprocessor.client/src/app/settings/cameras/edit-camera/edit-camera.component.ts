import { Component, inject, type OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import type { Camera } from '../camera';
import { EditCameraService } from './edit-camera.service';
import { ZoomFocus } from './zoomfocus';
import { CameraBasicInfoComponent } from './camera-basic-info/camera-basic-info.component';
import { CameraOpenAlprComponent } from './camera-openalpr/camera-openalpr.component';
import { CameraOverlayComponent } from './camera-overlay/camera-overlay.component';
import { CameraDayNightComponent } from './camera-daynight/camera-daynight.component';

@Component({
  selector: 'app-edit-camera',
  standalone: true,
  templateUrl: './edit-camera.component.html',
  styleUrls: ['./edit-camera.component.less'],
  imports: [
    MatDialogModule,
    MatButtonModule,
    CameraBasicInfoComponent,
    CameraOpenAlprComponent,
    CameraOverlayComponent,
    CameraDayNightComponent,
  ],
})
export class EditCameraComponent implements OnInit {
  dialogRef = inject<MatDialogRef<EditCameraComponent>>(MatDialogRef);
  private readonly snackBarService = inject(SnackbarService);
  private readonly editCameraService = inject(EditCameraService);
  data = inject<Camera>(MAT_DIALOG_DATA);

  public camera: Camera;
  public currentZoomFocus: ZoomFocus = new ZoomFocus();

  ngOnInit(): void {
    this.camera = { ...this.data };
    this.getZoomFocus();
  }

  onCameraChange(updatedCamera: Camera) {
    this.camera = updatedCamera;
  }

  onCurrentZoomFocusChange(updatedZoomFocus: ZoomFocus) {
    this.currentZoomFocus = updatedZoomFocus;
  }

  onTriggerDayMode() {
    this.editCameraService.triggerDayMode(this.camera.id).subscribe(() => {
      this.snackBarService.create('Day mode test sent successfully', SnackBarType.Info);
    });
  }

  onTriggerNightMode() {
    this.editCameraService.triggerNightMode(this.camera.id).subscribe(() => {
      this.snackBarService.create('Night mode test sent successfully', SnackBarType.Info);
    });
  }

  onTestOverlay() {
    this.editCameraService.triggerTestOverlay(this.camera.id).subscribe(() => {
      this.snackBarService.create('Overlay test sent successfully', SnackBarType.Info);
    });
  }

  onGetZoomFocus() {
    this.getZoomFocus();
  }

  onSetZoomFocus() {
    this.editCameraService.setZoomAndFocus(this.camera.id, this.currentZoomFocus).subscribe(() => {
      this.getZoomFocus();
    });
  }

  onTriggerAutofocus() {
    this.editCameraService.triggerAutofocus(this.camera.id).subscribe(() => {
      this.getZoomFocus();
    }, () => {
      this.snackBarService.create('Auto focus failed', SnackBarType.Error);
    });
  }

  onEditMask() {
    // This will be handled by the OpenALPR component
  }

  private getZoomFocus() {
    this.editCameraService.getZoomAndFocus(this.camera.id).subscribe((result) => {
      this.currentZoomFocus = result;
    });
  }

  onSave() {
    this.dialogRef.close(this.camera);
  }

  onCancel() {
    this.dialogRef.close(false);
  }
}
