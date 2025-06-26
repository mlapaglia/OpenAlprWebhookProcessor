import { Component, OnInit, inject } from '@angular/core'
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog'
import { Plate } from '../plate/plate'
import { MatButtonModule } from '@angular/material/button'
import { ReactiveFormsModule, FormsModule } from '@angular/forms'
import { MatInputModule } from '@angular/material/input'
import { MatFormFieldModule } from '@angular/material/form-field'

@Component({
  selector: 'app-edit-plate',
  templateUrl: './edit-plate.component.html',
  styleUrls: ['./edit-plate.component.less'],
  imports: [MatDialogModule, MatFormFieldModule, MatInputModule, ReactiveFormsModule, FormsModule, MatButtonModule],
})
export class EditPlateComponent implements OnInit {
  data = inject<Plate>(MAT_DIALOG_DATA)

  public plate: Plate

  ngOnInit(): void {
    this.plate = this.data
  }
}
