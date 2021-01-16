import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Camera } from '../camera';

@Component({
  selector: 'app-edit-camera',
  templateUrl: './edit-camera.component.html',
  styleUrls: ['./edit-camera.component.less']
})
export class EditCameraComponent implements OnInit {
  public camera: Camera;
  public hidePassword = true;

  constructor(
    public dialogRef: MatDialogRef<EditCameraComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any) { }

  ngOnInit(): void {
    this.camera = this.data.camera;
  }
}