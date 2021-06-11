import { animate, state, style, transition, trigger } from '@angular/animations';
import { AfterViewInit, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormGroup, FormGroupDirective, NgForm } from '@angular/forms';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { Ignore } from '@app/settings/ignores/ignore/ignore';
import { SettingsService } from '@app/settings/settings.service';
import { SignalrService } from '@app/signalr/signalr.service';
import { SnackbarService } from '@app/snackbar/snackbar.service';
import { SnackBarType } from '@app/snackbar/snackbartype';
import { Lightbox } from 'ngx-lightbox';
import { Subscription } from 'rxjs';
import { Plate } from './plate';
import { PlateRequest, PlateService } from './plate.service';
import { ErrorStateMatcher } from '@angular/material/core';

@Component({
  selector: 'app-plates',
  templateUrl: './plates.component.html',
  styleUrls: ['./plates.component.css'],
  animations: [
    trigger('detailExpand', [
      state('collapsed', style({height: '0px', minHeight: '0'})),
      state('expanded', style({height: '*'})),
      transition('expanded <=> collapsed', animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
    ])],
})
export class PlatesComponent implements OnInit, OnDestroy, AfterViewInit {
  public columnsToDisplay = [
    {
      id: 'openAlprCameraId',
      name: "Camera Id"
    },
    {
      id: 'plateNumber',
      name: "Plate Number"
    },
    {
      id: 'vehicleDescription',
      name: "Vehicle Description"
    },
    {
      id: 'processedPlateConfidence',
      name: "Confidence %"
    }];

  public rowsToDisplay = [
    'openAlprCameraId',
    'plateNumber',
    'vehicleDescription',
    'direction',
    'processedPlateConfidence',
    'receivedOn'
  ];
  
  public range: FormGroup;
  public plates: MatTableDataSource<Plate>;
  public totalNumberOfPlates: number;

  public filterPlateNumber: string;
  public filterPlateNumberIsValid: boolean = true;
  public filterStartOn: Date;
  public filterEndOn: Date;
  public filterStrictMatch: boolean;
  public filterStrictMatchEnabled: boolean = true;
  public filterIgnoredPlates: boolean;
  public filterIgnoredPlatesEnabled: boolean = true;
  public regexSearchEnabled: boolean;

  public addingToIgnoreList: boolean;
  public deletingPlate: boolean;
  public isLoading: boolean;

  private pageSize: number = 10;
  private pageNumber: number = 0;

  private subscriptions = new Subscription();
  
  @ViewChild(MatPaginator) paginator: MatPaginator;
  
  constructor(
    private plateService: PlateService,
    private lightbox: Lightbox,
    private signalRHub: SignalrService,
    private settingsService: SettingsService,
    private snackbarService: SnackbarService) {
      this.range = new FormGroup({
        start: new FormControl(),
        end: new FormControl()
      });
    }
    
  ngOnInit(): void {
    this.searchPlates();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  ngAfterViewInit(): void {
    this.subscribeForUpdates();
  }

  public subscribeForUpdates() {
    this.subscriptions.add(this.signalRHub.licensePlateReceived.subscribe(result => {
        this.searchPlates();
    }));
  }

  public openLightbox(url: string, plateNumber: string) {
    var albums = [{
      src: url,
      caption: plateNumber,
      thumb: url
    }];

    this.lightbox.open(albums, 0);
  }

  public onPaginatorPage($event) {
    this.pageSize = $event.pageSize;
    this.pageNumber = $event.pageIndex;

    this.searchPlates();
  }

  public searchPlates(plateNumber: string = '') {
    var request = new PlateRequest();

    if (!this.filterPlateNumberIsValid) {
      return;
    }

    if (plateNumber !== '') {
      this.filterPlateNumber = plateNumber;
    }

    request.pageNumber = this.pageNumber;
    request.pageSize = this.pageSize;
    request.endSearchOn = this.filterEndOn;
    request.startSearchOn = this.filterStartOn;
    request.plateNumber = this.filterPlateNumber;
    request.strictMatch = this.filterStrictMatch;
    request.filterIgnoredPlates = this.filterIgnoredPlates;
    request.regexSearchEnabled = this.regexSearchEnabled;

    this.isLoading = true;
    this.plateService.searchPlates(request).subscribe(result => {
      this.totalNumberOfPlates = result.totalCount;
      this.plates = new MatTableDataSource<Plate>(result.plates);
      this.isLoading = false;
    });
  }

  public clearFilters() {
    this.filterEndOn = null;
    this.filterStartOn = null;
    this.filterPlateNumber = '';
    this.filterPlateNumberIsValid = true;
    this.filterStrictMatch = false;
    this.filterStrictMatchEnabled = true;
    this.filterIgnoredPlates = false;
    this.filterIgnoredPlatesEnabled = true;
    this.regexSearchEnabled = false;

    this.searchPlates();
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
      this.searchPlates();
      this.snackbarService.create(`${plateNumber} deleted`, SnackBarType.Deleted);
    });
  }

  public validateSearchPlateNumber() {
    if (this.filterPlateNumber == '') {
      this.filterPlateNumberIsValid = true;
    }

    if (this.regexSearchEnabled) {
      try {
        new RegExp(this.filterPlateNumber);
        this.filterPlateNumberIsValid = true;
      }
      catch {
        this.filterPlateNumberIsValid = false;
      }
    }
    else {
      this.filterPlateNumberIsValid = true;
    }
  }

  public regexSearchToggled() {
    if (this.regexSearchEnabled) {
      this.filterStrictMatch = false;
      this.filterStrictMatchEnabled = false;
      this.filterIgnoredPlates = false;
      this.filterIgnoredPlatesEnabled = false;
    }
    else {
      this.filterStrictMatchEnabled = true;
      this.filterIgnoredPlatesEnabled = true;
    }
    this.validateSearchPlateNumber();
  }
}

export class MyErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(control: FormControl | null, form: FormGroupDirective | NgForm | null): boolean {
    const isSubmitted = form && form.submitted;
    return !!(control && control.invalid && (control.dirty || control.touched || isSubmitted));
  }
}
