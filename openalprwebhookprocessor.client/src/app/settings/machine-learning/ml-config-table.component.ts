import type { OnChanges } from '@angular/core';
import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import type { FormGroup, FormControl } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import type { MachineLearningConfigDto } from './machine-learning.service';
import type { FieldConfig } from './ml-config-field.component';
import { MlConfigFieldComponent } from './ml-config-field.component';

interface ConfigData {
  key: string;
  value: string;
  fieldConfig?: FieldConfig;
  formControlName?: string;
}

@Component({
  selector: 'app-ml-config-table',
  template: `
    <form [formGroup]="configForm">
      <table mat-table [dataSource]="configData" class="mat-elevation-z2" style="width: 100%;">
        <ng-container matColumnDef="key" i18n-matColumnDef>
          <th i18n mat-header-cell *matHeaderCellDef style="min-width: 200px;">Setting</th>
          <td mat-cell *matCellDef="let element"><strong>{{ element.key }}</strong></td>
        </ng-container>
        <ng-container matColumnDef="value" i18n-matColumnDef>
          <th i18n mat-header-cell *matHeaderCellDef>Value</th>
          <td mat-cell *matCellDef="let element" style="padding: 8px;">
            @if (isEditingConfiguration && element.fieldConfig && element.formControlName) {
              <div style="width: 100%;">
                <app-ml-config-field
                  [fieldConfig]="element.fieldConfig"
                  [control]="getFormControl(element.formControlName)" />
              </div>
            } @else {
              {{ element.value }}
            }
          </td>
        </ng-container>
        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
      </table>
    </form>
  `,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MlConfigFieldComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MlConfigTableComponent implements OnChanges {
  @Input() configuration: MachineLearningConfigDto | null = null;
  @Input() isEditingConfiguration = false;
  @Input() configForm!: FormGroup;

  public configData = new MatTableDataSource<ConfigData>([]);
  public displayedColumns: string[] = ['key', 'value'];

  ngOnChanges(): void {
    this.updateConfigTable();
  }

  public getFormControl(controlName: string): FormControl {
    return this.configForm.get(controlName) as FormControl;
  }

  private updateConfigTable(): void {
    if (!this.configuration) {
      this.configData = new MatTableDataSource<ConfigData>([]);
      return;
    }

    const data: ConfigData[] = [
      {
        key: 'Minimum Model Quality (R²)',
        value: this.configuration.minimumModelQuality.toString(),
        fieldConfig: { key: 'Minimum Model Quality (R²)', type: 'number', step: 0.001, min: 0.001, max: 1.0 },
        formControlName: 'minimumModelQuality',
      },
      {
        key: 'Minimum Training Data',
        value: this.configuration.minimumTrainingData.toLocaleString(),
        fieldConfig: { key: 'Minimum Training Data', type: 'number', min: 10, max: 1000000 },
        formControlName: 'minimumTrainingData',
      },
      {
        key: 'Training Batch Size',
        value: this.configuration.trainingBatchSize.toLocaleString(),
        fieldConfig: { key: 'Training Batch Size', type: 'number', min: 1000, max: 1000000 },
        formControlName: 'trainingBatchSize',
      },
      {
        key: 'Training Interval',
        value: this.formatTrainingInterval(this.configuration.trainingInterval),
        fieldConfig: { key: 'Training Interval', type: 'text', placeholder: '06:00:00' },
        formControlName: 'trainingInterval',
      },
      {
        key: 'Model File Name',
        value: this.configuration.modelFileName,
        fieldConfig: { key: 'Model File Name', type: 'text' },
        formControlName: 'modelFileName',
      },
      {
        key: 'Config Folder',
        value: this.configuration.configFolderName,
        fieldConfig: { key: 'Config Folder', type: 'text' },
        formControlName: 'configFolderName',
      },
      {
        key: 'ML Models Folder',
        value: this.configuration.mlModelsFolderName,
        fieldConfig: { key: 'ML Models Folder', type: 'text' },
        formControlName: 'mlModelsFolderName',
      },
    ];

    this.configData = new MatTableDataSource(data);
  }

  private formatTrainingInterval(interval: string): string {
    const parts = interval.split(':');
    if (parts.length >= 2) {
      const hours = parseInt(parts[0], 10);
      const minutes = parseInt(parts[1], 10);
      return `${hours}h ${minutes}m`;
    }
    return interval;
  }
}
