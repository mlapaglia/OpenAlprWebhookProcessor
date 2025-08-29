import { TestBed, type ComponentFixture, fakeAsync, tick } from '@angular/core/testing';
import type { MatSlideToggleChange } from '@angular/material/slide-toggle';
import { of, throwError } from 'rxjs';
import { PushoverComponent } from './pushover.component';
import { PushoverService } from './pushover.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import type { Pushover } from './pushover';

describe('PushoverComponent', () => {
  let component: PushoverComponent;
  let fixture: ComponentFixture<PushoverComponent>;
  let mockPushoverService: jasmine.SpyObj<PushoverService>;
  let mockSnackbarService: jasmine.SpyObj<SnackbarService>;

  beforeEach(async () => {
    mockPushoverService = jasmine.createSpyObj('PushoverService', ['getPushover', 'upsertPushover', 'testPushover']);
    mockSnackbarService = jasmine.createSpyObj('SnackbarService', ['create']);

    await TestBed.configureTestingModule({
      imports: [PushoverComponent],
      providers: [
        { provide: PushoverService, useValue: mockPushoverService },
        { provide: SnackbarService, useValue: mockSnackbarService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PushoverComponent);
    component = fixture.componentInstance;
  });

  describe('component initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.client).toBeUndefined();
      expect(component.isSaving).toBeUndefined();
      expect(component.isTesting).toBeUndefined();
    });

    it('should load pushover client on init', () => {
      const mockPushover: Pushover = {
        apiToken: 'app-token',
        userKey: 'user-key',
        isEnabled: true,
        sendPlatePreviewEnabled: true,
        sendEveryPlateEnabled: false,
      };

      mockPushoverService.getPushover.and.returnValue(of(mockPushover));

      component.ngOnInit();

      expect(mockPushoverService.getPushover).toHaveBeenCalled();
      expect(component.client).toEqual(mockPushover);
    });

    it('should handle error when loading pushover client', () => {
      mockPushoverService.getPushover.and.returnValue(throwError(() => new Error('Load failed')));

      component.ngOnInit();

      expect(mockPushoverService.getPushover).toHaveBeenCalled();
      expect(component.client).toBeUndefined();
    });
  });

  describe('saveClient', () => {
    beforeEach(() => {
      component.client = {
        apiToken: 'app-token',
        userKey: 'user-key',
        isEnabled: true,
        sendPlatePreviewEnabled: true,
        sendEveryPlateEnabled: false,
      };
    });

    it('should save client successfully', fakeAsync(() => {
      mockPushoverService.upsertPushover.and.returnValue(of(null));

      component.saveClient();

      tick();

      expect(mockPushoverService.upsertPushover).toHaveBeenCalledWith(component.client);
      expect(mockSnackbarService.create).toHaveBeenCalledWith('Pushover client saved.', SnackBarType.Saved);
    }));

    it('should handle save error', fakeAsync(() => {
      const errorMessage = 'Save failed';
      mockPushoverService.upsertPushover.and.returnValue(throwError(() => errorMessage));

      component.saveClient();

      tick();

      expect(mockPushoverService.upsertPushover).toHaveBeenCalledWith(component.client);
      expect(mockSnackbarService.create).toHaveBeenCalledWith('Pushover client save failed.', SnackBarType.Error, errorMessage);
    }));
  });

  describe('testClient', () => {
    it('should test client successfully', fakeAsync(() => {
      mockPushoverService.testPushover.and.returnValue(of(null));

      component.testClient();

      tick();

      expect(mockPushoverService.testPushover).toHaveBeenCalled();
      expect(mockSnackbarService.create).toHaveBeenCalledWith('Pushover client test successful.', SnackBarType.Successful);
    }));

    it('should handle test error', fakeAsync(() => {
      mockPushoverService.testPushover.and.returnValue(throwError(() => new Error('Test failed')));

      component.testClient();

      tick();

      expect(mockPushoverService.testPushover).toHaveBeenCalled();
      expect(mockSnackbarService.create).toHaveBeenCalledWith('Pushover client test failed.', SnackBarType.Error);
    }));
  });

  describe('onPushoverToggle', () => {
    beforeEach(() => {
      component.client = {
        apiToken: 'app-token',
        userKey: 'user-key',
        isEnabled: true,
        sendPlatePreviewEnabled: true,
        sendEveryPlateEnabled: false,
      };
    });

    it('should handle toggle off', fakeAsync(() => {
      const mockToggleEvent = {
        checked: false,
        source: {} as any,
      } as MatSlideToggleChange;

      mockPushoverService.upsertPushover.and.returnValue(of(null));

      component.onPushoverToggle(mockToggleEvent);

      expect(component.client.isEnabled).toBe(false);

      tick();

      expect(mockPushoverService.upsertPushover).toHaveBeenCalledWith(component.client);
    }));

    it('should not save when toggle is on', () => {
      const mockToggleEvent = {
        checked: true,
        source: {} as any,
      } as MatSlideToggleChange;

      component.onPushoverToggle(mockToggleEvent);

      expect(mockPushoverService.upsertPushover).not.toHaveBeenCalled();
    });

    it('should handle toggle off with error', fakeAsync(() => {
      const mockToggleEvent = {
        checked: false,
        source: {} as any,
      } as MatSlideToggleChange;

      mockPushoverService.upsertPushover.and.returnValue(throwError(() => new Error('Toggle failed')));

      component.onPushoverToggle(mockToggleEvent);

      expect(component.client.isEnabled).toBe(false);

      tick();

      expect(mockPushoverService.upsertPushover).toHaveBeenCalledWith(component.client);
    }));
  });

  describe('ngOnDestroy', () => {
    it('should call parent ngOnDestroy', () => {
      spyOn(component, 'ngOnDestroy').and.callThrough();

      component.ngOnDestroy();

      expect(component.ngOnDestroy).toHaveBeenCalled();
    });
  });
});
