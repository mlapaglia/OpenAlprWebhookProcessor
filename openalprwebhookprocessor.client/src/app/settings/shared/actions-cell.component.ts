import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import type { IPlateSetting } from './plate-setting.interface';

export interface ActionsCellConfig {
  entityName: string;
}

@Component({
  selector: 'app-actions-cell',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <!-- Display Mode Actions -->
    @if (!isEditing) {
      <div class="display-actions">
        <button
          mat-icon-button
          color="primary"
          (click)="startEdit.emit(setting)"
          matTooltip="Edit this {{config.entityName}}"
          [attr.aria-label]="'Edit ' + config.entityName + ' for ' + setting.plateNumber">
          <mat-icon i18n>edit</mat-icon>
        </button>

        <button
          mat-icon-button
          color="warn"
          (click)="confirmDelete.emit(setting)"
          matTooltip="Delete this {{config.entityName}}"
          [attr.aria-label]="'Delete ' + config.entityName + ' for ' + setting.plateNumber">
          <mat-icon i18n>delete</mat-icon>
        </button>
      </div>
    }

    <!-- Edit Mode Actions -->
    @if (isEditing) {
      <div class="edit-actions">
        <button
          mat-icon-button
          color="primary"
          (click)="saveEdit.emit(setting)"
          matTooltip="Save changes" i18n-matTooltip
          [attr.aria-label]="'Save changes for ' + setting.plateNumber">
          <mat-icon i18n>check</mat-icon>
        </button>

        <button
          mat-icon-button
          color="warn"
          (click)="cancelEdit.emit()"
          matTooltip="Cancel editing" i18n-matTooltip
          [attr.aria-label]="'Cancel editing for ' + setting.plateNumber">
          <mat-icon i18n>close</mat-icon>
        </button>
      </div>
    }
  `,
  styleUrls: ['./plate-settings-table.component.less'],
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
  ],
})
export class ActionsCellComponent<T extends IPlateSetting> {
  @Input() setting!: T;
  @Input() editingId: string | null = null;
  @Input() config!: ActionsCellConfig;
  @Output() startEdit = new EventEmitter<T>();
  @Output() saveEdit = new EventEmitter<T>();
  @Output() cancelEdit = new EventEmitter<void>();
  @Output() confirmDelete = new EventEmitter<T>();

  public get isEditing(): boolean {
    const id = this.setting.id || this.generateTempId(this.setting);
    return this.editingId === id;
  }

  private generateTempId(setting: T): string {
    return `temp_${setting.plateNumber}_${setting.strictMatch}_${Date.now()}`;
  }
}
