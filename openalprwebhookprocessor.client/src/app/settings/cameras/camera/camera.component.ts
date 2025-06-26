import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core'
import { Camera } from '../camera'
import { MatButtonModule } from '@angular/material/button'
import { MatTooltipModule } from '@angular/material/tooltip'
import { MatIconModule } from '@angular/material/icon'
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner'
import { MatCardModule } from '@angular/material/card'
import { DatePipe } from '@angular/common'

@Component({
  selector: 'app-camera',
  templateUrl: './camera.component.html',
  styleUrls: ['./camera.component.less'],
  imports: [MatCardModule, MatProgressSpinnerModule, MatIconModule, MatTooltipModule, MatButtonModule, DatePipe],
})
export class CameraComponent implements OnInit {
  @Input() camera: Camera
  @Output() add = new EventEmitter<number>()
  @Output() edit = new EventEmitter<string>()
  @Output() delete = new EventEmitter<string>()
  @Output() test = new EventEmitter<string>()

  public isLoadingImage = true
  public isLoadingFailed = false

  ngOnInit(): void {
    if (!this.camera.sampleImageUrl) {
      this.isLoadingImage = false
    }
  }

  public addCamera() {
    this.add.emit(undefined)
  }

  public editCamera() {
    this.edit.emit(this.camera.id)
  }

  public removeCamera() {
    this.delete.emit(this.camera.id)
  }

  public testCamera() {
    this.test.emit(this.camera.id)
  }

  public imageLoaded() {
    this.isLoadingImage = false
  }

  public imageFailedToLoad() {
    this.isLoadingImage = false
    this.isLoadingFailed = true
  }
}
