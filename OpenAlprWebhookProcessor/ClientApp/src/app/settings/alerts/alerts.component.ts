import { Component, OnInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { Alert } from '@app/_models';
import { SettingsService } from '../settings.service';

@Component({
  selector: 'app-alerts',
  templateUrl: './alerts.component.html',
  styleUrls: ['./alerts.component.less']
})
export class AlertsComponent implements OnInit {
  public alerts: MatTableDataSource<Alert>;
  public isSaving: boolean = false;

  constructor(private settingsService: SettingsService) { }

  public rowsToDisplay = [
    'plateNumber',
    'matchType',
    'description',
    'delete',
  ];

  ngOnInit(): void {
    this.getAlerts();
  }

  private getAlerts() {
    this.settingsService.getAlerts().subscribe(result => {
      this.alerts = new MatTableDataSource<Alert>(result);
    });
  }

  public deleteAlert(alert: Alert) {
    this.alerts.data.forEach((item, index) => {
      if(item === alert) {
        this.alerts.data.splice(index,1);
      }
    });

    this.alerts._updateChangeSubscription();
  }

  public addAlert() {
    this.alerts.data.push(new Alert());
    this.alerts._updateChangeSubscription();
  }

  public saveAlerts() {
    this.isSaving = true;
    this.settingsService.upsertAlerts(this.alerts.data).subscribe(() => {
      this.getAlerts();
      this.isSaving = false;
    });
  }
}
