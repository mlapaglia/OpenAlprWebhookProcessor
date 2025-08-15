import { Component, inject, type OnInit, type OnDestroy, type TemplateRef, ChangeDetectionStrategy, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatOptionModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatChipsModule } from '@angular/material/chips';
import type { IPlateSetting } from './plate-setting.interface';
import { type Observable } from 'rxjs';
import { AddNewSettingFormComponent } from './add-new-setting-form.component';
import { SettingsListComponent } from './settings-list.component';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

export interface PlateSettingsConfig<T extends IPlateSetting> {
  title: string
  subtitle: string
  emptyStateTitle: string
  emptyStateDescription: string
  addButtonText: string
  entityName: string
  createNew: () => T
  service: {
    getAll: () => Observable<T[]>
    upsert: (items: T[]) => Observable<unknown>
  }
}

@Component({
  selector: 'app-plate-settings-table',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './plate-settings-table.component.html',
  styleUrls: ['./plate-settings-table.component.less'],
  imports: [
    CommonModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
    ReactiveFormsModule,
    MatSelectModule,
    MatOptionModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatTooltipModule,
    MatCardModule,
    MatChipsModule,
    MatDialogModule,
    AddNewSettingFormComponent,
    SettingsListComponent,
  ],
})
export class PlateSettingsTableComponent<T extends IPlateSetting> extends OnPushBaseComponent implements OnInit, OnDestroy {
  readonly config = input.required<PlateSettingsConfig<T>>();
  readonly additionalContent = input<TemplateRef<unknown> | undefined>(undefined);
  readonly settingsChanged = output<T[]>();

  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  public settings = new MatTableDataSource<T>([]);
  public isSaving = false;
  public isLoading = false;
  public hasError = false;
  public isAddingSetting = false;
  public editingId: string | null = null;
  public editingSetting: T | null = null;

  public readonly displayedColumns = [
    'plateNumber',
    'description',
    'actions',
  ];

  ngOnInit(): void {
    this.loadSettings();
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  public loadSettings(): void {
    this.isLoading = true;
    this.hasError = false;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.config().service.getAll(),
      (result) => {
        this.settings.data = result;
        this.isLoading = false;
        this.settingsChanged.emit(result);
      },
      () => {
        this.hasError = true;
        this.isLoading = false;
        this.snackBar.open(`Failed to load ${this.config().entityName}s. Please try again.`, 'Close', {
          duration: 5000,
        });
      },
    );
  }

  public onAddSetting(newSetting: T): void {
    this.isAddingSetting = true;
    this.markForCheck();
    this.addSettingToData(newSetting);
    this.handleAddSettingSuccess();
  }

  public onResetForm(): void {
    // Child component handles its own form reset
  }

  private addSettingToData(newSetting: T): void {
    const currentData = [...this.settings.data];
    currentData.push(newSetting);
    this.settings.data = currentData;
    this.settingsChanged.emit(this.settings.data);
  }

  private handleAddSettingSuccess(): void {
    this.isAddingSetting = false;
    this.markForCheck();

    this.snackBar.open(`${this.config().entityName} added. Remember to save your changes.`, 'Close', {
      duration: 3000,
    });
  }

  public startEdit(setting: T): void {
    this.cancelEdit();
    this.editingId = setting.id || this.generateTempId(setting);
    this.editingSetting = { ...setting } as T;
    this.markForCheck();
  }

  public cancelEdit(): void {
    this.editingId = null;
    this.editingSetting = null;
    this.markForCheck();
  }

  public saveEdit(setting: T): void {
    if (!this.editingSetting) return;

    if (!this.editingSetting.plateNumber.trim()) {
      this.snackBar.open('Plate number is required.', 'Close', {
        duration: 3000,
      });
      return;
    }

    if (this.editingSetting.plateNumber.trim().length < 2) {
      this.snackBar.open('Plate number must be at least 2 characters.', 'Close', {
        duration: 3000,
      });
      return;
    }

    if (this.isDuplicatePlateForEdit(this.editingSetting.plateNumber.trim(), setting)) {
      this.snackBar.open('This plate number already exists.', 'Close', {
        duration: 3000,
      });
      return;
    }

    Object.assign(setting, {
      plateNumber: this.editingSetting.plateNumber.trim(),
      strictMatch: this.editingSetting.strictMatch,
      description: this.editingSetting.description.trim() || '',
    });

    this.cancelEdit();
    this.settingsChanged.emit(this.settings.data);
    this.markForCheck();
    this.snackBar.open('Changes saved locally. Remember to save all changes.', 'Close', {
      duration: 3000,
    });
  }

