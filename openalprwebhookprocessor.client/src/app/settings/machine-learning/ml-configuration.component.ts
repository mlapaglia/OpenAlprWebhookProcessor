import type { OnChanges } from '@angular/core';
import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import type { FormGroup } from '@angular/forms';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import type { MachineLearningConfigDto } from './machine-learning.service';

interface ConfigData {
  key: string;
  value: string;
}

@Component({
  selector: 'app-ml-configuration',
  template: `
    <mat-card>
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

      @if (configuration) {
        <mat-card-content>
          <form [formGroup]="configForm">
            <table mat-table [dataSource]="configData" class="mat-elevation-z2" style="width: 100%;">
              <ng-container matColumnDef="key" i18n-matColumnDef>
                <th i18n mat-header-cell *matHeaderCellDef style="min-width: 200px;">Setting</th>
                <td mat-cell *matCellDef="let element"><strong>{{ element.key }}</strong></td>
              </ng-container>
              <ng-container matColumnDef="value" i18n-matColumnDef>
                <th i18n mat-header-cell *matHeaderCellDef>Value</th>
                <td mat-cell *matCellDef="let element" style="padding: 8px;">
                  @if (isEditingConfiguration) {
                    <div style="width: 100%;">
                      @if (element.key === 'Minimum Model Quality (R²)') {
                        <mat-form-field appearance="outline" i18n-appearance style="width: 100%;" subscriptSizing="dynamic" i18n-subscriptSizing>
                          <input matInput type="number" step="0.001" min="0.001" max="1.0"
                                 formControlName="minimumModelQuality">
                          @if (getFieldError('minimumModelQuality')) {
                            <mat-error>{{ getFieldError('minimumModelQuality') }}</mat-error>
                          }
                        </mat-form-field>
                      } @else if (element.key === 'Minimum Training Data') {
                        <mat-form-field appearance="outline" i18n-appearance style="width: 100%;" subscriptSizing="dynamic" i18n-subscriptSizing>
                          <input matInput type="number" min="10" max="1000000"
                                 formControlName="minimumTrainingData">
                          @if (getFieldError('minimumTrainingData')) {
                            <mat-error>{{ getFieldError('minimumTrainingData') }}</mat-error>
                          }
                        </mat-form-field>
                      } @else if (element.key === 'Training Batch Size') {
                        <mat-form-field appearance="outline" i18n-appearance style="width: 100%;" subscriptSizing="dynamic" i18n-subscriptSizing>
                          <input matInput type="number" min="1000" max="1000000"
                                 formControlName="trainingBatchSize">
                          @if (getFieldError('trainingBatchSize')) {
                            <mat-error>{{ getFieldError('trainingBatchSize') }}</mat-error>
                          }
                        </mat-form-field>
                      } @else if (element.key === 'Training Interval') {
                        <mat-form-field appearance="outline" i18n-appearance style="width: 100%;" subscriptSizing="dynamic" i18n-subscriptSizing>
                          <input matInput type="text" formControlName="trainingInterval"
                                 placeholder="06:00:00">
                          @if (getFieldError('trainingInterval')) {
                            <mat-error>{{ getFieldError('trainingInterval') }}</mat-error>
                          }
                        </mat-form-field>
                      } @else if (element.key === 'Model File Name') {
                        <mat-form-field appearance="outline" i18n-appearance style="width: 100%;" subscriptSizing="dynamic" i18n-subscriptSizing>
                          <input matInput formControlName="modelFileName">
                          @if (getFieldError('modelFileName')) {
                            <mat-error>{{ getFieldError('modelFileName') }}</mat-error>
                          }
                        </mat-form-field>
                      } @else if (element.key === 'Config Folder') {
                        <mat-form-field appearance="outline" i18n-appearance style="width: 100%;" subscriptSizing="dynamic" i18n-subscriptSizing>
                          <input matInput formControlName="configFolderName">
                          @if (getFieldError('configFolderName')) {
                            <mat-error>{{ getFieldError('configFolderName') }}</mat-error>
                          }
                        </mat-form-field>
                      } @else if (element.key === 'ML Models Folder') {
                        <mat-form-field appearance="outline" i18n-appearance style="width: 100%;" subscriptSizing="dynamic" i18n-subscriptSizing>
                          <input matInput formControlName="mlModelsFolderName">
                          @if (getFieldError('mlModelsFolderName')) {
                            <mat-error>{{ getFieldError('mlModelsFolderName') }}</mat-error>
                          }
                        </mat-form-field>
                      }
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
        </mat-card-content>

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
      }
    </mat-card>
  `,
  styleUrls: ['./machine-learning.component.less'],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    DatePipe,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MlConfigurationComponent implements OnChanges {
  @Input() configuration: MachineLearningConfigDto | null = null;
  @Input() isLoadingConfiguration = false;
  @Input() isSavingConfiguration = false;
  @Input() isEditingConfiguration = false;
  @Input() configForm!: FormGroup;

  @Output() editConfiguration = new EventEmitter<void>();
  @Output() saveConfiguration = new EventEmitter<void>();
  @Output() cancelConfigurationEdit = new EventEmitter<void>();

  public configData = new MatTableDataSource<ConfigData>([]);
  public displayedColumns: string[] = ['key', 'value'];

  public get saveButtonText(): string {
    return this.isSavingConfiguration ? 'Saving...' : 'Save Configuration';
  }

  ngOnChanges(): void {
    this.updateConfigTable();
  }

  public getFieldError(fieldName: string): string {
    const field = this.configForm?.get(fieldName);
    if (field?.errors && field.touched) {
      if (field.errors['required']) return `${fieldName} is required`;
      if (field.errors['min']) return `${fieldName} must be at least ${field.errors['min'].min}`;
      if (field.errors['max']) return `${fieldName} must be at most ${field.errors['max'].max}`;
      if (field.errors['maxlength']) return `${fieldName} must be at most ${field.errors['maxlength'].requiredLength} characters`;
    }
    return '';
  }

  private updateConfigTable(): void {
    if (!this.configuration) {
      this.configData = new MatTableDataSource<ConfigData>([]);
      return;
    }

    const data: ConfigData[] = [
      { key: 'Minimum Model Quality (R²)', value: this.configuration.minimumModelQuality.toString() },
      { key: 'Minimum Training Data', value: this.configuration.minimumTrainingData.toLocaleString() },
      { key: 'Training Batch Size', value: this.configuration.trainingBatchSize.toLocaleString() },
      { key: 'Training Interval', value: this.formatTrainingInterval(this.configuration.trainingInterval) },
      { key: 'Model File Name', value: this.configuration.modelFileName },
      { key: 'Config Folder', value: this.configuration.configFolderName },
      { key: 'ML Models Folder', value: this.configuration.mlModelsFolderName },
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
