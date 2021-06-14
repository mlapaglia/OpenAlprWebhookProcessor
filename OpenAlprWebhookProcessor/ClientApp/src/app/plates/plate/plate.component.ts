import { Component, Input, OnInit } from '@angular/core';
import { Lightbox } from 'ngx-lightbox';
import { Url } from 'url';
import { Plate } from '../plate';

@Component({
  selector: 'app-plate',
  templateUrl: './plate.component.html',
  styleUrls: ['./plate.component.less', '../plates.component.css']
})
export class PlateComponent implements OnInit {
  @Input() plate: Plate;

  public loadingImage: boolean;
  public loadingImageFailed: boolean;
  public loadingPlateImage: boolean;
  public loadingPlateImageFailed: boolean;

  constructor(
    private lightbox: Lightbox
  ) { }

  ngOnInit(): void {
    this.loadingImage = true;
    this.loadingPlateImage = true;
  }

  public openLightbox(url: Url, plateNumber: string) {
    var albums = [{
      src: url.toString(),
      caption: plateNumber,
      thumb: url.toString()
    }];

    this.lightbox.open(albums, 0);
  }

  public imageLoaded() {
    this.loadingImage = false;
  }

  public imageFailedToLoad() {
    this.loadingImage = false;
    this.loadingImageFailed = true;
  }

  public plateImageLoaded() {
    this.loadingPlateImage = false;
  }

  public plateImageFailedToLoad() {
    this.loadingPlateImage = false;
    this.loadingPlateImageFailed = true;
  }
}
