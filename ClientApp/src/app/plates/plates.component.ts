import { animate, state, style, transition, trigger } from '@angular/animations';
import { AfterViewInit, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { SignalrService } from '@app/signalr/signalr.service';
import { Lightbox } from 'ngx-lightbox';
import { Subscription } from 'rxjs';
import { Plate } from './plate';
import { PlateService } from './plate.service';

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
    this.getRecentPlates();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  ngAfterViewInit(): void {
    this.subscribeForUpdates();
  }

  public getRecentPlates() {
    this.plateService.getRecentPlates(5, 0)
      .subscribe(result => {
        this.plates = new MatTableDataSource<Plate>(result.plates);
        this.totalNumberOfPlates = result.totalCount;
        this.plates.paginator = this.paginator;
      });
  }

  public subscribeForUpdates() {
    this.subscriptions.add(this.signalRHub.licensePlateReceived.subscribe(result => {
      this.getRecentPlates();
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
    this.plateService.getRecentPlates($event.pageSize, $event.pageIndex)
      .subscribe(result => {
        this.plates = new MatTableDataSource<Plate>(result.plates);
        this.totalNumberOfPlates = result.totalCount;
      });
  }
}
