import { TestBed } from '@angular/core/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { SignalrService } from './signalr.service';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { AccountService } from 'app/_services';
import * as signalR from '@microsoft/signalr';

describe('SignalrService', () => {
  let service: SignalrService;
  let mockSnackbarService: jasmine.SpyObj<SnackbarService>;
  let mockAccountService: jasmine.SpyObj<AccountService>;

  const mockUser = {
    id: 1,
    username: 'testuser',

    firstName: 'Test',
    lastName: 'User',
  };

  beforeEach(() => {
    const snackbarSpy = jasmine.createSpyObj('SnackbarService', ['create']);
    const accountSpy = jasmine.createSpyObj('AccountService', [], {
      userValue: mockUser,
    });

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        SignalrService,
        { provide: SnackbarService, useValue: snackbarSpy },
        { provide: AccountService, useValue: accountSpy },
      ],
    });

    service = TestBed.inject(SignalrService);
    mockSnackbarService = TestBed.inject(SnackbarService) as jasmine.SpyObj<SnackbarService>;
    mockAccountService = TestBed.inject(AccountService) as jasmine.SpyObj<AccountService>;
  });

  describe('Service Creation', () => {
    it('should be created', () => {
      expect(service).toBeTruthy();
    });

    it('should have initialized subjects', () => {
      expect(service.connectionEstablished).toBeDefined();
      expect(service.licensePlateReceived).toBeDefined();
      expect(service.licensePlateAlerted).toBeDefined();
      expect(service.processInformationLogged).toBeDefined();
      expect(service.openAlprAgentConnectionStatusChanged).toBeDefined();
      expect(service.connectionStatusChanged).toBeDefined();
    });

    it('should initialize isConnected as undefined', () => {
      expect(service.isConnected).toBeUndefined();
    });
  });

  describe('Authentication Checks', () => {
    it('should not start connection when user is null', () => {
      Object.defineProperty(mockAccountService, 'userValue', {
        get: () => null,
      });

      service.startConnection();

      expect(service['hubConnection']).toBeUndefined();
    });

    it('should not start connection when user has no id', () => {
      Object.defineProperty(mockAccountService, 'userValue', {
        get: () => ({ ...mockUser, id: null }),
      });

      service.startConnection();

      expect(service['hubConnection']).toBeUndefined();
    });

    it('should not start connection when user has empty id', () => {
      Object.defineProperty(mockAccountService, 'userValue', {
        get: () => ({ ...mockUser, id: '' }),
      });

      service.startConnection();

      expect(service['hubConnection']).toBeUndefined();
    });
  });

  describe('Connection State Management', () => {
    it('should not start when already connected', () => {
      const mockHubConnection = {
        state: signalR.HubConnectionState.Connected,
        start: jasmine.createSpy('start'),
        stop: jasmine.createSpy('stop'),
      } as any;

      service['hubConnection'] = mockHubConnection;
      spyOn(console, 'log');

      service.startConnection();

      expect(mockHubConnection.start).not.toHaveBeenCalled();
    });

    it('should not start when already connecting', () => {
      const mockHubConnection = {
        state: signalR.HubConnectionState.Connecting,
        start: jasmine.createSpy('start'),
        stop: jasmine.createSpy('stop'),
      } as any;

      service['hubConnection'] = mockHubConnection;
      spyOn(console, 'log');

      service.startConnection();

      expect(mockHubConnection.start).not.toHaveBeenCalled();
    });
  });

  describe('stopConnection', () => {
    it('should do nothing when hub connection is null', () => {
      service['hubConnection'] = null as any;

      service.stopConnection();

      expect(mockSnackbarService.create).not.toHaveBeenCalled();
    });

    it('should do nothing when hub connection is undefined', () => {
      service['hubConnection'] = undefined as any;

      service.stopConnection();

      expect(mockSnackbarService.create).not.toHaveBeenCalled();
    });

    it('should call stop on hub connection when it exists', () => {
      const mockHubConnection = {
        stop: jasmine.createSpy('stop').and.returnValue(Promise.resolve()),
      } as any;

      service['hubConnection'] = mockHubConnection;

      service.stopConnection();

      expect(mockHubConnection.stop).toHaveBeenCalled();
    });
  });

  describe('triggerConnectionStatusChange', () => {
    it('should update isConnected property', () => {
      service.triggerConnectionStatusChange(true);
      expect(service.isConnected).toBe(true);

      service.triggerConnectionStatusChange(false);
      expect(service.isConnected).toBe(false);
    });

    it('should emit connection status through subject', () => {
      spyOn(service.connectionStatusChanged, 'next');

      service.triggerConnectionStatusChange(true);
      expect(service.connectionStatusChanged.next).toHaveBeenCalledWith(true);

      service.triggerConnectionStatusChange(false);
      expect(service.connectionStatusChanged.next).toHaveBeenCalledWith(false);
    });

    it('should handle parameter name typo (isConencted)', () => {
      // Test that the method works despite the typo in parameter name
      service.triggerConnectionStatusChange(true);
      expect(service.isConnected).toBe(true);
    });
  });

  describe('Subject Integrity', () => {
    it('should maintain separate subject instances', () => {
      expect(service.connectionEstablished).not.toBe(service.licensePlateReceived);
      expect(service.licensePlateReceived).not.toBe(service.licensePlateAlerted);
      expect(service.licensePlateAlerted).not.toBe(service.processInformationLogged);
      expect(service.processInformationLogged).not.toBe(service.openAlprAgentConnectionStatusChanged);
      expect(service.openAlprAgentConnectionStatusChanged).not.toBe(service.connectionStatusChanged);
    });

    it('should allow multiple subscribers to subjects', () => {
      const subscriber1 = jasmine.createSpy('subscriber1');
      const subscriber2 = jasmine.createSpy('subscriber2');

      service.connectionEstablished.subscribe(subscriber1);
      service.connectionEstablished.subscribe(subscriber2);

      service.connectionEstablished.next(true);

      expect(subscriber1).toHaveBeenCalledWith(true);
      expect(subscriber2).toHaveBeenCalledWith(true);
    });
  });

  describe('Service Dependencies', () => {
    it('should have snackbar service injected', () => {
      expect(service['snackbarService']).toBeDefined();
    });

    it('should have account service injected', () => {
      expect(service['accountService']).toBeDefined();
    });
  });

  describe('Public Properties', () => {
    it('should expose all required public subjects', () => {
      expect(service.connectionEstablished).toBeInstanceOf(Object);
      expect(service.licensePlateReceived).toBeInstanceOf(Object);
      expect(service.licensePlateAlerted).toBeInstanceOf(Object);
      expect(service.processInformationLogged).toBeInstanceOf(Object);
      expect(service.openAlprAgentConnectionStatusChanged).toBeInstanceOf(Object);
      expect(service.connectionStatusChanged).toBeInstanceOf(Object);
    });

    it('should have isConnected property accessible', () => {
      // Property should be accessible even if initially undefined
      expect(service.isConnected).toBeUndefined(); // Initially undefined

      // Should be able to set the property value
      service.triggerConnectionStatusChange(true);
      expect(service.isConnected).toBe(true);

      service.triggerConnectionStatusChange(false);
      expect(service.isConnected).toBe(false);
    });
  });

  describe('Method Existence', () => {
    it('should have startConnection method', () => {
      expect(typeof service.startConnection).toBe('function');
    });

    it('should have stopConnection method', () => {
      expect(typeof service.stopConnection).toBe('function');
    });

    it('should have triggerConnectionStatusChange method', () => {
      expect(typeof service.triggerConnectionStatusChange).toBe('function');
    });
  });
});
