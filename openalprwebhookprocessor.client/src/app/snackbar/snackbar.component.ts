import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { MAT_SNACK_BAR_DATA, MatSnackBarRef } from '@angular/material/snack-bar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { SnackBarType } from './snackbartype';
import type { SnackBar } from './snackbar';

@Component({
  selector: 'app-snackbar',
  templateUrl: './snackbar.component.html',
  styleUrls: ['./snackbar.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatIconModule, MatButtonModule],
})
export class SnackbarComponent {
  data = inject<SnackBar>(MAT_SNACK_BAR_DATA);
  snackBarRef = inject(MatSnackBarRef);

  get getIcon() {
    switch (this.data.snackType) {
      case SnackBarType.Alert:
        return 'warning';
      case SnackBarType.Info:
        return 'info';
      case SnackBarType.Connected:
        return 'wifi';
      case SnackBarType.Disconnected:
        return 'wifi_off';
      case SnackBarType.Saved:
        return 'save';
      case SnackBarType.Deleted:
        return 'delete';
      case SnackBarType.Successful:
        return 'check_circle';
      case SnackBarType.Error:
        return 'error';
    }
  }

  get getIconColor() {
    switch (this.data.snackType) {
      case SnackBarType.Alert:
        return '#ff9800'; // Orange
      case SnackBarType.Info:
      case SnackBarType.Connected:
      case SnackBarType.Saved:
        return '#2196f3'; // Blue
      case SnackBarType.Successful:
        return '#4caf50'; // Green
      case SnackBarType.Error:
      case SnackBarType.Disconnected:
      case SnackBarType.Deleted:
        return '#f44336'; // Red
      default:
        return '#2196f3'; // Default blue
    }
  }

  dismiss() {
    this.snackBarRef.dismiss();
  }
}
