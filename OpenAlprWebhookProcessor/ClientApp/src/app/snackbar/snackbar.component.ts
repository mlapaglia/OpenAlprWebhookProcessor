import { Component, Inject, OnInit } from '@angular/core';
import { MAT_SNACK_BAR_DATA } from '@angular/material/snack-bar';
import { SnackBarType } from './snackbartype';

@Component({
  selector: 'app-snackbar',
  templateUrl: './snackbar.component.html',
  styleUrls: ['./snackbar.component.less']
})
export class SnackbarComponent implements OnInit {
  constructor(@Inject(MAT_SNACK_BAR_DATA) public data: any) { }

  ngOnInit() {}

  get getIcon() {
    switch (this.data.snackType as SnackBarType) {
      case SnackBarType.Alert:
        return 'taxi_alert';
      case SnackBarType.Info:
        return 'info';
      case SnackBarType.Connected:
        return 'signal_wifi_4_bar';
      case SnackBarType.Disconnected:
        return 'signal_cellular_off';
      case SnackBarType.Saved:
        return 'saved';
    }
  }
}
