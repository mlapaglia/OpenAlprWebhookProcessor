import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface LoadingConfig {
  title: string;
  entityName: string;
}

@Component({
  selector: 'app-plate-settings-loading',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <!-- Loading Overlay -->
    @if (isLoading()) {
      <div class="loading-overlay">
        <mat-card class="loading-card">
          <mat-card-content class="loading-content">
            <mat-spinner diameter="48" />
            <p i18n class="mat-body-1 loading-text">Loading {{config().entityName}}s...</p>
          </mat-card-content>
        </mat-card>
      </div>
    }

    <!-- Error State -->
    @if (hasError() && !isLoading()) {
      <mat-card class="error-card">
        <mat-card-content>
          <div class="error-content">
            <mat-icon i18n class="error-icon">error_outline</mat-icon>
            <h3 i18n class="mat-headline-6">Unable to Load {{config().title}}</h3>
            <p i18n class="mat-body-1">There was a problem loading your {{config().entityName}}s. Please check your connection and try again.</p>
            <button mat-raised-button color="primary" (click)="retry.emit()" class="retry-button">
              <mat-icon i18n>refresh</mat-icon>
              Retry
            </button>
          </div>
        </mat-card-content>
      </mat-card>
    }
  `,
  styleUrls: ['./plate-settings-table.component.less'],
  imports: [
    CommonModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    MatIconModule,
  ],
})
export class PlateSettingsLoadingComponent {
  readonly isLoading = input(false);
  readonly hasError = input(false);
  readonly config = input.required<LoadingConfig>();
  readonly retry = output<void>();
}
