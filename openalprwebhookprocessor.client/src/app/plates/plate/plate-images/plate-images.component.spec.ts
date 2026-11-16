import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { PlateImagesComponent } from './plate-images.component';
import { MatDialog } from '@angular/material/dialog';
import { ImageViewerDialogComponent } from './image-viewer-dialog.component';
import { Plate } from '../plate';

describe('PlateImagesComponent', () => {
  let component: PlateImagesComponent;
  let fixture: ComponentFixture<PlateImagesComponent>;
  let dialog: jasmine.SpyObj<MatDialog>;

  const mockPlate = new Plate({
    id: '1',
    plateNumber: 'ABC123',
    imageUrl: 'http://test.com/image.jpg',
    cropImageUrl: 'http://test.com/crop.jpg',
  });

  beforeEach(async () => {
    const dialogSpy = jasmine.createSpyObj<MatDialog>('MatDialog', ['open']);

    await TestBed.configureTestingModule({
      imports: [PlateImagesComponent],
      providers: [{ provide: MatDialog, useValue: dialogSpy }],
    })
      .compileComponents();

    fixture = TestBed.createComponent(PlateImagesComponent);
    component = fixture.componentInstance;
    dialog = TestBed.inject(MatDialog) as jasmine.SpyObj<MatDialog>;

    fixture.componentRef.setInput('plate', mockPlate);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with plate input', () => {
    expect(component.plate()).toEqual(mockPlate);
  });

  it('should open image viewer dialog with image URL and plate number', () => {
    component.openLightbox('http://test.com/test.jpg', 'TEST123');

    expect(dialog.open).toHaveBeenCalledWith(
      ImageViewerDialogComponent,
      jasmine.objectContaining({
        data: { src: 'http://test.com/test.jpg', caption: 'TEST123' },
      }));
  });

  it('should handle vehicle image loading', () => {
    component.vehicleImageLoaded();
    expect(component.loadingVehicleImage).toBe(false);
  });

  it('should handle vehicle image loading failure', () => {
    component.vehicleImageFailedToLoad();
    expect(component.loadingVehicleImage).toBe(false);
    expect(component.loadingVehicleImageFailed).toBe(true);
  });

  it('should handle plate image loading', () => {
    component.plateImageLoaded();
    expect(component.loadingPlateImage).toBe(false);
  });

  it('should handle plate image loading failure', () => {
    component.plateImageFailedToLoad();
    expect(component.loadingPlateImage).toBe(false);
    expect(component.loadingPlateImageFailed).toBe(true);
  });
});
