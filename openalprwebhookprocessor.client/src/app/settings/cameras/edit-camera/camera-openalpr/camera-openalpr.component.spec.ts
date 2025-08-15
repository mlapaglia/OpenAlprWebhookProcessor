import type { ComponentFixture } from '@angular/core/testing';
import { TestBed } from '@angular/core/testing';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { CameraOpenAlprComponent } from './camera-openalpr.component';
import { Camera, Manufacturer } from '../../camera';
import { CameraMaskComponent } from '../camera-mask/camera-mask.component';

describe('CameraOpenAlprComponent', () => {
  let component: CameraOpenAlprComponent;
  let fixture: ComponentFixture<CameraOpenAlprComponent>;

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
    openAlprEnabled: false,
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
        CameraOpenAlprComponent,
        ReactiveFormsModule,
        NoopAnimationsModule,
      ],
      providers: [FormBuilder],
    })
      .overrideComponent(CameraOpenAlprComponent, {
        remove: { imports: [CameraMaskComponent] },
        add: { imports: [] },
      })
      .compileComponents();

    fixture = TestBed.createComponent(CameraOpenAlprComponent);
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
      const freshFixture = TestBed.createComponent(CameraOpenAlprComponent);
      const freshComponent = freshFixture.componentInstance;

      expect(freshComponent.openAlprForm).toBeDefined();
      expect(freshComponent.openAlprForm.get('openAlprEnabled')?.value).toBe(false);
      expect(freshComponent.openAlprForm.get('openAlprName')?.value).toBe('');
      expect(freshComponent.openAlprForm.get('openAlprCameraId')?.value).toBe('');
    });

    it('should have correct form controls', () => {
      const formControls = [
        'openAlprEnabled',
        'openAlprName',
        'openAlprCameraId',
      ];

      formControls.forEach(control => {
        expect(component.openAlprForm.get(control)).toBeTruthy();
      });
    });

    it('should initialize isEditingMask to false', () => {
      expect(component.isEditingMask).toBe(false);
    });
  });

  describe('input and output properties', () => {
    it('should accept camera input', () => {
      const mockCamera = createMockCamera({ openAlprEnabled: true });
      fixture.componentRef.setInput('camera', mockCamera);

      expect(component.camera()).toEqual(mockCamera);
    });

    it('should emit cameraChange when form values change', () => {
      spyOn(component.cameraChange, 'emit');
      component.ngOnInit();

      component.openAlprForm.patchValue({ openAlprName: 'Updated Name' });

      expect(component.cameraChange.emit).toHaveBeenCalled();
    });

    it('should emit editMask when onEditMask is called', () => {
      spyOn(component.editMask, 'emit');

      component.onEditMask();

      expect(component.editMask.emit).toHaveBeenCalled();
    });
  });

  describe('ngOnInit lifecycle', () => {
    it('should patch form with camera values', () => {
      const camera = createMockCamera({
        openAlprEnabled: true,
        openAlprName: 'Test OpenALPR Camera',
        openAlprCameraId: 5,
      });
      fixture.componentRef.setInput('camera', camera);

      component.ngOnInit();

      expect(component.openAlprForm.value).toEqual({
        openAlprEnabled: true,
        openAlprName: 'Test OpenALPR Camera',
        openAlprCameraId: 5,
      });
    });

    it('should subscribe to form value changes', () => {
      spyOn(component.cameraChange, 'emit');
      component.ngOnInit();

      const newValues = {
        openAlprEnabled: true,
        openAlprName: 'New Camera Name',
        openAlprCameraId: 10,
      };

      component.openAlprForm.patchValue(newValues);

      expect(component.cameraChange.emit).toHaveBeenCalledWith(
        jasmine.objectContaining({
          openAlprEnabled: true,
          openAlprName: 'New Camera Name',
          openAlprCameraId: 10,
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
      component.openAlprForm.patchValue({ openAlprName: 'Updated' });

      expect(component.camera().id).toBe('preserve-me');
      expect(component.camera().manufacturer).toBe(Manufacturer.Dahua);
      expect(component.camera().platesSeen).toBe(500);
    });
  });

  describe('dynamic validation', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should add validators when openAlprEnabled is true', () => {
      const nameControl = component.openAlprForm.get('openAlprName');
      const idControl = component.openAlprForm.get('openAlprCameraId');

      // Clear the fields first
      nameControl?.setValue('');
      idControl?.setValue('');

      component.openAlprForm.patchValue({ openAlprEnabled: true });

      expect(nameControl?.hasError('required')).toBe(true);
      expect(idControl?.hasError('required')).toBe(true);

      nameControl?.setValue('Valid Name');
      expect(nameControl?.valid).toBe(true);

      idControl?.setValue(5);
      expect(idControl?.valid).toBe(true);
    });

    it('should remove validators when openAlprEnabled is false', () => {
      // First enable to set validators
      component.openAlprForm.patchValue({ openAlprEnabled: true });

      // Then disable to remove validators
      component.openAlprForm.patchValue({ openAlprEnabled: false });

      const nameControl = component.openAlprForm.get('openAlprName');
      const idControl = component.openAlprForm.get('openAlprCameraId');

      expect(nameControl?.valid).toBe(true);
      expect(idControl?.valid).toBe(true);
    });

    it('should validate camera ID minimum value', () => {
      component.openAlprForm.patchValue({ openAlprEnabled: true });
      const idControl = component.openAlprForm.get('openAlprCameraId');

      idControl?.setValue(0);
      expect(idControl?.hasError('min')).toBe(true);

      idControl?.setValue(-1);
      expect(idControl?.hasError('min')).toBe(true);

      idControl?.setValue(1);
      expect(idControl?.hasError('min')).toBe(false);

      idControl?.setValue(10);
      expect(idControl?.hasError('min')).toBe(false);
    });
  });

  describe('edit mask functionality', () => {
    it('should toggle isEditingMask when onEditMask is called', () => {
      expect(component.isEditingMask).toBe(false);

      component.onEditMask();
      expect(component.isEditingMask).toBe(true);

      component.onEditMask();
      expect(component.isEditingMask).toBe(false);
    });

    it('should emit editMask event when onEditMask is called', () => {
      spyOn(component.editMask, 'emit');

      component.onEditMask();

      expect(component.editMask.emit).toHaveBeenCalled();
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

      expect(title?.textContent?.trim()).toBe('OpenALPR Integration');
      expect(subtitle?.textContent?.trim()).toBe('Configure automatic license plate recognition settings');
    });

    it('should render openAlpr toggle', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const toggle = compiled.querySelector('mat-slide-toggle[formControlName="openAlprEnabled"]');

      expect(toggle).toBeTruthy();
    });

    it('should show openalpr settings when toggle is enabled', () => {
      component.openAlprForm.patchValue({ openAlprEnabled: true });
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const openalprSettings = compiled.querySelector('.openalpr-settings');

      expect(openalprSettings).toBeTruthy();
    });

    it('should hide openalpr settings when toggle is disabled', () => {
      component.openAlprForm.patchValue({ openAlprEnabled: false });
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const openalprSettings = compiled.querySelector('.openalpr-settings');

      expect(openalprSettings).toBeFalsy();
    });

    it('should render form fields when enabled', () => {
      component.openAlprForm.patchValue({ openAlprEnabled: true });
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;

      expect(compiled.querySelector('input[formControlName="openAlprName"]')).toBeTruthy();
      expect(compiled.querySelector('input[formControlName="openAlprCameraId"]')).toBeTruthy();
    });

    it('should render edit mask button when enabled', () => {
      component.openAlprForm.patchValue({ openAlprEnabled: true });
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const editMaskButton = compiled.querySelector('button');

      expect(editMaskButton).toBeTruthy();
    });

    it('should show validation errors when fields are invalid and touched', () => {
      component.openAlprForm.patchValue({ openAlprEnabled: true });
      const nameControl = component.openAlprForm.get('openAlprName');
      nameControl?.markAsTouched();
      nameControl?.setValue('');
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const error = compiled.querySelector('mat-error');

      expect(error?.textContent?.trim()).toContain('OpenALPR name is required');
    });

    it('should show camera ID validation errors', () => {
      component.openAlprForm.patchValue({ openAlprEnabled: true });
      const idControl = component.openAlprForm.get('openAlprCameraId');

      // Test required error
      idControl?.markAsTouched();
      idControl?.setValue('');
      fixture.detectChanges();

      let compiled = fixture.nativeElement as HTMLElement;
      let error = compiled.querySelector('mat-error');
      expect(error?.textContent?.trim()).toContain('Camera ID is required');

      // Test min error
      idControl?.setValue(0);
      fixture.detectChanges();

      compiled = fixture.nativeElement as HTMLElement;
      error = compiled.querySelector('mat-error');
      expect(error?.textContent?.trim()).toContain('Camera ID must be greater than 0');
    });
  });

  describe('user interactions', () => {
    beforeEach(() => {
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should update form values when user types', () => {
      component.openAlprForm.patchValue({ openAlprEnabled: true });
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const nameInput = compiled.querySelector('input[formControlName="openAlprName"]') as HTMLInputElement;

      nameInput.value = 'User Typed Name';
      nameInput.dispatchEvent(new Event('input'));

      expect(component.openAlprForm.get('openAlprName')?.value).toBe('User Typed Name');
    });
  });

  describe('form validation', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should be valid when openAlprEnabled is false', () => {
      component.openAlprForm.patchValue({ openAlprEnabled: false });

      expect(component.openAlprForm.valid).toBe(true);
    });

    it('should require name and ID when openAlprEnabled is true', () => {
      component.openAlprForm.patchValue({
        openAlprEnabled: true,
        openAlprName: '',
        openAlprCameraId: '',
      });

      expect(component.openAlprForm.valid).toBe(false);
      expect(component.openAlprForm.get('openAlprName')?.hasError('required')).toBe(true);
      expect(component.openAlprForm.get('openAlprCameraId')?.hasError('required')).toBe(true);
    });

    it('should be valid with proper values when openAlprEnabled is true', () => {
      component.openAlprForm.patchValue({
        openAlprEnabled: true,
        openAlprName: 'Valid Camera Name',
        openAlprCameraId: 5,
      });

      expect(component.openAlprForm.valid).toBe(true);
    });

    it('should validate camera ID range', () => {
      component.openAlprForm.patchValue({ openAlprEnabled: true });
      const idControl = component.openAlprForm.get('openAlprCameraId');

      // Test invalid values
      const invalidValues = [0, -1, -10];
      invalidValues.forEach(value => {
        idControl?.setValue(value);
        expect(idControl?.hasError('min')).toBe(true, `${value} should be invalid`);
      });

      // Test valid values
      const validValues = [1, 5, 100, 999];
      validValues.forEach(value => {
        idControl?.setValue(value);
        expect(idControl?.hasError('min')).toBe(false, `${value} should be valid`);
      });
    });
  });

  describe('edge cases', () => {
    it('should handle camera with undefined properties', () => {
      fixture.componentRef.setInput('camera', new Camera({
        openAlprEnabled: undefined as any,
        openAlprName: undefined as any,
        openAlprCameraId: undefined as any,
      }));

      expect(() => component.ngOnInit()).not.toThrow();
    });

    it('should handle zero and negative camera ID values', () => {
      fixture.componentRef.setInput('camera', createMockCamera({
        openAlprCameraId: 0,
      }));

      component.ngOnInit();

      expect(component.openAlprForm.get('openAlprCameraId')?.value).toBe(0);
    });

    it('should handle empty string values', () => {
      fixture.componentRef.setInput('camera', createMockCamera({
        openAlprName: '',
        openAlprCameraId: undefined as any,
      }));

      component.ngOnInit();

      expect(component.openAlprForm.get('openAlprName')?.value).toBe('');
    });

    it('should handle very large camera ID values', () => {
      const largeId = 999999;
      fixture.componentRef.setInput('camera', createMockCamera({
        openAlprCameraId: largeId,
      }));

      component.ngOnInit();

      expect(component.openAlprForm.get('openAlprCameraId')?.value).toBe(largeId);
    });
  });

  describe('form integration', () => {
    it('should emit camera changes multiple times for multiple field updates', () => {
      // Reset and create fresh spy since ngOnInit was already called in beforeEach
      const emitSpy = jasmine.createSpy('emit');
      component.cameraChange.emit = emitSpy;

      component.openAlprForm.patchValue({ openAlprName: 'First Update' });
      component.openAlprForm.patchValue({ openAlprCameraId: 10 });

      expect(emitSpy).toHaveBeenCalledTimes(2);
    });

    it('should maintain form state across multiple updates', () => {
      spyOn(component.cameraChange, 'emit');

      component.openAlprForm.patchValue({
        openAlprEnabled: true,
        openAlprName: 'Step 1',
      });

      expect(component.cameraChange.emit).toHaveBeenCalledWith(
        jasmine.objectContaining({
          openAlprEnabled: true,
          openAlprName: 'Step 1',
        }),
      );

      component.openAlprForm.patchValue({
        openAlprCameraId: 15,
      });

      expect(component.cameraChange.emit).toHaveBeenCalledWith(
        jasmine.objectContaining({
          openAlprCameraId: 15,
        }),
      );
    });

    it('should handle rapid toggle changes', () => {
      spyOn(component.cameraChange, 'emit');

      component.openAlprForm.patchValue({ openAlprEnabled: true });
      component.openAlprForm.patchValue({ openAlprEnabled: false });
      component.openAlprForm.patchValue({ openAlprEnabled: true });

      expect(component.cameraChange.emit).toHaveBeenCalledWith(
        jasmine.objectContaining({
          openAlprEnabled: true,
        }),
      );
    });
  });

  describe('mask editing state', () => {
    it('should maintain mask editing state independently of form state', () => {
      expect(component.isEditingMask).toBe(false);

      component.onEditMask();
      expect(component.isEditingMask).toBe(true);

      // Form changes shouldn't affect mask editing state
      component.openAlprForm.patchValue({ openAlprName: 'Test' });
      expect(component.isEditingMask).toBe(true);

      component.onEditMask();
      expect(component.isEditingMask).toBe(false);
    });

    it('should emit editMask event each time onEditMask is called', () => {
      spyOn(component.editMask, 'emit');

      component.onEditMask();
      component.onEditMask();
      component.onEditMask();

      expect(component.editMask.emit).toHaveBeenCalledTimes(3);
    });
  });
});
