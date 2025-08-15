import { Component, inject, type OnInit, type OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, type FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CommonModule } from '@angular/common';

import { Alert } from './alert';
import { AlertsService } from './alerts.service';
import { WebpushComponent } from './webpush/webpush.component';
import { PushoverComponent } from './pushover/pushover.component';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';
import { RefreshButtonComponent } from 'app/shared/refresh-button/refresh-button.component';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';

@Component({
  selector: 'app-alerts',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './alerts.component.html',
  styleUrls: ['./alerts.component.less'],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTabsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatTooltipModule,
    PushoverComponent,
    WebpushComponent,
    RefreshButtonComponent,
  ],
})
export class AlertsComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  private readonly alertsService = inject(AlertsService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly snackbarService = inject(SnackbarService);

  public selectedTabIndex = 0;
  public isLoading = false;
  public hasError = false;
  public isSaving = false;
  public editingAlert: Alert | null = null;
  public deletingAlertIds = new Set<string>();

  public alerts: Alert[] = [];

  public quickAddForm: FormGroup;
  public editForm: FormGroup;

  constructor() {
    super();
    this.quickAddForm = this.formBuilder.group({
      plateNumber: ['', [Validators.required, Validators.minLength(1)]],
      description: [''],
      strictMatch: [true],
    });

    this.editForm = this.formBuilder.group({
      plateNumber: ['', [Validators.required, Validators.minLength(1)]],
      description: [''],
      strictMatch: [true],
    });
  }

  ngOnInit(): void {
    this.loadAlerts();
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  public loadAlerts(): void {
    this.isLoading = true;
    this.hasError = false;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.alertsService.getAlerts(),
      (alerts) => {
        this.alerts = alerts;
        this.isLoading = false;
      },
      _ => {
        this.hasError = true;
        this.isLoading = false;
        this.snackbarService.create('Failed to load alert rules', SnackBarType.Error);
      },
    );
  }

  public addAlert(): void {
    if (this.quickAddForm.invalid || this.isSaving) return;

    const formValue = this.quickAddForm.value;
    const newAlert = new Alert({
      plateNumber: formValue.plateNumber.trim().toUpperCase(),
      description: formValue.description?.trim() ?? '',
      strictMatch: formValue.strictMatch,
    });

    this.isSaving = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.alertsService.addAlert(newAlert),
      () => {
        this.isSaving = false;
        this.quickAddForm.reset({ strictMatch: true });
        this.loadAlerts();
        this.snackbarService.create('Alert rule added successfully', SnackBarType.Saved);
      },
      _ => {
        this.isSaving = false;
        this.snackbarService.create('Failed to add alert rule', SnackBarType.Error);
      },
    );
  }

  public startEdit(alert: Alert): void {
    this.editingAlert = alert;
    this.editForm.patchValue({
      plateNumber: alert.plateNumber,
      description: alert.description,
      strictMatch: alert.strictMatch,
    });
    this.markForCheck();
  }

  public saveEdit(): void {
    if (this.editForm.invalid || this.isSaving || !this.editingAlert) return;

    const formValue = this.editForm.value;
    const updatedAlert = new Alert({
      ...this.editingAlert,
      plateNumber: formValue.plateNumber.trim().toUpperCase(),
      description: formValue.description?.trim() ?? '',
      strictMatch: formValue.strictMatch,
    });

    this.isSaving = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.alertsService.updateAlert(updatedAlert),
      () => {
        this.editingAlert = null;
        this.isSaving = false;
        this.editForm.reset();
        this.loadAlerts();
        this.snackbarService.create('Alert rule updated successfully', SnackBarType.Saved);
      },
      (error) => {
        console.error('Error updating alert:', error);
        this.isSaving = false;
        this.snackbarService.create('Failed to update alert rule', SnackBarType.Error, error as string);
      },
    );
  }

  public cancelEdit(): void {
    this.editingAlert = null;
    this.editForm.reset();
    this.markForCheck();
  }

  public isDeleting(alertId: string): boolean {
    return this.deletingAlertIds.has(alertId);
  }

  public deleteAlert(alert: Alert): void {
    if (!alert.id || this.deletingAlertIds.has(alert.id)) return;

    this.deletingAlertIds.add(alert.id);
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.alertsService.deleteAlert(alert.id),
      () => {
        this.alerts = this.alerts.filter(a => a.id !== alert.id);
        this.deletingAlertIds.delete(alert.id);
        this.snackbarService.create('Alert rule deleted successfully', SnackBarType.Successful);
      },
      (error) => {
        this.deletingAlertIds.delete(alert.id);
        this.snackbarService.create('Failed to delete alert rule', SnackBarType.Error, error as string);
      },
    );
  }
}
