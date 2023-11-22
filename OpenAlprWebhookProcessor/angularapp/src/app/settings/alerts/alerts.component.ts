import { Component, OnInit } from '@angular/core';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { Alert } from './alert';
import { AlertsService } from './alerts.service';
import { MatButtonModule } from '@angular/material/button';
import { MatOptionModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { WebpushComponent } from './webpush/webpush.component';
import { PushoverComponent } from './pushover/pushover.component';

@Component({
    selector: 'app-alerts',
    templateUrl: './alerts.component.html',
    styleUrls: ['./alerts.component.less'],
    standalone: true,
    imports: [PushoverComponent, WebpushComponent, MatTableModule, MatFormFieldModule, MatInputModule, ReactiveFormsModule, FormsModule, MatSelectModule, MatOptionModule, MatButtonModule]
})
export class AlertsComponent implements OnInit {
  public alerts: MatTableDataSource<Alert>;
  public isSaving: boolean = false;

  constructor(private alertsService: AlertsService) { }

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
    this.alertsService.getAlerts().subscribe(result => {
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
    this.alertsService.upsertAlerts(this.alerts.data).subscribe(() => {
      this.getAlerts();
      this.isSaving = false;
    });
  }

  public testAlerts() {

  }
}
