import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { type FormGroup } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import type { MachineLearningConfigDto } from './machine-learning.service';
import { MlConfigHeaderComponent } from './ml-config-header.component';
import { MlConfigTableComponent } from './ml-config-table.component';
import { MlConfigActionsComponent } from './ml-config-actions.component';



@Component({
  selector: 'app-ml-configuration',
  template: `
    <mat-card>
      <app-ml-config-header
        [configuration]="configuration"
        [isLoadingConfiguration]="isLoadingConfiguration"
        [isEditingConfiguration]="isEditingConfiguration" />

      @if (configuration) {
        <mat-card-content>
          <app-ml-config-table
            [configuration]="configuration"
            [isEditingConfiguration]="isEditingConfiguration"
            [configForm]="configForm" />
        </mat-card-content>

        <app-ml-config-actions
          [isEditingConfiguration]="isEditingConfiguration"
          [isSavingConfiguration]="isSavingConfiguration"
          [isLoadingConfiguration]="isLoadingConfiguration"
          [configForm]="configForm"
          (editConfiguration)="editConfiguration.emit()"
          (saveConfiguration)="saveConfiguration.emit()"
          (cancelConfigurationEdit)="cancelConfigurationEdit.emit()" />
      }
    </mat-card>
  `,
  styleUrls: ['./machine-learning.component.less'],
  imports: [
    CommonModule,
    MatCardModule,
    MlConfigHeaderComponent,
    MlConfigTableComponent,
    MlConfigActionsComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MlConfigurationComponent {
  @Input() configuration: MachineLearningConfigDto | null = null;
  @Input() isLoadingConfiguration = false;
  @Input() isSavingConfiguration = false;
  @Input() isEditingConfiguration = false;
  @Input() configForm!: FormGroup;

  @Output() editConfiguration = new EventEmitter<void>();
  @Output() saveConfiguration = new EventEmitter<void>();
  @Output() cancelConfigurationEdit = new EventEmitter<void>();
}
