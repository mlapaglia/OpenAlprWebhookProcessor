import { animate, state, style, transition, trigger } from '@angular/animations';
import { AfterViewInit, Component, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormGroup, FormGroupDirective, NgForm, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { SignalrService } from 'app/signalr/signalr.service';
import { Subscription } from 'rxjs';
import { Plate } from './plate/plate';
import { PlateRequest, PlateService } from './plate.service';
import { ErrorStateMatcher, MatNativeDateModule, MatOptionModule } from '@angular/material/core';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { Ignore } from 'app/settings/ignores/ignore';
import { SettingsService } from 'app/settings/settings.service';
import { Alert } from 'app/settings/alerts/alert';
import { AlertsService } from 'app/settings/alerts/alerts.service';
import { VehicleFilters } from './vehicleFilters';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { EditPlateComponent } from './edit-plate/edit-plate.component';
import { LocalStorageService } from 'app/_services/local-storage.service';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { PlateComponent } from './plate/plate.component';
import { MatIconModule } from '@angular/material/icon';
import { DatePipe, CommonModule } from '@angular/common';
import { MatExpansionModule } from '@angular/material/expansion';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
    selector: 'app-plates',
    templateUrl: './plates.component.html',
    styleUrls: ['./plates.component.less'],
    animations: [
        trigger('detailExpand', [
            state('collapsed', style({ height: '0px', minHeight: '0' })),
            state('expanded', style({ height: '*' })),
            transition('expanded <=> collapsed', animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
        ])
    ],
    standalone: true,
    imports: [
        MatExpansionModule,
        CommonModule,
        MatIconModule,
        PlateComponent,
        MatButtonModule,
        MatProgressSpinnerModule,
        MatPaginatorModule,
        MatCardModule,
        MatDialogModule,
        MatNativeDateModule,
        MatFormFieldModule,
        MatDatepickerModule,
        ReactiveFormsModule,
        FormsModule,
        MatInputModule,
        MatSelectModule,
        MatOptionModule,
        MatCheckboxModule,
        DatePipe,
    ],
})
export class PlatesComponent implements OnInit, OnDestroy, AfterViewInit {
  @Input() id: string;

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
  public visiblePlateIds: string[] = [];
  public totalNumberOfPlates: number;
  public todaysDate: Date;

  public filterPlateNumber: string;
  public filterPlateNumberIsValid: boolean = true;
  public filterStartOn: Date;
  public filterEndOn: Date;
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


  public pageSize: number = 10;
  private pageSizeCacheKey: string = "platePageSize";
  private pageNumber: number = 0;

  private eventSubscriptions = new Subscription();
  private searchSubscription = new Subscription();
  private filterSubscription = new Subscription();
  
  @ViewChild(MatPaginator) paginator: MatPaginator;
  
  constructor(
    private plateService: PlateService,
    private signalRHub: SignalrService,
    private snackbarService: SnackbarService,
    private alertsService: AlertsService,
    private settingsService: SettingsService,
    private localStorageService: LocalStorageService,
    private route: ActivatedRoute,
    private router: Router,
    public dialog: MatDialog) {
      this.range = new FormGroup({
        start: new FormControl(),
        end: new FormControl()
      });
    }
    
  ngOnInit(): void {
    const pageSize = this.localStorageService.getData(this.pageSizeCacheKey);
    this.pageSize = pageSize != '' ? parseInt(pageSize) : 25;
    this.setInitialDateFilter();
    this.populateFilters();
    this.eventSubscriptions.add(this.route.params.subscribe(params => {
      const id = params['id'] as string;

      if (id !== undefined) {
        this.getPlate(id);
      }
      else {
        this.searchPlates();
      }
    }));
  }

  ngOnDestroy(): void {
    this.eventSubscriptions.unsubscribe();
    this.searchSubscription.unsubscribe();
    this.filterSubscription.unsubscribe();
  }

  ngAfterViewInit(): void {
    this.subscribeForUpdates();
  }

  public setInitialDateFilter() {
    this.todaysDate = new Date();
    this.filterStartOn = new Date();
    this.filterEndOn = new Date();
    
    this.filterStartOn.setDate(new Date().getDate() - 15);
    this.filterEndOn = this.todaysDate;
  }

