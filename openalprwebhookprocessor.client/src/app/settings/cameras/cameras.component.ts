import { ChangeDetectionStrategy, Component, inject, type OnDestroy, type OnInit } from '@angular/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Camera } from './camera';
import { SettingsService } from '../settings.service';
import { EditCameraComponent } from './edit-camera/edit-camera.component';
import { CameraComponent } from './camera/camera.component';

import { MatGridListModule } from '@angular/material/grid-list';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

@Component({
  selector: 'app-cameras',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './cameras.component.html',
  styleUrls: ['./cameras.component.less'],
  imports: [MatGridListModule, CameraComponent, MatDialogModule],
})
export class CamerasComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  private readonly settingsService = inject(SettingsService);
  dialog = inject(MatDialog);

  public cameras: Camera[];

  ngOnInit(): void {
    this.getCameras();
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  public getCameras() {
    this.subscribeAndMarkForCheck(
      this.settingsService.getCameras(),
      result => {
        result.unshift(new Camera());
        this.cameras = result;
        this.markForCheck();
      });
  }

  openEditDialog(cameraId: string): void {
    let cameraToEdit = this.cameras.find(x => x.id == cameraId);

    cameraToEdit ??= this.cameras[0];

    const dialogRef = this.dialog.open(EditCameraComponent, {
      data: cameraToEdit,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getCameras();
      }
    });
  }

  public addCamera() {
    this.openEditDialog('');
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
