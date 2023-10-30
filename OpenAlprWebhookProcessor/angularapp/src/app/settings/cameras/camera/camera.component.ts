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
  @Output() edit: EventEmitter<string> = new EventEmitter();
  @Output() delete: EventEmitter<string> = new EventEmitter();
  @Output() test: EventEmitter<string> = new EventEmitter();

  public isLoadingImage: boolean = true;
  public isLoadingFailed: boolean = false;

  constructor() { }

  ngOnInit(): void {
    if (!this.camera.sampleImageUrl) {
      this.isLoadingImage = false;
    }
  }

  public addCamera() {
    this.add.emit(undefined);
  }

  public editCamera() {
    this.edit.emit(this.camera.id);
  }

  public removeCamera() {
    this.delete.emit(this.camera.id);
  }

  public testCamera() {
    this.test.emit(this.camera.id);
  }

  public imageLoaded() {
    this.isLoadingImage = false;
  }

  public imageFailedToLoad() {
    this.isLoadingImage = false;
    this.isLoadingFailed = true;
  }
}
