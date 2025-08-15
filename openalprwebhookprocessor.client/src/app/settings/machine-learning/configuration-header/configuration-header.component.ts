import { Component, ChangeDetectionStrategy, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { type MachineLearningConfigDto } from '../machine-learning.service';

@Component({
  selector: 'app-configuration-header',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-card-header>
      <mat-card-title i18n>
        Training Configuration
        @if (isLoadingConfiguration()) {
          <mat-progress-spinner style="margin-left: 16px;" color="primary" mode="indeterminate" i18n-mode diameter="20" />
        }
      </mat-card-title>
      <mat-card-subtitle i18n>
        @if (isEditingConfiguration()) {
          Edit ML training parameters
        } @else {
          Current ML training parameters
        }
        @if (configuration()?.lastUpdated) {
          <div style="margin-top: 8px; font-size: 12px; color: #666;">
            Last updated: {{ configuration()?.lastUpdated | date:'medium' }}
            @if (configuration()?.updatedBy) {
              by {{ configuration()?.updatedBy }}
            }
          </div>
        }
      </mat-card-subtitle>
    </mat-card-header>
  `,
  imports: [
    CommonModule,
    MatCardModule,
    MatProgressSpinnerModule,
  ],
})
export class ConfigurationHeaderComponent {
  readonly configuration = input<MachineLearningConfigDto | null>(null);
  readonly isLoadingConfiguration = input(false);
  readonly isEditingConfiguration = input(false);
}












