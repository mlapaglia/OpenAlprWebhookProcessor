import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Camera } from '../camera';

@Component({
  selector: 'app-camera',
  templateUrl: './camera.component.html',
  styleUrls: ['./camera.component.less']
})
export class CameraComponent implements OnInit {
  @Input() camera: Camera;
  @Output() add: EventEmitter<number> = new EventEmitter();
  @Output() edit: EventEmitter<number> = new EventEmitter();
  @Output() delete: EventEmitter<number> = new EventEmitter();
  @Output() test: EventEmitter<number> = new EventEmitter();

  constructor() { }

  ngOnInit(): void {
  }

  public addCamera() {
    this.add.emit(null);
  }

  public editCamera() {
    this.edit.emit(this.camera.openAlprCameraId);
  }

  public removeCamera() {
    this.delete.emit(this.camera.openAlprCameraId);
  }

  public testCamera() {
    this.test.emit(this.camera.openAlprCameraId);
  }
}
