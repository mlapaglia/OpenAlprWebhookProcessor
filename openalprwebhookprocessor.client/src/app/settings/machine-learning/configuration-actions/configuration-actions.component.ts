import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { type FormGroup } from '@angular/forms';
import { RefreshButtonComponent } from 'app/shared/refresh-button/refresh-button.component';

@Component({
  selector: 'app-configuration-actions',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-card-actions>
      @if (isEditingConfiguration()) {
        <app-refresh-button
          [isLoading]="isSavingConfiguration()"
          [disabled]="configForm().invalid || isSavingConfiguration()"
          buttonType="raised"
          buttonText="Save Notes"
          buttonRefreshingText="Saving..."
          color="primary"
          icon="save"
          (refreshStarted)="onSaveConfiguration()" />

        <button i18n mat-button (click)="onCancelConfiguration()" [disabled]="isSavingConfiguration()">
          Cancel
        </button>
      } @else {
        <button mat-raised-button color="accent" (click)="onEditConfiguration()" [disabled]="isLoadingConfiguration()">
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
    RefreshButtonComponent,
  ],
})
export class ConfigurationActionsComponent {
  readonly isEditingConfiguration = input(false);
  readonly isLoadingConfiguration = input(false);
  readonly isSavingConfiguration = input(false);
  readonly configForm = input.required<FormGroup>();

  readonly saveConfiguration = output<void>();
  readonly cancelConfigurationEdit = output<void>();
  readonly editConfiguration = output<void>();

  public onSaveConfiguration(): void {
    if (this.configForm().invalid || this.isSavingConfiguration()) return;
    this.saveConfiguration.emit();
  }

  public onCancelConfiguration(): void {
    this.cancelConfigurationEdit.emit();
  }

  public onEditConfiguration(): void {
    this.editConfiguration.emit();
  }
}
