import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import type { IPlateSetting } from './plate-setting.interface';
import { PlateNumberCellComponent } from './plate-number-cell.component';
import { DescriptionCellComponent } from './description-cell.component';
import { ActionsCellComponent } from './actions-cell.component';

export interface ListConfig {
  title: string;
  entityName: string;
  emptyStateTitle: string;
  emptyStateDescription: string;
}

@Component({
  selector: 'app-plate-settings-list',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-card class="settings-list-card">
      <mat-card-header>
        <mat-card-title i18n class="mat-headline-6">Current {{config.title}}</mat-card-title>
        <mat-card-subtitle i18n>
          {{settings.data.length}} {{settingsCountText}} configured
        </mat-card-subtitle>
        <div class="header-spacer"></div>
        <button
          mat-raised-button
          color="primary"
          [disabled]="isSaving || settings.data.length === 0"
          (click)="saveAll.emit()"
          class="save-button">
          @if (!isSaving) {
            <mat-icon i18n>save</mat-icon>
          }
          @if (isSaving) {
            <mat-spinner diameter="20" />
          }
          {{ saveButtonText }}
        </button>
      </mat-card-header>

      <mat-card-content class="table-content">
        <!-- Empty State -->
        @if (settings.data.length === 0) {
          <div class="empty-state">
            <div class="empty-state-content">
              <mat-icon class="empty-icon">{{emptyStateIcon}}</mat-icon>
              <h3 class="mat-headline-6">{{config.emptyStateTitle}}</h3>
              <p class="mat-body-1">{{config.emptyStateDescription}}</p>
              <button mat-stroked-button color="primary" (click)="scrollToForm.emit()">
                <mat-icon i18n>add</mat-icon>
                Add First {{config.entityName | titlecase}}
              </button>
            </div>
          </div>
        }

        <!-- Settings Table -->
        @if (settings.data.length > 0) {
          <div class="table-container">
            <table
              mat-table
              [dataSource]="settings"
              [trackBy]="trackByFn"
              class="settings-table"
              multiTemplateDataRows>

              <ng-container matColumnDef="plateNumber" i18n-matColumnDef>
                <th mat-header-cell *matHeaderCellDef class="plate-header">
                  <mat-icon i18n class="column-icon">local_police</mat-icon>
                  Plate Number
                </th>
                <td mat-cell *matCellDef="let element" class="plate-cell">
                  <app-plate-number-cell
                    [setting]="element"
                    [editingId]="editingId"
                    [editingSetting]="editingSetting"
                    (editingSettingChange)="onEditingSettingChange($event)" />
                </td>
              </ng-container>

              <ng-container matColumnDef="description" i18n-matColumnDef>
                <th mat-header-cell *matHeaderCellDef class="description-header">
                  <mat-icon i18n class="column-icon">notes</mat-icon>
                  Description
                </th>
                <td mat-cell *matCellDef="let element" class="description-cell">
                  <app-description-cell
                    [setting]="element"
                    [editingId]="editingId"
                    [editingSetting]="editingSetting"
                    (editingSettingChange)="onEditingSettingChange($event)" />
                </td>
              </ng-container>

              <ng-container matColumnDef="actions" i18n-matColumnDef>
                <th i18n mat-header-cell *matHeaderCellDef class="actions-header">
                  Actions
                </th>
                <td mat-cell *matCellDef="let element" class="actions-cell">
                  <app-actions-cell
                    [setting]="element"
                    [editingId]="editingId"
                    [config]="{entityName: config.entityName}"
                    (startEdit)="startEdit.emit($event)"
                    (saveEdit)="saveEdit.emit($event)"
                    (cancelEdit)="cancelEdit.emit()"
                    (confirmDelete)="confirmDelete.emit($event)" />
                </td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
              <tr mat-row *matRowDef="let element; columns: displayedColumns;"
                  class="table-row"></tr>
            </table>
          </div>
        }
      </mat-card-content>
    </mat-card>
  `,
  styleUrls: ['./plate-settings-table.component.less'],
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatOptionModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatChipsModule,
    PlateNumberCellComponent,
    DescriptionCellComponent,
    ActionsCellComponent,
  ],
})
export class PlateSettingsListComponent<T extends IPlateSetting> {
  @Input() settings = new MatTableDataSource<T>([]);
  @Input() config!: ListConfig;
  @Input() isSaving = false;
  @Input() editingId: string | null = null;
  @Input() editingSetting: T | null = null;

  @Output() saveAll = new EventEmitter<void>();
  @Output() scrollToForm = new EventEmitter<void>();
  @Output() startEdit = new EventEmitter<T>();
  @Output() saveEdit = new EventEmitter<T>();
  @Output() cancelEdit = new EventEmitter<void>();
  @Output() confirmDelete = new EventEmitter<T>();
  @Output() editingSettingChange = new EventEmitter<T>();

  public readonly displayedColumns = [
    'plateNumber',
    'description',
    'actions',
  ];

  // Computed properties
  public get settingsCountText(): string {
    const count = this.settings.data.length;
    return count === 1 ? this.config.entityName : (`${this.config.entityName}s`);
  }

  public get emptyStateIcon(): string {
    return this.config.entityName === 'ignore rule' ? 'block' : 'notifications';
  }

  public get saveButtonText(): string {
    return this.isSaving ? 'Saving...' : 'Save All Changes';
  }

  public isEditing(setting: T): boolean {
    const id = setting.id || this.generateTempId(setting);
    return this.editingId === id;
  }

  public trackByFn(index: number, item: T): any {
    return item.id || index;
  }

  public onEditingSettingChange(editingSetting: T): void {
    this.editingSettingChange.emit(editingSetting);
  }



  private generateTempId(setting: T): string {
    return `temp_${setting.plateNumber}_${setting.strictMatch}_${Date.now()}`;
  }
}
