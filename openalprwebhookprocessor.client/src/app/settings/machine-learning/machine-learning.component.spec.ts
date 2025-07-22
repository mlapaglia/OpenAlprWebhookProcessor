import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing'
import { ReactiveFormsModule } from '@angular/forms'
import { MatCardModule } from '@angular/material/card'
import { MatButtonModule } from '@angular/material/button'
import { MatIconModule } from '@angular/material/icon'
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner'
import { MatTableModule } from '@angular/material/table'
import { MatFormFieldModule } from '@angular/material/form-field'
import { MatInputModule } from '@angular/material/input'
import { MatDividerModule } from '@angular/material/divider'
import { BrowserAnimationsModule } from '@angular/platform-browser/animations'
import { of, throwError, Subject } from 'rxjs'

import { MachineLearningComponent } from './machine-learning.component'
import { MachineLearningService, ModelInfo, TrainingStatus, MachineLearningConfigDto } from './machine-learning.service'
import { SnackbarService } from 'app/snackbar/snackbar.service'
import { SnackBarType } from 'app/snackbar/snackbartype'

describe('MachineLearningComponent', () => {
  let component: MachineLearningComponent
  let fixture: ComponentFixture<MachineLearningComponent>
  let mockMlService: jasmine.SpyObj<MachineLearningService>
  let mockSnackbarService: jasmine.SpyObj<SnackbarService>

  const mockModelInfo: ModelInfo = {
    modelAvailable: true,
    modelType: 'Linear Regression',
    features: ['feature1', 'feature2', 'feature3'],
    description: 'Test model description',
    trainingSchedule: 'Daily at 6 AM',
    lastUpdated: '2025-01-15T10:30:00Z'
  }

  const mockTrainingStatus: TrainingStatus = {
    isTraining: false,
    lastTrainingStarted: '2025-01-15T06:00:00Z',
    lastTrainingCompleted: '2025-01-15T07:30:00Z',
    lastTrainingSuccessful: true,
    trainingDataCount: 50000,
    modelMetrics: {
      rSquared: 0.85,
      meanAbsoluteError: 2.5,
      rootMeanSquaredError: 3.2
    },
    modelFile: {
      lastSaved: '2025-01-15T07:30:00Z',
      fileSizeBytes: 1048576
    },
    configuration: {
      trainingInterval: '06:00:00',
      minimumTrainingData: 100,
      minimumModelQuality: 0.05,
      batchSize: 50000
    }
  }

  const mockConfiguration: MachineLearningConfigDto = {
    minimumModelQuality: 0.05,
    minimumTrainingData: 100,
    trainingBatchSize: 50000,
    trainingInterval: '06:00:00',
    modelFileName: 'license-plate-prediction-model.zip',
    configFolderName: 'config',
    mlModelsFolderName: 'ml-models',
    lastUpdated: '2025-01-15T10:00:00Z',
    updatedBy: 'test-user'
  }

  beforeEach(async () => {
    const mlServiceSpy = jasmine.createSpyObj('MachineLearningService', [
      'getModelInfo',
      'getTrainingStatus',
      'getConfiguration',
      'triggerTraining',
      'saveConfiguration'
    ])

    const snackbarServiceSpy = jasmine.createSpyObj('SnackbarService', ['create'])

    await TestBed.configureTestingModule({
      imports: [
        MachineLearningComponent,
        ReactiveFormsModule,
        MatCardModule,
        MatButtonModule,
        MatIconModule,
        MatProgressSpinnerModule,
        MatTableModule,
        MatFormFieldModule,
        MatInputModule,
        MatDividerModule,
        BrowserAnimationsModule
      ],
      providers: [
        { provide: MachineLearningService, useValue: mlServiceSpy },
        { provide: SnackbarService, useValue: snackbarServiceSpy }
      ]
    }).compileComponents()

    fixture = TestBed.createComponent(MachineLearningComponent)
    component = fixture.componentInstance
    mockMlService = TestBed.inject(MachineLearningService) as jasmine.SpyObj<MachineLearningService>
    mockSnackbarService = TestBed.inject(SnackbarService) as jasmine.SpyObj<SnackbarService>

    // Setup default successful responses
    mockMlService.getModelInfo.and.returnValue(of(mockModelInfo))
    mockMlService.getTrainingStatus.and.returnValue(of(mockTrainingStatus))
    mockMlService.getConfiguration.and.returnValue(of(mockConfiguration))
    mockMlService.triggerTraining.and.returnValue(of({ message: 'Training triggered', timestamp: new Date().toISOString() }))
    mockMlService.saveConfiguration.and.returnValue(of({}))
  })

  afterEach(() => {
    fixture.destroy()
  })

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy()
    })

    it('should initialize form with default values', () => {
      expect(component.configForm.get('minimumModelQuality')?.value).toBe(0.05)
      expect(component.configForm.get('minimumTrainingData')?.value).toBe(100)
      expect(component.configForm.get('trainingBatchSize')?.value).toBe(50000)
      expect(component.configForm.get('trainingInterval')?.value).toBe('06:00:00')
      expect(component.configForm.get('modelFileName')?.value).toBe('license-plate-prediction-model.zip')
      expect(component.configForm.get('configFolderName')?.value).toBe('config')
      expect(component.configForm.get('mlModelsFolderName')?.value).toBe('ml-models')
    })

    it('should load all data on init', fakeAsync(() => {
      component.ngOnInit()
      tick()

      expect(mockMlService.getModelInfo).toHaveBeenCalled()
      expect(mockMlService.getTrainingStatus).toHaveBeenCalled()
      expect(mockMlService.getConfiguration).toHaveBeenCalled()
      expect(component.modelInfo).toEqual(mockModelInfo)
      expect(component.trainingStatus).toEqual(mockTrainingStatus)
      expect(component.configuration).toEqual(mockConfiguration)
    }))

    it('should setup auto-refresh subscription', fakeAsync(() => {
      spyOn(component, 'ngOnDestroy').and.callThrough()
      
      component.ngOnInit()
      tick(30000) // Advance by 30 seconds
      
      expect(mockMlService.getTrainingStatus).toHaveBeenCalledTimes(2) // Initial + auto-refresh
      
      component.ngOnDestroy()
      expect(component.ngOnDestroy).toHaveBeenCalled()
    }))
  })

  describe('Data Loading', () => {
    it('should handle model info loading error', fakeAsync(() => {
      mockMlService.getModelInfo.and.returnValue(throwError(() => new Error('API Error')))
      
      component.ngOnInit()
      tick()

      expect(mockSnackbarService.create).toHaveBeenCalledWith('Failed to load model information', SnackBarType.Error)
      expect(component.isLoadingModelInfo).toBe(false)
    }))

    it('should handle training status loading error', fakeAsync(() => {
      mockMlService.getTrainingStatus.and.returnValue(throwError(() => new Error('API Error')))
      
      component.ngOnInit()
      tick()

      expect(mockSnackbarService.create).toHaveBeenCalledWith('Failed to load training status', SnackBarType.Error)
      expect(component.isLoadingTrainingStatus).toBe(false)
    }))

    it('should handle configuration loading error', fakeAsync(() => {
      mockMlService.getConfiguration.and.returnValue(throwError(() => new Error('API Error')))
      
      component.ngOnInit()
      tick()

      expect(mockSnackbarService.create).toHaveBeenCalledWith('Failed to load configuration', SnackBarType.Error)
      expect(component.isLoadingConfiguration).toBe(false)
    }))

    it('should update features table when model info loads', fakeAsync(() => {
      component.ngOnInit()
      tick()

      expect(component.featuresData.data.length).toBe(3)
      expect(component.featuresData.data[0].feature).toBe('feature1')
      expect(component.featuresData.data[1].feature).toBe('feature2')
      expect(component.featuresData.data[2].feature).toBe('feature3')
    }))
  })

  describe('Training Operations', () => {
    beforeEach(fakeAsync(() => {
      component.ngOnInit()
      tick()
    }))

    it('should trigger training successfully', fakeAsync(() => {
      component.triggerTraining()
      tick()

      expect(mockMlService.triggerTraining).toHaveBeenCalled()
      expect(mockSnackbarService.create).toHaveBeenCalledWith('Training triggered successfully', SnackBarType.Successful)
      expect(component.isTriggering).toBe(false)
      
      // Should reload training status after 2 seconds
      tick(2000)
      expect(mockMlService.getTrainingStatus).toHaveBeenCalledTimes(2) // Initial + reload
    }))

    it('should handle training trigger error', fakeAsync(() => {
      mockMlService.triggerTraining.and.returnValue(throwError(() => new Error('Training failed')))
      
      component.triggerTraining()
      tick()

      expect(mockSnackbarService.create).toHaveBeenCalledWith('Failed to trigger training', SnackBarType.Error)
      expect(component.isTriggering).toBe(false)
    }))

    it('should not trigger training when already triggering', () => {
      component.isTriggering = true
      
      component.triggerTraining()
      
      expect(mockMlService.triggerTraining).not.toHaveBeenCalled()
    })

    it('should refresh data when refresh button clicked', () => {
      spyOn(component, 'refreshData').and.callThrough()
      
      component.refreshData()
      
      expect(mockMlService.getModelInfo).toHaveBeenCalledTimes(2) // Initial + refresh
      expect(mockMlService.getTrainingStatus).toHaveBeenCalledTimes(2) // Initial + refresh  
      expect(mockMlService.getConfiguration).toHaveBeenCalledTimes(2) // Initial + refresh
    })
  })

  describe('Configuration Management', () => {
    beforeEach(fakeAsync(() => {
      component.ngOnInit()
      tick()
    }))

    it('should enter edit mode', () => {
      component.editConfiguration()
      
      expect(component.isEditingConfiguration).toBe(true)
    })

    it('should cancel edit mode and reset form', () => {
      component.editConfiguration()
      component.configForm.patchValue({ minimumModelQuality: 0.1 })
      
      component.cancelConfigurationEdit()
      
      expect(component.isEditingConfiguration).toBe(false)
      expect(component.configForm.get('minimumModelQuality')?.value).toBe(0.05) // Reset to original
    })

    it('should save configuration successfully', fakeAsync(() => {
      component.editConfiguration()
      component.configForm.patchValue({ minimumModelQuality: 0.1 })
      
      component.saveConfiguration()
      tick()

      expect(mockMlService.saveConfiguration).toHaveBeenCalledWith(jasmine.objectContaining({
        minimumModelQuality: 0.1,
        minimumTrainingData: 100,
        trainingBatchSize: 50000,
        trainingInterval: '06:00:00',
        modelFileName: 'license-plate-prediction-model.zip',
        configFolderName: 'config',
        mlModelsFolderName: 'ml-models'
      }))
      expect(mockSnackbarService.create).toHaveBeenCalledWith('Configuration saved successfully', SnackBarType.Successful)
      expect(component.isEditingConfiguration).toBe(false)
      expect(component.isSavingConfiguration).toBe(false)
    }))

    it('should handle configuration save error', fakeAsync(() => {
      mockMlService.saveConfiguration.and.returnValue(throwError(() => new Error('Save failed')))
      component.editConfiguration()
      
      component.saveConfiguration()
      tick()

      expect(mockSnackbarService.create).toHaveBeenCalledWith('Failed to save configuration', SnackBarType.Error)
      expect(component.isSavingConfiguration).toBe(false)
    }))

    it('should not save invalid configuration', () => {
      component.editConfiguration()
      component.configForm.patchValue({ minimumModelQuality: -1 }) // Invalid value
      
      component.saveConfiguration()
      
      expect(mockMlService.saveConfiguration).not.toHaveBeenCalled()
    })

    it('should not save when already saving', () => {
      component.isSavingConfiguration = true
      
      component.saveConfiguration()
      
      expect(mockMlService.saveConfiguration).not.toHaveBeenCalled()
    })
  })

  describe('Form Validation', () => {
    it('should validate minimum model quality', () => {
      const field = component.configForm.get('minimumModelQuality')
      
      field?.setValue(-1)
      field?.markAsTouched()
      expect(component.getFieldError('minimumModelQuality')).toContain('must be at least')
      
      field?.setValue(2)
      expect(component.getFieldError('minimumModelQuality')).toContain('must be at most')
      
      field?.setValue(0.5)
      expect(component.getFieldError('minimumModelQuality')).toBe('')
    })

    it('should validate minimum training data', () => {
      const field = component.configForm.get('minimumTrainingData')
      
      field?.setValue(5)
      field?.markAsTouched()
      expect(component.getFieldError('minimumTrainingData')).toContain('must be at least')
      
      field?.setValue(2000000)
      expect(component.getFieldError('minimumTrainingData')).toContain('must be at most')
      
      field?.setValue(1000)
      expect(component.getFieldError('minimumTrainingData')).toBe('')
    })

    it('should validate training batch size', () => {
      const field = component.configForm.get('trainingBatchSize')
      
      field?.setValue(500)
      field?.markAsTouched()
      expect(component.getFieldError('trainingBatchSize')).toContain('must be at least')
      
      field?.setValue(2000000)
      expect(component.getFieldError('trainingBatchSize')).toContain('must be at most')
      
      field?.setValue(50000)
      expect(component.getFieldError('trainingBatchSize')).toBe('')
    })

    it('should validate required fields', () => {
      const field = component.configForm.get('modelFileName')
      
      field?.setValue('')
      field?.markAsTouched()
      expect(component.getFieldError('modelFileName')).toContain('is required')
      
      field?.setValue('test.zip')
      expect(component.getFieldError('modelFileName')).toBe('')
    })
  })

  describe('Data Display Helpers', () => {
    beforeEach(fakeAsync(() => {
      component.ngOnInit()
      tick()
    }))

    it('should format file size correctly', () => {
      expect(component['formatFileSize'](1024)).toBe('1 KB')
      expect(component['formatFileSize'](1048576)).toBe('1 MB')
      expect(component['formatFileSize'](1073741824)).toBe('1 GB')
      expect(component['formatFileSize'](0)).toBe('0 Bytes')
    })

    it('should format training interval correctly', () => {
      expect(component.formatTrainingInterval('06:30:00')).toBe('6h 30m')
      expect(component.formatTrainingInterval('24:00:00')).toBe('24h 0m')
      expect(component.formatTrainingInterval('invalid')).toBe('invalid')
    })

    it('should get correct result colors', () => {
      expect(component.getResultColor('Success')).toBe('#32de84')
      expect(component.getResultColor('Failed: Error')).toBe('#D2122E')
      expect(component.getResultColor('In Progress')).toBe('#ff9800')
      expect(component.getResultColor('Unknown')).toBe('#666666')
    })

    it('should get training result based on status', () => {
      // Test in progress
      const trainingStatus = { ...mockTrainingStatus, isTraining: true }
      component.trainingStatus = trainingStatus
      expect(component['getTrainingResult']()).toBe('In Progress')

      // Test failed
      const failedStatus = { ...mockTrainingStatus, lastError: 'Training failed', lastTrainingSuccessful: false }
      component.trainingStatus = failedStatus
      expect(component['getTrainingResult']()).toBe('Failed: Training failed')

      // Test success
      const successStatus = { ...mockTrainingStatus, lastTrainingSuccessful: true }
      component.trainingStatus = successStatus
      expect(component['getTrainingResult']()).toBe('Success')

      // Test not completed
      const notCompletedStatus = { ...mockTrainingStatus, lastTrainingSuccessful: false }
      component.trainingStatus = notCompletedStatus
      expect(component['getTrainingResult']()).toBe('Not completed')
    })
  })

  describe('Table Data Updates', () => {
    beforeEach(fakeAsync(() => {
      component.ngOnInit()
      tick()
    }))

    it('should update status table data', () => {
      expect(component.statusData.data.length).toBe(7)
      expect(component.statusData.data.find(row => row.key === 'Training Status')?.value).toBe('Idle')
      expect(component.statusData.data.find(row => row.key === 'Training Data Count')?.value).toBe('50,000')
    })

    it('should update metrics table data', () => {
      expect(component.metricsData.data.length).toBe(3)
      expect(component.metricsData.data.find(row => row.key === 'R-Squared (R²)')?.value).toBe('0.8500')
      expect(component.metricsData.data.find(row => row.key === 'Mean Absolute Error')?.value).toBe('2.50 hours')
    })

    it('should update config table data', () => {
      expect(component.configData.data.length).toBe(7)
      expect(component.configData.data.find(row => row.key === 'Minimum Model Quality (R²)')?.value).toBe('0.05')
      expect(component.configData.data.find(row => row.key === 'Training Interval')?.value).toBe('6h 0m')
    })

    it('should handle missing model metrics', () => {
      const statusWithoutMetrics = { ...mockTrainingStatus, modelMetrics: undefined }
      component.trainingStatus = statusWithoutMetrics
      component['updateMetricsTable']()
      
      expect(component.metricsData.data.length).toBe(0)
    })

    it('should handle missing model file info', () => {
      const statusWithoutFile = { ...mockTrainingStatus, modelFile: undefined }
      component.trainingStatus = statusWithoutFile
      component['updateStatusTable']()
      
      const modelFileRow = component.statusData.data.find(row => row.key === 'Model File Last Saved')
      const fileSizeRow = component.statusData.data.find(row => row.key === 'Model File Size')
      
      expect(modelFileRow?.value).toBe('Not saved')
      expect(fileSizeRow?.value).toBe('N/A')
    })
  })

  describe('Component Cleanup', () => {
    it('should unsubscribe from refresh subscription on destroy', () => {
      component.ngOnInit()
      const subscription = component['refreshSubscription']
      spyOn(subscription!, 'unsubscribe')
      
      component.ngOnDestroy()
      
      expect(subscription!.unsubscribe).toHaveBeenCalled()
    })

    it('should handle destroy when no subscription exists', () => {
      expect(() => component.ngOnDestroy()).not.toThrow()
    })
  })
})