import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';

import { CameraImageComponent } from './camera-image.component';

describe('CameraImageComponent', () => {
  let component: CameraImageComponent;
  let fixture: ComponentFixture<CameraImageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CameraImageComponent],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CameraImageComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display image when imageDataUrl is provided and not loading or failed', () => {
    fixture.componentRef.setInput('imageDataUrl', 'data:image/jpeg;base64,test');
    fixture.componentRef.setInput('isLoadingImage', false);
    fixture.componentRef.setInput('isLoadingFailed', false);

    fixture.detectChanges();

    const imgElement = fixture.debugElement.query(By.css('img'));
    expect(imgElement).toBeTruthy();
    expect(imgElement.nativeElement.src).toBe('data:image/jpeg;base64,test');
  });

  it('should not display image when loading', () => {
    fixture.componentRef.setInput('imageDataUrl', 'data:image/jpeg;base64,test');
    fixture.componentRef.setInput('isLoadingImage', true);
    fixture.componentRef.setInput('isLoadingFailed', false);

    fixture.detectChanges();

    const imgElement = fixture.debugElement.query(By.css('img'));
    expect(imgElement).toBeFalsy();
  });

  it('should not display image when failed', () => {
    fixture.componentRef.setInput('imageDataUrl', 'data:image/jpeg;base64,test');
    fixture.componentRef.setInput('isLoadingImage', false);
    fixture.componentRef.setInput('isLoadingFailed', true);

    fixture.detectChanges();

    const imgElement = fixture.debugElement.query(By.css('img'));
    expect(imgElement).toBeFalsy();
  });

  it('should show loading spinner when isLoadingImage is true', () => {
    fixture.componentRef.setInput('isLoadingImage', true);

    fixture.detectChanges();

    const spinnerElement = fixture.debugElement.query(By.css('mat-progress-spinner'));
    expect(spinnerElement).toBeTruthy();
  });

  it('should not show loading spinner when not loading', () => {
    fixture.componentRef.setInput('isLoadingImage', false);

    fixture.detectChanges();

    const spinnerElement = fixture.debugElement.query(By.css('mat-progress-spinner'));
    expect(spinnerElement).toBeFalsy();
  });

  it('should show help icon when no sample image URL is provided', () => {
    fixture.componentRef.setInput('sampleImageUrl', undefined);
    fixture.componentRef.setInput('isLoadingImage', false);
    fixture.componentRef.setInput('isLoadingFailed', false);

    fixture.detectChanges();

    const iconElement = fixture.debugElement.query(By.css('mat-icon'));
    expect(iconElement).toBeTruthy();
    expect(iconElement.nativeElement.textContent.trim()).toBe('help_center');
  });

  it('should show help icon when loading failed and not currently loading', () => {
    fixture.componentRef.setInput('sampleImageUrl', 'http://example.com/image.jpg');
    fixture.componentRef.setInput('isLoadingImage', false);
    fixture.componentRef.setInput('isLoadingFailed', true);

    fixture.detectChanges();

    const iconElement = fixture.debugElement.query(By.css('mat-icon'));
    expect(iconElement).toBeTruthy();
    expect(iconElement.nativeElement.textContent.trim()).toBe('help_center');
  });

  it('should not show help icon when sample image URL exists and not failed', () => {
    fixture.componentRef.setInput('sampleImageUrl', 'http://example.com/image.jpg');
    fixture.componentRef.setInput('isLoadingImage', false);
    fixture.componentRef.setInput('isLoadingFailed', false);

    fixture.detectChanges();

    const iconElement = fixture.debugElement.query(By.css('mat-icon'));
    expect(iconElement).toBeFalsy();
  });
});
