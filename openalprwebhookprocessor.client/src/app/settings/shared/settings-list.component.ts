import { Component, Input, Output, EventEmitter, inject, type TrackByFunction } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import type { IPlateSetting } from './plate-setting.interface';
import type { PlateSettingsConfig } from './plate-settings-table.component';
import { SettingsHeaderComponent } from './settings-header.component';
import { SettingsEmptyStateComponent } from './settings-empty-state.component';
import { PlateNumberCellComponent } from './plate-number-cell.component';
import { DescriptionCellComponent } from './description-cell.component';
import { ActionsCellComponent } from './actions-cell.component';

@Component({
  selector: 'app-settings-list',
  templateUrl: './settings-list.component.html',
  imports: [
    CommonModule,
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
export class SettingsListComponent<T extends IPlateSetting> {
  @Input() config!: PlateSettingsConfig<T>;
  @Input() settings = new MatTableDataSource<T>([]);
  @Input() isSaving = false;
  @Input() editingId: string | null = null;
  @Input() editingSetting: T | null = null;
  @Input() trackByFn!: TrackByFunction<T>;

  @Output() saveSettings = new EventEmitter<void>();
  @Output() scrollToForm = new EventEmitter<void>();
  @Output() startEdit = new EventEmitter<T>();
  @Output() confirmDelete = new EventEmitter<T>();
  @Output() saveEdit = new EventEmitter<T>();
  @Output() cancelEdit = new EventEmitter<void>();

  private readonly snackBar = inject(MatSnackBar);

  public readonly displayedColumns = [
    'plateNumber',
    'description',
    'actions',
  ];

  public isEditing(setting: T): boolean {
    const id = setting.id || this.generateTempId(setting);
    return this.editingId === id;
  }

  public onSaveSettings(): void {
    this.saveSettings.emit();
  }

  public onScrollToForm(): void {
    this.scrollToForm.emit();
  }

  public onStartEdit(element: T): void {
    this.startEdit.emit(element);
  }

  public onConfirmDelete(element: T): void {
    this.confirmDelete.emit(element);
  }

  public onSaveEdit(element: T): void {
    this.saveEdit.emit(element);
  }

  public onCancelEdit(): void {
    this.cancelEdit.emit();
  }

  private generateTempId(setting: T): string {
    return `temp_${setting.plateNumber}_${setting.strictMatch ? 'strict' : 'lenient'}`;
  }
}
