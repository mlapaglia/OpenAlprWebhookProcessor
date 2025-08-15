import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { PlateSettingsLoadingComponent, type LoadingConfig } from './plate-settings-loading.component';

describe('PlateSettingsLoadingComponent', () => {
  let component: PlateSettingsLoadingComponent;
  let fixture: ComponentFixture<PlateSettingsLoadingComponent>;

  const mockConfig: LoadingConfig = {
    title: 'Test Rules',
    entityName: 'test rule',
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlateSettingsLoadingComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(PlateSettingsLoadingComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('config', mockConfig);
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should show loading overlay when isLoading is true', async () => {
    fixture.componentRef.setInput('isLoading', true);
    fixture.componentRef.setInput('hasError', false);
    fixture.detectChanges();
    await fixture.whenStable();

    const loadingOverlay = fixture.debugElement.query(By.css('.loading-overlay'));
    const spinner = fixture.debugElement.query(By.css('mat-spinner'));
    const loadingText = fixture.debugElement.query(By.css('.loading-text'));

    expect(loadingOverlay).toBeTruthy();
    expect(spinner).toBeTruthy();
    expect(loadingText.nativeElement.textContent.trim()).toBe('Loading test rules...');
  });

  it('should not show loading overlay when isLoading is false', () => {
    fixture.componentRef.setInput('isLoading', false);
    fixture.detectChanges();

    const loadingOverlay = fixture.debugElement.query(By.css('.loading-overlay'));

    expect(loadingOverlay).toBeFalsy();
  });

  it('should show error state when hasError is true and not loading', async () => {
    fixture.componentRef.setInput('hasError', true);
    fixture.componentRef.setInput('isLoading', false);
    fixture.detectChanges();
    await fixture.whenStable();

    const errorCard = fixture.debugElement.query(By.css('.error-card'));
    const errorIcon = fixture.debugElement.query(By.css('.error-icon'));
    const retryButton = fixture.debugElement.query(By.css('.retry-button'));

    expect(errorCard).toBeTruthy();
    expect(errorIcon).toBeTruthy();
    expect(errorIcon.nativeElement.textContent.trim()).toBe('error_outline');
    expect(retryButton).toBeTruthy();
  });

  it('should not show error state when hasError is true but isLoading is also true', () => {
    fixture.componentRef.setInput('hasError', true);
    fixture.componentRef.setInput('isLoading', true);
    fixture.detectChanges();

    const errorCard = fixture.debugElement.query(By.css('.error-card'));

    expect(errorCard).toBeFalsy();
  });

  it('should not show error state when hasError is false', () => {
    fixture.componentRef.setInput('hasError', false);
    fixture.componentRef.setInput('isLoading', false);
    fixture.detectChanges();

    const errorCard = fixture.debugElement.query(By.css('.error-card'));

    expect(errorCard).toBeFalsy();
  });

  it('should display correct error title from config', async () => {
    fixture.componentRef.setInput('hasError', true);
    fixture.componentRef.setInput('isLoading', false);
    fixture.detectChanges();
    await fixture.whenStable();

    const titleElement = fixture.debugElement.query(By.css('h3'));
    expect(titleElement.nativeElement.textContent.trim()).toBe('Unable to Load Test Rules');
  });

  it('should display correct error message with entity name from config', async () => {
    fixture.componentRef.setInput('hasError', true);
    fixture.componentRef.setInput('isLoading', false);
    fixture.detectChanges();
    await fixture.whenStable();

    const messageElement = fixture.debugElement.query(By.css('p'));
    expect(messageElement.nativeElement.textContent.trim()).toContain('There was a problem loading your test rules.');
  });

  it('should emit retry event when retry button is clicked', async () => {
    spyOn(component.retry, 'emit');
    fixture.componentRef.setInput('hasError', true);
    fixture.componentRef.setInput('isLoading', false);
    fixture.detectChanges();
    await fixture.whenStable();

    const retryButton = fixture.debugElement.query(By.css('.retry-button'));
    retryButton.nativeElement.click();

    expect(component.retry.emit).toHaveBeenCalled();
  });

  it('should show refresh icon in retry button', async () => {
    fixture.componentRef.setInput('hasError', true);
    fixture.componentRef.setInput('isLoading', false);
    fixture.detectChanges();
    await fixture.whenStable();

    const retryButtonIcon = fixture.debugElement.query(By.css('.retry-button mat-icon'));
    expect(retryButtonIcon.nativeElement.textContent.trim()).toBe('refresh');
  });

  it('should not show any content when both isLoading and hasError are false', () => {
    fixture.componentRef.setInput('isLoading', false);
    fixture.componentRef.setInput('hasError', false);
    fixture.detectChanges();

    const loadingOverlay = fixture.debugElement.query(By.css('.loading-overlay'));
    const errorCard = fixture.debugElement.query(By.css('.error-card'));

    expect(loadingOverlay).toBeFalsy();
    expect(errorCard).toBeFalsy();
  });
});
