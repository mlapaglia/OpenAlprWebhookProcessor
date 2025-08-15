import { ChangeDetectionStrategy, Component, inject, type OnDestroy, type OnInit } from '@angular/core';
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
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';
import { RefreshButtonComponent } from 'app/shared/refresh-button/refresh-button.component';
import { SettingsService } from '../../settings.service';

@Component({
  selector: 'app-edit-camera',
  changeDetection: ChangeDetectionStrategy.OnPush,
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
    RefreshButtonComponent,
  ],
})
export class EditCameraComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  dialogRef = inject<MatDialogRef<EditCameraComponent>>(MatDialogRef);
  private readonly snackBarService = inject(SnackbarService);
  private readonly editCameraService = inject(EditCameraService);
  private readonly settingsService = inject(SettingsService);
  data = inject<Camera>(MAT_DIALOG_DATA);

  public camera: Camera;
  public currentZoomFocus: ZoomFocus = new ZoomFocus();
  public isSaving = false;

  ngOnInit(): void {
    this.camera = { ...this.data };
    this.getZoomFocus();
    this.markForCheck();
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  onCameraChange(updatedCamera: Camera) {
    this.camera = updatedCamera;
    this.markForCheck();
  }

  onCurrentZoomFocusChange(updatedZoomFocus: ZoomFocus) {
    this.currentZoomFocus = updatedZoomFocus;
    this.markForCheck();
  }

  onTriggerDayMode() {
    this.subscribeAndMarkForCheck(
      this.editCameraService.triggerDayMode(this.camera.id),
      () => {
        this.snackBarService.create('Day mode test sent successfully', SnackBarType.Info);
      },
    );
  }

  onTriggerNightMode() {
    this.subscribeAndMarkForCheck(
      this.editCameraService.triggerNightMode(this.camera.id),
      () => {
        this.snackBarService.create('Night mode test sent successfully', SnackBarType.Info);
      },
    );
  }

  onTestOverlay() {
    this.subscribeAndMarkForCheck(
      this.editCameraService.triggerTestOverlay(this.camera.id),
      () => {
        this.snackBarService.create('Overlay test sent successfully', SnackBarType.Info);
      },
    );
  }

  onGetZoomFocus() {
    this.getZoomFocus();
  }

  onSetZoomFocus() {
    this.subscribeAndMarkForCheck(
      this.editCameraService.setZoomAndFocus(this.camera.id, this.currentZoomFocus),
      () => {
        this.getZoomFocus();
      },
    );
  }

  onTriggerAutofocus() {
    this.subscribeAndMarkForCheck(
      this.editCameraService.triggerAutofocus(this.camera.id),
      () => {
        this.getZoomFocus();
      },
      () => {
        this.snackBarService.create('Auto focus failed', SnackBarType.Error);
      },
    );
  }

  onEditMask() {
    // This will be handled by the OpenALPR component
  }

  private getZoomFocus() {
    this.subscribeAndMarkForCheck(
      this.editCameraService.getZoomAndFocus(this.camera.id),
      (result) => {
        this.currentZoomFocus = result;
      },
    );
  }

  onSave() {
    this.isSaving = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.settingsService.upsertCamera(this.camera),
      () => {
        this.isSaving = false;
        this.snackBarService.create('Camera saved successfully', SnackBarType.Successful);
        this.dialogRef.close(this.camera);
      },
      (error) => {
        this.isSaving = false;
        this.snackBarService.create('Failed to save camera', SnackBarType.Error, error as string);
        this.markForCheck();
      },
    );
  }

  onCancel() {
    this.dialogRef.close(false);
  }
}
