import type { ComponentFixture } from '@angular/core/testing';
import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { of, throwError, Subject } from 'rxjs';

import { MachineLearningComponent } from './machine-learning.component';
import type { ModelInfo, TrainingStatus, MachineLearningConfigDto } from './machine-learning.service';
import { MachineLearningService } from './machine-learning.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';

describe('MachineLearningComponent', () => {
  let component: MachineLearningComponent;
  let fixture: ComponentFixture<MachineLearningComponent>;
  let mockMlService: jasmine.SpyObj<MachineLearningService>;
  let mockSnackbarService: jasmine.SpyObj<SnackbarService>;

  const mockModelInfo: ModelInfo = {
    modelAvailable: true,
    modelType: 'Linear Regression',
    features: ['feature1', 'feature2', 'feature3'],
    description: 'Test model description',
    trainingSchedule: 'Daily at 6 AM',
    lastUpdated: '2025-01-15T10:30:00Z',
  };

  const mockTrainingStatus: TrainingStatus = {
    isTraining: false,
    lastTrainingStarted: '2025-01-15T06:00:00Z',
    lastTrainingCompleted: '2025-01-15T07:30:00Z',
    lastTrainingSuccessful: true,
    trainingDataCount: 50000,
    modelMetrics: {
      rSquared: 0.85,
      meanAbsoluteError: 2.5,
      rootMeanSquaredError: 3.2,
    },
    modelFile: {
      lastSaved: '2025-01-15T07:30:00Z',
      fileSizeBytes: 1048576,
    },
    configuration: {
      trainingInterval: '06:00:00',
      minimumTrainingData: 100,
      minimumModelQuality: 0.05,
      batchSize: 50000,
    },
  };

  const mockConfiguration: MachineLearningConfigDto = {
    minimumModelQuality: 0.05,
    minimumTrainingData: 100,
    trainingBatchSize: 50000,
    trainingInterval: '06:00:00',
    modelFileName: 'license-plate-prediction-model.zip',
    configFolderName: 'config',
    mlModelsFolderName: 'ml-models',
    lastUpdated: '2025-01-15T10:00:00Z',
    updatedBy: 'test-user',
  };

  beforeEach(async () => {
    const mlServiceSpy = jasmine.createSpyObj('MachineLearningService', [
      'getModelInfo',
      'getTrainingStatus',
      'getConfiguration',
      'triggerTraining',
      'saveConfiguration',
    ]);

    const snackbarServiceSpy = jasmine.createSpyObj('SnackbarService', ['create']);

    await TestBed.configureTestingModule({
      imports: [
        MachineLearningComponent,
        BrowserAnimationsModule,
      ],
      providers: [
        { provide: MachineLearningService, useValue: mlServiceSpy },
        { provide: SnackbarService, useValue: snackbarServiceSpy },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(MachineLearningComponent);
    component = fixture.componentInstance;
    mockMlService = TestBed.inject(MachineLearningService) as jasmine.SpyObj<MachineLearningService>;
    mockSnackbarService = TestBed.inject(SnackbarService) as jasmine.SpyObj<SnackbarService>;

    // Setup default successful responses
    mockMlService.getModelInfo.and.returnValue(of(mockModelInfo));
    mockMlService.getTrainingStatus.and.returnValue(of(mockTrainingStatus));
    mockMlService.getConfiguration.and.returnValue(of(mockConfiguration));
    mockMlService.triggerTraining.and.returnValue(of({ message: 'Training triggered', timestamp: new Date().toISOString() }));
    mockMlService.saveConfiguration.and.returnValue(of({}));
  });

  afterEach(() => {
    fixture.destroy();
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });



    it('should load all data on init', fakeAsync(() => {
      component.ngOnInit();
      tick();

      expect(mockMlService.getModelInfo).toHaveBeenCalled();
      expect(mockMlService.getTrainingStatus).toHaveBeenCalled();
      expect(mockMlService.getConfiguration).toHaveBeenCalled();
      expect(component.modelInfo).toEqual(mockModelInfo);
      expect(component.trainingStatus).toEqual(mockTrainingStatus);
      expect(component.configuration).toEqual(mockConfiguration);
    }));

    it('should setup auto-refresh subscription', fakeAsync(() => {
      spyOn(component, 'ngOnDestroy').and.callThrough();

      component.ngOnInit();
      tick(30000); // Advance by 30 seconds

      expect(mockMlService.getTrainingStatus).toHaveBeenCalledTimes(2); // Initial + auto-refresh

      component.ngOnDestroy();
      expect(component.ngOnDestroy).toHaveBeenCalled();
    }));
  });

  describe('Data Loading', () => {
    it('should handle model info loading error', fakeAsync(() => {
      mockMlService.getModelInfo.and.returnValue(throwError(() => new Error('API Error')));

      component.ngOnInit();
      tick();

      expect(mockSnackbarService.create).toHaveBeenCalledWith('Failed to load model information', SnackBarType.Error);
      expect(component.isLoadingModelInfo).toBe(false);
    }));

    it('should handle training status loading error', fakeAsync(() => {
      mockMlService.getTrainingStatus.and.returnValue(throwError(() => new Error('API Error')));

      component.ngOnInit();
      tick();

      expect(mockSnackbarService.create).toHaveBeenCalledWith('Failed to load training status', SnackBarType.Error);
      expect(component.isLoadingTrainingStatus).toBe(false);

      component.ngOnDestroy();
    }));

    it('should handle configuration loading error', fakeAsync(() => {
      mockMlService.getConfiguration.and.returnValue(throwError(() => new Error('API Error')));

      component.ngOnInit();
      tick();

      expect(mockSnackbarService.create).toHaveBeenCalledWith('Failed to load configuration', SnackBarType.Error);
      expect(component.isLoadingConfiguration).toBe(false);
    }));

    it('should update features table when model info loads', fakeAsync(() => {
      component.ngOnInit();
      tick();

      expect(component.featuresData.data.length).toBe(3);
      expect(component.featuresData.data[0].feature).toBe('feature1');
      expect(component.featuresData.data[1].feature).toBe('feature2');
      expect(component.featuresData.data[2].feature).toBe('feature3');
    }));
  });

  describe('Training Operations', () => {
    beforeEach(fakeAsync(() => {
      component.ngOnInit();
      tick();
    }));

    it('should trigger training successfully', fakeAsync(() => {
      const initialCalls = mockMlService.getTrainingStatus.calls.count();

      component.triggerTraining();
      tick();

      expect(mockMlService.triggerTraining).toHaveBeenCalled();
      expect(mockSnackbarService.create).toHaveBeenCalledWith('Training triggered successfully', SnackBarType.Successful);
      expect(component.isTriggering).toBe(false);

      // Should reload training status after 2 seconds
      tick(2000);
      expect(mockMlService.getTrainingStatus).toHaveBeenCalledTimes(initialCalls + 1); // Initial + reload
    }));

    it('should handle training trigger error', fakeAsync(() => {
      mockMlService.triggerTraining.and.returnValue(throwError(() => new Error('Training failed')));

      component.triggerTraining();
      tick();

      expect(mockSnackbarService.create).toHaveBeenCalledWith('Failed to trigger training', SnackBarType.Error);
      expect(component.isTriggering).toBe(false);
    }));

    it('should not trigger training when already triggering', () => {
      component.isTriggering = true;

      component.triggerTraining();

      expect(mockMlService.triggerTraining).not.toHaveBeenCalled();
    });

    it('should refresh data when refresh button clicked', () => {
      spyOn(component, 'refreshData').and.callThrough();
      const initialTrainingStatusCalls = mockMlService.getTrainingStatus.calls.count();
      const initialModelInfoCalls = mockMlService.getModelInfo.calls.count();
      const initialConfigCalls = mockMlService.getConfiguration.calls.count();

      component.refreshData();

      expect(mockMlService.getModelInfo).toHaveBeenCalledTimes(initialModelInfoCalls + 1); // Initial + refresh
      expect(mockMlService.getTrainingStatus).toHaveBeenCalledTimes(initialTrainingStatusCalls + 1); // Initial + refresh
      expect(mockMlService.getConfiguration).toHaveBeenCalledTimes(initialConfigCalls + 1); // Initial + refresh
    });
  });

  describe('Configuration Management', () => {
    beforeEach(fakeAsync(() => {
      component.ngOnInit();
      tick();
    }));

    it('should enter edit mode', () => {
      component.editConfiguration();

      expect(component.isEditingConfiguration).toBe(true);
    });

    it('should cancel edit mode', () => {
      component.editConfiguration();

      component.cancelConfigurationEdit();

      expect(component.isEditingConfiguration).toBe(false);
    });

    it('should save configuration successfully', fakeAsync(() => {
      component.editConfiguration();
      const testConfig: MachineLearningConfigDto = {
        minimumModelQuality: 0.1,
        minimumTrainingData: 100,
        trainingBatchSize: 50000,
        trainingInterval: '06:00:00',
        modelFileName: 'license-plate-prediction-model.zip',
        configFolderName: 'config',
        mlModelsFolderName: 'ml-models',
      };

      component.saveConfiguration(testConfig);
      tick();

      expect(mockMlService.saveConfiguration).toHaveBeenCalledWith(testConfig);
      expect(mockSnackbarService.create).toHaveBeenCalledWith('Configuration saved successfully', SnackBarType.Successful);
      expect(component.isEditingConfiguration).toBe(false);
      expect(component.isSavingConfiguration).toBe(false);
    }));

    it('should handle configuration save error', fakeAsync(() => {
      mockMlService.saveConfiguration.and.returnValue(throwError(() => new Error('Save failed')));
      component.editConfiguration();
      const testConfig: MachineLearningConfigDto = {
        minimumModelQuality: 0.1,
        minimumTrainingData: 100,
        trainingBatchSize: 50000,
        trainingInterval: '06:00:00',
        modelFileName: 'license-plate-prediction-model.zip',
        configFolderName: 'config',
        mlModelsFolderName: 'ml-models',
      };

      component.saveConfiguration(testConfig);
      tick();

      expect(mockSnackbarService.create).toHaveBeenCalledWith('Failed to save configuration', SnackBarType.Error);
      expect(component.isSavingConfiguration).toBe(false);
    }));

    it('should not save when already saving', () => {
      component.isSavingConfiguration = true;
      const testConfig: MachineLearningConfigDto = {
        minimumModelQuality: 0.1,
        minimumTrainingData: 100,
        trainingBatchSize: 50000,
        trainingInterval: '06:00:00',
        modelFileName: 'license-plate-prediction-model.zip',
        configFolderName: 'config',
        mlModelsFolderName: 'ml-models',
      };

      component.saveConfiguration(testConfig);

      expect(mockMlService.saveConfiguration).not.toHaveBeenCalled();
    });
  });

  describe('Component Cleanup', () => {
    it('should unsubscribe from refresh subscription on destroy', () => {
      component.ngOnInit();
      const subscription = component['refreshSubscription'];
      spyOn(subscription!, 'unsubscribe');

      component.ngOnDestroy();

      expect(subscription!.unsubscribe).toHaveBeenCalled();
    });

    it('should handle destroy when no subscription exists', () => {
      expect(() => component.ngOnDestroy()).not.toThrow();
    });
  });
});
