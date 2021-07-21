import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Plate } from '../plate/plate';

@Component({
  selector: 'app-edit-plate',
  templateUrl: './edit-plate.component.html',
  styleUrls: ['./edit-plate.component.less']
})
export class EditPlateComponent implements OnInit {

  public plate: Plate;
  
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any
  ) { }

  ngOnInit(): void {
    this.plate = this.data.plate;
  }
}
