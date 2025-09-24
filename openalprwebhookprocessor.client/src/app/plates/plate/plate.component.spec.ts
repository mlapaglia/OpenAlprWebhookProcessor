import type { ComponentFixture } from '@angular/core/testing';
import { TestBed } from '@angular/core/testing';
import { PlateComponent } from './plate.component';
import { Lightbox, LightboxConfig, LightboxEvent } from 'ngx-lightbox';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { DatePipe } from '@angular/common';
import { PlateService } from '../plate.service';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('PlateComponent', () => {
  let component: PlateComponent;
  let fixture: ComponentFixture<PlateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlateComponent],
      providers: [
        DatePipe,
        Lightbox,
        LightboxConfig,
        LightboxEvent,
        PlateService,
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
      ],
    })
      .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PlateComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('plate', {
      id: 123,
      plateNumber: 'TEST123',
      vehicleDescription: 'Test Vehicle',
      openAlprCameraId: 'test-camera',
      direction: 90,
      receivedOn: new Date(),
      isAlert: false,
      isIgnore: false,
      isOpen: false,
      cropImageUrl: 'http://test.com/crop.jpg',
      imageUrl: 'http://test.com/image.jpg',
      processedPlateConfidence: 95,
      canBeEnriched: false,
    });
    fixture.componentRef.setInput('isVisible', false);

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
