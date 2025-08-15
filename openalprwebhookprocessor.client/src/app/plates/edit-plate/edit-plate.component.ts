import { Component, inject, ChangeDetectionStrategy, type OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { PlateService } from '../plate.service';
import { MatButtonModule } from '@angular/material/button';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Plate } from '../plate/plate';

interface EditPlateData {
  plateId: string;
  currentPlateNumber: string;
}

@Component({
  selector: 'app-edit-plate',
  templateUrl: './edit-plate.component.html',
  styleUrls: ['./edit-plate.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MatDialogModule, MatFormFieldModule, MatInputModule,
    ReactiveFormsModule, FormsModule, MatButtonModule,
    MatProgressSpinnerModule,
  ],
})

export class EditPlateComponent implements OnInit {
  private readonly dialogRef = inject(MatDialogRef<EditPlateComponent>);
  private readonly plateService = inject(PlateService);
  data = inject<EditPlateData>(MAT_DIALOG_DATA);

  public plateNumber = '';
  public isSaving = false;

  ngOnInit(): void {
    this.plateNumber = this.data.currentPlateNumber;
  }

  onSave(): void {
    if (this.isSaving || !this.plateNumber.trim()) return;

    this.isSaving = true;

    const plateToUpdate = new Plate({
      id: this.data.plateId,
      plateNumber: this.plateNumber.trim(),
    });

    this.plateService.upsertPlate(plateToUpdate).subscribe({
      next: () => {
        this.dialogRef.close({ plateId: this.data.plateId, newPlateNumber: this.plateNumber.trim() });
      },
      error: () => {
        this.isSaving = false;
      },
    });
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