  public editPlate(plateNumber: string) {
    this.openEditDialog(plateNumber);
  }

  public subscribeForUpdates() {
    this.eventSubscriptions.add(this.signalRHub.licensePlateReceived.subscribe(() => {
        if (!this.isLoading) {
          this.searchPlates();
        }
    }));
  }

  public plateOpened(plateId: string) {
    this.visiblePlateIds.push(plateId);
  }

  public plateClosed(plateIdToRemove: string) {
    this.visiblePlateIds.forEach((plateId, index) => {
      if(plateId === plateIdToRemove) {
        this.visiblePlateIds.splice(index, 1);
      }
    })
  }

  public onPaginatorPage($event) {
    this.pageSize = $event.pageSize;
    this.localStorageService.setData(this.pageSizeCacheKey, this.pageSize);

    this.pageNumber = $event.pageIndex;

    this.searchPlates();
  }

  public populateFilters() {
    this.filterSubscription.add(this.plateService.getFilters().subscribe(result => {
      this.vehicleFilters = result;
    }));
  }

  public getPlate(plateId: string) {
    this.isLoading = true;
    this.searchSubscription.add(this.plateService.getPlate(plateId).subscribe((plateResponse) => {
      this.totalNumberOfPlates = 1;
      this.plates = [
        plateResponse.plate
      ];

      this.plates[0].isOpen = true;
      this.isLoading = false;
    }, () => {
      this.isLoading = false;
      this.snackbarService.create(`Error searching for plate, check the logs`, SnackBarType.Error)
    }));
  }

  public searchPlates(plateNumber: string = '') {
    this.router.navigate(["/plates"]);
    
    if (!this.filterPlateNumberIsValid) {
      return;
    }

    if (plateNumber !== '') {
      this.filterPlateNumber = plateNumber;
    }

    this.filterStartOn?.setUTCHours(0,0,0,0);
    this.filterEndOn?.setUTCHours(23,59,59,999);

    const request = new PlateRequest();

    request.pageNumber = this.pageNumber;
    request.pageSize = this.pageSize;
    request.endSearchOn = this.filterEndOn;
    request.startSearchOn = this.filterStartOn;
    request.plateNumber = this.filterPlateNumber;
    request.strictMatch = this.filterStrictMatch;
    request.filterIgnoredPlates = this.filterIgnoredPlates;
    request.filterPlatesSeenLessThan = this.filterPlatesSeenLessThan ? 10 : 0;
    request.regexSearchEnabled = this.regexSearchEnabled;
    request.vehicleColor = this.filterVehicleColor;
    request.vehicleMake = this.filterVehicleMake;
    request.vehicleModel = this.filterVehicleModel;
    request.vehicleType = this.filterVehicleType;
    request.vehicleRegion = this.filterVehicleRegion;

    if (this.isLoading) {
      this.searchSubscription.unsubscribe();
    }

    this.isLoading = true;
    this.searchSubscription.add(this.plateService.searchPlates(request).subscribe(result => {
      this.totalNumberOfPlates = result.totalCount;
      this.plates = result.plates;
      this.isLoading = false;
    }, () => {
      this.isLoading = false;
      this.snackbarService.create(`Error searching for plates, check the logs`, SnackBarType.Error)
    }));
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
    const ignore = new Ignore();

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
    const alert = new Alert();

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
    this.setInitialDateFilter();

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
    this.plateService.enrichPlate(plateId).subscribe(() => {
      this.isEnrichingPlate = false;
      this.snackbarService.create("Plate successfully enriched.", SnackBarType.Saved);
    },
    () => {
      this.isEnrichingPlate = false;
      this.snackbarService.create("Failed to enrich plate, check the logs.", SnackBarType.Error);
    })
  }

  public openEditDialog(plateId: string): void {
    const plateToEdit = this.plates.find(x => x.id == plateId);

    const dialogRef = this.dialog.open(EditPlateComponent, {
      data: plateToEdit
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        const plateToSave = this.plates.find(x => x.id == plateId);

        if(plateToSave !== undefined) {
          this.plateService.upsertPlate(plateToSave).subscribe(() => {
            this.searchPlates();
          });
        }
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
