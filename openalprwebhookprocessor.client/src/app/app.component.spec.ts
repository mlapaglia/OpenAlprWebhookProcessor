import { TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { BreakpointObserver } from '@angular/cdk/layout';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SwUpdate } from '@angular/service-worker';
import { Router } from '@angular/router';
import { of, Subject } from 'rxjs';
import { AppComponent } from './app.component';
import { AccountService } from './_services';
import { SignalrService } from './signalr/signalr.service';
import { PushSubscriberService } from './_services/push-subscriber.service';
import type { User } from './_models';

describe('AppComponent', () => {
  let component: AppComponent;
  let mockAccountService: jasmine.SpyObj<AccountService>;
  let mockSignalrService: jasmine.SpyObj<SignalrService>;
  let mockSwUpdate: jasmine.SpyObj<SwUpdate>;
  let mockPushSubscriberService: jasmine.SpyObj<PushSubscriberService>;
  let mockSnackBar: jasmine.SpyObj<MatSnackBar>;
  let mockBreakpointObserver: jasmine.SpyObj<BreakpointObserver>;
  let mockRouter: jasmine.SpyObj<Router>;
  let userSubject: Subject<User>;
  let connectionStatusSubject: Subject<boolean>;
  let breakpointSubject: Subject<any>;
  let versionUpdatesSubject: Subject<any>;
  let unrecoverableSubject: Subject<any>;
  let routerEventsSubject: Subject<any>;

  beforeEach(async () => {
    userSubject = new Subject<User>();
    connectionStatusSubject = new Subject<boolean>();
    breakpointSubject = new Subject<any>();
    versionUpdatesSubject = new Subject<any>();
    unrecoverableSubject = new Subject<any>();
    routerEventsSubject = new Subject<any>();

    mockAccountService = jasmine.createSpyObj('AccountService', ['logout'], {
      user: userSubject.asObservable(),
    });

    mockSignalrService = jasmine.createSpyObj('SignalrService', ['startConnection', 'stopConnection'], {
      connectionStatusChanged: connectionStatusSubject.asObservable(),
    });

    mockSwUpdate = jasmine.createSpyObj('SwUpdate', [], {
      isEnabled: true,
      versionUpdates: versionUpdatesSubject.asObservable(),
      unrecoverable: unrecoverableSubject.asObservable(),
    });

    mockPushSubscriberService = jasmine.createSpyObj('PushSubscriberService', ['subscribe']);
    mockSnackBar = jasmine.createSpyObj('MatSnackBar', ['open']);
    mockBreakpointObserver = jasmine.createSpyObj('BreakpointObserver', ['observe']);
    mockBreakpointObserver.observe.and.returnValue(breakpointSubject.asObservable());

    mockRouter = jasmine.createSpyObj('Router', [], {
      url: '/', // Default to home route
      events: routerEventsSubject.asObservable(),
    });

    const mockSnackBarRef = {
      onAction: jasmine.createSpy('onAction').and.returnValue(of({})),
    };
    mockSnackBar.open.and.returnValue(mockSnackBarRef as any);

    await TestBed.configureTestingModule({
      imports: [AppComponent, NoopAnimationsModule],
      providers: [
        { provide: AccountService, useValue: mockAccountService },
        { provide: SignalrService, useValue: mockSignalrService },
        { provide: SwUpdate, useValue: mockSwUpdate },
        { provide: PushSubscriberService, useValue: mockPushSubscriberService },
        { provide: MatSnackBar, useValue: mockSnackBar },
        { provide: BreakpointObserver, useValue: mockBreakpointObserver },
        { provide: Router, useValue: mockRouter },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(AppComponent);
    component = fixture.componentInstance;
  });

  describe('constructor', () => {
    it('should initialize with default values', () => {
      expect(component.topBarVisible).toBe(false); // Should be false initially (no user authenticated)
      expect(component.isMobile).toBe(false);
      expect(component.sidenavOpened).toBe(true);
      expect(component.title).toBe('openalprwebhookprocessor.client');
    });

    it('should have settings navigation items', () => {
      expect(component.settingsNavItems).toBeDefined();
      expect(component.settingsNavItems.length).toBeGreaterThan(0);

      const expectedItems = [
        { linkTitle: 'Cameras', icon: 'videocam', link: '/settings/cameras' },
        { linkTitle: 'OpenALPR Agent', icon: 'api', link: '/settings/agent' },
        { linkTitle: 'Alerts', icon: 'notifications_active', link: '/settings/alerts' },
        { linkTitle: 'Ignores', icon: 'alarm_off', link: '/settings/ignores' },
        { linkTitle: 'Webhook Forwards', icon: 'forward_to_inbox', link: '/settings/forwards' },
        { linkTitle: 'Machine Learning', icon: 'psychology', link: '/settings/machine-learning' },
        { linkTitle: 'System Logs', icon: 'library_books', link: '/settings/logs' },
        { linkTitle: 'Enrichers', icon: 'merge_type', link: '/settings/enrichers' },
        { linkTitle: 'Users', icon: 'person', link: '/settings/users' },
        { linkTitle: 'Debug', icon: 'bug_report', link: '/settings/debug' },
      ];

      expect(component.settingsNavItems).toEqual(expectedItems);
    });
  });

  describe('user subscription', () => {
    it('should handle authenticated user', () => {
      const user: User = {
        id: 1,
        username: 'testuser',
        firstName: 'Test',
        lastName: 'User',
        password: '',
        twoFactorEnabled: false,
        isDeleting: false,
      };

      userSubject.next(user);

      expect(component.user).toEqual(user);
      expect(mockSignalrService.startConnection).toHaveBeenCalled();
    });

    it('should handle unauthenticated user', () => {
      const user: User = {
        id: 0,
        username: '',
        firstName: '',
        lastName: '',
        password: '',
        isDeleting: false,
        twoFactorEnabled: false,
      };

      userSubject.next(user);

      expect(component.user).toEqual(user);
      expect(mockSignalrService.stopConnection).toHaveBeenCalled();
    });

    it('should handle user without id', () => {
      const user: User = {
        id: undefined,
        username: 'testuser',
        firstName: 'Test',
        lastName: 'User',
        password: '',
        isDeleting: false,
      } as any;

      userSubject.next(user);

      expect(component.topBarVisible).toBe(false); // Should be false when user has no id (not authenticated)
      expect(mockSignalrService.stopConnection).toHaveBeenCalled(); // Should stop connection without id
    });
  });

  describe('mobile detection', () => {
    it('should set mobile state and close sidenav on mobile', () => {
      component.ngOnInit(); // Initialize the breakpoint observer

      breakpointSubject.next({ matches: true });

      expect(component.isMobile).toBe(true);
      expect(component.sidenavOpened).toBe(false);
    });

    it('should set desktop state and open sidenav on desktop', () => {
      component.ngOnInit(); // Initialize the breakpoint observer

      // First set to mobile
      component.isMobile = true;
      component.sidenavOpened = false;

      breakpointSubject.next({ matches: false });

      expect(component.isMobile).toBe(false);
      expect(component.sidenavOpened).toBe(true);
    });
  });



  describe('ngOnInit', () => {
    it('should subscribe for updates and push notifications', () => {
      component.ngOnInit();

      expect(mockPushSubscriberService.subscribe).toHaveBeenCalled();
    });

    it('should handle connection status changes', () => {
      component.ngOnInit();

      connectionStatusSubject.next(true);
      expect(component.isSignalrConnected).toBe(true);

      connectionStatusSubject.next(false);
      expect(component.isSignalrConnected).toBe(false);
    });
  });

  describe('ngOnDestroy', () => {
    it('should stop SignalR connection', () => {
      component.ngOnDestroy();

      expect(mockSignalrService.stopConnection).toHaveBeenCalled();
    });
  });

  describe('logout', () => {
    it('should call account service logout', () => {
      component.logout();

      expect(mockAccountService.logout).toHaveBeenCalled();
    });
  });

  describe('subscribeForUpdates', () => {
    it('should subscribe to connection status changes', () => {
      component.subscribeForUpdates();

      connectionStatusSubject.next(true);
      expect(component.isSignalrConnected).toBe(true);
    });
  });

  describe('toggleSidenav', () => {
    it('should toggle sidenav state', () => {
      const initialState = component.sidenavOpened;

      component.toggleSidenav();
      expect(component.sidenavOpened).toBe(!initialState);

      component.toggleSidenav();
      expect(component.sidenavOpened).toBe(initialState);
    });
  });

  describe('closeSidenavOnMobile', () => {
    it('should close sidenav when on mobile', () => {
      component.isMobile = true;
      component.sidenavOpened = true;

      component.closeSidenavOnMobile();

      expect(component.sidenavOpened).toBe(false);
    });

    it('should not close sidenav when on desktop', () => {
      component.isMobile = false;
      component.sidenavOpened = true;

      component.closeSidenavOnMobile();

      expect(component.sidenavOpened).toBe(true);
    });
  });

  describe('navigation items configuration', () => {
    it('should have all required navigation properties', () => {
      component.settingsNavItems.forEach(item => {
        expect(item.linkTitle).toBeDefined();
        expect(typeof item.linkTitle).toBe('string');
        expect(item.linkTitle.length).toBeGreaterThan(0);

        expect(item.icon).toBeDefined();
        expect(typeof item.icon).toBe('string');
        expect(item.icon.length).toBeGreaterThan(0);

        expect(item.link).toBeDefined();
        expect(typeof item.link).toBe('string');
        expect(item.link.startsWith('/settings/')).toBe(true);
      });
    });
  });
});
