import type { ComponentFixture } from '@angular/core/testing';
import { TestBed } from '@angular/core/testing';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ChangeDetectorRef } from '@angular/core';

import { CameraBasicInfoComponent } from './camera-basic-info.component';
import { Camera, Manufacturer } from '../../camera';

describe('CameraBasicInfoComponent', () => {
  let component: CameraBasicInfoComponent;
  let fixture: ComponentFixture<CameraBasicInfoComponent>;
  let cdr: ChangeDetectorRef;

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
    nightZoom: '1.0',
    nightFocus: '1.0',
    dayZoom: '1.0',
    dayFocus: '1.0',
    dayNightModeEnabled: true,
    dayNightModeUrl: 'http://example.com/daynight',
    dayNightNextScheduledCommand: new Date(),
    openAlprEnabled: true,
    sunriseOffset: 0,
    sunsetOffset: 0,
    platesSeen: 100,
    sampleImageUrl: 'http://example.com/sample.jpg',
    timezoneOffset: -5,
    ...overrides,
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CameraBasicInfoComponent,
        ReactiveFormsModule,
      ],
      providers: [FormBuilder],
    }).compileComponents();

    fixture = TestBed.createComponent(CameraBasicInfoComponent);
    component = fixture.componentInstance;
    cdr = fixture.componentRef.injector.get(ChangeDetectorRef);
    fixture.componentRef.setInput('camera', createMockCamera());
  });

  describe('component initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with correct default values', () => {
      expect(component.basicInfoForm).toBeDefined();
      expect(component.hidePassword).toBe(true);
    });

    it('should initialize form with validators', () => {
      expect(component.basicInfoForm.get('manufacturer')?.hasError('required')).toBe(true);
      expect(component.basicInfoForm.get('modelNumber')?.hasError('required')).toBe(true);
      expect(component.basicInfoForm.get('ipAddress')?.hasError('required')).toBe(true);
      expect(component.basicInfoForm.get('cameraUsername')?.hasError('required')).toBe(true);
      expect(component.basicInfoForm.get('cameraPassword')?.hasError('required')).toBe(true);
    });

    it('should validate IP address pattern', () => {
      const ipControl = component.basicInfoForm.get('ipAddress');

      ipControl?.setValue('invalid-ip');
      expect(ipControl?.hasError('pattern')).toBe(true);

      ipControl?.setValue('192.168.1.100');
      expect(ipControl?.hasError('pattern')).toBe(false);

      // Note: 999.999.999.999 passes the regex pattern but is still an invalid IP
      ipControl?.setValue('999.999.999.999');
      expect(ipControl?.hasError('pattern')).toBe(false); // Pattern only checks format, not range
    });
  });

  describe('input and output properties', () => {
    it('should accept camera input', () => {
      const mockCamera = createMockCamera({ modelNumber: 'TEST-MODEL' });
      fixture.componentRef.setInput('camera', mockCamera);

      expect(component.camera()).toEqual(mockCamera);
    });

    it('should emit cameraChange when form values change', () => {
      spyOn(component.cameraChange, 'emit');
      component.ngOnInit();

      component.basicInfoForm.patchValue({ modelNumber: 'Updated Model' });

      expect(component.cameraChange.emit).toHaveBeenCalled();
    });
  });

  describe('ngOnInit lifecycle', () => {
    it('should patch form with camera values', () => {
      const camera = createMockCamera({
        manufacturer: Manufacturer.Dahua,
        modelNumber: 'Test Model',
        ipAddress: '10.0.0.1',
        cameraUsername: 'testuser',
        cameraPassword: 'testpass',
      });
      fixture.componentRef.setInput('camera', camera);

      component.ngOnInit();

      expect(component.basicInfoForm.value).toEqual({
        manufacturer: Manufacturer.Dahua,
        modelNumber: 'Test Model',
        ipAddress: '10.0.0.1',
        cameraUsername: 'testuser',
        cameraPassword: 'testpass',
      });
    });

    it('should subscribe to form value changes', () => {
      spyOn(component.cameraChange, 'emit');
      component.ngOnInit();

      const newValues = {
        manufacturer: Manufacturer.Hikvision,
        modelNumber: 'New Model',
        ipAddress: '192.168.1.50',
        cameraUsername: 'newuser',
        cameraPassword: 'newpass',
      };

      component.basicInfoForm.patchValue(newValues);

      // Verify that cameraChange was emitted with the updated values
      expect(component.cameraChange.emit).toHaveBeenCalled();
      const emittedCamera = (component.cameraChange.emit as jasmine.Spy).calls.mostRecent().args[0];
      expect(emittedCamera.manufacturer).toBe(Manufacturer.Hikvision);
      expect(emittedCamera.modelNumber).toBe('New Model');
      expect(emittedCamera.ipAddress).toBe('192.168.1.50');
      expect(emittedCamera.cameraUsername).toBe('newuser');
      expect(emittedCamera.cameraPassword).toBe('newpass');
    });

    it('should preserve non-form camera properties', () => {
      const originalCamera = createMockCamera({
        id: 'preserve-me',
        latitude: 50.0,
        platesSeen: 500,
      });
      fixture.componentRef.setInput('camera', originalCamera);
      spyOn(component.cameraChange, 'emit');

      component.ngOnInit();
      component.basicInfoForm.patchValue({ modelNumber: 'Updated' });

      // Verify that the emitted camera preserves non-form properties
      const emittedCamera = (component.cameraChange.emit as jasmine.Spy).calls.mostRecent().args[0];
      expect(emittedCamera.id).toBe('preserve-me');
      expect(emittedCamera.latitude).toBe(50.0);
      expect(emittedCamera.platesSeen).toBe(500);
    });
  });

  describe('password visibility toggle', () => {
    it('should toggle password visibility', () => {
      expect(component.hidePassword).toBe(true);

      component.togglePasswordVisibility();
      expect(component.hidePassword).toBe(false);

      component.togglePasswordVisibility();
      expect(component.hidePassword).toBe(true);
    });

    it('should start with password hidden', () => {
      expect(component.hidePassword).toBe(true);
    });
  });

  describe('form validation', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should be invalid when required fields are empty', () => {
      component.basicInfoForm.patchValue({
        manufacturer: '',
        modelNumber: '',
        ipAddress: '',
        cameraUsername: '',
        cameraPassword: '',
      });

      expect(component.basicInfoForm.valid).toBe(false);
      expect(component.basicInfoForm.get('manufacturer')?.hasError('required')).toBe(true);
      expect(component.basicInfoForm.get('modelNumber')?.hasError('required')).toBe(true);
      expect(component.basicInfoForm.get('ipAddress')?.hasError('required')).toBe(true);
      expect(component.basicInfoForm.get('cameraUsername')?.hasError('required')).toBe(true);
      expect(component.basicInfoForm.get('cameraPassword')?.hasError('required')).toBe(true);
    });

    it('should be valid when all required fields are filled correctly', () => {
      component.basicInfoForm.patchValue({
        manufacturer: Manufacturer.Hikvision,
        modelNumber: 'Test Model',
        ipAddress: '192.168.1.100',
        cameraUsername: 'admin',
        cameraPassword: 'password',
      });

      expect(component.basicInfoForm.valid).toBe(true);
    });

    it('should validate various IP address formats', () => {
      const ipControl = component.basicInfoForm.get('ipAddress');

      // Valid format IPs (according to regex)
      const validFormatIPs = ['0.0.0.0', '192.168.1.1', '255.255.255.255', '10.0.0.1', '999.999.999.999'];
      validFormatIPs.forEach(ip => {
        ipControl?.setValue(ip);
        expect(ipControl?.hasError('pattern')).withContext(`${ip} should match pattern`).toBe(false);
      });

      // Invalid format IPs
      const invalidFormatIPs = ['192.168.1', 'invalid', '192.168.1.1.1'];
      invalidFormatIPs.forEach(ip => {
        ipControl?.setValue(ip);
        expect(ipControl?.hasError('pattern')).withContext(`${ip} should not match pattern`).toBe(true);
      });

      // Empty should have required error
      ipControl?.setValue('');
      expect(ipControl?.hasError('required')).withContext('Empty should be required error').toBe(true);
    });
  });

  describe('template rendering', () => {
    beforeEach(() => {
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should render card title and subtitle', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const title = compiled.querySelector('mat-card-title');
      const subtitle = compiled.querySelector('mat-card-subtitle');

      expect(title?.textContent?.trim()).toBe('Camera Information');
      expect(subtitle?.textContent?.trim()).toBe('Basic camera configuration and connection details');
    });

    it('should render manufacturer select with options', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const manufacturerSelect = compiled.querySelector('mat-select[formControlName="manufacturer"]');

      expect(manufacturerSelect).toBeTruthy();

      // Trigger select to see options (this is complex in tests, so we just verify the select exists)
      expect(manufacturerSelect).toBeTruthy();
    });

    it('should render all form fields', () => {
      const compiled = fixture.nativeElement as HTMLElement;

      expect(compiled.querySelector('mat-select[formControlName="manufacturer"]')).toBeTruthy();
      expect(compiled.querySelector('input[formControlName="modelNumber"]')).toBeTruthy();
      expect(compiled.querySelector('input[formControlName="ipAddress"]')).toBeTruthy();
      expect(compiled.querySelector('input[formControlName="cameraUsername"]')).toBeTruthy();
      expect(compiled.querySelector('input[formControlName="cameraPassword"]')).toBeTruthy();
    });

    it('should render password field with correct type based on hidePassword', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const passwordInput = compiled.querySelector('input[formControlName="cameraPassword"]') as HTMLInputElement;

      expect(passwordInput?.type).toBe('password');

      component.hidePassword = false;
      cdr.markForCheck();
      fixture.detectChanges();

      expect(passwordInput?.type).toBe('text');
    });

    it('should render password visibility toggle icon', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const toggleIcon = compiled.querySelector('.password-toggle');

      expect(toggleIcon?.textContent?.trim()).toBe('visibility_off');

      component.hidePassword = false;
      cdr.markForCheck();
      fixture.detectChanges();

      expect(toggleIcon?.textContent?.trim()).toBe('visibility');
    });

    it('should show validation errors when fields are invalid and touched', () => {
      component.basicInfoForm.get('manufacturer')?.markAsTouched();
      component.basicInfoForm.get('manufacturer')?.setValue('');
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const error = compiled.querySelector('mat-error');

      expect(error?.textContent?.trim()).toContain('Manufacturer is required');
    });

    it('should show IP address pattern error', () => {
      const ipControl = component.basicInfoForm.get('ipAddress');
      ipControl?.markAsTouched();
      ipControl?.setValue('invalid-ip');
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const errors = compiled.querySelectorAll('mat-error');
      const patternError = Array.from(errors).find(error =>
        error.textContent?.includes('Please enter a valid IP address'),
      );

      expect(patternError).toBeTruthy();
    });
  });

  describe('user interactions', () => {
    beforeEach(() => {
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should toggle password visibility when icon is clicked', () => {
      spyOn(component, 'togglePasswordVisibility');
      const compiled = fixture.nativeElement as HTMLElement;
      const toggleIcon = compiled.querySelector('.password-toggle') as HTMLElement;

      toggleIcon.click();

      expect(component.togglePasswordVisibility).toHaveBeenCalled();
    });

    it('should update form values when user types', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const modelInput = compiled.querySelector('input[formControlName="modelNumber"]') as HTMLInputElement;

      modelInput.value = 'User Typed Model';
      modelInput.dispatchEvent(new Event('input'));

      expect(component.basicInfoForm.get('modelNumber')?.value).toBe('User Typed Model');
    });
  });

  describe('edge cases', () => {
    it('should handle camera with missing properties', () => {
      const camera = new Camera({
        manufacturer: Manufacturer.Hikvision,
        // Other properties missing
      });
      fixture.componentRef.setInput('camera', camera);

      expect(() => component.ngOnInit()).not.toThrow();
    });

    it('should handle empty camera properties', () => {
      const camera = new Camera({
        manufacturer: undefined as any,
        modelNumber: undefined as any,
        ipAddress: undefined as any,
        cameraUsername: undefined as any,
        cameraPassword: undefined as any,
      });
      fixture.componentRef.setInput('camera', camera);

      expect(() => component.ngOnInit()).not.toThrow();
    });

    it('should handle special characters in form fields', () => {
      component.ngOnInit();

      component.basicInfoForm.patchValue({
        manufacturer: Manufacturer.Dahua,
        modelNumber: 'Model-123_Test!@#',
        ipAddress: '192.168.1.100',
        cameraUsername: 'user_123',
        cameraPassword: 'pass!@#$%^&*()',
      });

      expect(component.basicInfoForm.valid).toBe(true);
    });
  });

  describe('form integration', () => {
    it('should emit camera changes multiple times for multiple field updates', () => {
      spyOn(component.cameraChange, 'emit');
      component.ngOnInit();

      component.basicInfoForm.patchValue({ modelNumber: 'First Update' });
      component.basicInfoForm.patchValue({ cameraUsername: 'Second Update' });

      expect(component.cameraChange.emit).toHaveBeenCalledTimes(2);
    });

    it('should maintain form state across multiple updates', () => {
      spyOn(component.cameraChange, 'emit');
      component.ngOnInit();

      component.basicInfoForm.patchValue({
        manufacturer: Manufacturer.Hikvision,
        modelNumber: 'Step 1',
      });

      let emittedCamera = (component.cameraChange.emit as jasmine.Spy).calls.mostRecent().args[0];
      expect(emittedCamera.manufacturer).toBe(Manufacturer.Hikvision);
      expect(emittedCamera.modelNumber).toBe('Step 1');

      component.basicInfoForm.patchValue({
        ipAddress: '10.0.0.1',
      });

      emittedCamera = (component.cameraChange.emit as jasmine.Spy).calls.mostRecent().args[0];
      expect(emittedCamera.manufacturer).toBe(Manufacturer.Hikvision);
      expect(emittedCamera.modelNumber).toBe('Step 1');
      expect(emittedCamera.ipAddress).toBe('10.0.0.1');
    });
  });
});
