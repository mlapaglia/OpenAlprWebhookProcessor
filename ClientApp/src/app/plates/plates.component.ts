import { animate, state, style, transition, trigger } from '@angular/animations';
import { Component, OnInit, ViewChild } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { Lightbox } from 'ngx-lightbox';
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
export class PlatesComponent implements OnInit {
  columnsToDisplay = ['openAlprCameraId', 'plateNumber', 'vehicleDescription', 'processedPlateConfidence', 'receivedOn'];
  rowsToDisplay = ['openAlprCameraId', 'plateNumber', 'vehicleDescription', 'direction', 'processedPlateConfidence', 'receivedOn'];
  plates: MatTableDataSource<Plate>;
  
  public totalNumberOfPlates: number;

  @ViewChild(MatPaginator) paginator: MatPaginator;
  
  constructor(
    private plateService: PlateService,
    private lightbox: Lightbox) {
    }
    
  ngOnInit(): void {
    this.plateService.getRecentPlates(5, 0)
    .subscribe(result => {
      this.plates = new MatTableDataSource<Plate>(result.plates);
      this.totalNumberOfPlates = result.totalCount;
      this.plates.paginator = this.paginator;
    });
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
