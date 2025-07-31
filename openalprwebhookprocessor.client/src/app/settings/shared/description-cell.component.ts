import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import type { IPlateSetting } from './plate-setting.interface';

@Component({
  selector: 'app-description-cell',
  template: `
    <!-- Display Mode -->
    @if(!isEditing) {
      <span class="description-text" [class.no-description]="!setting.description">
        {{setting.description || 'No description provided'}}
      </span>
    }
    
    <!-- Edit Mode -->
    @if(isEditing && editingSetting) {
      <mat-form-field appearance="outline" i18n-appearance class="description-edit-field">
        <mat-label i18n>Description</mat-label>
        <textarea 
          matInput 
          [(ngModel)]="editingSetting.description"
          placeholder="Add description..." i18n-placeholder
          rows="2"
          maxlength="200">
        </textarea>
      </mat-form-field>
    }
  `,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
  ],
})
export class DescriptionCellComponent<T extends IPlateSetting> {
  @Input() setting!: T;
  @Input() isEditing = false;
  @Input() editingSetting: T | null = null;
}