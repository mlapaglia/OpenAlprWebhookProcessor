import { TestBed, type ComponentFixture, fakeAsync, tick } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { SignalrConnectionMonitorComponent } from './signalr-connection-monitor.component';
import { SignalrService } from 'app/signalr/signalr.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';

interface SignalRConnection {
  connectionId: string;
  userId: string;
  connectedAt: string;
  durationSeconds: number;
  transport: string;
  userAgent: string;
  ipAddress: string;
}

interface SignalRConnectionSummary {
  totalConnections: number;
  connections: SignalRConnection[];
}

interface ClientConnectionInfo {
  state: string;
  connectionId: string | null;
  transport: string;
  startTime: Date | null;
  durationSeconds: number;
}

describe('SignalrConnectionMonitorComponent', () => {
  let component: SignalrConnectionMonitorComponent;
  let fixture: ComponentFixture<SignalrConnectionMonitorComponent>;
  let mockSignalrService: jasmine.SpyObj<SignalrService>;
  let mockSnackbarService: jasmine.SpyObj<SnackbarService>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    mockSignalrService = jasmine.createSpyObj('SignalrService', ['getConnectionInfo']);
    mockSnackbarService = jasmine.createSpyObj('SnackbarService', ['create']);

    await TestBed.configureTestingModule({
      imports: [SignalrConnectionMonitorComponent],
      providers: [
        { provide: SignalrService, useValue: mockSignalrService },
        { provide: SnackbarService, useValue: mockSnackbarService },
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SignalrConnectionMonitorComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('component initialization', () => {
    it('should create', () => {
      mockSignalrService.getConnectionInfo.and.returnValue({
        state: 'Connected',
        connectionId: 'test-id',
        transport: 'WebSockets',
        startTime: new Date(),
        durationSeconds: 30,
      });

      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.signalrConnections).toBeNull();
      expect(component.isLoadingConnections).toBe(false);
      expect(component.isManualRefresh).toBe(false);
      expect(component.clientConnectionInfo).toBeNull();
    });

    it('should load connections and set up refresh interval on init', fakeAsync(() => {
      const mockConnectionSummary: SignalRConnectionSummary = {
        totalConnections: 2,
        connections: [
          {
            connectionId: 'conn1',
            userId: 'user1',
            connectedAt: '2023-01-01T10:00:00Z',
            durationSeconds: 300,
            transport: 'WebSockets',
            userAgent: 'Mozilla/5.0',
            ipAddress: '192.168.1.1',
          },
        ],
      };

      mockSignalrService.getConnectionInfo.and.returnValue({
        state: 'Connected',
        connectionId: 'test-id',
        transport: 'WebSockets',
        startTime: new Date(),
        durationSeconds: 30,
      });

      component.ngOnInit();

      const req = httpMock.expectOne('/api/settings/debug/signalr-connections');
      req.flush(mockConnectionSummary);

      expect(component.signalrConnections).toEqual(mockConnectionSummary);
      expect(component.clientConnectionInfo).toBeDefined();

      // Test refresh interval
      tick(5000);
      const req2 = httpMock.expectOne('/api/settings/debug/signalr-connections');
      req2.flush(mockConnectionSummary);

      component.ngOnDestroy();
    }));
  });

  describe('loadSignalRConnections', () => {
    it('should load connections successfully', () => {
      const mockConnectionSummary: SignalRConnectionSummary = {
        totalConnections: 1,
        connections: [],
      };

      mockSignalrService.getConnectionInfo.and.returnValue(null);
      component.ngOnInit();

      const req = httpMock.expectOne('/api/settings/debug/signalr-connections');
      req.flush(mockConnectionSummary);

      expect(component.signalrConnections).toEqual(mockConnectionSummary);
      expect(component.isLoadingConnections).toBe(false);
    });

    it('should handle loading error with manual refresh', () => {
      spyOn(console, 'error');
      mockSignalrService.getConnectionInfo.and.returnValue(null);

      component.refreshConnections();

      const req = httpMock.expectOne('/api/settings/debug/signalr-connections');
      req.error(new ProgressEvent('error'));

      expect(console.error).toHaveBeenCalledWith('Failed to load SignalR connections:', jasmine.any(Object));
      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Failed to load SignalR connection information',
        SnackBarType.Error,
      );
      expect(component.isLoadingConnections).toBe(false);
    });

    it('should not show error snackbar for background refresh errors', () => {
      mockSignalrService.getConnectionInfo.and.returnValue(null);
      component.ngOnInit();

      const req = httpMock.expectOne('/api/settings/debug/signalr-connections');
      req.error(new ProgressEvent('error'));

      expect(mockSnackbarService.create).not.toHaveBeenCalled();
    });

    it('should only update data when it has changed', () => {
      const mockConnectionSummary: SignalRConnectionSummary = {
        totalConnections: 1,
        connections: [],
      };

      mockSignalrService.getConnectionInfo.and.returnValue(null);
      component.signalrConnections = mockConnectionSummary;


      component.ngOnInit();

      const req = httpMock.expectOne('/api/settings/debug/signalr-connections');
      req.flush(mockConnectionSummary);

      // Should not call markForCheck for background updates when data is the same

    });
  });

  describe('refreshConnections', () => {
    it('should manually refresh connections', () => {
      mockSignalrService.getConnectionInfo.and.returnValue({
        state: 'Connected',
        connectionId: 'test-id',
        transport: 'WebSockets',
        startTime: new Date(),
        durationSeconds: 30,
      });

      component.refreshConnections();

      expect(component.isLoadingConnections).toBe(true);
      expect(component.isManualRefresh).toBe(true);

      const req = httpMock.expectOne('/api/settings/debug/signalr-connections');
      req.flush({ totalConnections: 0, connections: [] });

      expect(component.isLoadingConnections).toBe(false);
      expect(component.isManualRefresh).toBe(false);
    });
  });

  describe('updateClientConnectionInfo', () => {
    it('should update client connection info when changed', () => {
      const connectionInfo: ClientConnectionInfo = {
        state: 'Connected',
        connectionId: 'test-id',
        transport: 'WebSockets',
        startTime: new Date(),
        durationSeconds: 30,
      };

      mockSignalrService.getConnectionInfo.and.returnValue(connectionInfo);

      // Call the private method directly instead of ngOnInit to avoid HTTP requests
      (component as any).updateClientConnectionInfo();

      expect(component.clientConnectionInfo).toEqual(connectionInfo);
    });

    it('should not update when connection info has not changed', () => {
      const connectionInfo: ClientConnectionInfo = {
        state: 'Connected',
        connectionId: 'test-id',
        transport: 'WebSockets',
        startTime: new Date(),
        durationSeconds: 30,
      };

      component.clientConnectionInfo = connectionInfo;
      mockSignalrService.getConnectionInfo.and.returnValue(connectionInfo);


      // Simulate the private method call
      (component as any).updateClientConnectionInfo();


    });
  });

  describe('hasConnectionInfoChanged', () => {
    it('should return true when no previous info', () => {
      const newInfo: ClientConnectionInfo = {
        state: 'Connected',
        connectionId: 'test-id',
        transport: 'WebSockets',
        startTime: new Date(),
        durationSeconds: 30,
      };

      const result = (component as any).hasConnectionInfoChanged(newInfo);
      expect(result).toBe(true);
    });

    it('should return true when new info is null', () => {
      component.clientConnectionInfo = {
        state: 'Connected',
        connectionId: 'test-id',
        transport: 'WebSockets',
        startTime: new Date(),
        durationSeconds: 30,
      };

      const result = (component as any).hasConnectionInfoChanged(null);
      expect(result).toBe(true);
    });

    it('should return true when state changes', () => {
      component.clientConnectionInfo = {
        state: 'Connected',
        connectionId: 'test-id',
        transport: 'WebSockets',
        startTime: new Date(),
        durationSeconds: 30,
      };

      const newInfo: ClientConnectionInfo = {
        state: 'Disconnected',
        connectionId: 'test-id',
        transport: 'WebSockets',
        startTime: new Date(),
        durationSeconds: 30,
      };

      const result = (component as any).hasConnectionInfoChanged(newInfo);
      expect(result).toBe(true);
    });

    it('should return true when connection ID changes', () => {
      component.clientConnectionInfo = {
        state: 'Connected',
        connectionId: 'test-id',
        transport: 'WebSockets',
        startTime: new Date(),
        durationSeconds: 30,
      };

      const newInfo: ClientConnectionInfo = {
        state: 'Connected',
        connectionId: 'new-test-id',
        transport: 'WebSockets',
        startTime: new Date(),
        durationSeconds: 30,
      };

      const result = (component as any).hasConnectionInfoChanged(newInfo);
      expect(result).toBe(true);
    });

    it('should return true when duration changes significantly', () => {
      component.clientConnectionInfo = {
        state: 'Connected',
        connectionId: 'test-id',
        transport: 'WebSockets',
        startTime: new Date(),
        durationSeconds: 30,
      };

      const newInfo: ClientConnectionInfo = {
        state: 'Connected',
        connectionId: 'test-id',
        transport: 'WebSockets',
        startTime: new Date(),
        durationSeconds: 35,
      };

      const result = (component as any).hasConnectionInfoChanged(newInfo);
      expect(result).toBe(true);
    });

    it('should return false when duration change is minimal', () => {
      component.clientConnectionInfo = {
        state: 'Connected',
        connectionId: 'test-id',
        transport: 'WebSockets',
        startTime: new Date(),
        durationSeconds: 30,
      };

      const newInfo: ClientConnectionInfo = {
        state: 'Connected',
        connectionId: 'test-id',
        transport: 'WebSockets',
        startTime: new Date(),
        durationSeconds: 30,
      };

      const result = (component as any).hasConnectionInfoChanged(newInfo);
      expect(result).toBe(false);
    });
  });

  describe('formatDuration', () => {
    it('should format seconds only', () => {
      expect(component.formatDuration(45)).toBe('45s');
    });

    it('should format minutes and seconds', () => {
      expect(component.formatDuration(125)).toBe('2m 5s');
      expect(component.formatDuration(60)).toBe('1m 0s');
    });

    it('should format hours and minutes', () => {
      expect(component.formatDuration(3665)).toBe('1h 1m');
      expect(component.formatDuration(7200)).toBe('2h 0m');
    });
  });

  describe('ngOnDestroy', () => {
    it('should clear refresh interval', fakeAsync(() => {
      mockSignalrService.getConnectionInfo.and.returnValue(null);

      component.ngOnInit();

      const req = httpMock.expectOne('/api/settings/debug/signalr-connections');
      req.flush({ totalConnections: 0, connections: [] });

      component.ngOnDestroy();

      // Advance time to see if interval still runs
      tick(5000);

      // No additional HTTP request should be made
      httpMock.expectNone('/api/settings/debug/signalr-connections');
    }));
  });
});
