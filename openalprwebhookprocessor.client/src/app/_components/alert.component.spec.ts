import { TestBed, fakeAsync, tick, type ComponentFixture } from '@angular/core/testing';
import { Router, NavigationStart } from '@angular/router';
import { Subject } from 'rxjs';
import { AlertComponent } from './alert.component';
import { AlertService } from 'app/_services';
import { Alert, AlertType } from 'app/_models';

describe('AlertComponent', () => {
  let component: AlertComponent;
  let fixture: ComponentFixture<AlertComponent>;
  let mockAlertService: jasmine.SpyObj<AlertService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let alertSubject: Subject<Alert>;
  let routerEventsSubject: Subject<any>;

  beforeEach(async () => {
    alertSubject = new Subject<Alert>();
    routerEventsSubject = new Subject<any>();

    mockAlertService = jasmine.createSpyObj('AlertService', ['onAlert', 'clear']);
    mockRouter = jasmine.createSpyObj('Router', [], { events: routerEventsSubject.asObservable() });

    mockAlertService.onAlert.and.returnValue(alertSubject.asObservable());

    await TestBed.configureTestingModule({
      imports: [AlertComponent],
      providers: [
        { provide: AlertService, useValue: mockAlertService },
        { provide: Router, useValue: mockRouter },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AlertComponent);
    component = fixture.componentInstance;
  });

  describe('component initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.id()).toBe('default-alert');
      expect(component.fade()).toBe(true);
      expect(component.alerts).toEqual([]);
    });

    it('should accept input properties', () => {
      fixture.componentRef.setInput('id', 'custom-alert');
      fixture.componentRef.setInput('fade', false);

      expect(component.id()).toBe('custom-alert');
      expect(component.fade()).toBe(false);
    });

    it('should subscribe to alert service on init', () => {
      component.ngOnInit();

      expect(mockAlertService.onAlert).toHaveBeenCalledWith(component.id());
    });
  });

  describe('alert handling', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should clear alerts when empty alert message is received', () => {
      const alert1 = new Alert({ id: 'test', message: 'Test 1', type: AlertType.Info });
      const alert2 = new Alert({ id: 'test', message: 'Test 2', type: AlertType.Success, keepAfterRouteChange: true });

      component.alerts = [alert1, alert2];

      alertSubject.next(new Alert({ id: 'test', message: '' }));

      expect(component.alerts.length).toBe(1);
      expect(component.alerts[0].message).toBe('Test 2');
      expect(component.alerts[0].keepAfterRouteChange).toBeUndefined();
    });

    it('should add alert with CSS class when message is provided', () => {
      const alert = new Alert({ id: 'test', message: 'Test message', type: AlertType.Success });

      alertSubject.next(alert);

      expect(component.alerts.length).toBe(1);
      expect(component.alerts[0].message).toBe('Test message');
      expect(component.alerts[0].cssClass).toBeDefined();
    });

    it('should auto-close alert when autoClose is true', fakeAsync(() => {
      spyOn(component, 'removeAlert');
      const alert = new Alert({ id: 'test', message: 'Auto close', type: AlertType.Info, autoClose: true });

      alertSubject.next(alert);
      expect(component.alerts.length).toBe(1);

      tick(3000);
      expect(component.removeAlert).toHaveBeenCalledWith(alert);
    }));

    it('should not auto-close alert when autoClose is false', fakeAsync(() => {
      const alert = new Alert({ id: 'test', message: 'No auto close', type: AlertType.Info, autoClose: false });

      alertSubject.next(alert);
      expect(component.alerts.length).toBe(1);

      tick(3000);
      expect(component.alerts.length).toBe(1);
    }));
  });

  describe('navigation handling', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should clear alerts on navigation start', () => {
      const navigationEvent = new NavigationStart(1, '/test');

      routerEventsSubject.next(navigationEvent);

      expect(mockAlertService.clear).toHaveBeenCalledWith(component.id());
    });

    it('should not clear alerts on non-navigation events', () => {
      const otherEvent = { id: 1 };

      routerEventsSubject.next(otherEvent);

      expect(mockAlertService.clear).not.toHaveBeenCalled();
    });
  });

  describe('removeAlert', () => {
    const testAlert = new Alert({ id: 'test', message: 'Test', type: AlertType.Info });

    beforeEach(() => {
      component.alerts = [testAlert];
    });

    it('should remove alert immediately when fade is disabled', () => {
      fixture.componentRef.setInput('fade', false);

      component.removeAlert(testAlert);

      expect(component.alerts).toEqual([]);
    });

    it('should fade out alert when fade is enabled', fakeAsync(() => {
      fixture.componentRef.setInput('fade', true);

      component.removeAlert(testAlert);

      expect(testAlert.fade).toBe(true);
      expect(component.alerts.length).toBe(1);

      tick(250);
      expect(component.alerts).toEqual([]);
    }));

    it('should handle removing non-existent alert gracefully', () => {
      const nonExistentAlert = new Alert({ id: 'other', message: 'Other', type: AlertType.Error });

      expect(() => component.removeAlert(nonExistentAlert)).not.toThrow();
      expect(component.alerts).toEqual([testAlert]);
    });
  });

  describe('cssClass', () => {
    it('should return undefined for null/undefined alert', () => {
      expect(component.cssClass(null as any)).toBeUndefined();
      expect(component.cssClass(undefined as any)).toBeUndefined();
    });

    it('should generate correct CSS classes for success alert', () => {
      const alert = new Alert({ type: AlertType.Success, message: 'Success' });

      const cssClass = component.cssClass(alert);

      expect(cssClass).toContain('alert');
      expect(cssClass).toContain('alert-dismissable');
      expect(cssClass).toContain('mt-4');
      expect(cssClass).toContain('container');
      expect(cssClass).toContain('alert-success');
    });

    it('should generate correct CSS classes for error alert', () => {
      const alert = new Alert({ type: AlertType.Error, message: 'Error' });

      const cssClass = component.cssClass(alert);

      expect(cssClass).toContain('alert-danger');
    });

    it('should generate correct CSS classes for info alert', () => {
      const alert = new Alert({ type: AlertType.Info, message: 'Info' });

      const cssClass = component.cssClass(alert);

      expect(cssClass).toContain('alert-info');
    });

    it('should generate correct CSS classes for warning alert', () => {
      const alert = new Alert({ type: AlertType.Warning, message: 'Warning' });

      const cssClass = component.cssClass(alert);

      expect(cssClass).toContain('alert-warning');
    });

    it('should include fade class when alert has fade property', () => {
      const alert = new Alert({ type: AlertType.Success, message: 'Success', fade: true });

      const cssClass = component.cssClass(alert);

      expect(cssClass).toContain('fade');
    });

    it('should not include fade class when alert does not have fade property', () => {
      const alert = new Alert({ type: AlertType.Success, message: 'Success', fade: false });

      const cssClass = component.cssClass(alert);

      expect(cssClass).not.toContain('fade');
    });
  });

  describe('component cleanup', () => {
    it('should handle destroy when subscriptions are undefined', () => {
      const testComponent = TestBed.createComponent(AlertComponent).componentInstance;
      expect(() => testComponent.ngOnDestroy()).not.toThrow();
    });

    it('should call base class ngOnDestroy', () => {
      spyOn(Object.getPrototypeOf(Object.getPrototypeOf(component)), 'ngOnDestroy');

      component.ngOnDestroy();

      expect(Object.getPrototypeOf(Object.getPrototypeOf(component)).ngOnDestroy).toHaveBeenCalled();
    });
  });
});
