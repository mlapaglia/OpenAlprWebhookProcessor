import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { MatChipsModule } from '@angular/material/chips';
import type { IPlateSetting } from './plate-setting.interface';

@Component({
  selector: 'app-plate-number-cell',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <!-- Display Mode -->
    @if (!isEditing) {
      <div class="plate-display">
        <span class="plate-number">{{setting.plateNumber}}</span>
        <mat-chip-set>
          <mat-chip [color]="setting.strictMatch ? 'accent' : 'primary'" selected>
            {{setting.strictMatch ? 'Strict' : 'Lenient'}}
          </mat-chip>
        </mat-chip-set>
      </div>
    }

    <!-- Edit Mode -->
    @if (isEditing && editingSetting) {
      <div class="plate-edit">
        <mat-form-field appearance="outline" i18n-appearance class="plate-edit-field">
          <mat-label i18n>Plate Number</mat-label>
          <input
            matInput
            [(ngModel)]="editingSetting.plateNumber"
            (ngModelChange)="onEditingChange()"
            placeholder="Enter plate number" i18n-placeholder
            maxlength="20"
            autocomplete="off">
        </mat-form-field>

        <mat-form-field appearance="outline" i18n-appearance class="match-edit-field">
          <mat-label i18n>Match Type</mat-label>
          <mat-select [(ngModel)]="editingSetting.strictMatch" (ngModelChange)="onEditingChange()">
            <mat-option i18n [value]="true">Strict Match</mat-option>
            <mat-option i18n [value]="false">Lenient Match</mat-option>
          </mat-select>
        </mat-form-field>
      </div>
    }
  `,
  styleUrls: ['./plate-settings-table.component.less'],
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatOptionModule,
    MatChipsModule,
  ],
})
export class PlateNumberCellComponent<T extends IPlateSetting> {
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
