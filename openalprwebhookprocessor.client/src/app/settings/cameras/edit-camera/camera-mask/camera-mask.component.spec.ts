import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { CameraMaskComponent } from './camera-mask.component';
import { CameraMaskService } from './camera-mask.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { Camera } from '../../camera';
import { type Coordinate } from './coordinate';
import { type PageEvent } from '@angular/material/paginator';
import { type MatButtonToggleChange } from '@angular/material/button-toggle';

describe(CameraMaskComponent.name, () => {
  let component: CameraMaskComponent;
  let fixture: ComponentFixture<CameraMaskComponent>;
  let mockCameraMaskService: jasmine.SpyObj<CameraMaskService>;
  let mockSnackbarService: jasmine.SpyObj<SnackbarService>;
  let mockCanvas: HTMLCanvasElement;
  let mockContext: jasmine.SpyObj<CanvasRenderingContext2D>;
  let mockSavingCanvas: HTMLCanvasElement;
  let mockSavingContext: jasmine.SpyObj<CanvasRenderingContext2D>;

  const mockCamera = new Camera({
    id: 'test-camera-id',
    sampleImageUrl: '/test-image.jpg',
  });

  const mockCoordinates: Coordinate[] = [
    { x: 100, y: 100 },
    { x: 200, y: 100 },
    { x: 200, y: 200 },
    { x: 100, y: 200 },
  ];

  beforeEach(async () => {
    // Mock canvas context methods
    mockContext = jasmine.createSpyObj('CanvasRenderingContext2D', [
      'clearRect', 'drawImage', 'beginPath', 'moveTo', 'lineTo', 'closePath',
      'fill', 'stroke', 'arc', 'reset', 'fillRect',
    ]);

    mockSavingContext = jasmine.createSpyObj('CanvasRenderingContext2D', ['drawImage', 'fillRect', 'beginPath', 'moveTo', 'lineTo', 'closePath', 'fill']);

    (mockSavingContext as any).canvas = mockSavingCanvas;

    // Mock canvas elements
    mockCanvas = {
      getContext: jasmine.createSpy('getContext').and.returnValue(mockContext),
      width: 960,
      height: 540,
      addEventListener: jasmine.createSpy('addEventListener'),
      getBoundingClientRect: jasmine.createSpy('getBoundingClientRect').and.returnValue({
        left: 0,
        top: 0,
        width: 960,
        height: 540,
      }),
      style: { cursor: 'default' },
      toDataURL: jasmine.createSpy('toDataURL').and.returnValue('data:image/png;base64,mock'),
    } as any;

    mockSavingCanvas = {
      getContext: jasmine.createSpy('getContext').and.returnValue(mockSavingContext),
      width: 1920,
      height: 1080,
      hidden: false,
      toDataURL: jasmine.createSpy('toDataURL').and.returnValue('data:image/png;base64,mock'),
    } as any;

    mockCameraMaskService = jasmine.createSpyObj('CameraMaskService', [
      'getPlateCaptures',
      'getPlateCapture',
      'getCameraMaskCoordinates',
      'upsertImageMask',
    ]);

    mockSnackbarService = jasmine.createSpyObj('SnackbarService', ['create']);

    await TestBed.configureTestingModule({
      imports: [CameraMaskComponent],
      providers: [
        { provide: CameraMaskService, useValue: mockCameraMaskService },
        { provide: SnackbarService, useValue: mockSnackbarService },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    // Setup default mock returns
    mockCameraMaskService.getPlateCaptures.and.returnValue(of(['plate1.jpg', 'plate2.jpg']));
    mockCameraMaskService.getPlateCapture.and.returnValue(of(new Blob()));
    mockCameraMaskService.getCameraMaskCoordinates.and.returnValue(of([]));
    mockCameraMaskService.upsertImageMask.and.returnValue(of({}));

    fixture = TestBed.createComponent(CameraMaskComponent);
    component = fixture.componentInstance;

    // Mock ViewChild elements using Object.defineProperty for signals
    Object.defineProperty(component, 'canvas', {
      value: () => ({ nativeElement: mockCanvas }),
      writable: false,
    });
    Object.defineProperty(component, 'savingCanvas', {
      value: () => ({ nativeElement: mockSavingCanvas }),
      writable: false,
    });
    Object.defineProperty(component, 'measureDiv', {
      value: () => ({
        nativeElement: {
          appendChild: jasmine.createSpy('appendChild'),
          removeChild: jasmine.createSpy('removeChild'),
        },
      }),
      writable: false,
    });

    // Initialize contexts
    component.ctx = mockContext;
    component.savingCtx = mockSavingContext;
    component.sampleCtx = mockContext; // Add this line

    fixture.componentRef.setInput('camera', mockCamera);
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Component Initialization', () => {
    it('should initialize with default values', () => {
      expect(component.coordinates).toEqual([]);
      expect(component.isLoadingSnapshot).toBe(false);
      expect(component.isClosed).toBe(false);
      expect(component.isDragging).toBe(false);
      expect(component.dragStartIndex).toBe(-1);
      expect(component.dotRadius).toBe(7.5);
      expect(component.lineThickness).toBe(4);
      expect(component.forgiveness).toBe(30);
      expect(component.targetWidth).toBe(960);
      expect(component.imageInValidState).toBe(true);
      expect(component.paginatorIndex).toBe(0);
      expect(component.samplePlates).toEqual([]);
    });

    it('should call initialization methods on ngOnInit', () => {
      spyOn(component, 'getSamplePlates' as any);
      spyOn(component, 'prepareCanvases' as any);
      spyOn(component, 'addEventHandlers' as any);

      component.ngOnInit();

      expect((component as any).getSamplePlates).toHaveBeenCalled();
      expect((component as any).prepareCanvases).toHaveBeenCalled();
      expect((component as any).addEventHandlers).toHaveBeenCalled();
    });

    it('should prepare canvases correctly', () => {
      (component as any).prepareCanvases();

      expect(mockCanvas.getContext).toHaveBeenCalledWith('2d');
      expect(mockSavingCanvas.getContext).toHaveBeenCalledWith('2d');
      expect(component.ctx).toBe(mockContext);
      expect(component.savingCtx).toBe(mockSavingContext);
      expect(component.savingCtx.canvas.hidden).toBe(true);
    });

    it('should handle canvas context null error', () => {
      mockCanvas.getContext = jasmine.createSpy('getContext').and.returnValue(null);

      expect(() => (component as any).prepareCanvases()).toThrowError('ctx is null');
    });

    it('should add event listeners', () => {
      (component as any).addEventHandlers();

      expect(mockCanvas.addEventListener).toHaveBeenCalledWith('mousedown', jasmine.any(Function));
      expect(mockCanvas.addEventListener).toHaveBeenCalledWith('mouseup', jasmine.any(Function));
      expect(mockCanvas.addEventListener).toHaveBeenCalledWith('click', jasmine.any(Function));
      expect(mockCanvas.addEventListener).toHaveBeenCalledWith('mouseleave', jasmine.any(Function));
    });

    it('should call super.ngOnDestroy on destroy', () => {
      spyOn(Object.getPrototypeOf(Object.getPrototypeOf(component)), 'ngOnDestroy');

      component.ngOnDestroy();

      expect(Object.getPrototypeOf(Object.getPrototypeOf(component)).ngOnDestroy).toHaveBeenCalled();
    });
  });

  describe('Sample Plates Management', () => {
    it('should load sample plates on initialization', () => {
      spyOn(component, 'loadImageIntoCanvas' as any);

      (component as any).getSamplePlates();

      expect(mockCameraMaskService.getPlateCaptures).toHaveBeenCalledWith('test-camera-id');
      expect(component.samplePlates).toEqual(['plate1.jpg', 'plate2.jpg']);
      expect((component as any).loadImageIntoCanvas).toHaveBeenCalledWith('/test-image.jpg');
    });

    it('should handle page event for pagination', () => {
      spyOn(component, 'loadImageIntoCanvas' as any);
      component.samplePlates = ['plate1.jpg', 'plate2.jpg', 'plate3.jpg'];

      const pageEvent: PageEvent = {
        pageIndex: 1,
        pageSize: 1,
        length: 3,
      };

      component.handlePageEvent(pageEvent);

      expect((component as any).loadImageIntoCanvas).toHaveBeenCalledWith('plate2.jpg');
    });

    it('should handle toggle to snapshot mode', () => {
      spyOn(component, 'loadImageIntoCanvas' as any);

      const toggleEvent: MatButtonToggleChange = {
        value: 'snapshot',
        source: null as any,
      };

      component.handleToggleChange(toggleEvent);

      expect(component.samplePlates).toEqual([`/api/images/${mockCamera.id}/snapshot`]);
      expect((component as any).loadImageIntoCanvas).toHaveBeenCalledWith(`/api/images/${mockCamera.id}/snapshot`);
    });

    it('should handle toggle to sample plates mode', () => {
      spyOn(component, 'getSamplePlates' as any);

      const toggleEvent: MatButtonToggleChange = {
        value: 'samples',
        source: null as any,
      };

      component.handleToggleChange(toggleEvent);

      expect((component as any).getSamplePlates).toHaveBeenCalled();
    });
  });

  describe('Image Loading and Scaling', () => {
    let mockImage: HTMLImageElement;
    let mockBlob: Blob;
    let mockFileReader: FileReader;

    beforeEach(() => {
      mockImage = {
        onload: null,
        onerror: null,
        src: '',
        width: 1920,
        height: 1080,
      } as any;

      mockBlob = new Blob(['mock'], { type: 'image/jpeg' });

      mockFileReader = {
        onload: null,
        result: 'data:image/jpeg;base64,mock',
        readAsDataURL: jasmine.createSpy('readAsDataURL'),
      } as any;

      spyOn(window, 'Image').and.returnValue(mockImage);
      spyOn(window, 'FileReader').and.returnValue(mockFileReader);
    });

    it('should load image successfully', () => {
      spyOn(component, 'loadMaskCoordinates' as any);
      mockCameraMaskService.getPlateCapture.and.returnValue(of(mockBlob));

      (component as any).loadImageIntoCanvas('/test-image.jpg');

      expect(component.isLoadingSnapshot).toBe(true);
      expect(mockCameraMaskService.getPlateCapture).toHaveBeenCalledWith('/test-image.jpg');

      // Simulate FileReader loading
      (mockFileReader as any).onload();

      expect(mockImage.src).toBe('data:image/jpeg;base64,mock');

      // Simulate image loading
      component.image = mockImage;
      (mockImage as any).onload();

      expect(component.imageInValidState).toBe(true);
      expect(component.scaleFactor).toBe(2); // 1920 / 960
      expect((component as any).loadMaskCoordinates).toHaveBeenCalled();
    });

    it('should handle image loading error', () => {
      mockCameraMaskService.getPlateCapture.and.returnValue(of(mockBlob));

      (component as any).loadImageIntoCanvas('/test-image.jpg');

      // Simulate FileReader loading
      (mockFileReader as any).onload();

      // Simulate image error
      component.image = mockImage;
      (mockImage as any).onerror();

      expect(component.imageInValidState).toBe(false);
    });

    it('should scale image dimensions correctly', () => {
      spyOn(component, 'loadMaskCoordinates' as any);
      mockCameraMaskService.getPlateCapture.and.returnValue(of(mockBlob));

      (component as any).loadImageIntoCanvas('/test-image.jpg');
      (mockFileReader as any).onload();

      component.image = mockImage;
      (mockImage as any).onload();

      expect(component.imageWidth).toBe(960);
      expect(component.imageHeight).toBe(540); // 960 / (1920/1080)
    });
  });

  describe('Coordinate Management', () => {
    beforeEach(() => {
      component.ctx = mockContext;
      component.image = { width: 1920, height: 1080 } as any;
      component.scaleFactor = 2;
      component.imageWidth = 960;
      component.imageHeight = 540;
    });

    it('should load mask coordinates and scale them', () => {
      const serverCoordinates = [
        { x: 200, y: 200 },  // Will be scaled down by factor of 2
        { x: 400, y: 400 },
      ];
      mockCameraMaskService.getCameraMaskCoordinates.and.returnValue(of(serverCoordinates));
      spyOn(component, 'closePolygon' as any);
      spyOn(component, 'draw' as any);

      (component as any).loadMaskCoordinates();

      expect(mockCameraMaskService.getCameraMaskCoordinates).toHaveBeenCalledWith('test-camera-id');
      expect(component.coordinates).toEqual([
        { x: 100, y: 100 },  // Scaled down
        { x: 200, y: 200 },
      ]);
      expect((component as any).closePolygon).toHaveBeenCalled();
      expect((component as any).draw).toHaveBeenCalled();
      expect(component.isLoadingSnapshot).toBe(false);
    });

    it('should add point to coordinates', () => {
      spyOn(component, 'draw' as any);

      (component as any).addPoint(150, 150);

      expect(component.coordinates).toContain({ x: 150, y: 150 });
      expect((component as any).draw).toHaveBeenCalled();
    });

    it('should move point at specific index', () => {
      component.coordinates = [...mockCoordinates];
      spyOn(component, 'draw' as any);

      component.movePoint(1, 250, 150);

      expect(component.coordinates[1]).toEqual({ x: 250, y: 150 });
      expect((component as any).draw).toHaveBeenCalled();
    });

    it('should close polygon when more than 2 points', () => {
      spyOn(component, 'draw' as any);
      component.coordinates = [...mockCoordinates];

      (component as any).closePolygon();

      expect(component.isClosed).toBe(true);
      expect((component as any).draw).toHaveBeenCalled();
    });

    it('should not close polygon with 2 or fewer points', () => {
      component.coordinates = [{ x: 100, y: 100 }, { x: 200, y: 200 }];

      (component as any).closePolygon();

      expect(component.isClosed).toBe(false);
    });

    it('should check if point is near coordinates', () => {
      component.coordinates = [...mockCoordinates];

      expect((component as any).isNearPoint(0, { x: 105, y: 105 })).toBe(true);
      expect((component as any).isNearPoint(0, { x: 150, y: 150 })).toBe(false);
    });

    it('should find closest point to mouse position', () => {
      component.coordinates = [...mockCoordinates];

      const closestIndex = (component as any).findClosestPoint({ x: 105, y: 105 });
      expect(closestIndex).toBe(0);

      const noClosePoint = (component as any).findClosestPoint({ x: 500, y: 500 });
      expect(noClosePoint).toBe(-1);
    });

    it('should check if point is inside polygon', () => {
      const isInside = component.isPointInPolygon(150, 150, mockCoordinates);
      expect(isInside).toBe(true);

      const isOutside = component.isPointInPolygon(50, 50, mockCoordinates);
      expect(isOutside).toBe(false);
    });
  });

  describe('Mask Operations', () => {
    beforeEach(() => {
      component.ctx = mockContext;
      component.savingCtx = mockSavingContext;
      component.image = { width: 1920, height: 1080 } as any;
      component.scaleFactor = 2;
      component.coordinates = [...mockCoordinates];

    });

    it('should save mask successfully', () => {
      component.saveMask();

      expect(mockSavingContext.drawImage).toHaveBeenCalled();
      expect(mockSavingContext.fillRect).toHaveBeenCalledWith(0, 0, 1920, 1080);
      expect(mockCameraMaskService.upsertImageMask).toHaveBeenCalledWith(
        jasmine.objectContaining({
          cameraId: 'test-camera-id',
          coordinates: jasmine.any(Array),
          imageMask: jasmine.any(String),
        }),
      );
      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Camera mask saved.',
        SnackBarType.Saved,
      );
    });

    it('should handle save mask error', () => {
      mockCameraMaskService.upsertImageMask.and.returnValue(throwError(() => new Error('Save failed')));

      component.saveMask();

      expect(mockSnackbarService.create).toHaveBeenCalledWith(
        'Camera mask failed.',
        SnackBarType.Error,
      );
    });

    it('should reset canvas', () => {
      spyOn(component, 'draw' as any);
      component.coordinates = [...mockCoordinates];
      component.isClosed = true;
      component.isDragging = true;

      component.resetCanvas();

      expect(component.coordinates.length).toBe(0);
      expect(component.isClosed).toBe(false);
      expect(component.isDragging).toBe(false);
      expect((component as any).draw).toHaveBeenCalled();
    });

    it('should undo last point when polygon not closed', () => {
      spyOn(component, 'draw' as any);
      component.coordinates = [...mockCoordinates];
      component.isClosed = false;

      component.undoLastPoint();

      expect(component.coordinates.length).toBe(3);
      expect((component as any).draw).toHaveBeenCalled();
    });

    it('should not undo when polygon is closed', () => {
      component.coordinates = [...mockCoordinates];
      component.isClosed = true;
      const originalLength = component.coordinates.length;

      component.undoLastPoint();

      expect(component.coordinates.length).toBe(originalLength);
    });

    it('should not undo when no coordinates exist', () => {
      component.coordinates = [];

      component.undoLastPoint();

      expect(component.coordinates.length).toBe(0);
    });

    it('should do nothing on cancel mask', () => {
      const originalState = { ...component };

      component.cancelMask();

      // Component state should remain unchanged
      expect(component.coordinates).toEqual(originalState.coordinates);
    });
  });

  describe('Drawing Operations', () => {
    beforeEach(() => {
      component.ctx = mockContext;
      component.image = { width: 1920, height: 1080 } as any;
      component.imageWidth = 960;
      component.imageHeight = 540;
      component.coordinates = [...mockCoordinates];
    });

    it('should draw canvas with coordinates', () => {
      component.currentPos = { x: 0, y: 0 };

      (component as any).draw();

      expect(mockContext.clearRect).toHaveBeenCalledWith(0, 0, 960, 540);
      expect(mockContext.drawImage).toHaveBeenCalled();
      expect(mockContext.beginPath).toHaveBeenCalled();
      expect(mockContext.moveTo).toHaveBeenCalledWith(100, 100);
      expect(mockContext.lineTo).toHaveBeenCalled();
    });

    it('should handle drawing error gracefully', () => {
      component.currentPos = { x: 0, y: 0 };
      mockContext.drawImage.and.throwError('Canvas error');

      expect(() => (component as any).draw()).not.toThrow();
      expect(mockContext.reset).toHaveBeenCalled();
    });

    it('should draw filled polygon when closed', () => {
      component.isClosed = true;
      component.currentPos = { x: 0, y: 0 };

      (component as any).draw();

      expect(mockContext.closePath).toHaveBeenCalled();
      expect(mockContext.fill).toHaveBeenCalled();
    });

    it('should draw current position line when not closed', () => {
      component.isClosed = false;
      component.currentPos = { x: 300, y: 300 };

      (component as any).draw();

      expect(mockContext.lineTo).toHaveBeenCalledWith(300, 300);
    });
  });

  describe('Mouse Event Handling', () => {
    let mockMouseEvent: MouseEvent;

    beforeEach(() => {
      component.imageInValidState = true;
      component.coordinates = [...mockCoordinates];

      mockMouseEvent = {
        clientX: 150,
        clientY: 150,
        preventDefault: jasmine.createSpy('preventDefault'),
      } as any;

      spyOn(component, 'getMousePosition' as any).and.returnValue({ x: 150, y: 150 });
    });

    it('should handle mouse down for dragging point', () => {
      component.isClosed = true;
      // Don't spy on findClosestPoint, set up coordinates so it naturally finds a close point
      component.coordinates = [{ x: 150, y: 150 }]; // Close to mock mouse position

      (component as any).handleMouseDown(mockMouseEvent);

      expect(component.isDragging).toBe(true);
      expect(component.dragStartIndex).toBe(0);
    });

    it('should handle mouse down for dragging polygon', () => {
      component.isClosed = true;
      spyOn(component, 'isPointInPolygon').and.returnValue(true);
      spyOn(component, 'findClosestPoint' as any).and.returnValue(-1);

      (component as any).handleMouseDown(mockMouseEvent);

      expect(component.isDragging).toBe(true);
      expect(component.dragStartIndex).toBe(-1);
      expect(mockCanvas.style.cursor).toBe('move');
    });

    it('should handle mouse move when dragging point', () => {
      component.isDragging = true;
      component.dragStartIndex = 0;
      spyOn(component, 'movePoint');

      (component as any).handleMouseMove(mockMouseEvent);

      expect(component.movePoint).toHaveBeenCalledWith(0, 150, 150);
    });

    it('should handle mouse move when drawing polygon', () => {
      component.isClosed = false;
      component.isDragging = false;
      spyOn(component, 'draw' as any);

      (component as any).handleMouseMove(mockMouseEvent);

      expect(component.currentPos).toEqual({ x: 150, y: 150 });
      expect((component as any).draw).toHaveBeenCalled();
    });

    it('should handle mouse up', () => {
      component.isDragging = true;
      component.dragStartIndex = 2;

      (component as any).handleMouseUp();

      expect(component.isDragging).toBe(false);
      expect(component.dragStartIndex).toBe(-1);
      expect(mockCanvas.style.cursor).toBe('default');
    });

    it('should handle click to add point', () => {
      component.isClosed = false;
      component.isDragging = false;
      spyOn(component, 'isNearPoint' as any).and.returnValue(false);
      spyOn(component, 'addPoint' as any);

      (component as any).handleClick(mockMouseEvent);

      expect((component as any).addPoint).toHaveBeenCalledWith(150, 150);
    });

    it('should handle click to close polygon', () => {
      component.isClosed = false;
      component.isDragging = false;
      spyOn(component, 'isNearPoint' as any).and.returnValue(true);
      spyOn(component, 'closePolygon' as any);

      (component as any).handleClick(mockMouseEvent);

      expect((component as any).closePolygon).toHaveBeenCalled();
    });

    it('should handle mouse leave', () => {
      component.isDragging = true;
      component.dragStartIndex = 1;
      spyOn(component, 'draw' as any);

      (component as any).handleMouseLeave();

      expect(component.isDragging).toBe(false);
      expect(component.dragStartIndex).toBe(-1);
      expect(component.currentPos).toEqual({ x: 0, y: 0 });
      expect(mockCanvas.style.cursor).toBe('default');
      expect((component as any).draw).toHaveBeenCalled();
    });

    it('should not handle events when image is invalid', () => {
      component.imageInValidState = false;
      spyOn(component, 'addPoint' as any);

      (component as any).handleClick(mockMouseEvent);
      (component as any).handleMouseDown(mockMouseEvent);
      (component as any).handleMouseMove(mockMouseEvent);

      expect((component as any).addPoint).not.toHaveBeenCalled();
      expect(component.isDragging).toBe(false);
    });

    it('should get correct mouse position', () => {
      ((component as any).getMousePosition as jasmine.Spy).and.callThrough();

      (mockCanvas.getBoundingClientRect as jasmine.Spy).calls.reset();

      const result = (component as any).getMousePosition(mockMouseEvent);

      expect(result).toEqual({ x: 150, y: 150 });
      expect(mockCanvas.getBoundingClientRect).toHaveBeenCalled();
    });
  });

  describe('Service Integration', () => {
    beforeEach(() => {
      component.ctx = mockContext;
      component.image = { width: 1920, height: 1080 } as any;
    });

    it('should call service when loading sample plates', () => {
      spyOn(component, 'loadImageIntoCanvas' as any);

      (component as any).getSamplePlates();

      expect(mockCameraMaskService.getPlateCaptures).toHaveBeenCalledWith('test-camera-id');
    });

    it('should call service when loading mask coordinates', () => {
      component.coordinates = [];

      (component as any).loadMaskCoordinates();

      expect(mockCameraMaskService.getCameraMaskCoordinates).toHaveBeenCalledWith('test-camera-id');
    });
  });
});
