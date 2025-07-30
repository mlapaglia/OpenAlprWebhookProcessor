import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import type { IPlateSetting } from './plate-setting.interface';

@Component({
  selector: 'app-description-cell',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <!-- Display Mode -->
    @if (!isEditing) {
      <span class="description-text" [class.no-description]="!setting.description">
        {{setting.description || 'No description provided'}}
      </span>
    }

    <!-- Edit Mode -->
    @if (isEditing && editingSetting) {
      <mat-form-field appearance="outline" i18n-appearance class="description-edit-field">
        <mat-label i18n>Description</mat-label>
        <textarea
          matInput
          [(ngModel)]="editingSetting.description"
          (ngModelChange)="onEditingChange()"
          placeholder="Add description..." i18n-placeholder
          rows="2"
          maxlength="200">
        </textarea>
      </mat-form-field>
    }
  `,
  styleUrls: ['./plate-settings-table.component.less'],
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
  ],
})
export class DescriptionCellComponent<T extends IPlateSetting> {
  @Input() setting!: T;
  @Input() editingId: string | null = null;
  @Input() editingSetting: T | null = null;
  @Output() editingSettingChange = new EventEmitter<T>();

  public get isEditing(): boolean {
    const id = this.setting.id || this.generateTempId(this.setting);
    return this.editingId === id;
  }

  public onEditingChange(): void {
    if (this.editingSetting) {
      this.editingSettingChange.emit(this.editingSetting);
    }
  }

  private generateTempId(setting: T): string {
    return `temp_${setting.plateNumber}_${setting.strictMatch}_${Date.now()}`;
  }
}
