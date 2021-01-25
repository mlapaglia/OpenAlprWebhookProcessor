import { animate, state, style, transition, trigger } from '@angular/animations';
import { AfterViewInit, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { SignalrService } from '@app/signalr/signalr.service';
import { Lightbox } from 'ngx-lightbox';
import { Subscription } from 'rxjs';
import { Plate } from './plate';
import { PlateRequest, PlateService } from './plate.service';

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
  public filterStartOn: Date;
  public filterEndOn: Date;
  public filterStrictMatch: boolean;
  public filterIgnoredPlates: boolean;

  private pageSize: number = 10;
  private pageNumber: number = 0;

  private subscriptions = new Subscription();
  
  @ViewChild(MatPaginator) paginator: MatPaginator;
  
  constructor(
    private plateService: PlateService,
    private lightbox: Lightbox,
    private signalRHub: SignalrService) {
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
    var albums =[ {
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

    this.plateService.searchPlates(request).subscribe(result => {
      this.totalNumberOfPlates = result.totalCount;
      this.plates = new MatTableDataSource<Plate>(result.plates);
    });
  }

  public clearFilters() {
    this.filterEndOn = null;
    this.filterStartOn = null;
    this.filterPlateNumber = '';
    this.filterStrictMatch = false;
    this.filterIgnoredPlates = false;

    this.searchPlates();
  }
}
