import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { of, throwError } from 'rxjs';
import { ImageModalComponent } from './image-modal.component';
import { SettingsService } from '../../../settings.service';

describe('ImageModalComponent', () => {
  let component: ImageModalComponent;
  let fixture: ComponentFixture<ImageModalComponent>;
  let mockSettingsService: jasmine.SpyObj<SettingsService>;
  let mockDialogRef: jasmine.SpyObj<MatDialogRef<ImageModalComponent>>;
  let mockDialogData: { agentId: string; cameraId: number; cameraName: string };

  beforeEach(async () => {
    mockSettingsService = jasmine.createSpyObj('SettingsService', ['getCameraSnapshot']);
    mockDialogRef = jasmine.createSpyObj('MatDialogRef', ['close']);
    mockDialogData = {
      agentId: 'test-agent-123',
      cameraId: 456,
      cameraName: 'Test Camera',
    };

    await TestBed.configureTestingModule({
      imports: [ImageModalComponent],
      providers: [
        { provide: SettingsService, useValue: mockSettingsService },
        { provide: MatDialogRef, useValue: mockDialogRef },
        { provide: MAT_DIALOG_DATA, useValue: mockDialogData },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ImageModalComponent);
    component = fixture.componentInstance;

    // Setup default mock to prevent HTTP calls during component creation
    mockSettingsService.getCameraSnapshot.and.returnValue(of(new Blob()));
  });

  describe('component initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.isLoading).toBe(true);
      expect(component.imageUrl).toBe(null);
      expect(component.errorMessage).toBe(null);
    });

    it('should have correct dialog data', () => {
      expect(component.data).toEqual(mockDialogData);
      expect(component.data.agentId).toBe('test-agent-123');
      expect(component.data.cameraId).toBe(456);
      expect(component.data.cameraName).toBe('Test Camera');
    });

    it('should call loadCameraSnapshot on init', () => {
      spyOn(component as any, 'loadCameraSnapshot');

      component.ngOnInit();

      expect((component as any).loadCameraSnapshot).toHaveBeenCalled();
    });
  });

  describe('loadCameraSnapshot', () => {
    it('should successfully load camera snapshot', () => {
      const mockBlob = new Blob(['test image data'], { type: 'image/jpeg' });
      mockSettingsService.getCameraSnapshot.and.returnValue(of(mockBlob));
      spyOn(URL, 'createObjectURL').and.returnValue('blob:test-url');

      component.ngOnInit();

      expect(mockSettingsService.getCameraSnapshot).toHaveBeenCalledWith('test-agent-123', 456);
      expect(URL.createObjectURL).toHaveBeenCalledWith(mockBlob);
      expect(component.imageUrl).toEqual('blob:test-url');
      expect(component.isLoading).toBe(false);
      expect(component.errorMessage).toBeNull();
    });

    it('should handle error when loading camera snapshot fails', () => {
      mockSettingsService.getCameraSnapshot.and.returnValue(throwError(() => new Error('Network error')));

      component.ngOnInit();

      expect(mockSettingsService.getCameraSnapshot).toHaveBeenCalledWith('test-agent-123', 456);
      expect(component.imageUrl).toBeNull();
      expect(component.isLoading).toBe(false);
      expect(component.errorMessage).toBe('Failed to load camera snapshot');
    });

    it('should reset loading state and error message when called', () => {
      // Set initial state
      component.isLoading = false;
      component.errorMessage = 'Previous error';
      mockSettingsService.getCameraSnapshot.and.returnValue(of(new Blob()));
      spyOn(URL, 'createObjectURL').and.returnValue('blob:test-url');
      spyOn(component as any, 'markForCheck');

      (component as any).loadCameraSnapshot();

      expect(component.isLoading).toBe(false); // Will be set to false after successful load
      expect(component.errorMessage).toBeNull();
      expect((component as any).markForCheck).toHaveBeenCalled();
    });
  });

  describe('onClose', () => {
    it('should close dialog and revoke object URL when imageUrl exists', () => {
      component.imageUrl = 'blob:test-url';
      spyOn(URL, 'revokeObjectURL');

      component.onClose();

      expect(URL.revokeObjectURL).toHaveBeenCalledWith('blob:test-url');
      expect(mockDialogRef.close).toHaveBeenCalled();
    });

    it('should close dialog without revoking URL when imageUrl is null', () => {
      component.imageUrl = null;
      spyOn(URL, 'revokeObjectURL');

      component.onClose();

      expect(URL.revokeObjectURL).not.toHaveBeenCalled();
      expect(mockDialogRef.close).toHaveBeenCalled();
    });
  });

  describe('onRetry', () => {
    it('should call loadCameraSnapshot', () => {
      spyOn(component as any, 'loadCameraSnapshot');

      component.onRetry();

      expect((component as any).loadCameraSnapshot).toHaveBeenCalled();
    });

    it('should reset state and reload snapshot on retry', () => {
      // Set error state
      component.isLoading = false;
      component.errorMessage = 'Previous error';
      component.imageUrl = null;

      const mockBlob = new Blob(['test image data'], { type: 'image/jpeg' });
      mockSettingsService.getCameraSnapshot.and.returnValue(of(mockBlob));
      spyOn(URL, 'createObjectURL').and.returnValue('blob:new-url');

      component.onRetry();

      expect(mockSettingsService.getCameraSnapshot).toHaveBeenCalledWith('test-agent-123', 456);
      expect(component.imageUrl).not.toBeNull();
      expect(component.imageUrl).toContain('blob:');
      expect(component.isLoading).toBe(false);
      expect(component.errorMessage).toBeNull();
    });
  });

  describe('template rendering', () => {
    it('should display correct camera name from dialog data', () => {
      expect(component.data.cameraName).toBe('Test Camera');
    });

    it('should have correct initial state', () => {
      expect(component.isLoading).toBe(true);
      expect(component.imageUrl).toBeNull();
      expect(component.errorMessage).toBeNull();
    });
  });

  describe('memory management', () => {
    it('should clean up object URL on close', () => {
      component.imageUrl = 'blob:test-url-to-cleanup';
      spyOn(URL, 'revokeObjectURL');

      component.onClose();

      expect(URL.revokeObjectURL).toHaveBeenCalledWith('blob:test-url-to-cleanup');
    });

    it('should handle multiple object URLs correctly', () => {
      const mockBlob1 = new Blob(['image1'], { type: 'image/jpeg' });
      const mockBlob2 = new Blob(['image2'], { type: 'image/jpeg' });

      spyOn(URL, 'createObjectURL').and.returnValues('blob:url1', 'blob:url2');
      spyOn(URL, 'revokeObjectURL');

      // First load
      mockSettingsService.getCameraSnapshot.and.returnValue(of(mockBlob1));
      component['loadCameraSnapshot']();
      expect(component.imageUrl).toEqual('blob:url1');

      // Second load (retry)
      mockSettingsService.getCameraSnapshot.and.returnValue(of(mockBlob2));
      component.onRetry();
      expect(component.imageUrl).toEqual('blob:url2');

      // Close should revoke the latest URL
      component.onClose();
      expect(URL.revokeObjectURL).toHaveBeenCalledWith('blob:url2');
    });
  });
});
