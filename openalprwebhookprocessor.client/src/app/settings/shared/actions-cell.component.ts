import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import type { IPlateSetting } from './plate-setting.interface';
import type { PlateSettingsConfig } from './plate-settings-table.component';

@Component({
  selector: 'app-actions-cell',
  template: `
    <!-- Display Mode Actions -->
    @if(!isEditing) {
      <div class="display-actions">
        <button
          mat-icon-button
          color="primary"
          (click)="onStartEdit()"
          [matTooltip]="'Edit this ' + config.entityName"
          [attr.aria-label]="'Edit ' + config.entityName + ' for ' + setting.plateNumber">
          <mat-icon i18n>edit</mat-icon>
        </button>

        <button
          mat-icon-button
          color="warn"
          (click)="onConfirmDelete()"
          [matTooltip]="'Delete this ' + config.entityName"
          [attr.aria-label]="'Delete ' + config.entityName + ' for ' + setting.plateNumber">
          <mat-icon i18n>delete</mat-icon>
        </button>
      </div>
    }

    <!-- Edit Mode Actions -->
    @if(isEditing) {
      <div class="edit-actions">
        <button
          mat-icon-button
          color="primary"
          (click)="onSaveEdit()"
          matTooltip="Save changes" i18n-matTooltip
          [attr.aria-label]="'Save changes for ' + setting.plateNumber">
          <mat-icon i18n>check</mat-icon>
        </button>

        <button
          mat-icon-button
          color="warn"
          (click)="onCancelEdit()"
          matTooltip="Cancel editing" i18n-matTooltip
          [attr.aria-label]="'Cancel editing for ' + setting.plateNumber">
          <mat-icon i18n>close</mat-icon>
        </button>
      </div>
    }
  `,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
  ],
})
export class ActionsCellComponent<T extends IPlateSetting> {
  @Input() setting!: T;
  @Input() config!: PlateSettingsConfig<T>;
  @Input() isEditing = false;

  @Output() startEdit = new EventEmitter<void>();
  @Output() confirmDelete = new EventEmitter<void>();
  @Output() saveEdit = new EventEmitter<void>();
  @Output() cancelEdit = new EventEmitter<void>();

  public onStartEdit(): void {
    this.startEdit.emit();
  }

  public onConfirmDelete(): void {
    this.confirmDelete.emit();
  }

  public onSaveEdit(): void {
    this.saveEdit.emit();
  }

  public onCancelEdit(): void {
    this.cancelEdit.emit();
  }
}
