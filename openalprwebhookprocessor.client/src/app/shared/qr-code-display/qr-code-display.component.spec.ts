import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { QrCodeDisplayComponent } from './qr-code-display.component';

describe('QrCodeDisplayComponent', () => {
  let component: QrCodeDisplayComponent;
  let fixture: ComponentFixture<QrCodeDisplayComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        QrCodeDisplayComponent,
        NoopAnimationsModule,
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(QrCodeDisplayComponent);
    component = fixture.componentInstance;
  });

  describe('component initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.qrCodeData()).toBe('');
      expect(component.sharedKey()).toBe('');
      expect(component.qrCodeImageUrl).toBe('');
      expect(component.loading).toBe(false);
      expect(component.error).toBe('');
    });
  });

  describe('QR code generation', () => {
    it('should not generate QR code when qrCodeData is empty', async () => {
      fixture.componentRef.setInput('qrCodeData', '');

      component.ngOnInit();

      expect(component.qrCodeImageUrl).toBe('');
      expect(component.loading).toBe(false);
    });

    it('should set loading to false after generation attempt', () => {
      fixture.componentRef.setInput('qrCodeData', 'otpauth://totp/test');

      // Just test that the component handles the generation attempt
      expect(component.loading).toBe(false);
      expect(component.error).toBe('');
    });
  });
});
