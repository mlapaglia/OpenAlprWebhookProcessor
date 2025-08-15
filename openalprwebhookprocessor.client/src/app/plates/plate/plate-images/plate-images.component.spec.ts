import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { PlateImagesComponent } from './plate-images.component';
import { Lightbox, LightboxConfig, LightboxEvent } from 'ngx-lightbox';
import { Plate } from '../plate';

describe('PlateImagesComponent', () => {
  let component: PlateImagesComponent;
  let fixture: ComponentFixture<PlateImagesComponent>;
  let lightbox: jasmine.SpyObj<Lightbox>;

  const mockPlate = new Plate({
    id: '1',
    plateNumber: 'ABC123',
    imageUrl: 'http://test.com/image.jpg',
    cropImageUrl: 'http://test.com/crop.jpg',
  });

  beforeEach(async () => {
    const lightboxSpy = jasmine.createSpyObj('Lightbox', ['open']);

    await TestBed.configureTestingModule({
      imports: [PlateImagesComponent],
      providers: [
        { provide: Lightbox, useValue: lightboxSpy },
        LightboxConfig,
        LightboxEvent,
      ],
    })
      .compileComponents();

    fixture = TestBed.createComponent(PlateImagesComponent);
    component = fixture.componentInstance;
    lightbox = TestBed.inject(Lightbox) as jasmine.SpyObj<Lightbox>;

    fixture.componentRef.setInput('plate', mockPlate);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with plate input', () => {
    expect(component.plate()).toEqual(mockPlate);
  });

  it('should open lightbox with image URL and plate number', () => {
    component.openLightbox('http://test.com/test.jpg', 'TEST123');

    expect(lightbox.open).toHaveBeenCalledWith(
      [
        {
          src: 'http://test.com/test.jpg',
          caption: 'TEST123',
          thumb: 'http://test.com/test.jpg',
        },
      ], 0);
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
