import { Component, type OnChanges, ChangeDetectionStrategy, input } from '@angular/core';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { type FormGroup, ReactiveFormsModule } from '@angular/forms';
import { type MachineLearningConfigDto } from '../machine-learning.service';
import { ConfigurationFieldComponent } from '../configuration-field/configuration-field.component';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

@Component({
  selector: 'app-configuration-table',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (configForm()) {
      <form [formGroup]="configForm()">
        <table mat-table [dataSource]="configData" class="mat-elevation-z2" style="width: 100%;">
          <ng-container matColumnDef="key" i18n-matColumnDef>
            <th i18n mat-header-cell *matHeaderCellDef style="min-width: 200px;">Setting</th>
            <td mat-cell *matCellDef="let element"><strong>{{ element.key }}</strong></td>
          </ng-container>
          <ng-container matColumnDef="value" i18n-matColumnDef>
            <th i18n mat-header-cell *matHeaderCellDef>Value</th>
            <td mat-cell *matCellDef="let element" style="padding: 8px;">
              <app-configuration-field
                [fieldKey]="element.key"
                [fieldValue]="element.value"
                [isEditing]="isEditingConfiguration()"
                [configForm]="configForm()" />
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>
      </form>
    }
  `,
  imports: [
    ReactiveFormsModule,
    MatTableModule,
    ConfigurationFieldComponent,
  ],
})
export class ConfigurationTableComponent extends OnPushBaseComponent implements OnChanges {
  readonly configuration = input.required<MachineLearningConfigDto | null>();
  readonly isEditingConfiguration = input.required<boolean>();
  readonly configForm = input.required<FormGroup>();

  public configData = new MatTableDataSource<{ key: string; value: string }>();
  public displayedColumns: string[] = ['key', 'value'];

  ngOnChanges(): void {
    this.updateConfigTable();
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  private updateConfigTable(): void {
    const config = this.configuration();
    if (!config) return;

    const data = [
      { key: 'Minimum Model Quality (R²)', value: config.minimumModelQuality.toString() },
      { key: 'Minimum Training Data', value: config.minimumTrainingData.toLocaleString()  },
      { key: 'Training Batch Size', value: config.trainingBatchSize.toLocaleString() },
      { key: 'Training Interval', value: this.formatTrainingInterval(config.trainingInterval) },
      { key: 'Model File Name', value: config.modelFileName },
      { key: 'Config Folder', value: config.configFolderName },
      { key: 'ML Models Folder', value: config.mlModelsFolderName },
    ];

    this.configData = new MatTableDataSource(data);
    this.markForCheck();
  }

  private formatTrainingInterval(interval: string): string {
    const parts = interval.split(':');
    if (parts.length >= 2) {
      const hours = parseInt(parts[0]);
      const minutes = parseInt(parts[1]);
      return `${hours}h ${minutes}m`;
    }
    return interval;
  }
}












