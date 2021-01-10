import { animate, state, style, transition, trigger } from '@angular/animations';
import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { FormControl } from '@angular/forms';
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
export class PlatesComponent implements OnInit, AfterViewInit {
  columnsToDisplay = ['openAlprCameraId', 'plateNumber', 'vehicleDescription', 'processedPlateConfidence', 'receivedOn'];
  rowsToDisplay = ['openAlprCameraId', 'plateNumber', 'vehicleDescription', 'direction', 'processedPlateConfidence', 'receivedOn'];
  plates: MatTableDataSource<Plate>;

  myControl = new FormControl();
  options: string[] = ['One', 'Two', 'Three'];
  
  @ViewChild(MatPaginator) paginator: MatPaginator;
  
  constructor(
    private plateService: PlateService,
    private lightbox: Lightbox) { }

  ngOnInit(): void {
    this.plateService.getRecentPlates()
      .subscribe(result => {
        this.plates = new MatTableDataSource<Plate>(result);
        this.plates.paginator = this.paginator;
      });
  }

  ngAfterViewInit(): void {
    
  }

  public openLightbox(url: string, plateNumber: string) {
    var albums =[ {
      src: url,
      caption: plateNumber,
      thumb: url
    }];

    this.lightbox.open(albums, 0);
  }
}
