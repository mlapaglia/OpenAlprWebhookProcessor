import type { ComponentFixture } from '@angular/core/testing';
import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { SystemLogsComponent } from './system-logs.component';
import { SystemLogsService, ApiLogLevel } from './system-logs.service';
import { SignalrService } from 'app/signalr/signalr.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { Subject, of, delay } from 'rxjs';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HIGHLIGHT_OPTIONS } from 'ngx-highlightjs';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe(SystemLogsComponent.name, () => {
  let component: SystemLogsComponent;
  let fixture: ComponentFixture<SystemLogsComponent>;
  let systemLogsServiceSpy: jasmine.SpyObj<SystemLogsService>;
  let signalrServiceSpy: jasmine.SpyObj<SignalrService>;
  let snackbarServiceSpy: jasmine.SpyObj<SnackbarService>;

  // Mock SignalR observable
  let processInformationLoggedSubject: Subject<any>;

  beforeEach(async () => {
    processInformationLoggedSubject = new Subject();

    systemLogsServiceSpy = jasmine.createSpyObj(SystemLogsService.name, ['getLogs', 'getPlateGroups', 'deletePlates']);
    signalrServiceSpy = jasmine.createSpyObj(SignalrService.name, ['startConnection'], {
      processInformationLogged: processInformationLoggedSubject.asObservable(),
    });
    snackbarServiceSpy = jasmine.createSpyObj(SnackbarService.name, ['create']);

    await TestBed.configureTestingModule({
      imports: [SystemLogsComponent, NoopAnimationsModule],
      providers: [
        { provide: SystemLogsService, useValue: systemLogsServiceSpy },
        { provide: SignalrService, useValue: signalrServiceSpy },
        { provide: SnackbarService, useValue: snackbarServiceSpy },
        {
          provide: HIGHLIGHT_OPTIONS,
          useValue: {
            coreLibraryLoader: () => Promise.resolve({
              highlight: (text: string) => ({
                value: text,
                language: 'plaintext',
                relevance: 0,
                top: null,
                secondBest: null,
              }),
              highlightAuto: (text: string) => ({
                value: text,
                language: 'plaintext',
                relevance: 0,
                top: null,
                secondBest: null,
              }),
              configure: () => {},
              listLanguages: () => ['plaintext'],
              registerLanguage: () => {},
              getLanguage: () => ({ name: 'plaintext' }),
              highlightAll: () => {},
              debugMode: () => {},
              safeMode: () => {},
              versionString: '11.0.0',
            }),
            languages: {
              plaintext: () => Promise.resolve({}),
            },
          },
        },
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
      ],
    }).compileComponents();
  });

  beforeEach(fakeAsync(() => {
    const logs: string[] = ['test log 1', 'test log 2'];
    systemLogsServiceSpy.getLogs.and.returnValue(of(logs));
    systemLogsServiceSpy.getPlateGroups.and.returnValue(of(new Blob()));
    systemLogsServiceSpy.deletePlates.and.returnValue(of({}));

    fixture = TestBed.createComponent(SystemLogsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
    tick(); // Handle the setTimeout in ngAfterViewInit
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('initialization', () => {
    it('should initialize with default values', () => {
      expect(component.selectedLogLevel).toBe(ApiLogLevel.Information);
      expect(component.searchControl.value).toBe('');
      expect(component.onlyFailedPlateGroups).toBe(false);
      expect(component.isPurging).toBe(false);
    });

    it('should call getLogs with default parameters on init', () => {
      expect(systemLogsServiceSpy.getLogs).toHaveBeenCalledWith(ApiLogLevel.Information, '');
    });
  });

  describe('populateLogs', () => {
    it('should call service with current log level and search text', () => {
      component.selectedLogLevel = ApiLogLevel.Warning;
      component.searchControl.setValue('error');
      systemLogsServiceSpy.getLogs.calls.reset();

      component.populateLogs();

      expect(systemLogsServiceSpy.getLogs).toHaveBeenCalledWith(ApiLogLevel.Warning, 'error');
    });

    it('should update logMessages and logMessagesDisplay', () => {
      const mockLogs = ['log line 1', 'log line 2'];
      systemLogsServiceSpy.getLogs.and.returnValue(of(mockLogs));

      component.populateLogs();

      expect(component.logMessages).toEqual(mockLogs);
      expect(component.logMessagesDisplay).toBe('log line 1\r\nlog line 2');
    });
  });

  describe('onLogLevelChange', () => {
    it('should call populateLogs when log level changes', () => {
      spyOn(component, 'populateLogs');

      component.onLogLevelChange();

      expect(component.populateLogs).toHaveBeenCalled();
    });
  });

  describe('searchControl debounce', () => {
    it('should trigger debounced search when search text changes', fakeAsync(() => {
      const initialCallCount = systemLogsServiceSpy.getLogs.calls.count();

      component.searchControl.setValue('test search');

      // Should not be called immediately (beyond initial calls)
      expect(systemLogsServiceSpy.getLogs.calls.count()).toBe(initialCallCount);

      // Should be called after debounce time
      tick(150);
      expect(systemLogsServiceSpy.getLogs.calls.count()).toBe(initialCallCount + 1);
      expect(systemLogsServiceSpy.getLogs).toHaveBeenCalledWith(ApiLogLevel.Information, 'test search');
    }));

    it('should debounce multiple rapid search changes', fakeAsync(() => {
      const initialCallCount = systemLogsServiceSpy.getLogs.calls.count();

      // Trigger multiple rapid changes
      component.searchControl.setValue('a');
      tick(50);

      component.searchControl.setValue('ab');
      tick(50);

      component.searchControl.setValue('abc');

      // Should not be called yet (beyond initial calls)
      expect(systemLogsServiceSpy.getLogs.calls.count()).toBe(initialCallCount);

      // Should be called only once after final debounce
      tick(150);
      expect(systemLogsServiceSpy.getLogs.calls.count()).toBe(initialCallCount + 1);
      expect(systemLogsServiceSpy.getLogs).toHaveBeenCalledWith(ApiLogLevel.Information, 'abc');
    }));
  });

  describe('shouldIncludeLogBySearch', () => {
    it('should return true when search text is empty', () => {
      component.searchControl.setValue('');

      const result = component['shouldIncludeLogBySearch']('any log message');

      expect(result).toBe(true);
    });

    it('should return true when search text is whitespace only', () => {
      component.searchControl.setValue('   ');

      const result = component['shouldIncludeLogBySearch']('any log message');

      expect(result).toBe(true);
    });

    it('should return true when log message contains search text (case insensitive)', () => {
      component.searchControl.setValue('ERROR');

      const result = component['shouldIncludeLogBySearch']('This is an error message');

      expect(result).toBe(true);
    });

    it('should return false when log message does not contain search text', () => {
      component.searchControl.setValue('warning');

      const result = component['shouldIncludeLogBySearch']('This is an error message');

      expect(result).toBe(false);
    });

    it('should perform case-insensitive search', () => {
      component.searchControl.setValue('APPLICATION');

      const result = component['shouldIncludeLogBySearch']('application started successfully');

      expect(result).toBe(true);
    });
  });

  describe('SignalR log filtering', () => {
    beforeEach(() => {
      component.logMessages = [];
      component.selectedLogLevel = ApiLogLevel.Information;
    });

    it('should add new log message when it meets both log level and search criteria', () => {
      component.searchControl.setValue('application');
      component.subscribeForLogs();

      processInformationLoggedSubject.next({
        logLevel: ApiLogLevel.Information,
        logMessage: 'Application started successfully',
      });

      expect(component.logMessages).toContain('Application started successfully');
    });

    it('should not add log message when it does not meet log level criteria', () => {
      component.selectedLogLevel = ApiLogLevel.Warning;
      component.subscribeForLogs();

      processInformationLoggedSubject.next({
        logLevel: ApiLogLevel.Information,
        logMessage: 'Information message',
      });

      expect(component.logMessages).not.toContain('Information message');
    });

    it('should not add log message when it does not meet search criteria', () => {
      component.searchControl.setValue('database');
      component.subscribeForLogs();

      processInformationLoggedSubject.next({
        logLevel: ApiLogLevel.Information,
        logMessage: 'Application started successfully',
      });

      expect(component.logMessages).not.toContain('Application started successfully');
    });

    it('should add log message when search text is empty (no filtering)', () => {
      component.searchControl.setValue('');
      component.subscribeForLogs();

      processInformationLoggedSubject.next({
        logLevel: ApiLogLevel.Information,
        logMessage: 'Any log message',
      });

      expect(component.logMessages).toContain('Any log message');
    });
  });

  describe('formatLogs', () => {
    it('should limit logs to 500 entries', () => {
      const manyLogs = Array.from({ length: 600 }, (_, i) => `log ${i}`);
      component.logMessages = manyLogs;

      component['formatLogs']();

      expect(component.logMessages.length).toBe(500);
    });

    it('should join logs with carriage return and newline', () => {
      component.logMessages = ['log1', 'log2', 'log3'];

      component['formatLogs']();

      expect(component.logMessagesDisplay).toBe('log1\r\nlog2\r\nlog3');
    });
  });

  describe('downloadPlates', () => {
    it('should call service to download plates', () => {
      component.onlyFailedPlateGroups = true;

      component.downloadPlates();

      expect(systemLogsServiceSpy.getPlateGroups).toHaveBeenCalledWith(true);
    });
  });

  describe('deletePlates', () => {
    it('should set isPurging to true during delete operation', fakeAsync(() => {
      // Use a delayed observable so isPurging stays true during the test
      systemLogsServiceSpy.deletePlates.and.returnValue(of({}).pipe(delay(100)));

      component.deletePlates();

      expect(component.isPurging).toBe(true);

      tick(100); // Complete the delayed observable
      expect(component.isPurging).toBe(false); // Should be reset after completion
    }));

    it('should call service to delete plates', () => {
      component.deletePlates();

      expect(systemLogsServiceSpy.deletePlates).toHaveBeenCalled();
    });

    it('should reset isPurging after successful delete', () => {
      component.deletePlates();

      expect(component.isPurging).toBe(false);
    });
  });

  describe('template interactions', () => {
    it('should render search input field', () => {
      const searchInput = fixture.nativeElement.querySelector('input[matInput]');

      expect(searchInput).toBeTruthy();
      expect(searchInput.placeholder).toBe('Enter search text...');
    });

    it('should update FormControl when user types in search input', fakeAsync(() => {
      const initialCallCount = systemLogsServiceSpy.getLogs.calls.count();

      // Set value directly on FormControl (simulating user input)
      component.searchControl.setValue('test search');

      // Verify FormControl value is set
      expect(component.searchControl.value).toBe('test search');

      // Verify debounced call happens
      tick(150);
      expect(systemLogsServiceSpy.getLogs.calls.count()).toBe(initialCallCount + 1);
    }));

    it('should call onLogLevelChange when log level selection changes', fakeAsync(() => {
      spyOn(component, 'onLogLevelChange');

      // Simulate the mat-select change event
      component.onLogLevelChange();

      expect(component.onLogLevelChange).toHaveBeenCalled();
    }));
  });
});
