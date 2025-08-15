import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { SettingsService } from './settings.service';
import { type ScheduledJobsResponse, ScheduledJobType } from './debug/scheduled-jobs/scheduled-job';

describe('SettingsService', () => {
  let service: SettingsService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [SettingsService],
    });
    service = TestBed.inject(SettingsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getScheduledJobs', () => {
    it('should fetch scheduled jobs from the API', () => {
      const mockResponse: ScheduledJobsResponse = {
        scheduledJobs: [
          {
            jobId: 'test-job-1',
            jobType: ScheduledJobType.ClearOverlay,
            scheduledExecutionTime: '2024-01-15T10:30:00Z',
            cameraId: 'camera-1',
          },
        ],
      };

      service.getScheduledJobs().subscribe((response) => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne('/api/settings/scheduled-jobs');
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('should handle empty response', () => {
      const mockResponse: ScheduledJobsResponse = {
        scheduledJobs: [],
      };

      service.getScheduledJobs().subscribe((response) => {
        expect(response.scheduledJobs).toEqual([]);
      });

      const req = httpMock.expectOne('/api/settings/scheduled-jobs');
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });
  });
});
