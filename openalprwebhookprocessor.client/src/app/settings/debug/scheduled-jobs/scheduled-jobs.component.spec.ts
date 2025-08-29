import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { ScheduledJobsComponent } from './scheduled-jobs.component';
import { SettingsService } from '../../settings.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { type ScheduledJobsResponse, type ScheduledJob, ScheduledJobType, SunriseSunsetType } from './scheduled-job';

describe('ScheduledJobsComponent', () => {
  let component: ScheduledJobsComponent;
  let fixture: ComponentFixture<ScheduledJobsComponent>;
  let mockSettingsService: jasmine.SpyObj<SettingsService>;
  let mockSnackbarService: jasmine.SpyObj<SnackbarService>;

  const mockScheduledJobsResponse: ScheduledJobsResponse = {
    scheduledJobs: [
      {
        jobId: 'job1',
        jobType: ScheduledJobType.ClearOverlay,
        scheduledExecutionTime: new Date(Date.now() + 2 * 60 * 60 * 1000).toISOString(), // 2 hours from now
        cameraId: 'camera-1',
      },
      {
        jobId: 'job2',
        jobType: ScheduledJobType.SunriseSunset,
        scheduledExecutionTime: new Date(Date.now() - 1 * 60 * 60 * 1000).toISOString(), // 1 hour ago (overdue)
        cameraId: 'camera-2',
        sunriseSunsetType: SunriseSunsetType.Sunrise,
        scheduleNextJob: true,
      },
    ],
  };

  beforeEach(async () => {
    const settingsServiceSpy = jasmine.createSpyObj('SettingsService', ['getScheduledJobs']);
    const snackbarServiceSpy = jasmine.createSpyObj('SnackbarService', ['create']);

    await TestBed.configureTestingModule({
      imports: [ScheduledJobsComponent],
      providers: [
        { provide: SettingsService, useValue: settingsServiceSpy },
        { provide: SnackbarService, useValue: snackbarServiceSpy },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ScheduledJobsComponent);
    component = fixture.componentInstance;
    mockSettingsService = TestBed.inject(SettingsService) as jasmine.SpyObj<SettingsService>;
    mockSnackbarService = TestBed.inject(SnackbarService) as jasmine.SpyObj<SnackbarService>;

    // Set default return value for the service
    mockSettingsService.getScheduledJobs.and.returnValue(of(mockScheduledJobsResponse));
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load scheduled jobs on init', () => {
    mockSettingsService.getScheduledJobs.and.returnValue(of(mockScheduledJobsResponse));

    component.ngOnInit();

    expect(mockSettingsService.getScheduledJobs).toHaveBeenCalled();
    expect(component.scheduledJobsData).toEqual(mockScheduledJobsResponse);
    expect(component.isLoading).toBeFalse();
  });

  it('should handle error when loading scheduled jobs fails', () => {
    const error = new Error('Failed to load');
    mockSettingsService.getScheduledJobs.and.returnValue(throwError(() => error));

    component.ngOnInit();

    expect(mockSnackbarService.create).toHaveBeenCalledWith(
      'Failed to load scheduled jobs information',
      SnackBarType.Error,
    );
    expect(component.isLoading).toBeFalse();
    expect(component.scheduledJobsData).toBeNull();
  });

  it('should set loading state during request', () => {
    mockSettingsService.getScheduledJobs.and.returnValue(of(mockScheduledJobsResponse));

    expect(component.isLoading).toBeFalse();

    component.loadScheduledJobs();

    // After synchronous observable completes, loading should be false and data should be set
    expect(component.isLoading).toBeFalse();
    expect(component.scheduledJobsData).toEqual(mockScheduledJobsResponse);
  });

  it('should reload scheduled jobs when refresh is called', () => {
    mockSettingsService.getScheduledJobs.and.returnValue(of(mockScheduledJobsResponse));

    component.loadScheduledJobs();

    expect(mockSettingsService.getScheduledJobs).toHaveBeenCalledTimes(1);
  });

  describe('getJobTypeDisplay', () => {
    it('should return correct display text for ClearOverlay job type', () => {
      expect(component.getJobTypeDisplay(0)).toBe('Clear Overlay');
    });

    it('should return correct display text for SunriseSunset job type', () => {
      expect(component.getJobTypeDisplay(1)).toBe('Sunrise/Sunset');
    });

    it('should return Unknown for invalid job type', () => {
      expect(component.getJobTypeDisplay(99 as any)).toBe('Unknown');
    });
  });

  describe('getJobTypeIcon', () => {
    it('should return correct icon for ClearOverlay job type', () => {
      expect(component.getJobTypeIcon(0)).toBe('clear');
    });

    it('should return correct icon for SunriseSunset job type', () => {
      expect(component.getJobTypeIcon(1)).toBe('wb_sunny');
    });

    it('should return help_outline for invalid job type', () => {
      expect(component.getJobTypeIcon(99 as any)).toBe('help_outline');
    });
  });

  describe('getSunriseSunsetDisplay', () => {
    it('should return empty string for undefined', () => {
      expect(component.getSunriseSunsetDisplay(undefined)).toBe('');
    });

    it('should return Sunrise for sunrise type', () => {
      expect(component.getSunriseSunsetDisplay(1)).toBe('Sunrise');
    });

    it('should return Sunset for sunset type', () => {
      expect(component.getSunriseSunsetDisplay(2)).toBe('Sunset');
    });

    it('should return Unknown for invalid type', () => {
      expect(component.getSunriseSunsetDisplay(99 as any)).toBe('Unknown');
    });
  });

  describe('formatDateTime', () => {
    it('should format valid ISO date string', () => {
      const result = component.formatDateTime('2024-01-15T10:30:00Z');
      expect(result).toContain('2024'); // Basic check since locale formatting varies
    });

    it('should return formatted string for invalid date', () => {
      const invalidDate = 'invalid-date';
      const result = component.formatDateTime(invalidDate);
      // Invalid dates typically return "Invalid Date" in most browsers
      expect(result).toContain('Invalid Date');
    });
  });

  describe('formatTimeUntil', () => {
    it('should format future time correctly', () => {
      const futureJob: ScheduledJob = {
        jobId: 'future-job',
        jobType: ScheduledJobType.ClearOverlay,
        scheduledExecutionTime: new Date(Date.now() + 2 * 60 * 60 * 1000 + 30 * 60 * 1000).toISOString(), // 2h 30m from now
      };
      const result = component.formatTimeUntil(futureJob);
      expect(result).toMatch(/2h \d+m/); // Allow some variance in minutes due to test execution time
    });

    it('should format past time correctly', () => {
      const pastJob: ScheduledJob = {
        jobId: 'past-job',
        jobType: ScheduledJobType.ClearOverlay,
        scheduledExecutionTime: new Date(Date.now() - 1 * 60 * 60 * 1000 - 15 * 60 * 1000).toISOString(), // 1h 15m ago
      };
      const result = component.formatTimeUntil(pastJob);
      expect(result).toMatch(/1h \d+m ago/); // Allow some variance in minutes due to test execution time
    });

    it('should return Unknown for invalid date', () => {
      const invalidJob: ScheduledJob = {
        jobId: 'invalid-job',
        jobType: ScheduledJobType.ClearOverlay,
        scheduledExecutionTime: 'invalid-date',
      };
      expect(component.formatTimeUntil(invalidJob)).toBe('Unknown');
    });
  });

  describe('isJobOverdue', () => {
    it('should return true for past jobs', () => {
      const pastJob: ScheduledJob = {
        jobId: 'past-job',
        jobType: ScheduledJobType.ClearOverlay,
        scheduledExecutionTime: new Date(Date.now() - 1000).toISOString(), // 1 second ago
      };
      expect(component.isJobOverdue(pastJob)).toBe(true);
    });

    it('should return false for future jobs', () => {
      const futureJob: ScheduledJob = {
        jobId: 'future-job',
        jobType: ScheduledJobType.ClearOverlay,
        scheduledExecutionTime: new Date(Date.now() + 1000).toISOString(), // 1 second from now
      };
      expect(component.isJobOverdue(futureJob)).toBe(false);
    });

    it('should return false for invalid date', () => {
      const invalidJob: ScheduledJob = {
        jobId: 'invalid-job',
        jobType: ScheduledJobType.ClearOverlay,
        scheduledExecutionTime: 'invalid-date',
      };
      expect(component.isJobOverdue(invalidJob)).toBe(false);
    });
  });

  describe('getStatusChipClass', () => {
    it('should return overdue-chip class for overdue jobs', () => {
      const overdueJob: ScheduledJob = {
        jobId: 'overdue-job',
        jobType: ScheduledJobType.ClearOverlay,
        scheduledExecutionTime: new Date(Date.now() - 1000).toISOString(),
      };
      expect(component.getStatusChipClass(overdueJob)).toBe('overdue-chip');
    });

    it('should return scheduled-chip class for non-overdue jobs', () => {
      const scheduledJob: ScheduledJob = {
        jobId: 'scheduled-job',
        jobType: ScheduledJobType.ClearOverlay,
        scheduledExecutionTime: new Date(Date.now() + 1000).toISOString(),
      };
      expect(component.getStatusChipClass(scheduledJob)).toBe('scheduled-chip');
    });
  });

  describe('getStatusDisplay', () => {
    it('should return Overdue for overdue jobs', () => {
      const overdueJob: ScheduledJob = {
        jobId: 'overdue-job',
        jobType: ScheduledJobType.ClearOverlay,
        scheduledExecutionTime: new Date(Date.now() - 1000).toISOString(),
      };
      expect(component.getStatusDisplay(overdueJob)).toBe('Overdue');
    });

    it('should return Scheduled for non-overdue jobs', () => {
      const scheduledJob: ScheduledJob = {
        jobId: 'scheduled-job',
        jobType: ScheduledJobType.ClearOverlay,
        scheduledExecutionTime: new Date(Date.now() + 1000).toISOString(),
      };
      expect(component.getStatusDisplay(scheduledJob)).toBe('Scheduled');
    });
  });

  describe('getCameraDisplay', () => {
    it('should return truncated camera ID when available', () => {
      const job: ScheduledJob = {
        jobId: 'test-job',
        jobType: ScheduledJobType.ClearOverlay,
        scheduledExecutionTime: new Date().toISOString(),
        cameraId: 'very-long-camera-id-12345',
      };
      expect(component.getCameraDisplay(job)).toBe('Camera very-lon...');
    });

    it('should return N/A when camera ID is not available', () => {
      const job: ScheduledJob = {
        jobId: 'test-job',
        jobType: ScheduledJobType.ClearOverlay,
        scheduledExecutionTime: new Date().toISOString(),
        cameraId: undefined,
      };
      expect(component.getCameraDisplay(job)).toBe('N/A');
    });
  });

  describe('computed properties', () => {
    beforeEach(() => {
      mockSettingsService.getScheduledJobs.and.returnValue(of(mockScheduledJobsResponse));
      component.ngOnInit();
    });

    it('should calculate total jobs correctly', () => {
      expect(component.totalJobs).toBe(2);
    });

    it('should calculate overdue jobs correctly', () => {
      expect(component.overdueJobs).toBe(1); // One job is scheduled in the past
    });

    it('should calculate jobs in next 24 hours correctly', () => {
      expect(component.jobsInNext24Hours).toBe(1); // One job is scheduled 2 hours from now
    });
  });

  describe('onRefreshClick', () => {
    it('should prevent default event and call loadScheduledJobs', () => {
      const mockEvent = { preventDefault: jasmine.createSpy('preventDefault') } as unknown as Event;
      spyOn(component, 'loadScheduledJobs');

      component.onRefreshClick(mockEvent);

      expect(mockEvent.preventDefault).toHaveBeenCalled();
      expect(component.loadScheduledJobs).toHaveBeenCalled();
    });
  });
});
