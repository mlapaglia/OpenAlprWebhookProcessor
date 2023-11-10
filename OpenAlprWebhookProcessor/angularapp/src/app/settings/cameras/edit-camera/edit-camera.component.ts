import { animate, style, transition, trigger } from '@angular/animations';
import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { Camera } from '../camera';
import { EditCameraService } from './edit-camera.service';
import { ZoomFocus } from './zoomfocus';

@Component({
  selector: 'app-edit-camera',
  templateUrl: './edit-camera.component.html',
  styleUrls: ['./edit-camera.component.less'],
  animations: [
    trigger(
      'inOutAnimation', 
      [
        transition(
          ':enter', [
            style({ height: 0, opacity: 0 }),
            animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)', 
                    style({ height: '*', opacity: 1 }))]),
        transition(
          ':leave', [
            style({ height: '*', opacity: 1 }),
            animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)', 
                    style({ height: 0, opacity: 0 }))])
      ])]
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
    @Inject(MAT_DIALOG_DATA) public data: any) { }

  ngOnInit(): void {
    this.camera = this.data.camera;
    this.getZoomFocus();
  }

  public triggerDayMode() {
    this.editCameraService.triggerDayMode(this.camera.id).subscribe(_ => {
      this.snackBarService.create("day mode test sent successfully", SnackBarType.Info);
    });
  }

  public triggerNightMode() {
    this.editCameraService.triggerNightMode(this.camera.id).subscribe(_ => {
      this.snackBarService.create("night mode test sent successfully", SnackBarType.Info);
    });
  }

  public testOverlay() {
    this.editCameraService.triggerTestOverlay(this.camera.id).subscribe(_ => {
      this.snackBarService.create("overlay test sent successfully", SnackBarType.Info);
    });
  }

  public getZoomFocus() {
    this.editCameraService.getZoomAndFocus(this.camera.id).subscribe(result => {
      this.currentZoomFocus = result;
    });
  }

  public setZoomFocus() {
    this.editCameraService.setZoomAndFocus(this.camera.id, this.currentZoomFocus).subscribe(_ => {
      this.getZoomFocus();
    });
  }

  public triggerAutofocus() {
    this.editCameraService.triggerAutofocus(this.camera.id).subscribe(success => {
      this.getZoomFocus();
    },
    (error) => {
      this.snackBarService.create("auto focus failed", SnackBarType.Error);
    });
  }

  public editMask() {
      this.isEditingMask = !this.isEditingMask;
  }
}