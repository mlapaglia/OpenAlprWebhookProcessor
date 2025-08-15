import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { SystemLogsService, ApiLogLevel } from './system-logs.service';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe(SystemLogsService.name, () => {
  let service: SystemLogsService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [],
      providers: [SystemLogsService, provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()],
    });
    service = TestBed.inject(SystemLogsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getLogs', () => {
    it('should call API with default log level when no parameters provided', () => {
      const mockLogs = ['log1', 'log2'];

      service.getLogs().subscribe(logs => {
        expect(logs).toEqual(mockLogs);
      });

      const req = httpMock.expectOne('/api/logs?logLevel=2');
      expect(req.request.method).toBe('GET');
      req.flush(mockLogs);
    });

    it('should call API with specified log level', () => {
      const mockLogs = ['log1', 'log2'];

      service.getLogs(ApiLogLevel.Warning).subscribe(logs => {
        expect(logs).toEqual(mockLogs);
      });

      const req = httpMock.expectOne('/api/logs?logLevel=3');
      expect(req.request.method).toBe('GET');
      req.flush(mockLogs);
    });

    it('should call API with log level and search parameter', () => {
      const mockLogs = ['filtered log1', 'filtered log2'];
      const searchText = 'error';

      service.getLogs(ApiLogLevel.Error, searchText).subscribe(logs => {
        expect(logs).toEqual(mockLogs);
      });

      const req = httpMock.expectOne('/api/logs?logLevel=4&search=error');
      expect(req.request.method).toBe('GET');
      req.flush(mockLogs);
    });

    it('should encode search parameter in URL', () => {
      const mockLogs = ['log1'];
      const searchText = 'search with spaces & special chars';

      service.getLogs(ApiLogLevel.Information, searchText).subscribe();

      const expectedUrl = `/api/logs?logLevel=2&search=${encodeURIComponent(searchText)}`;
      const req = httpMock.expectOne(expectedUrl);
      expect(req.request.method).toBe('GET');
      req.flush(mockLogs);
    });

    it('should not include search parameter when search text is empty', () => {
      const mockLogs = ['log1'];

      service.getLogs(ApiLogLevel.Information, '').subscribe();

      const req = httpMock.expectOne('/api/logs?logLevel=2');
      expect(req.request.method).toBe('GET');
      req.flush(mockLogs);
    });

    it('should not include search parameter when search text is whitespace only', () => {
      const mockLogs = ['log1'];

      service.getLogs(ApiLogLevel.Information, '   ').subscribe();

      const req = httpMock.expectOne('/api/logs?logLevel=2');
      expect(req.request.method).toBe('GET');
      req.flush(mockLogs);
    });

    it('should trim search text before encoding', () => {
      const mockLogs = ['log1'];
      const searchText = '  trimmed  ';

      service.getLogs(ApiLogLevel.Information, searchText).subscribe();

      const req = httpMock.expectOne('/api/logs?logLevel=2&search=trimmed');
      expect(req.request.method).toBe('GET');
      req.flush(mockLogs);
    });
  });

  describe('getPlateGroups', () => {
    it('should call API with onlyFailedPlateGroups parameter', () => {
      const mockBlob = new Blob(['test'], { type: 'application/json' });

      service.getPlateGroups(true).subscribe(blob => {
        expect(blob).toEqual(mockBlob);
      });

      const req = httpMock.expectOne('/api/settings/debug/plates?onlyFailedPlateGroups=true');
      expect(req.request.method).toBe('GET');
      req.flush(mockBlob);
    });
  });

  describe('deletePlates', () => {
    it('should call DELETE endpoint', () => {
      service.deletePlates().subscribe();

      const req = httpMock.expectOne('/settings/debug/plates');
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });
});
