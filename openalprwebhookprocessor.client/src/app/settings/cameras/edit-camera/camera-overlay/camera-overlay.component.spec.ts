import type { ComponentFixture } from '@angular/core/testing';
import { TestBed } from '@angular/core/testing';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { CameraOverlayComponent } from './camera-overlay.component';
import { Camera, Manufacturer } from '../../camera';

describe('CameraOverlayComponent', () => {
  let component: CameraOverlayComponent;
  let fixture: ComponentFixture<CameraOverlayComponent>;

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
    updateOverlayEnabled: false,
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

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CameraOverlayComponent,
        ReactiveFormsModule,
        NoopAnimationsModule,
      ],
      providers: [FormBuilder],
    }).compileComponents();

    fixture = TestBed.createComponent(CameraOverlayComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('camera', createMockCamera());
    fixture.detectChanges();
  });

  describe('component initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize form with default values', () => {
      // Create a fresh component without calling ngOnInit
      const freshFixture = TestBed.createComponent(CameraOverlayComponent);
      const freshComponent = freshFixture.componentInstance;

      expect(freshComponent.overlayForm).toBeDefined();
      expect(freshComponent.overlayForm.get('updateOverlayEnabled')?.value).toBe(false);
      expect(freshComponent.overlayForm.get('updateOverlayTextUrl')?.value).toBe('');
    });

    it('should have correct form controls', () => {
      const formControls = [
        'updateOverlayEnabled',
        'updateOverlayTextUrl',
      ];

      formControls.forEach(control => {
        expect(component.overlayForm.get(control)).toBeTruthy();
      });
    });
  });

  describe('input and output properties', () => {
    it('should accept camera input', () => {
      const mockCamera = createMockCamera({ updateOverlayEnabled: true });
      fixture.componentRef.setInput('camera', mockCamera);

      expect(component.camera()).toEqual(mockCamera);
    });

    it('should emit cameraChange when form values change', () => {
      spyOn(component.cameraChange, 'emit');
      component.ngOnInit();

      component.overlayForm.patchValue({ updateOverlayTextUrl: 'http://updated.com' });

      expect(component.cameraChange.emit).toHaveBeenCalled();
    });

    it('should emit testOverlay when onTestOverlay is called', () => {
      spyOn(component.testOverlay, 'emit');

      component.onTestOverlay();

      expect(component.testOverlay.emit).toHaveBeenCalled();
    });
  });

  describe('ngOnInit lifecycle', () => {
    it('should patch form with camera values', () => {
      const camera = createMockCamera({
        updateOverlayEnabled: true,
        updateOverlayTextUrl: 'https://test.com/overlay',
      });
      fixture.componentRef.setInput('camera', camera);

      component.ngOnInit();

      expect(component.overlayForm.value).toEqual({
        updateOverlayEnabled: true,
        updateOverlayTextUrl: 'https://test.com/overlay',
      });
    });

    it('should subscribe to form value changes', () => {
      spyOn(component.cameraChange, 'emit');
      component.ngOnInit();

      const newValues = {
        updateOverlayEnabled: true,
        updateOverlayTextUrl: 'https://new.com/endpoint',
      };

      component.overlayForm.patchValue(newValues);

      expect(component.cameraChange.emit).toHaveBeenCalledWith(
        jasmine.objectContaining({
          updateOverlayEnabled: true,
          updateOverlayTextUrl: 'https://new.com/endpoint',
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
      component.overlayForm.patchValue({ updateOverlayTextUrl: 'https://updated.com' });

      expect(component.camera().id).toBe('preserve-me');
      expect(component.camera().manufacturer).toBe(Manufacturer.Dahua);
      expect(component.camera().platesSeen).toBe(500);
    });
  });

  describe('dynamic validation', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should add validators when updateOverlayEnabled is true', () => {
      const urlControl = component.overlayForm.get('updateOverlayTextUrl');

      // Clear the field first
      urlControl?.setValue('');

      component.overlayForm.patchValue({ updateOverlayEnabled: true });

      expect(urlControl?.hasError('required')).toBe(true);

      urlControl?.setValue('invalid-url');
      expect(urlControl?.hasError('pattern')).toBe(true);

      urlControl?.setValue('http://valid-url.com');
      expect(urlControl?.valid).toBe(true);
    });

    it('should remove validators when updateOverlayEnabled is false', () => {
      // First enable to set validators
      component.overlayForm.patchValue({ updateOverlayEnabled: true });

      // Then disable to remove validators
      component.overlayForm.patchValue({ updateOverlayEnabled: false });

      const urlControl = component.overlayForm.get('updateOverlayTextUrl');
      expect(urlControl?.valid).toBe(true);
    });

    it('should validate URL pattern correctly', () => {
      component.overlayForm.patchValue({ updateOverlayEnabled: true });
      const urlControl = component.overlayForm.get('updateOverlayTextUrl');

      // Invalid URLs
      const invalidUrls = ['invalid', 'ftp://test.com', 'not-a-url', 'mailto:test@test.com'];
      invalidUrls.forEach(url => {
        urlControl?.setValue(url);
        expect(urlControl?.hasError('pattern')).toBe(true, `${url} should be invalid`);
      });

      // Valid URLs
      const validUrls = ['http://test.com', 'https://example.org/api', 'http://192.168.1.1/endpoint'];
      validUrls.forEach(url => {
        urlControl?.setValue(url);
        expect(urlControl?.hasError('pattern')).toBe(false, `${url} should be valid`);
      });
    });
  });

  describe('test overlay functionality', () => {
    it('should call testOverlay.emit when onTestOverlay is called', () => {
      spyOn(component.testOverlay, 'emit');

      component.onTestOverlay();

      expect(component.testOverlay.emit).toHaveBeenCalled();
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

      expect(title?.textContent?.trim()).toBe('Overlays');
      expect(subtitle?.textContent?.trim()).toBe('Configure camera overlay text updates');
    });

    it('should render overlay toggle', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const toggle = compiled.querySelector('mat-slide-toggle[formControlName="updateOverlayEnabled"]');

      expect(toggle).toBeTruthy();
    });

    it('should show overlay settings when toggle is enabled', () => {
      component.overlayForm.patchValue({ updateOverlayEnabled: true });
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const overlaySettings = compiled.querySelector('.overlay-settings');

      expect(overlaySettings).toBeTruthy();
    });

    it('should hide overlay settings when toggle is disabled', () => {
      component.overlayForm.patchValue({ updateOverlayEnabled: false });
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const overlaySettings = compiled.querySelector('.overlay-settings');

      expect(overlaySettings).toBeFalsy();
    });

    it('should render form fields when enabled', () => {
      component.overlayForm.patchValue({ updateOverlayEnabled: true });
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;

      expect(compiled.querySelector('input[formControlName="updateOverlayTextUrl"]')).toBeTruthy();
    });

    it('should render test overlay button when enabled', () => {
      component.overlayForm.patchValue({ updateOverlayEnabled: true });
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const testButton = compiled.querySelector('button');

      expect(testButton).toBeTruthy();
    });

    it('should render button when overlay is enabled', () => {
      component.overlayForm.patchValue({
        updateOverlayEnabled: true,
        updateOverlayTextUrl: 'http://test.com',
      });
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const testButton = compiled.querySelector('button');

      expect(testButton).toBeTruthy();
    });

    it('should show validation errors when fields are invalid and touched', () => {
      component.overlayForm.patchValue({ updateOverlayEnabled: true });
      const urlControl = component.overlayForm.get('updateOverlayTextUrl');
      urlControl?.markAsTouched();
      urlControl?.setValue('');
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const error = compiled.querySelector('mat-error');

      expect(error?.textContent?.trim()).toContain('Overlay URL is required');
    });

    it('should show pattern validation errors', () => {
      component.overlayForm.patchValue({ updateOverlayEnabled: true });
      const urlControl = component.overlayForm.get('updateOverlayTextUrl');
      urlControl?.markAsTouched();
      urlControl?.setValue('invalid-url');
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const error = compiled.querySelector('mat-error');

      expect(error?.textContent?.trim()).toContain('Please enter a valid URL starting with http:// or https://');
    });
  });

  describe('user interactions', () => {
    beforeEach(() => {
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should call onTestOverlay method', () => {
      spyOn(component.testOverlay, 'emit');

      component.onTestOverlay();

      expect(component.testOverlay.emit).toHaveBeenCalled();
    });

    it('should update form values when user types', () => {
      component.overlayForm.patchValue({ updateOverlayEnabled: true });
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const urlInput = compiled.querySelector('input[formControlName="updateOverlayTextUrl"]') as HTMLInputElement;

      urlInput.value = 'https://user-typed.com';
      urlInput.dispatchEvent(new Event('input'));

      expect(component.overlayForm.get('updateOverlayTextUrl')?.value).toBe('https://user-typed.com');
    });
  });

  describe('form validation', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should be valid when updateOverlayEnabled is false', () => {
      component.overlayForm.patchValue({ updateOverlayEnabled: false });

      expect(component.overlayForm.valid).toBe(true);
    });

    it('should require URL when updateOverlayEnabled is true', () => {
      component.overlayForm.patchValue({
        updateOverlayEnabled: true,
        updateOverlayTextUrl: '',
      });

      expect(component.overlayForm.valid).toBe(false);
      expect(component.overlayForm.get('updateOverlayTextUrl')?.hasError('required')).toBe(true);
    });

    it('should validate URL format when updateOverlayEnabled is true', () => {
      component.overlayForm.patchValue({
        updateOverlayEnabled: true,
        updateOverlayTextUrl: 'invalid-url',
      });

      expect(component.overlayForm.valid).toBe(false);
      expect(component.overlayForm.get('updateOverlayTextUrl')?.hasError('pattern')).toBe(true);
    });

    it('should be valid with proper URL when updateOverlayEnabled is true', () => {
      component.overlayForm.patchValue({
        updateOverlayEnabled: true,
        updateOverlayTextUrl: 'https://example.com/api',
      });

      expect(component.overlayForm.valid).toBe(true);
    });

    it('should validate various URL formats', () => {
      component.overlayForm.patchValue({ updateOverlayEnabled: true });
      const urlControl = component.overlayForm.get('updateOverlayTextUrl');

      // Test valid URLs
      const validUrls = [
        'http://example.com',
        'https://example.com',
        'http://192.168.1.1/endpoint',
        'https://subdomain.example.org/path/to/endpoint',
        'http://localhost:8080/api',
      ];

      validUrls.forEach(url => {
        urlControl?.setValue(url);
        expect(urlControl?.valid).toBe(true, `${url} should be valid`);
      });

      // Test invalid URLs
      const invalidUrls = [
        'ftp://example.com',
        'example.com',
        'mailto:test@example.com',
        'data:text/plain;base64,SGVsbG8gV29ybGQ=',
      ];

      invalidUrls.forEach(url => {
        urlControl?.setValue(url);
        expect(urlControl?.hasError('pattern')).toBe(true, `${url} should be invalid`);
      });
    });
  });

  describe('edge cases', () => {
    it('should handle camera with undefined properties', () => {
      fixture.componentRef.setInput('camera', new Camera({
        updateOverlayEnabled: undefined as any,
        updateOverlayTextUrl: undefined as any,
      }));

      expect(() => component.ngOnInit()).not.toThrow();
    });

    it('should handle empty string values', () => {
      fixture.componentRef.setInput('camera', createMockCamera({
        updateOverlayTextUrl: '',
      }));

      component.ngOnInit();

      expect(component.overlayForm.get('updateOverlayTextUrl')?.value).toBe('');
    });

    it('should handle very long URLs', () => {
      const longUrl = `https://example.com/${'a'.repeat(1000)}`;
      fixture.componentRef.setInput('camera', createMockCamera({
        updateOverlayTextUrl: longUrl,
      }));

      component.ngOnInit();

      expect(component.overlayForm.get('updateOverlayTextUrl')?.value).toBe(longUrl);
    });

    it('should handle URLs with special characters', () => {
      const specialUrl = 'https://example.com/path?param=value&other=123#anchor';
      fixture.componentRef.setInput('camera', createMockCamera({
        updateOverlayTextUrl: specialUrl,
      }));

      component.ngOnInit();

      expect(component.overlayForm.get('updateOverlayTextUrl')?.value).toBe(specialUrl);
    });
  });

  describe('form integration', () => {
    it('should update camera properties with form changes', () => {
      spyOn(component.cameraChange, 'emit');

      component.overlayForm.patchValue({ updateOverlayEnabled: true });
      component.overlayForm.patchValue({ updateOverlayTextUrl: 'http://test.com' });

      expect(component.cameraChange.emit).toHaveBeenCalledWith(
        jasmine.objectContaining({
          updateOverlayEnabled: true,
        }),
      );
      expect(component.cameraChange.emit).toHaveBeenCalledWith(
        jasmine.objectContaining({
          updateOverlayTextUrl: 'http://test.com',
        }),
      );
    });

    it('should maintain form state across multiple updates', () => {
      spyOn(component.cameraChange, 'emit');

      component.overlayForm.patchValue({
        updateOverlayEnabled: true,
      });

      expect(component.cameraChange.emit).toHaveBeenCalledWith(
        jasmine.objectContaining({
          updateOverlayEnabled: true,
        }),
      );

      component.overlayForm.patchValue({
        updateOverlayTextUrl: 'https://step2.com',
      });

      expect(component.cameraChange.emit).toHaveBeenCalledWith(
        jasmine.objectContaining({
          updateOverlayTextUrl: 'https://step2.com',
        }),
      );
    });

    it('should handle rapid toggle changes', () => {
      spyOn(component.cameraChange, 'emit');

      component.overlayForm.patchValue({ updateOverlayEnabled: true });
      component.overlayForm.patchValue({ updateOverlayEnabled: false });
      component.overlayForm.patchValue({ updateOverlayEnabled: true });

      expect(component.cameraChange.emit).toHaveBeenCalledWith(
        jasmine.objectContaining({
          updateOverlayEnabled: true,
        }),
      );
    });
  });

  describe('test overlay functionality', () => {
    it('should emit testOverlay event each time onTestOverlay is called', () => {
      spyOn(component.testOverlay, 'emit');

      component.onTestOverlay();
      component.onTestOverlay();
      component.onTestOverlay();

      expect(component.testOverlay.emit).toHaveBeenCalledTimes(3);
    });
  });
});
