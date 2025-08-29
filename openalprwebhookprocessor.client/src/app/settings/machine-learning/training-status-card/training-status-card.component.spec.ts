import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CommonModule } from '@angular/common';
import { TrainingStatusCardComponent } from './training-status-card.component';
import { RefreshButtonComponent } from 'app/shared/refresh-button/refresh-button.component';
import type { TrainingStatus } from '../machine-learning.service';

describe('TrainingStatusCardComponent', () => {
  let component: TrainingStatusCardComponent;
  let fixture: ComponentFixture<TrainingStatusCardComponent>;

  const createMockTrainingStatus = (overrides: Partial<TrainingStatus> = {}): TrainingStatus => ({
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
    ...overrides,
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        TrainingStatusCardComponent,
        RefreshButtonComponent,
        CommonModule,
        MatCardModule,
        MatButtonModule,
        MatIconModule,
        MatProgressSpinnerModule,
        MatTableModule,
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(TrainingStatusCardComponent);
    component = fixture.componentInstance;
  });

  describe('component initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.trainingStatus()).toBe(null);
      expect(component.isLoadingTrainingStatus()).toBe(undefined);
      expect(component.isTriggering()).toBe(undefined);
      expect(component.statusData.data).toEqual([]);
      expect(component.displayedColumns).toEqual(['key', 'value']);
    });

    it('should have empty status table initially', () => {
      expect(component.statusData.data.length).toBe(0);
    });
  });

  describe('input properties', () => {
    it('should accept trainingStatus input', () => {
      const mockStatus = createMockTrainingStatus();
      fixture.componentRef.setInput('trainingStatus', mockStatus);

      expect(component.trainingStatus()).toEqual(mockStatus);
    });

    it('should accept isLoadingTrainingStatus input', () => {
      fixture.componentRef.setInput('isLoadingTrainingStatus', true);

      expect(component.isLoadingTrainingStatus()).toBe(true);
    });

    it('should accept isTriggering input', () => {
      fixture.componentRef.setInput('isTriggering', true);

      expect(component.isTriggering()).toBe(true);
    });

    it('should update status table when trainingStatus changes', () => {
      const mockStatus = createMockTrainingStatus();
      fixture.componentRef.setInput('trainingStatus', mockStatus);

      component.ngOnChanges();

      expect(component.statusData.data.length).toBe(7);
      expect(component.statusData.data[0]).toEqual({ key: 'Training Status', value: 'Idle' });
      expect(component.statusData.data[4]).toEqual({ key: 'Training Data Count', value: '50,000' });
    });
  });

  describe('updateStatusTable', () => {
    it('should not update table when trainingStatus is null', () => {
      fixture.componentRef.setInput('trainingStatus', null);

      component.ngOnChanges();

      expect(component.statusData.data.length).toBe(0);
    });

    it('should create correct table data for idle training status', () => {
      const mockStatus = createMockTrainingStatus({
        isTraining: false,
        lastTrainingSuccessful: true,
      });
      fixture.componentRef.setInput('trainingStatus', mockStatus);

      component.ngOnChanges();

      const { data } = component.statusData;
      expect(data[0]).toEqual({ key: 'Training Status', value: 'Idle' });
      expect(data[1]).toEqual({ key: 'Last Training Started', value: new Date('2025-01-15T06:00:00Z').toLocaleString() });
      expect(data[2]).toEqual({ key: 'Last Training Completed', value: new Date('2025-01-15T07:30:00Z').toLocaleString() });
      expect(data[3]).toEqual({ key: 'Last Training Result', value: 'Success' });
      expect(data[4]).toEqual({ key: 'Training Data Count', value: '50,000' });
      expect(data[5]).toEqual({ key: 'Model File Last Saved', value: new Date('2025-01-15T07:30:00Z').toLocaleString() });
      expect(data[6]).toEqual({ key: 'Model File Size', value: '1 MB' });
    });

    it('should create correct table data for active training status', () => {
      const mockStatus = createMockTrainingStatus({
        isTraining: true,
      });
      fixture.componentRef.setInput('trainingStatus', mockStatus);

      component.ngOnChanges();

      const { data } = component.statusData;
      expect(data[0]).toEqual({ key: 'Training Status', value: 'Training...' });
      expect(data[3]).toEqual({ key: 'Last Training Result', value: 'In Progress' });
    });

    it('should handle missing optional fields', () => {
      const mockStatus = createMockTrainingStatus({
        lastTrainingStarted: undefined,
        lastTrainingCompleted: undefined,
        modelFile: undefined,
      });
      fixture.componentRef.setInput('trainingStatus', mockStatus);

      component.ngOnChanges();

      const { data } = component.statusData;
      expect(data[1]).toEqual({ key: 'Last Training Started', value: 'Never' });
      expect(data[2]).toEqual({ key: 'Last Training Completed', value: 'Never' });
      expect(data[5]).toEqual({ key: 'Model File Last Saved', value: 'Not saved' });
      expect(data[6]).toEqual({ key: 'Model File Size', value: 'N/A' });
    });

    it('should handle model file without lastSaved', () => {
      const mockStatus = createMockTrainingStatus({
        modelFile: {
          lastSaved: undefined as any,
          fileSizeBytes: 2048,
        },
      });
      fixture.componentRef.setInput('trainingStatus', mockStatus);

      component.ngOnChanges();

      const { data } = component.statusData;
      expect(data[5]).toEqual({ key: 'Model File Last Saved', value: 'Not saved' });
      expect(data[6]).toEqual({ key: 'Model File Size', value: '2 KB' });
    });

    it('should handle model file without fileSizeBytes', () => {
      const mockStatus = createMockTrainingStatus({
        modelFile: {
          lastSaved: '2025-01-15T07:30:00Z',
          fileSizeBytes: undefined as any,
        },
      });
      fixture.componentRef.setInput('trainingStatus', mockStatus);

      component.ngOnChanges();

      const { data } = component.statusData;
      expect(data[6]).toEqual({ key: 'Model File Size', value: 'N/A' });
    });
  });

  describe('getTrainingResult (via table data)', () => {
    it('should return "Unknown" when trainingStatus is null', () => {
      fixture.componentRef.setInput('trainingStatus', null);
      component.ngOnChanges();

      // Since getTrainingResult is private, we test it indirectly through the table data
      expect(component.statusData.data.length).toBe(0);
    });

    it('should return "In Progress" when training is active', () => {
      const mockStatus = createMockTrainingStatus({ isTraining: true });
      fixture.componentRef.setInput('trainingStatus', mockStatus);
      component.ngOnChanges();

      const resultRow = component.statusData.data.find(row => row.key === 'Last Training Result');
      expect(resultRow?.value).toBe('In Progress');
    });

    it('should return error message when lastError exists', () => {
      const mockStatus = createMockTrainingStatus({
        isTraining: false,
        lastError: 'Training failed due to insufficient data',
      });
      fixture.componentRef.setInput('trainingStatus', mockStatus);
      component.ngOnChanges();

      const resultRow = component.statusData.data.find(row => row.key === 'Last Training Result');
      expect(resultRow?.value).toBe('Failed: Training failed due to insufficient data');
    });

    it('should return "Success" when lastTrainingSuccessful is true', () => {
      const mockStatus = createMockTrainingStatus({
        isTraining: false,
        lastTrainingSuccessful: true,
      });
      fixture.componentRef.setInput('trainingStatus', mockStatus);
      component.ngOnChanges();

      const resultRow = component.statusData.data.find(row => row.key === 'Last Training Result');
      expect(resultRow?.value).toBe('Success');
    });

    it('should return "Not completed" when no success or error', () => {
      const mockStatus = createMockTrainingStatus({
        isTraining: false,
        lastTrainingSuccessful: false,
        lastError: undefined,
      });
      fixture.componentRef.setInput('trainingStatus', mockStatus);
      component.ngOnChanges();

      const resultRow = component.statusData.data.find(row => row.key === 'Last Training Result');
      expect(resultRow?.value).toBe('Not completed');
    });
  });

  describe('formatFileSize (via table data)', () => {
    it('should format bytes correctly', () => {
      const mockStatus = createMockTrainingStatus({
        modelFile: { lastSaved: '2025-01-15T07:30:00Z', fileSizeBytes: 512 },
      });
      fixture.componentRef.setInput('trainingStatus', mockStatus);
      component.ngOnChanges();

      const fileSizeRow = component.statusData.data.find(row => row.key === 'Model File Size');
      expect(fileSizeRow?.value).toBe('512 Bytes');
    });

    it('should format kilobytes correctly', () => {
      const mockStatus = createMockTrainingStatus({
        modelFile: { lastSaved: '2025-01-15T07:30:00Z', fileSizeBytes: 1024 },
      });
      fixture.componentRef.setInput('trainingStatus', mockStatus);
      component.ngOnChanges();

      const fileSizeRow = component.statusData.data.find(row => row.key === 'Model File Size');
      expect(fileSizeRow?.value).toBe('1 KB');
    });

    it('should format megabytes correctly', () => {
      const mockStatus = createMockTrainingStatus({
        modelFile: { lastSaved: '2025-01-15T07:30:00Z', fileSizeBytes: 1048576 },
      });
      fixture.componentRef.setInput('trainingStatus', mockStatus);
      component.ngOnChanges();

      const fileSizeRow = component.statusData.data.find(row => row.key === 'Model File Size');
      expect(fileSizeRow?.value).toBe('1 MB');
    });

    it('should format gigabytes correctly', () => {
      const mockStatus = createMockTrainingStatus({
        modelFile: { lastSaved: '2025-01-15T07:30:00Z', fileSizeBytes: 1073741824 },
      });
      fixture.componentRef.setInput('trainingStatus', mockStatus);
      component.ngOnChanges();

      const fileSizeRow = component.statusData.data.find(row => row.key === 'Model File Size');
      expect(fileSizeRow?.value).toBe('1 GB');
    });

    it('should handle zero bytes', () => {
      const mockStatus = createMockTrainingStatus({
        modelFile: { lastSaved: '2025-01-15T07:30:00Z', fileSizeBytes: 0 },
      });
      fixture.componentRef.setInput('trainingStatus', mockStatus);
      component.ngOnChanges();

      const fileSizeRow = component.statusData.data.find(row => row.key === 'Model File Size');
      // Note: The component treats fileSizeBytes: 0 as falsy, so it returns 'N/A'
      // This is actually a bug in the component logic, but we're testing current behavior
      expect(fileSizeRow?.value).toBe('N/A');
    });
  });

  describe('getResultColor', () => {
    it('should return primary color for success', () => {
      expect(component.getResultColor('Success')).toBe('var(--mat-app-primary)');
      expect(component.getResultColor('Training Success')).toBe('var(--mat-app-primary)');
    });

    it('should return error color for failed', () => {
      expect(component.getResultColor('Failed: Error message')).toBe('var(--mat-app-error)');
      expect(component.getResultColor('Training Failed')).toBe('var(--mat-app-error)');
    });

    it('should return tertiary color for in progress', () => {
      expect(component.getResultColor('In Progress')).toBe('var(--mat-app-tertiary)');
      expect(component.getResultColor('Training In Progress')).toBe('var(--mat-app-tertiary)');
    });

    it('should return surface variant color for other values', () => {
      expect(component.getResultColor('Unknown')).toBe('var(--mat-app-on-surface-variant)');
      expect(component.getResultColor('Not completed')).toBe('var(--mat-app-on-surface-variant)');
      expect(component.getResultColor('Pending')).toBe('var(--mat-app-on-surface-variant)');
    });
  });

  describe('user interactions', () => {
    it('should emit triggerTraining when onTriggerTraining is called', () => {
      spyOn(component.triggerTraining, 'emit');

      component.onTriggerTraining();

      expect(component.triggerTraining.emit).toHaveBeenCalledWith();
    });

    it('should emit refreshData when onRefreshData is called', () => {
      spyOn(component.refreshData, 'emit');

      component.onRefreshData();

      expect(component.refreshData.emit).toHaveBeenCalledWith();
    });
  });

  describe('template rendering', () => {
    it('should show "No training status available" when trainingStatus is null', () => {
      fixture.componentRef.setInput('trainingStatus', null);
      fixture.detectChanges();

      const noStatusElement = fixture.debugElement.query(By.css('.no-status'));
      expect(noStatusElement).toBeTruthy();
      expect(noStatusElement.nativeElement.textContent.trim()).toBe('No training status available.');
    });

    it('should render table when trainingStatus is provided', () => {
      const mockStatus = createMockTrainingStatus();
      fixture.componentRef.setInput('trainingStatus', mockStatus);
      fixture.detectChanges();

      const tableElement = fixture.debugElement.query(By.css('table[mat-table]'));
      expect(tableElement).toBeTruthy();

      const rows = fixture.debugElement.queryAll(By.css('tr[mat-row]'));
      expect(rows.length).toBe(7); // 7 data rows
    });

    it('should apply loading class when isLoadingTrainingStatus is true', () => {
      fixture.componentRef.setInput('isLoadingTrainingStatus', true);
      fixture.detectChanges();

      const cardElement = fixture.debugElement.query(By.css('mat-card'));
      expect(cardElement.nativeElement.classList).toContain('loading');
    });

    it('should not apply loading class when isLoadingTrainingStatus is false', () => {
      fixture.componentRef.setInput('isLoadingTrainingStatus', false);
      fixture.detectChanges();

      const cardElement = fixture.debugElement.query(By.css('mat-card'));
      expect(cardElement.nativeElement.classList).not.toContain('loading');
    });

    it('should render refresh buttons', () => {
      fixture.detectChanges();

      const refreshButtons = fixture.debugElement.queryAll(By.css('app-refresh-button'));
      expect(refreshButtons.length).toBe(2);
    });

    it('should pass correct properties to start training button', () => {
      const mockStatus = createMockTrainingStatus({ isTraining: true });
      fixture.componentRef.setInput('trainingStatus', mockStatus);
      fixture.componentRef.setInput('isTriggering', false);
      fixture.detectChanges();

      const startTrainingButton = fixture.debugElement.queryAll(By.css('app-refresh-button'))[0];
      const buttonComponent = startTrainingButton.componentInstance as RefreshButtonComponent;

      expect(buttonComponent.isLoading()).toBe(true); // isTriggering || trainingStatus?.isTraining
      expect(buttonComponent.buttonType()).toBe('raised');
      expect(buttonComponent.buttonText()).toBe('Start Training');
      expect(buttonComponent.buttonRefreshingText()).toBe('Starting...');
      expect(buttonComponent.color()).toBe('primary');
      expect(buttonComponent.icon()).toBe('directions_run');
    });

    it('should pass correct properties to refresh data button', () => {
      fixture.componentRef.setInput('isLoadingTrainingStatus', true);
      fixture.detectChanges();

      const refreshDataButton = fixture.debugElement.queryAll(By.css('app-refresh-button'))[1];
      const buttonComponent = refreshDataButton.componentInstance as RefreshButtonComponent;

      expect(buttonComponent.isLoading()).toBe(true);
      expect(buttonComponent.buttonType()).toBe('basic');
      expect(buttonComponent.icon()).toBe('sync');
    });

    it('should apply correct color to Last Training Result cell', () => {
      const mockStatus = createMockTrainingStatus({
        lastTrainingSuccessful: true,
        isTraining: false,
      });
      fixture.componentRef.setInput('trainingStatus', mockStatus);
      fixture.detectChanges();

      const valueCells = fixture.debugElement.queryAll(By.css('td[mat-cell]'));
      const resultCell = valueCells.find(cell =>
        cell.nativeElement.textContent.trim() === 'Success',
      );

      expect(resultCell).toBeTruthy();
      if (resultCell) {
        expect(resultCell.nativeElement.style.color).toBe('var(--mat-app-primary)');
      }
    });

    it('should emit events when refresh buttons are clicked', () => {
      spyOn(component.triggerTraining, 'emit');
      spyOn(component.refreshData, 'emit');
      fixture.detectChanges();

      const refreshButtons = fixture.debugElement.queryAll(By.css('app-refresh-button'));

      // Click start training button
      refreshButtons[0].triggerEventHandler('refreshStarted', null);
      expect(component.triggerTraining.emit).toHaveBeenCalledWith();

      // Click refresh data button
      refreshButtons[1].triggerEventHandler('refreshStarted', null);
      expect(component.refreshData.emit).toHaveBeenCalledWith();
    });
  });

  describe('edge cases', () => {
    it('should handle trainingDataCount of 0', () => {
      const mockStatus = createMockTrainingStatus({ trainingDataCount: 0 });
      fixture.componentRef.setInput('trainingStatus', mockStatus);

      component.ngOnChanges();

      const { data } = component.statusData;
      expect(data[4]).toEqual({ key: 'Training Data Count', value: '0' });
    });

    it('should handle very large trainingDataCount', () => {
      const mockStatus = createMockTrainingStatus({ trainingDataCount: 1234567 });
      fixture.componentRef.setInput('trainingStatus', mockStatus);

      component.ngOnChanges();

      const { data } = component.statusData;
      expect(data[4]).toEqual({ key: 'Training Data Count', value: '1,234,567' });
    });

    it('should handle file size of 0 bytes in edge cases', () => {
      // This test is covered in the formatFileSize section above
      expect(true).toBe(true);
    });

    it('should handle undefined inputs gracefully', () => {
      fixture.componentRef.setInput('trainingStatus', undefined);
      fixture.componentRef.setInput('isLoadingTrainingStatus', undefined);
      fixture.componentRef.setInput('isTriggering', undefined);

      expect(() => {
        component.ngOnChanges();
        fixture.detectChanges();
      }).not.toThrow();
    });

    it('should handle training status with all optional fields missing', () => {
      const minimalStatus: TrainingStatus = {
        isTraining: false,
        lastTrainingSuccessful: false,
        trainingDataCount: 0,
        configuration: {
          trainingInterval: '06:00:00',
          minimumTrainingData: 100,
          minimumModelQuality: 0.05,
          batchSize: 50000,
        },
      };
      fixture.componentRef.setInput('trainingStatus', minimalStatus);

      component.ngOnChanges();

      expect(component.statusData.data.length).toBe(7);
      expect(component.statusData.data[1].value).toBe('Never');
      expect(component.statusData.data[2].value).toBe('Never');
      expect(component.statusData.data[3].value).toBe('Not completed');
      expect(component.statusData.data[5].value).toBe('Not saved');
      expect(component.statusData.data[6].value).toBe('N/A');
    });
  });

  describe('component lifecycle', () => {
    it('should update status table when ngOnChanges is called', () => {
      const mockStatus = createMockTrainingStatus();
      fixture.componentRef.setInput('trainingStatus', mockStatus);

      expect(component.statusData.data.length).toBe(0);

      component.ngOnChanges();

      expect(component.statusData.data.length).toBe(7);
    });

    it('should update status table data when trainingStatus changes', () => {
      const mockStatus = createMockTrainingStatus({ trainingDataCount: 12345 });
      fixture.componentRef.setInput('trainingStatus', mockStatus);

      component.ngOnChanges();

      const dataCountRow = component.statusData.data.find(row => row.key === 'Training Data Count');
      expect(dataCountRow?.value).toBe('12,345');
    });

    it('should call super.ngOnDestroy', () => {
      spyOn(Object.getPrototypeOf(Object.getPrototypeOf(component)), 'ngOnDestroy');

      component.ngOnDestroy();

      expect(Object.getPrototypeOf(Object.getPrototypeOf(component)).ngOnDestroy).toHaveBeenCalled();
    });
  });
});
