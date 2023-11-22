import { Component, OnInit } from '@angular/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Camera } from './camera';
import { SettingsService } from '../settings.service';
import { EditCameraComponent } from './edit-camera/edit-camera.component';
import { CameraComponent } from './camera/camera.component';
import { NgFor } from '@angular/common';
import { MatGridListModule } from '@angular/material/grid-list';

@Component({
    selector: 'app-cameras',
    templateUrl: './cameras.component.html',
    styleUrls: ['./cameras.component.less'],
    standalone: true,
    imports: [MatGridListModule, NgFor, CameraComponent, MatDialogModule]
})
export class CamerasComponent implements OnInit {
  public cameras: Camera[];
  
  constructor(
    private settingsService: SettingsService,
    public dialog: MatDialog) { }

  ngOnInit(): void {
    this.getCameras();
  }

  public getCameras() {
    this.settingsService.getCameras().subscribe(result => {
      result.unshift(new Camera());
      this.cameras = result;
    });
  }

  openEditDialog(cameraId: string): void {
    let cameraToEdit = this.cameras.find(x => x.id == cameraId);

    if (!cameraToEdit) {
      cameraToEdit = this.cameras[0];
    }

    const dialogRef = this.dialog.open(EditCameraComponent, {
      data: cameraToEdit
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        let cameraToSave = this.cameras.find(x => x.id == cameraId);

        if (!cameraToSave) {
          cameraToSave = this.cameras[0];
        }

        this.settingsService.upsertCamera(cameraToSave).subscribe(() => {
          this.getCameras();
        });
      }
    });
  }

  public addCamera() {
    this.openEditDialog("");
  }

  public deleteCamera($event: string) {
    this.settingsService.deleteCamera($event).subscribe(() => {
      this.getCameras();
    });
  }

  public editCamera($event: string) {
    this.openEditDialog($event);
  }
}
