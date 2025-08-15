import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SnackbarComponent } from './snackbar.component';
import { SnackBarType } from './snackbartype';
import { SnackBar } from './snackbar';

@Injectable({
  providedIn: 'root',
})
export class SnackbarService {
  private readonly snackBar = inject(MatSnackBar);

  create(message: string, snackBarType: SnackBarType, message2?: string) {
    this.snackBar.openFromComponent(
      SnackbarComponent,
      {
        horizontalPosition: 'right',
        verticalPosition: 'bottom',
        duration: this.getDuration(snackBarType),
        data: new SnackBar({
          message,
          message2,
          snackType: snackBarType,
        }),
      });
  }

  private getDuration(snackBarType: SnackBarType): number {
    switch (snackBarType) {
      case SnackBarType.Error:
        return 5000;
      case SnackBarType.Alert:
        return 4000;
      default:
        return 3000;
    }
  }
}
