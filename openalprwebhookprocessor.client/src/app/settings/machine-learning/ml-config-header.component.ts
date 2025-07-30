import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import type { MachineLearningConfigDto } from './machine-learning.service';

@Component({
  selector: 'app-ml-config-header',
  template: `
    <mat-card-header>
      <mat-card-title i18n>
        Training Configuration
        @if (isLoadingConfiguration) {
          <mat-progress-spinner style="margin-left: 16px;" color="primary" mode="indeterminate" i18n-mode diameter="20" />
        }
      </mat-card-title>
      <mat-card-subtitle i18n>
        @if (isEditingConfiguration) {
          Edit ML training parameters
        } @else {
          Current ML training parameters
        }
        @if (configuration?.lastUpdated) {
          <div i18n style="margin-top: 8px; font-size: 12px; color: #666;">
            Last updated: {{ configuration?.lastUpdated | date:'medium' }}
            @if (configuration?.updatedBy) {
              by {{ configuration?.updatedBy }}
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
    DatePipe,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MlConfigHeaderComponent {
  @Input() configuration: MachineLearningConfigDto | null = null;
  @Input() isLoadingConfiguration = false;
  @Input() isEditingConfiguration = false;
}
