import { Component, Input, inject, Output, EventEmitter } from '@angular/core';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { PlateService } from '../plate.service';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TextFieldModule } from '@angular/cdk/text-field';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import type { Plate } from './plate';

@Component({
  selector: 'app-plate-notes',
  templateUrl: './plate-notes.component.html',
  styleUrls: ['./plate-notes.component.css'],
  imports: [
    CommonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    TextFieldModule,
    FormsModule,
  ],
})
export class PlateNotesComponent {
  private readonly plateService = inject(PlateService);
  private readonly snackbarService = inject(SnackbarService);

  @Input() plate!: Plate;
  @Output() plateChange = new EventEmitter<Plate>();

  public isSavingNotes = false;

  public saveNotes() {
    this.isSavingNotes = true;
    this.plateService.upsertPlate(this.plate).subscribe(() => {
      this.isSavingNotes = false;
      this.snackbarService.create(`Notes saved for: ${this.plate.plateNumber}`, SnackBarType.Saved);
      this.plateChange.emit(this.plate);
    },
    () => {
      this.isSavingNotes = false;
      this.snackbarService.create(`Failed to save notes for: ${this.plate.plateNumber}`, SnackBarType.Error);
    });
  }

  public clearNotes() {
    this.plate.notes = '';
    this.plateChange.emit(this.plate);
  }
}