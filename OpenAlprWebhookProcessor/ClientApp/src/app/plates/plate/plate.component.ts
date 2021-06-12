import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Ignore } from '@app/settings/ignores/ignore/ignore';
import { SettingsService } from '@app/settings/settings.service';
import { SnackbarService } from '@app/snackbar/snackbar.service';
import { SnackBarType } from '@app/snackbar/snackbartype';
import { Lightbox } from 'ngx-lightbox';
import { Url } from 'url';
import { Plate } from '../plate';
import { PlateService } from '../plate.service';

@Component({
  selector: 'app-plate',
  templateUrl: './plate.component.html',
  styleUrls: ['./plate.component.less', '../plates.component.css']
})
export class PlateComponent implements OnInit {
  @Output() searchPlatesEvent = new EventEmitter<string>();
  @Input() plate: Plate;

  public addingToIgnoreList: boolean;
  public deletingPlate: boolean;
  public loadingImage: boolean;
  public loadingImageFailed: boolean;
  public loadingPlateImage: boolean;
  public loadingPlateImageFailed: boolean;

  constructor(
    private settingsService: SettingsService,
    private snackbarService: SnackbarService,
    private plateService: PlateService,
    private lightbox: Lightbox
  ) { }

  ngOnInit(): void {
    this.loadingImage = true;
    this.loadingPlateImage = true;
  }

  public addToIgnoreList(plateNumber: string = '') {
    this.addingToIgnoreList = true;
    var ignore = new Ignore();

    ignore.plateNumber = plateNumber;
    ignore.strictMatch = true;
    ignore.description = 'Added from plate list';

    this.settingsService.addIgnore(ignore).subscribe(() => {
      this.addingToIgnoreList = false;
      this.snackbarService.create(`${plateNumber} added to ignore list`, SnackBarType.Saved);
    });
  }

  public deletePlate(plateId: string = '', plateNumber: string = '') {
    this.deletingPlate = true;

    this.plateService.deletePlate(plateId).subscribe(() => {
      this.deletingPlate = false;
      this.snackbarService.create(`${plateNumber} deleted`, SnackBarType.Deleted);
      this.searchPlatesEvent.emit();
    });
  }

  public openLightbox(url: Url, plateNumber: string) {
    var albums = [{
      src: url.toString(),
      caption: plateNumber,
      thumb: url.toString()
    }];

    this.lightbox.open(albums, 0);
  }

  public searchPlates(plateNumber: string) {
    this.searchPlatesEvent.emit(plateNumber);
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
