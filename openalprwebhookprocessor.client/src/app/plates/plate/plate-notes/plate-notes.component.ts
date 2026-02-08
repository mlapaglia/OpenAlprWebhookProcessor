import { Component, input, model, ChangeDetectionStrategy } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TextFieldModule } from '@angular/cdk/text-field';
import { FormsModule } from '@angular/forms';

import type { Plate } from '../plate';
import { RefreshButtonComponent } from 'app/shared/refresh-button/refresh-button.component';

@Component({
  selector: 'app-plate-notes',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './plate-notes.component.html',
  styleUrls: ['./plate-notes.component.css'],
  imports: [
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    TextFieldModule,
    FormsModule,
    RefreshButtonComponent
],
})
export class PlateNotesComponent {
  readonly plate = model.required<Plate>();
  readonly isSavingNotes = input<boolean>(false);
  readonly saveNotes = input.required<(plate: Plate) => void>();
  readonly clearNotes = input.required<(plate: Plate) => void>();

  public onSaveNotes() {
    this.saveNotes()(this.plate());
  }

  public onClearNotes() {
    this.clearNotes()(this.plate());
  }
}
