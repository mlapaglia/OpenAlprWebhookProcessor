import { Component, type OnChanges, type SimpleChanges, inject, ChangeDetectionStrategy, input, output } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { CommonModule } from '@angular/common';
import { FormBuilder, Validators, type FormGroup } from '@angular/forms';
import { type MachineLearningConfigDto } from '../machine-learning.service';
import { ConfigurationHeaderComponent } from '../configuration-header/configuration-header.component';
import { ConfigurationTableComponent } from '../configuration-table/configuration-table.component';
import { ConfigurationActionsComponent } from '../configuration-actions/configuration-actions.component';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

@Component({
  selector: 'app-configuration-card',
  templateUrl: './configuration-card.component.html',
  styleUrls: ['./configuration-card.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    MatCardModule,
    ConfigurationHeaderComponent,
    ConfigurationTableComponent,
    ConfigurationActionsComponent,
  ],
})
export class ConfigurationCardComponent extends OnPushBaseComponent implements OnChanges {
  private readonly fb = inject(FormBuilder);

  readonly configuration = input<MachineLearningConfigDto | null>(null);
  readonly isLoadingConfiguration = input.required<boolean>();
  readonly isEditingConfiguration = input.required<boolean>();
  readonly isSavingConfiguration = input.required<boolean>();

  readonly saveConfiguration = output<MachineLearningConfigDto>();
  readonly cancelConfigurationEdit = output<void>();
  readonly editConfiguration = output<void>();

  public configForm: FormGroup;

  constructor() {
    super();
    this.configForm = this.fb.group({
      minimumModelQuality: [0.05, [Validators.required, Validators.min(0.001), Validators.max(1.0)]],
      minimumTrainingData: [100, [Validators.required, Validators.min(10), Validators.max(1000000)]],
      trainingBatchSize: [50000, [Validators.required, Validators.min(1000), Validators.max(1000000)]],
      trainingInterval: ['06:00:00', [Validators.required]],
      modelFileName: ['license-plate-prediction-model.zip', [Validators.required, Validators.maxLength(255)]],
      configFolderName: ['config', [Validators.required, Validators.maxLength(255)]],
      mlModelsFolderName: ['ml-models', [Validators.required, Validators.maxLength(255)]],
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    // eslint-disable-next-line @typescript-eslint/no-unnecessary-condition
    if (changes['configuration']) {
      const config = this.configuration();
      if (config !== null) {
        this.updateConfigForm(config);
      }
    }
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  private updateConfigForm(config: MachineLearningConfigDto): void {
    this.configForm.patchValue({
      minimumModelQuality: config.minimumModelQuality,
      minimumTrainingData: config.minimumTrainingData,
      trainingBatchSize: config.trainingBatchSize,
      trainingInterval: config.trainingInterval,
      modelFileName: config.modelFileName,
      configFolderName: config.configFolderName,
      mlModelsFolderName: config.mlModelsFolderName,
    });
    this.markForCheck();
  }

  public onSaveConfiguration(): void {
    if (this.configForm.invalid || this.isSavingConfiguration()) return;

    const formValue = this.configForm.value;
    const configDto: MachineLearningConfigDto = {
      minimumModelQuality: formValue.minimumModelQuality,
      minimumTrainingData: formValue.minimumTrainingData,
      trainingBatchSize: formValue.trainingBatchSize,
      trainingInterval: formValue.trainingInterval,
      modelFileName: formValue.modelFileName,
      configFolderName: formValue.configFolderName,
      mlModelsFolderName: formValue.mlModelsFolderName,
    };

    this.saveConfiguration.emit(configDto);
  }

  public onCancelConfigurationEdit(): void {
    this.cancelConfigurationEdit.emit();
  }

  public onEditConfiguration(): void {
    this.editConfiguration.emit();
  }
}












