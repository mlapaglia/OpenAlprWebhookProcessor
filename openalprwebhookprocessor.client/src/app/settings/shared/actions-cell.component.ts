import { Component, ChangeDetectionStrategy, type OnDestroy, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import type { IPlateSetting } from './plate-setting.interface';
import type { PlateSettingsConfig } from './plate-settings-table.component';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

@Component({
  selector: 'app-actions-cell',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <!-- Display Mode Actions -->
    @if(!isEditing()) {
      <div class="display-actions">
        <button
          mat-icon-button
          color="primary"
          (click)="onStartEdit()"
          [matTooltip]="'Edit this ' + config()?.entityName"
          [attr.aria-label]="'Edit ' + config()?.entityName + ' for ' + setting()?.plateNumber">
          <mat-icon i18n>edit</mat-icon>
        </button>

        <button
          mat-icon-button
          color="warn"
          (click)="onConfirmDelete()"
          [matTooltip]="'Delete this ' + config()?.entityName"
          [attr.aria-label]="'Delete ' + config()?.entityName + ' for ' + setting()?.plateNumber">
          <mat-icon i18n>delete</mat-icon>
        </button>
      </div>
    }

    <!-- Edit Mode Actions -->
    @if(isEditing()) {
      <div class="edit-actions">
        <button
          mat-icon-button
          color="primary"
          (click)="onSaveEdit()"
          matTooltip="Save changes" i18n-matTooltip
          [attr.aria-label]="'Save changes for ' + setting()?.plateNumber">
          <mat-icon i18n>check</mat-icon>
        </button>

        <button
          mat-icon-button
          color="warn"
          (click)="onCancelEdit()"
          matTooltip="Cancel editing" i18n-matTooltip
          [attr.aria-label]="'Cancel editing for ' + setting()?.plateNumber">
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
export class ActionsCellComponent<T extends IPlateSetting> extends OnPushBaseComponent implements OnDestroy {
  readonly setting = input<T>();
  readonly config = input<PlateSettingsConfig<T>>();
  readonly isEditing = input(false);

  readonly startEdit = output<void>();
  readonly confirmDelete = output<void>();
  readonly saveEdit = output<void>();
  readonly cancelEdit = output<void>();

  public onStartEdit(): void {
    this.startEdit.emit();
    this.markForCheck();
  }

  public onConfirmDelete(): void {
    this.confirmDelete.emit();
    this.markForCheck();
  }

  public onSaveEdit(): void {
    this.saveEdit.emit();
    this.markForCheck();
  }

  public onCancelEdit(): void {
    this.cancelEdit.emit();
    this.markForCheck();
  }
}
