import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import type { FormGroup } from '@angular/forms';

@Component({
  selector: 'app-ml-config-actions',
  template: `
    <mat-card-actions>
      @if (isEditingConfiguration) {
        <button mat-raised-button color="primary"
                (click)="saveConfiguration.emit()"
                [disabled]="configForm.invalid || isSavingConfiguration">
          @if (isSavingConfiguration) {
            <mat-progress-spinner color="primary" mode="indeterminate" i18n-mode diameter="20" style="margin-right: 8px;" />
          }
          {{ saveButtonText }}
        </button>
        <button i18n mat-button (click)="cancelConfigurationEdit.emit()" [disabled]="isSavingConfiguration">
          Cancel
        </button>
      } @else {
        <button mat-raised-button color="accent" (click)="editConfiguration.emit()" [disabled]="isLoadingConfiguration">
          <mat-icon i18n>edit</mat-icon>
          Edit Configuration
        </button>
      }
    </mat-card-actions>
  `,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatCardModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MlConfigActionsComponent {
  @Input() isEditingConfiguration = false;
  @Input() isSavingConfiguration = false;
  @Input() isLoadingConfiguration = false;
  @Input() configForm!: FormGroup;

  @Output() editConfiguration = new EventEmitter<void>();
  @Output() saveConfiguration = new EventEmitter<void>();
  @Output() cancelConfigurationEdit = new EventEmitter<void>();

  public get saveButtonText(): string {
    return this.isSavingConfiguration ? 'Saving...' : 'Save Configuration';
  }
}
