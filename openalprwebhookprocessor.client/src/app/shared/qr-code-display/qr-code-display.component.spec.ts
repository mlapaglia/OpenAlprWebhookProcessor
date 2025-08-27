import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { By } from '@angular/platform-browser';

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

    it('should call generateQrCode on init when qrCodeData is provided', () => {
      spyOn(component, 'generateQrCode');
      fixture.componentRef.setInput('qrCodeData', 'otpauth://totp/test:user@example.com?secret=TESTSECRET');

      component.ngOnInit();

      expect(component.generateQrCode).toHaveBeenCalled();
    });

    it('should not call generateQrCode on init when qrCodeData is empty', () => {
      spyOn(component, 'generateQrCode');
      fixture.componentRef.setInput('qrCodeData', '');

      component.ngOnInit();

      expect(component.generateQrCode).not.toHaveBeenCalled();
    });
  });

  describe('QR code generation', () => {
    it('should not generate QR code when qrCodeData is empty', async () => {
      fixture.componentRef.setInput('qrCodeData', '');

      await component.generateQrCode();

      expect(component.qrCodeImageUrl).toBe('');
      expect(component.loading).toBe(false);
    });

    it('should successfully generate QR code with valid data', async () => {
      fixture.componentRef.setInput('qrCodeData', 'otpauth://totp/TestApp:user@example.com?secret=TESTSECRET&issuer=TestApp');

      await component.generateQrCode();

      expect(component.loading).toBe(false);
      expect(component.error).toBe('');
      expect(component.qrCodeImageUrl).toContain('data:image/png;base64,');
    });

    it('should clear previous error on new generation attempt', async () => {
      // First, set an error
      component.error = 'Previous error';

      fixture.componentRef.setInput('qrCodeData', 'otpauth://totp/test:user@example.com?secret=TESTSECRET');

      await component.generateQrCode();

      expect(component.error).toBe('');
    });
  });

  describe('clipboard functionality', () => {
    it('should not attempt to copy when sharedKey is empty', async () => {
      fixture.componentRef.setInput('sharedKey', '');

      // This should complete without throwing an error
      await expectAsync(component.copyToClipboard()).toBeResolved();
    });


  });

  describe('template rendering', () => {
    it('should show loading spinner when loading is true', () => {
      component.loading = true;
      fixture.detectChanges();

      const loadingElement = fixture.debugElement.query(By.css('.loading'));
      const spinner = fixture.debugElement.query(By.css('mat-spinner'));

      expect(loadingElement).toBeTruthy();
      expect(spinner).toBeTruthy();
    });

    it('should show error message when error is set', () => {
      component.error = 'Test error message';
      fixture.detectChanges();

      const errorElement = fixture.debugElement.query(By.css('.error'));
      const errorText = errorElement.query(By.css('p'));

      expect(errorElement).toBeTruthy();
      expect(errorText.nativeElement.textContent.trim()).toBe('Test error message');
    });

    it('should display QR code image when qrCodeImageUrl is set', () => {
      component.qrCodeImageUrl = 'data:image/png;base64,testimage';
      component.loading = false;
      component.error = '';
      fixture.detectChanges();

      const qrImage = fixture.debugElement.query(By.css('.qr-code'));

      expect(qrImage).toBeTruthy();
      expect(qrImage.nativeElement.src).toBe('data:image/png;base64,testimage');
      expect(qrImage.nativeElement.alt).toBe('QR Code for two-factor authentication setup');
    });

    it('should display shared key card when sharedKey is provided', () => {
      fixture.componentRef.setInput('sharedKey', 'TESTSHAREDKEY123');
      component.qrCodeImageUrl = 'data:image/png;base64,test';
      component.loading = false;
      component.error = '';
      fixture.detectChanges();

      const sharedKeyCard = fixture.debugElement.query(By.css('.shared-key-card'));
      const sharedKeyCode = fixture.debugElement.query(By.css('.shared-key-code'));

      expect(sharedKeyCard).toBeTruthy();
      expect(sharedKeyCode.nativeElement.textContent.trim()).toBe('TESTSHAREDKEY123');
    });

    it('should not display shared key card when sharedKey is empty', () => {
      fixture.componentRef.setInput('sharedKey', '');
      component.qrCodeImageUrl = 'data:image/png;base64,test';
      component.loading = false;
      component.error = '';
      fixture.detectChanges();

      const sharedKeyCard = fixture.debugElement.query(By.css('.shared-key-card'));

      expect(sharedKeyCard).toBeFalsy();
    });

    it('should trigger copyToClipboard when copy button is clicked', () => {
      const copyToClipboardSpy = spyOn(component, 'copyToClipboard').and.stub();

      fixture.componentRef.setInput('sharedKey', 'TESTKEY');
      component.qrCodeImageUrl = 'data:image/png;base64,test';
      component.loading = false;
      component.error = '';
      fixture.detectChanges();

      const copyButton = fixture.debugElement.query(By.css('.copy-button'));
      copyButton.nativeElement.click();

      expect(copyToClipboardSpy).toHaveBeenCalled();
    });

    it('should not display QR code or shared key when loading', () => {
      fixture.componentRef.setInput('sharedKey', 'TESTKEY');
      component.qrCodeImageUrl = 'data:image/png;base64,test';
      component.loading = true;
      component.error = '';
      fixture.detectChanges();

      const qrDisplay = fixture.debugElement.query(By.css('.qr-code-display'));

      expect(qrDisplay).toBeFalsy();
    });

    it('should not display QR code or shared key when error is present', () => {
      fixture.componentRef.setInput('sharedKey', 'TESTKEY');
      component.qrCodeImageUrl = 'data:image/png;base64,test';
      component.loading = false;
      component.error = 'Test error';
      fixture.detectChanges();

      const qrDisplay = fixture.debugElement.query(By.css('.qr-code-display'));

      expect(qrDisplay).toBeFalsy();
    });
  });
});
