import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { Plate } from '../plate/plate';
import { MatButtonModule } from '@angular/material/button';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';

@Component({
    selector: 'app-edit-plate',
    templateUrl: './edit-plate.component.html',
    styleUrls: ['./edit-plate.component.less'],
    standalone: true,
    imports: [MatDialogModule, MatFormFieldModule, MatInputModule, ReactiveFormsModule, FormsModule, MatButtonModule]
})
export class EditPlateComponent implements OnInit {

    public plate: Plate;
  
    constructor(
        @Inject(MAT_DIALOG_DATA) public data: Plate
    ) { }

    ngOnInit(): void {
        this.plate = this.data;
    }
}
