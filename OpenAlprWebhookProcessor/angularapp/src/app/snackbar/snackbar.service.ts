import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SnackbarComponent } from './snackbar.component';
import { SnackBarType } from './snackbartype';

@Injectable({
  providedIn: 'root'
})
export class SnackbarService {
  constructor(private snackBar: MatSnackBar) { }

  create(message: string, snackBarType: SnackBarType) {
    this.snackBar.openFromComponent(
      SnackbarComponent,
      {
        horizontalPosition: 'right',
        verticalPosition: 'bottom',
        duration: 1500,
        data: {
          message: message,
          snackType: snackBarType
        },
      });
  }
}
