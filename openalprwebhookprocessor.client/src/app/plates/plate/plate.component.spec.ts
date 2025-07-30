import type { ComponentFixture } from '@angular/core/testing';
import { TestBed } from '@angular/core/testing';
import { PlateComponent } from './plate.component';
import { Lightbox, LightboxConfig, LightboxEvent } from 'ngx-lightbox';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { DatePipe } from '@angular/common';
import { Plate } from './plate';
import { PlateService } from '../plate.service';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('PlateComponent', () => {
  let component: PlateComponent;
  let fixture: ComponentFixture<PlateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        BrowserAnimationsModule,
        PlateComponent,
      ],
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
    component.plate = new Plate({
      id: 'test-id',
      plateNumber: 'TEST123',
      vehicleDescription: 'Test Vehicle',
      cropImageUrl: new URL('http://test.com/crop.jpg'),
      imageUrl: new URL('http://test.com/image.jpg'),
      processedPlateConfidence: 95,
      receivedOn: new Date(),
      canBeEnriched: false,
    });

    // Initialize image URLs to prevent 404s in tests
    component.vehicleImageUrl = 'http://test.com/image.jpg';
    component.plateImageUrl = 'http://test.com/crop.jpg';
    component.isVisible = false; // Prevent automatic image loading

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