  public isEditing(setting: T): boolean {
    const id = setting.id || this.generateTempId(setting);
    return this.editingId === id;
  }



  public deleteSetting(settingToDelete: T): void {
    const currentData = [...this.settings.data];
    const filteredData = currentData.filter(setting => setting !== settingToDelete);
    this.settings.data = filteredData;
    this.settingsChanged.emit(this.settings.data);
    this.markForCheck();
  }

  public confirmDelete(setting: T): void {
    const plateNumber = setting.plateNumber || 'Unknown';

    const dialogRef = this.dialog.open(ConfirmDeleteDialogComponent, {
      width: '400px',
      data: {
        title: 'Confirm Delete',
        message: `Are you sure you want to delete the ${this.config().entityName} for "${plateNumber}"?`,
        confirmText: 'Delete',
        cancelText: 'Cancel',
      },
    });

    this.subscribeAndMarkForCheck(
      dialogRef.afterClosed(),
      (result) => {
        if (result === true) {
          this.deleteSetting(setting);
          this.snackBar.open(`${this.config().entityName} removed. Remember to save your changes.`, 'Close', {
            duration: 3000,
          });
        }
      },
    );
  }

  public scrollToForm(): void {
    const formElement = document.querySelector('.add-setting-card');
    if (formElement) {
      formElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }

  public saveSettings(): void {
    if (this.isSaving) {
      return;
    }

    const validSettings = this.settings.data.filter(setting =>
      setting.plateNumber && setting.plateNumber.trim().length > 0,
    );

    if (validSettings.length === 0) {
      this.snackBar.open(`Please add at least one ${this.config().entityName} with a plate number.`, 'Close', {
        duration: 3000,
      });
      return;
    }

    this.isSaving = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.config().service.upsert(validSettings),
      () => {
        this.isSaving = false;
        this.snackBar.open(`${this.config().entityName}s saved successfully!`, 'Close', {
          duration: 3000,
        });
        this.loadSettings();
      },
      () => {
        this.isSaving = false;
        this.snackBar.open(`Failed to save ${this.config().entityName}s. Please try again.`, 'Close', {
          duration: 5000,
        });
      },
    );
  }

  public trackByFn(index: number, item: T): string | number {
    return item.id || index;
  }

  private isDuplicatePlate(plateNumber: string): boolean {
    if (!plateNumber) return false;

    return this.settings.data.some(setting =>
      setting.plateNumber.toLowerCase() === plateNumber.toLowerCase(),
    );
  }

  private isDuplicatePlateForEdit(plateNumber: string, currentSetting: T): boolean {
    if (!plateNumber) return false;

    return this.settings.data.some(setting =>
      setting !== currentSetting &&
      setting.plateNumber.toLowerCase() === plateNumber.toLowerCase(),
    );
  }

  private generateTempId(setting: T): string {
    return `temp_${setting.plateNumber}_${setting.strictMatch}_${Date.now()}`;
  }
}

export interface ConfirmDeleteDialogData {
  title: string;
  message: string;
  confirmText: string;
  cancelText: string;
}

@Component({
  selector: 'app-confirm-delete-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h2 mat-dialog-title>{{ data.title }}</h2>
    <mat-dialog-content>
      <p>{{ data.message }}</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">{{ data.cancelText }}</button>
      <button mat-raised-button color="warn" (click)="onConfirm()">{{ data.confirmText }}</button>
    </mat-dialog-actions>
  `,
  imports: [MatDialogModule, MatButtonModule],
})
export class ConfirmDeleteDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<ConfirmDeleteDialogComponent>);
  public readonly data = inject<ConfirmDeleteDialogData>(MAT_DIALOG_DATA);

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    this.dialogRef.close(true);
  }
}
