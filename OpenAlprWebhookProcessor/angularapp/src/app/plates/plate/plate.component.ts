import { DatePipe } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { Lightbox } from 'ngx-lightbox';
import { Url } from 'url';
import { PlateService } from '../plate.service';
import { Plate } from './plate';
import { PlateStatisticsData } from './plateStatistics';

@Component({
  selector: 'app-plate',
  templateUrl: './plate.component.html',
  styleUrls: ['./plate.component.less']
})
export class PlateComponent implements OnInit {
  @Input() plate: Plate;

  public loadingImage: boolean;
  public loadingImageFailed: boolean;
  public loadingPlateImage: boolean;
  public loadingPlateImageFailed: boolean;
  public loadingStatistics: boolean;
  public loadingStatisticsFailed: boolean;
  public isSavingNotes: boolean;

  public plateStatistics: PlateStatisticsData[] = [];
  public displayedColumns: string[] = ['key', 'value'];

  constructor(
    private lightbox: Lightbox,
    private plateService: PlateService,
    private datePipe: DatePipe,
    private snackbarService: SnackbarService,
  ) { }

  ngOnInit(): void {
    this.loadingImage = true;
    this.loadingPlateImage = true;
    this.getPlateStatistics();
  }

  private getPlateStatistics() {
    this.loadingStatistics = true;
    this.plateService.getPlateStatistics(this.plate.plateNumber).subscribe(result => {
      this.loadingStatistics = false;
      this.plateStatistics.push({
        key: "Confidence",
        value: this.plate.processedPlateConfidence + "%",
      });

      this.plateStatistics.push({
        key: "Seen past 90 days",
        value: result.last90Days.toString(),
      });

      this.plateStatistics.push({
        key: "Total Seen",
        value: result.totalSeen.toString(),
      });

      this.plateStatistics.push({
        key: "First seen",
        value: this.datePipe.transform(result.firstSeen, 'medium') || "",
      });

      this.plateStatistics.push({
        key: "Last seen",
        value: this.datePipe.transform(result.lastSeen , 'medium') || "",
      });

      this.plateStatistics.push({
        key: "Processing time",
        value: this.plate.openAlprProcessingTimeMs.toString() + "ms",
      });

      this.plateStatistics.push({
        key: "Possible plates",
        value: this.plate.possiblePlateNumbers,
      });

      this.plateStatistics.push({
        key: "Region",
        value: this.plate.region,
      });
    },
    error => {
      this.loadingStatistics = false;
      this.loadingStatisticsFailed = true;
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

  public saveNotes() {
    this.isSavingNotes = true;
    this.plateService.upsertPlate(this.plate).subscribe(_ => {
      this.isSavingNotes = false;
      this.snackbarService.create(`Notes saved for: ${this.plate.plateNumber}`, SnackBarType.Saved);
    });
  }

  public clearNotes() {
    this.plate.notes = '';
  }
}
