import { Component, type TrackByFunction, ChangeDetectionStrategy, type OnDestroy, output, input } from '@angular/core';

import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import type { IPlateSetting } from './plate-setting.interface';
import type { PlateSettingsConfig } from './plate-settings-table.component';
import { SettingsHeaderComponent } from './settings-header.component';
import { SettingsEmptyStateComponent } from './settings-empty-state.component';
import { PlateNumberCellComponent } from './plate-number-cell.component';
import { DescriptionCellComponent } from './description-cell.component';
import { ActionsCellComponent } from './actions-cell.component';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

@Component({
  selector: 'app-settings-list',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './settings-list.component.html',
  imports: [
    MatTableModule,
    MatIconModule,
    MatCardModule,
    SettingsHeaderComponent,
    SettingsEmptyStateComponent,
    PlateNumberCellComponent,
    DescriptionCellComponent,
    ActionsCellComponent,
  ],
})
export class SettingsListComponent<T extends IPlateSetting> extends OnPushBaseComponent implements OnDestroy {
  readonly config = input<PlateSettingsConfig<T>>();
  readonly settings = input<MatTableDataSource<T>>(new MatTableDataSource<T>([]));
  readonly isSaving = input(false);
  readonly editingId = input<string | null>(null);
  readonly editingSetting = input<T | null>(null);
  readonly trackByFn = input<TrackByFunction<T>>();

  readonly saveSettings = output<void>();
  readonly scrollToForm = output<void>();
  readonly startEdit = output<T>();
  readonly confirmDelete = output<T>();
  readonly saveEdit = output<T>();
  readonly cancelEdit = output<void>();

  public readonly displayedColumns = [
    'plateNumber',
    'description',
    'actions',
  ];

  public get computedData(): (T & { isCurrentlyEditing: boolean, elementId: string })[] {
    return this.settings().data.map(element => {
      const elementId = element.id || this.generateTempId(element);
      return {
        ...element,
        isCurrentlyEditing: this.editingId() === elementId,
        elementId,
      };
    });
  }

  public onSaveSettings(): void {
    this.saveSettings.emit();
    this.markForCheck();
  }

  public onScrollToForm(): void {
    this.scrollToForm.emit();
    this.markForCheck();
  }

  public onStartEdit(element: T): void {
    this.startEdit.emit(element);
    this.markForCheck();
  }

  public onConfirmDelete(element: T): void {
    this.confirmDelete.emit(element);
    this.markForCheck();
  }

  public onSaveEdit(element: T): void {
    this.saveEdit.emit(element);
    this.markForCheck();
  }

  public onCancelEdit(): void {
    this.cancelEdit.emit();
    this.markForCheck();
  }

  private generateTempId(setting: T): string {
    return `temp_${setting.plateNumber}_${setting.strictMatch ? 'strict' : 'lenient'}`;
  }
}
