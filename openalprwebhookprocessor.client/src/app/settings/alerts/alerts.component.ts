import { Component, inject, type OnInit, type OnDestroy } from '@angular/core';
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
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';

import { Alert } from './alert';
import { AlertsService } from './alerts.service';
import { WebpushComponent } from './webpush/webpush.component';
import { PushoverComponent } from './pushover/pushover.component';

@Component({
  selector: 'app-alerts',
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
  ],
})
export class AlertsComponent implements OnInit, OnDestroy {
  private readonly alertsService = inject(AlertsService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly subscriptions = new Subscription();

  // UI State
  public selectedTabIndex = 0;
  public isLoading = false;
  public hasError = false;
  public isSaving = false;
  public editingAlert: Alert | null = null;

  // Data
  public alerts: Alert[] = [];

  // Forms
  public quickAddForm: FormGroup;
  public editForm: FormGroup;

  constructor() {
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

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  public loadAlerts(): void {
    this.isLoading = true;
    this.hasError = false;

    const sub = this.alertsService.getAlerts().subscribe({
      next: (alerts) => {
        this.alerts = alerts;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading alerts:', error);
        this.hasError = true;
        this.isLoading = false;
        this.snackBar.open('Failed to load alert rules', 'Close', { duration: 5000 });
      },
    });

    this.subscriptions.add(sub);
  }

  public addAlert(): void {
    if (this.quickAddForm.invalid || this.isSaving) return;

    const formValue = this.quickAddForm.value;
    const newAlert = new Alert({
      plateNumber: formValue.plateNumber.trim().toUpperCase(),
      description: formValue.description?.trim() ?? '',
      strictMatch: formValue.strictMatch,
    });

    this.saveAlerts([...this.alerts, newAlert], 'Alert rule added successfully');
    this.quickAddForm.reset({
      plateNumber: '',
      description: '',
      strictMatch: true,
    });
  }

  public startEdit(alert: Alert): void {
    this.editingAlert = alert;
    this.editForm.patchValue({
      plateNumber: alert.plateNumber,
      description: alert.description,
      strictMatch: alert.strictMatch,
    });
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

    const updatedAlerts = this.alerts.map(alert =>
      alert.id === this.editingAlert!.id ? updatedAlert : alert,
    );

    this.saveAlerts(updatedAlerts, 'Alert rule updated successfully');
  }

  public cancelEdit(): void {
    this.editingAlert = null;
    this.editForm.reset();
  }

  public deleteAlert(alert: Alert): void {
    if (this.isSaving) return;

    const updatedAlerts = this.alerts.filter(a => a.id !== alert.id);
    this.saveAlerts(updatedAlerts, 'Alert rule deleted successfully');
  }

  private saveAlerts(alerts: Alert[], successMessage: string): void {
    this.isSaving = true;

    const sub = this.alertsService.upsertAlerts(alerts).subscribe({
      next: () => {
        this.alerts = alerts;
        this.editingAlert = null;
        this.isSaving = false;
        this.snackBar.open(successMessage, 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error saving alerts:', error);
        this.isSaving = false;
        this.snackBar.open('Failed to save alert rules', 'Close', { duration: 5000 });
      },
    });

    this.subscriptions.add(sub);
  }
}
