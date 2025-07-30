import { Component, inject, ViewChild, type TemplateRef } from '@angular/core';
import { Alert } from './alert';
import { AlertsService } from './alerts.service';
import { WebpushComponent } from './webpush/webpush.component';
import { PushoverComponent } from './pushover/pushover.component';
import { type PlateSettingsConfig, PlateSettingsTableComponent } from '../shared/plate-settings-table.component';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-alerts',
  templateUrl: './alerts.component.html',
  styleUrls: ['./alerts.component.less'],
  imports: [
    CommonModule,
    PushoverComponent,
    WebpushComponent,
    PlateSettingsTableComponent,
  ],
})
export class AlertsComponent {
  @ViewChild('additionalContent', { static: true }) additionalContent!: TemplateRef<any>;

  private readonly alertsService = inject(AlertsService);

  public alertsConfig: PlateSettingsConfig<Alert> = {
    title: 'License Plate Alerts',
    subtitle: 'Configure license plates to monitor for alerts. These plates will trigger notifications when detected.',
    emptyStateTitle: 'No Alert Rules',
    emptyStateDescription: 'You haven\'t created any alert rules yet. Add your first rule to get started with notifications.',
    addButtonText: 'Add Alert Rule',
    entityName: 'alert rule',
    createNew: () => new Alert({
      plateNumber: '',
      strictMatch: true,
      description: '',
    }),
    service: {
      getAll: () => this.alertsService.getAlerts(),
      upsert: (items: Alert[]) => this.alertsService.upsertAlerts(items),
    },
  };
}
