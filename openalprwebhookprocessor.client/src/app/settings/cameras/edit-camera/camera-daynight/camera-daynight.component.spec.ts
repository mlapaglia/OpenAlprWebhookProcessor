import type { ComponentFixture } from '@angular/core/testing';
import { TestBed } from '@angular/core/testing';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';

import { CameraDayNightComponent } from './camera-daynight.component';
import { Camera, Manufacturer } from '../../camera';
import type { ZoomFocus } from '../zoomfocus';

describe('CameraDayNightComponent', () => {
  let component: CameraDayNightComponent;
  let fixture: ComponentFixture<CameraDayNightComponent>;

  const createMockCamera = (overrides: Partial<Camera> = {}): Camera => new Camera({
    id: '1',
    latitude: 40.7128,
    longitude: -74.0060,
    ipAddress: '192.168.1.100',
    manufacturer: Manufacturer.Hikvision,
    modelNumber: 'DS-2CD2043G0-I',
    openAlprName: 'Test Camera',
    openAlprCameraId: 1,
    cameraPassword: 'testpass',
    cameraUsername: 'admin',
    updateOverlayEnabled: true,
    updateOverlayTextUrl: 'http://example.com/overlay',
    nightZoom: '10',
    nightFocus: '20',
    dayZoom: '5',
    dayFocus: '15',
    dayNightModeEnabled: false,
    dayNightModeUrl: 'http://example.com/daynight',
    dayNightNextScheduledCommand: new Date(),
    openAlprEnabled: true,
    sunriseOffset: 30,
    sunsetOffset: -30,
    platesSeen: 100,
    sampleImageUrl: 'http://example.com/sample.jpg',
    timezoneOffset: -5,
    ...overrides,
  });

  const createMockZoomFocus = (overrides: Partial<ZoomFocus> = {}): ZoomFocus => ({
    zoom: 10,
    focus: 20,
    ...overrides,
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CameraDayNightComponent,
        ReactiveFormsModule,
      ],
      providers: [FormBuilder],
    }).compileComponents();

    fixture = TestBed.createComponent(CameraDayNightComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('camera', createMockCamera());
    fixture.componentRef.setInput('currentZoomFocus', createMockZoomFocus());
    fixture.detectChanges();
  });

  describe('component initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize form with default values', () => {
      // Create a fresh component without calling ngOnInit
      const freshFixture = TestBed.createComponent(CameraDayNightComponent);
      const freshComponent = freshFixture.componentInstance;

      expect(freshComponent.dayNightForm).toBeDefined();
      expect(freshComponent.dayNightForm.get('dayNightModeEnabled')?.value).toBe(false);
      expect(freshComponent.dayNightForm.get('latitude')?.value).toBe('');
      expect(freshComponent.dayNightForm.get('longitude')?.value).toBe('');
    });

    it('should have correct form controls', () => {
      const formControls = [
        'dayNightModeEnabled',
        'latitude',
        'longitude',
        'sunsetOffset',
        'sunriseOffset',
        'timezoneOffset',
        'dayNightModeUrl',
        'dayZoom',
        'dayFocus',
        'nightZoom',
        'nightFocus',
      ];

      formControls.forEach(control => {
        expect(component.dayNightForm.get(control)).toBeTruthy();
      });
    });
  });

  describe('input and output properties', () => {
    it('should accept camera input', () => {
      const mockCamera = createMockCamera({ dayNightModeEnabled: true });
      fixture.componentRef.setInput('camera', mockCamera);

      expect(component.camera()).toEqual(mockCamera);
    });

    it('should accept currentZoomFocus input', () => {
      const mockZoomFocus = createMockZoomFocus({ zoom: 15, focus: 25 });
      fixture.componentRef.setInput('currentZoomFocus', mockZoomFocus);

      expect(component.currentZoomFocus()).toEqual(mockZoomFocus);
    });

    it('should emit cameraChange when form values change', () => {
      spyOn(component.cameraChange, 'emit');
      component.ngOnInit();

      component.dayNightForm.patchValue({ latitude: 50.0 });

      expect(component.cameraChange.emit).toHaveBeenCalled();
    });

    it('should emit currentZoomFocusChange when zoom/focus changes', () => {
      spyOn(component.currentZoomFocusChange, 'emit');

      component.onCurrentZoomFocusChange('zoom', 15);

      expect(component.currentZoomFocusChange.emit).toHaveBeenCalledWith({
        zoom: 15,
        focus: 20,
      });
    });
  });

  describe('ngOnInit lifecycle', () => {
    it('should patch form with camera values', () => {
      const camera = createMockCamera({
        dayNightModeEnabled: true,
        latitude: 51.5074,
        longitude: -0.1278,
        sunsetOffset: 15,
        sunriseOffset: -15,
        timezoneOffset: 0,
        dayNightModeUrl: 'http://test.com/api',
        dayZoom: '8',
        dayFocus: '12',
        nightZoom: '16',
        nightFocus: '24',
      });
      fixture.componentRef.setInput('camera', camera);

      component.ngOnInit();

      expect(component.dayNightForm.value).toEqual({
        dayNightModeEnabled: true,
        latitude: 51.5074,
        longitude: -0.1278,
        sunsetOffset: 15,
        sunriseOffset: -15,
        timezoneOffset: 0,
        dayNightModeUrl: 'http://test.com/api',
        dayZoom: '8',
        dayFocus: '12',
        nightZoom: '16',
        nightFocus: '24',
      });
    });

    it('should subscribe to form value changes', () => {
      spyOn(component.cameraChange, 'emit');
      component.ngOnInit();

      const newValues = {
        latitude: 48.8566,
        longitude: 2.3522,
        dayNightModeEnabled: true,
      };

      component.dayNightForm.patchValue(newValues);

      expect(component.cameraChange.emit).toHaveBeenCalledWith(
        jasmine.objectContaining({
          latitude: 48.8566,
          longitude: 2.3522,
          dayNightModeEnabled: true,
        }),
      );
    });

    it('should preserve non-form camera properties', () => {
      const originalCamera = createMockCamera({
        id: 'preserve-me',
        manufacturer: Manufacturer.Dahua,
        platesSeen: 500,
      });
      fixture.componentRef.setInput('camera', originalCamera);

      component.ngOnInit();
      component.dayNightForm.patchValue({ latitude: 50.0 });

      expect(component.camera()?.id).toBe('preserve-me');
      expect(component.camera()?.manufacturer).toBe(Manufacturer.Dahua);
      expect(component.camera()?.platesSeen).toBe(500);
    });
  });

  describe('dynamic validation', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should add validators when dayNightModeEnabled is true', () => {
      const urlControl = component.dayNightForm.get('dayNightModeUrl');

      // Clear the field first
      urlControl?.setValue('');

      component.dayNightForm.patchValue({ dayNightModeEnabled: true });

      expect(urlControl?.hasError('required')).toBe(true);

      urlControl?.setValue('invalid-url');
      expect(urlControl?.hasError('pattern')).toBe(true);

      urlControl?.setValue('http://valid-url.com');
      expect(urlControl?.valid).toBe(true);
    });

    it('should remove validators when dayNightModeEnabled is false', () => {
      // First enable to set validators
      component.dayNightForm.patchValue({ dayNightModeEnabled: true });

      // Then disable to remove validators
      component.dayNightForm.patchValue({ dayNightModeEnabled: false });

      const urlControl = component.dayNightForm.get('dayNightModeUrl');
      expect(urlControl?.valid).toBe(true);
    });

    it('should validate URL pattern correctly', () => {
      component.dayNightForm.patchValue({ dayNightModeEnabled: true });
      const urlControl = component.dayNightForm.get('dayNightModeUrl');

      // Invalid URLs
      const invalidUrls = ['invalid', 'ftp://test.com', 'not-a-url'];
      invalidUrls.forEach(url => {
        urlControl?.setValue(url);
        expect(urlControl?.hasError('pattern')).withContext(`${url} should be invalid`).toBe(true);
      });

      // Valid URLs
      const validUrls = ['http://test.com', 'https://example.org/api'];
      validUrls.forEach(url => {
        urlControl?.setValue(url);
        expect(urlControl?.hasError('pattern')).withContext(`${url} should be valid`).toBe(false);
      });
    });
  });

  describe('event emitters', () => {
    it('should emit triggerDayMode when onTriggerDayMode is called', () => {
      spyOn(component.triggerDayMode, 'emit');

      component.onTriggerDayMode();

      expect(component.triggerDayMode.emit).toHaveBeenCalled();
    });

    it('should emit triggerNightMode when onTriggerNightMode is called', () => {
      spyOn(component.triggerNightMode, 'emit');

      component.onTriggerNightMode();

      expect(component.triggerNightMode.emit).toHaveBeenCalled();
    });

    it('should emit getZoomFocus when onGetZoomFocus is called', () => {
      spyOn(component.getZoomFocus, 'emit');

      component.onGetZoomFocus();

      expect(component.getZoomFocus.emit).toHaveBeenCalled();
    });

    it('should emit setZoomFocus when onSetZoomFocus is called', () => {
      spyOn(component.setZoomFocus, 'emit');

      component.onSetZoomFocus();

      expect(component.setZoomFocus.emit).toHaveBeenCalled();
    });

    it('should emit triggerAutofocus when onTriggerAutofocus is called', () => {
      spyOn(component.triggerAutofocus, 'emit');

      component.onTriggerAutofocus();

      expect(component.triggerAutofocus.emit).toHaveBeenCalled();
    });
  });

  describe('zoom and focus handling', () => {
    it('should handle zoom change from input event', () => {
      spyOn(component.currentZoomFocusChange, 'emit');
      const event = { target: { value: '25' } } as any;

      component.onZoomChange(event);

      expect(component.currentZoomFocusChange.emit).toHaveBeenCalledWith({
        zoom: 25,
        focus: 20,
      });
    });

    it('should handle focus change from input event', () => {
      spyOn(component.currentZoomFocusChange, 'emit');
      const event = { target: { value: '35' } } as any;

      component.onFocusChange(event);

      expect(component.currentZoomFocusChange.emit).toHaveBeenCalledWith({
        zoom: 10,
        focus: 35,
      });
    });

    it('should update currentZoomFocus correctly', () => {
      const initialZoomFocus = createMockZoomFocus({ zoom: 5, focus: 10 });
      fixture.componentRef.setInput('currentZoomFocus', initialZoomFocus);
      spyOn(component.currentZoomFocusChange, 'emit');

      component.onCurrentZoomFocusChange('zoom', 15);

      expect(component.currentZoomFocusChange.emit).toHaveBeenCalledWith({ zoom: 15, focus: 10 });

      component.onCurrentZoomFocusChange('focus', 25);

      expect(component.currentZoomFocusChange.emit).toHaveBeenCalledWith({ zoom: 5, focus: 25 });
    });
  });

  describe('ngOnChanges lifecycle', () => {
    it('should call updateCurrentZoomFocusDisplay', () => {
      spyOn(component as any, 'updateCurrentZoomFocusDisplay');

      component.ngOnChanges();

      expect((component as any).updateCurrentZoomFocusDisplay).toHaveBeenCalled();
    });
  });

  describe('template rendering', () => {
    beforeEach(() => {
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should render card title and subtitle', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const title = compiled.querySelector('mat-card-title span');
      const subtitle = compiled.querySelector('mat-card-subtitle');

      expect(title?.textContent?.trim()).toBe('Day/Night Toggle');
      expect(subtitle?.textContent?.trim()).toBe('Configure automatic day/night mode switching and camera focus settings');
    });

    it('should render day/night mode toggle', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const toggle = compiled.querySelector('mat-slide-toggle[formControlName="dayNightModeEnabled"]');

      expect(toggle).toBeTruthy();
    });

    it('should show daynight settings when toggle is enabled', () => {
      component.dayNightForm.patchValue({ dayNightModeEnabled: true });
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const daynightSettings = compiled.querySelector('.daynight-settings');

      expect(daynightSettings).toBeTruthy();
    });

    it('should hide daynight settings when toggle is disabled', () => {
      component.dayNightForm.patchValue({ dayNightModeEnabled: false });
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const daynightSettings = compiled.querySelector('.daynight-settings');

      expect(daynightSettings).toBeFalsy();
    });

    it('should render form fields', () => {
      component.dayNightForm.patchValue({ dayNightModeEnabled: true });
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;

      expect(compiled.querySelector('input[formControlName="latitude"]')).toBeTruthy();
      expect(compiled.querySelector('input[formControlName="longitude"]')).toBeTruthy();
      expect(compiled.querySelector('input[formControlName="dayNightModeUrl"]')).toBeTruthy();
    });
  });

  describe('form validation', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should be valid when dayNightModeEnabled is false', () => {
      component.dayNightForm.patchValue({ dayNightModeEnabled: false });

      expect(component.dayNightForm.valid).toBe(true);
    });

    it('should require URL when dayNightModeEnabled is true', () => {
      component.dayNightForm.patchValue({
        dayNightModeEnabled: true,
        dayNightModeUrl: '',
      });

      expect(component.dayNightForm.valid).toBe(false);
      expect(component.dayNightForm.get('dayNightModeUrl')?.hasError('required')).toBe(true);
    });

    it('should validate URL format when dayNightModeEnabled is true', () => {
      component.dayNightForm.patchValue({
        dayNightModeEnabled: true,
        dayNightModeUrl: 'invalid-url',
      });

      expect(component.dayNightForm.valid).toBe(false);
      expect(component.dayNightForm.get('dayNightModeUrl')?.hasError('pattern')).toBe(true);
    });

    it('should be valid with proper URL when dayNightModeEnabled is true', () => {
      component.dayNightForm.patchValue({
        dayNightModeEnabled: true,
        dayNightModeUrl: 'https://example.com/api',
      });

      expect(component.dayNightForm.valid).toBe(true);
    });
  });

  describe('edge cases', () => {
    it('should handle camera with undefined properties', () => {
      fixture.componentRef.setInput('camera', new Camera({
        dayNightModeEnabled: undefined as any,
        latitude: undefined as any,
        longitude: undefined as any,
      }));

      expect(() => component.ngOnInit()).not.toThrow();
    });

    it('should handle zero values for numeric fields', () => {
      fixture.componentRef.setInput('camera', createMockCamera({
        latitude: 0,
        longitude: 0,
        sunsetOffset: 0,
        sunriseOffset: 0,
        timezoneOffset: 0,
      }));

      component.ngOnInit();

      expect(component.dayNightForm.get('latitude')?.value).toBe(0);
      expect(component.dayNightForm.get('longitude')?.value).toBe(0);
    });

    it('should handle negative values for offset fields', () => {
      fixture.componentRef.setInput('camera', createMockCamera({
        sunsetOffset: -60,
        sunriseOffset: -30,
        timezoneOffset: -8,
      }));

      component.ngOnInit();

      expect(component.dayNightForm.get('sunsetOffset')?.value).toBe(-60);
      expect(component.dayNightForm.get('sunriseOffset')?.value).toBe(-30);
      expect(component.dayNightForm.get('timezoneOffset')?.value).toBe(-8);
    });

    it('should handle string values for zoom/focus input events', () => {
      spyOn(component.currentZoomFocusChange, 'emit');
      const zoomEvent = { target: { value: '0' } } as any;
      const focusEvent = { target: { value: '100' } } as any;

      component.onZoomChange(zoomEvent);
      expect(component.currentZoomFocusChange.emit).toHaveBeenCalledWith({
        zoom: 0,
        focus: 20,
      });

      component.onFocusChange(focusEvent);
      expect(component.currentZoomFocusChange.emit).toHaveBeenCalledWith({
        zoom: 10,
        focus: 100,
      });
    });
  });

  describe('form integration', () => {
    it('should emit camera changes multiple times for multiple field updates', () => {
      // Reset and create fresh spy since ngOnInit was already called in beforeEach
      const emitSpy = jasmine.createSpy('emit');
      component.cameraChange.emit = emitSpy;

      component.dayNightForm.patchValue({ latitude: 10 });
      component.dayNightForm.patchValue({ longitude: 20 });

      expect(emitSpy).toHaveBeenCalledTimes(2);
    });

    it('should maintain form state across multiple updates', () => {
      spyOn(component.cameraChange, 'emit');
      component.ngOnInit();

      component.dayNightForm.patchValue({
        dayNightModeEnabled: true,
        latitude: 40.7128,
      });

      expect(component.cameraChange.emit).toHaveBeenCalledWith(
        jasmine.objectContaining({
          dayNightModeEnabled: true,
          latitude: 40.7128,
        }),
      );

      component.dayNightForm.patchValue({
        longitude: -74.0060,
      });

      expect(component.cameraChange.emit).toHaveBeenCalledWith(
        jasmine.objectContaining({
          longitude: -74.0060,
        }),
      );
    });
  });
});
