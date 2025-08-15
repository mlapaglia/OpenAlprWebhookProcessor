import { Injectable, inject, signal } from '@angular/core';
import { PlateService } from '../../plate.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import type { Plate } from '../plate';
import { Observable } from 'rxjs';

@Injectable()
export class PlateNotesStateService {
  private readonly plateService = inject(PlateService);
  private readonly snackbarService = inject(SnackbarService);

  private readonly _isSavingNotes = signal(false);

  readonly isSavingNotes = this._isSavingNotes.asReadonly();

  saveNotes(plate: Plate): Observable<Plate> {
    this._isSavingNotes.set(true);

    return new Observable(subscriber => {
      this.plateService.upsertPlate(plate).subscribe({
        next: () => {
          this._isSavingNotes.set(false);
          this.snackbarService.create(`Notes saved for: ${plate.plateNumber}`, SnackBarType.Saved);
          subscriber.next(plate);
          subscriber.complete();
        },
        error: () => {
          this._isSavingNotes.set(false);
          this.snackbarService.create(`Failed to save notes for: ${plate.plateNumber}`, SnackBarType.Error);
          subscriber.error();
        },
      });
    });
  }

  clearNotes(plate: Plate): Plate {
    const updatedPlate = { ...plate };
    updatedPlate.notes = '';
    return updatedPlate;
  }
}
