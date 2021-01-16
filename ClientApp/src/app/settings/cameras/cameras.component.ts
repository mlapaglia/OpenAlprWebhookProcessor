import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Camera } from './camera';
import { CameraService } from './camera.service';
import { EditCameraComponent } from './edit-camera/edit-camera.component';

@Component({
  selector: 'app-cameras',
  templateUrl: './cameras.component.html',
  styleUrls: ['./cameras.component.less']
})
export class CamerasComponent implements OnInit {
  public cameras: Camera[];
  
  constructor(
    private cameraService: CameraService,
    public dialog: MatDialog) { }
    private cameraEditId: string;

  ngOnInit(): void {
    this.getCameras();
  }

  public getCameras() {
    this.cameraService.getCameras().subscribe(result => {
      result.unshift(new Camera());
      this.cameras = result;
    });
  }

  openEditDialog(cameraId: string): void {
    var cameraToEdit = this.cameras.find(x => x.openAlprCameraId == cameraId);

    if (!cameraToEdit) {
      cameraToEdit = this.cameras[0];
    }

    const dialogRef = this.dialog.open(EditCameraComponent, {
      data: { 'camera': cameraToEdit }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        var cameraToSave = this.cameras.find(x => x.openAlprCameraId == cameraId);
        this.cameraService.upsertCamera(cameraToSave).subscribe(result => {

        });
      }
    });
  }

  public addCamera() {
    this.openEditDialog(null);
  }

  public deleteCamera($event: string) {

  }

  public editCamera($event: string) {
    this.openEditDialog($event);
  }
}
