import { animate, state, style, transition, trigger } from '@angular/animations';
import { AfterViewInit, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormGroup, FormGroupDirective, NgForm } from '@angular/forms';
import { MatPaginator } from '@angular/material/paginator';
import { SignalrService } from '@app/signalr/signalr.service';
import { Subscription } from 'rxjs';
import { Plate } from './plate/plate';
import { PlateRequest, PlateService } from './plate.service';
import { ErrorStateMatcher } from '@angular/material/core';
import { SnackbarService } from '@app/snackbar/snackbar.service';
import { SnackBarType } from '@app/snackbar/snackbartype';
import { Ignore } from '@app/settings/ignores/ignore/ignore';
import { SettingsService } from '@app/settings/settings.service';
import { Alert } from '@app/settings/alerts/alert/alert';
import { AlertsService } from '@app/settings/alerts/alerts.service';
import { VehicleFilters } from './vehicleFilters';
import { MatDialog } from '@angular/material/dialog';
import { EditPlateComponent } from './edit-plate/edit-plate.component';
import { LocalStorageService } from '@app/_services/local-storage.service';
import { timeStamp } from 'console';

@Component({
  selector: 'app-plates',
  templateUrl: './plates.component.html',
  styleUrls: ['./plates.component.less'],
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

  panelOpenState = false;
  public range: FormGroup;
  public plates: Plate[] = [];
  public totalNumberOfPlates: number;
  public todaysDate = new Date();

  public filterPlateNumber: string;
  public filterPlateNumberIsValid: boolean = true;
  public filterStartOn = new Date();
  public filterEndOn = new Date();
  public filterStrictMatch: boolean;
  public filterStrictMatchEnabled: boolean = true;
  public filterIgnoredPlates: boolean;
  public filterIgnoredPlatesEnabled: boolean = true;
  public filterPlatesSeenLessThan: boolean;
  public regexSearchEnabled: boolean;
  public filterVehicleMake: string;
  public filterVehicleModel: string;
  public filterVehicleType: string;
  public filterVehicleColor: string;
  public filterVehicleRegion: string;
  
  public vehicleFilters: VehicleFilters = {} as VehicleFilters;

  public isDeletingPlate: boolean;
  public isEnrichingPlate: boolean;

  public isAddingToIgnoreList: boolean;
  public isAddingToAlertList: boolean;

  public isLoading: boolean;
  public isSignalrConnected: boolean;

  public pageSize: number = 10;
  private pageSizeCacheKey: string = "platePageSize";
  private pageNumber: number = 0;

  private eventSubscriptions = new Subscription();
  private searchSubscription = new Subscription();

  @ViewChild(MatPaginator) paginator: MatPaginator;
  
  constructor(
    private plateService: PlateService,
    private signalRHub: SignalrService,
    private snackbarService: SnackbarService,
    private alertsService: AlertsService,
    private settingsService: SettingsService,
    private localStorageService: LocalStorageService,
    public dialog: MatDialog) {
      this.range = new FormGroup({
        start: new FormControl(),
        end: new FormControl()
      });
      this.filterStartOn.setDate(new Date().getDate() - 15);
      this.filterEndOn = this.todaysDate;
    }
    
  ngOnInit(): void {
    this.isSignalrConnected = this.signalRHub.isConnected;
    var pageSize = this.localStorageService.getData(this.pageSizeCacheKey);
    this.pageSize = pageSize != null ? parseInt(pageSize) : 25;
    this.searchPlates();
    this.populateFilters();
  }

  ngOnDestroy(): void {
    this.eventSubscriptions.unsubscribe();
    this.searchSubscription.unsubscribe();
  }

  ngAfterViewInit(): void {
    this.subscribeForUpdates();
  }

  public editPlate(plateNumber: string) {
    this.openEditDialog(plateNumber);
  }

  public subscribeForUpdates() {
    this.eventSubscriptions.add(this.signalRHub.connectionStatusChanged.subscribe(status => {
      this.isSignalrConnected = status;
    }));

    this.eventSubscriptions.add(this.signalRHub.licensePlateReceived.subscribe(_ => {
        if (!this.isLoading) {
          this.searchPlates();
        }
    }));
  }

  public onPaginatorPage($event) {
    this.pageSize = $event.pageSize;
    this.localStorageService.setData(this.pageSizeCacheKey, this.pageSize);

    this.pageNumber = $event.pageIndex;

    this.searchPlates();
  }

  public populateFilters() {
    this.plateService.getFilters().subscribe(result => {
      this.vehicleFilters = result;
    });
  }

  public searchPlates(plateNumber: string = '') {
    if (!this.filterPlateNumberIsValid) {
      return;
    }

    if (plateNumber !== '') {
      this.filterPlateNumber = plateNumber;
    }

    this.filterStartOn?.setUTCHours(0,0,0,0);
    this.filterEndOn?.setUTCHours(23,59,59,999);

    var request = new PlateRequest();

    request.pageNumber = this.pageNumber;
    request.pageSize = this.pageSize;
    request.endSearchOn = this.filterEndOn;
    request.startSearchOn = this.filterStartOn;
    request.plateNumber = this.filterPlateNumber;
    request.strictMatch = this.filterStrictMatch;
    request.filterIgnoredPlates = this.filterIgnoredPlates;
    request.filterPlatesSeenLessThan = this.filterPlatesSeenLessThan ? 10 : 0;
    request.regexSearchEnabled = this.regexSearchEnabled;
    request.vehicleMake = this.filterVehicleMake;
    request.vehicleModel = this.filterVehicleModel;
    request.vehicleType = this.filterVehicleType;
    request.vehicleRegion = this.filterVehicleRegion;

    if (this.isLoading) {
      this.searchSubscription.unsubscribe();
    }

    this.isLoading = true;
    this.searchSubscription = this.plateService.searchPlates(request).subscribe(result => {
      this.totalNumberOfPlates = result.totalCount;
      this.plates = result.plates;
      this.isLoading = false;
    }, error => {
      this.isLoading = false;
    });
  }

  public deletePlate(plateId: string = '', plateNumber: string = '') {
    this.isDeletingPlate = true;

    this.plateService.deletePlate(plateId).subscribe(() => {
      this.isDeletingPlate = false;
      this.snackbarService.create(`${plateNumber} deleted`, SnackBarType.Deleted);
      this.searchPlates();
    });
  }

  public addToIgnoreList(plateNumber: string = '') {
    this.isAddingToIgnoreList = true;
    var ignore = new Ignore();

    ignore.plateNumber = plateNumber;
    ignore.strictMatch = true;
    ignore.description = 'Added from plate list';

    this.settingsService.addIgnore(ignore).subscribe(() => {
      this.isAddingToIgnoreList = false;
      this.snackbarService.create(`${plateNumber} added to ignore list`, SnackBarType.Saved);
      this.searchPlates();
    });
  }

  public addToAlertList(plateNumber: string = '') {
    this.isAddingToAlertList = true;
    var alert = new Alert();

    alert.plateNumber = plateNumber;
    alert.strictMatch = true;
    alert.description = 'Added from plate list';

    this.alertsService.addAlert(alert).subscribe(() => {
      this.isAddingToAlertList = false;
      this.snackbarService.create(`${plateNumber} added to alert list`, SnackBarType.Saved);
      this.searchPlates();
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
    this.filterVehicleMake = '';
    this.filterVehicleModel = '';
    this.filterVehicleType = '';
    this.filterVehicleColor = '';
    this.filterVehicleRegion = '';
    this.filterPlatesSeenLessThan = false;

    this.searchPlates();
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

  public enrichPlate(plateId: string) {
    this.isEnrichingPlate = true;
    this.plateService.enrichPlate(plateId).subscribe(_ => {
      this.isEnrichingPlate = false;
      this.snackbarService.create("Plate successfully enriched.", SnackBarType.Saved);
    },
    _ => {
      this.isEnrichingPlate = false;
      this.snackbarService.create("Failed to enrich plate, check the logs.", SnackBarType.Error);
    })
  }

  public openEditDialog(plateId: string): void {
    var plateToEdit = this.plates.find(x => x.id == plateId);

    const dialogRef = this.dialog.open(EditPlateComponent, {
      data: { 'plate': plateToEdit }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        var plateToSave = this.plates.find(x => x.id == plateId);

        this.plateService.upsertPlate(plateToSave).subscribe(_ => {
          this.searchPlates();
        });
      }
    });
  }
}

export class MyErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(control: FormControl | null, form: FormGroupDirective | NgForm | null): boolean {
    const isSubmitted = form && form.submitted;
    return !!(control && control.invalid && (control.dirty || control.touched || isSubmitted));
  }
}
