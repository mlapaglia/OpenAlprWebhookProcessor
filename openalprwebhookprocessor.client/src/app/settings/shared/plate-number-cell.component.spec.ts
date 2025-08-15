import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { PlateNumberCellComponent } from './plate-number-cell.component';
import type { IPlateSetting } from '../shared/plate-setting.interface';

interface TestSetting extends IPlateSetting {
  testProp: string;
}

describe('PlateNumberCellComponent', () => {
  let component: PlateNumberCellComponent<TestSetting>;
  let fixture: ComponentFixture<PlateNumberCellComponent<TestSetting>>;

  const createMockSetting = (overrides: Partial<TestSetting> = {}): TestSetting => ({
    id: '1',
    plateNumber: 'ABC123',
    strictMatch: true,
    description: 'Test description',
    testProp: 'test',
    ...overrides,
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        PlateNumberCellComponent,
        FormsModule,
        NoopAnimationsModule,
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PlateNumberCellComponent<TestSetting>);
    component = fixture.componentInstance;

    // Set initial values
    fixture.componentRef.setInput('setting', createMockSetting());
    fixture.componentRef.setInput('isEditing', false);
    fixture.componentRef.setInput('editingSetting', null);

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display plate number when not editing', () => {
    fixture.componentRef.setInput('setting', createMockSetting({ plateNumber: 'ABC123' }));
    fixture.componentRef.setInput('isEditing', false);
    fixture.detectChanges();

    const plateNumberElement = fixture.debugElement.query(By.css('.plate-number'));
    expect(plateNumberElement).toBeTruthy();
    expect(plateNumberElement.nativeElement.textContent.trim()).toBe('ABC123');
  });

  it('should display "Strict" chip when strictMatch is true and not editing', () => {
    fixture.componentRef.setInput('setting', createMockSetting({ strictMatch: true }));
    fixture.componentRef.setInput('isEditing', false);
    fixture.detectChanges();

    const chipElement = fixture.debugElement.query(By.css('mat-chip'));
    expect(chipElement).toBeTruthy();
    expect(chipElement.nativeElement.textContent.trim()).toBe('Strict');
  });

  it('should display "Lenient" chip when strictMatch is false and not editing', () => {
    fixture.componentRef.setInput('setting', createMockSetting({ strictMatch: false }));
    fixture.componentRef.setInput('isEditing', false);
    fixture.detectChanges();

    const chipElement = fixture.debugElement.query(By.css('mat-chip'));
    expect(chipElement).toBeTruthy();
    expect(chipElement.nativeElement.textContent.trim()).toBe('Lenient');
  });

  it('should not show edit fields when not editing', () => {
    fixture.componentRef.setInput('isEditing', false);
    fixture.detectChanges();

    const editElement = fixture.debugElement.query(By.css('.plate-edit'));
    expect(editElement).toBeFalsy();
  });

  it('should show edit fields when editing and editingSetting is provided', () => {
    fixture.componentRef.setInput('isEditing', true);
    fixture.componentRef.setInput('editingSetting', createMockSetting());
    fixture.detectChanges();

    const editElement = fixture.debugElement.query(By.css('.plate-edit'));
    const plateInputElement = fixture.debugElement.query(By.css('input'));
    const matchSelectElement = fixture.debugElement.query(By.css('mat-select'));

    expect(editElement).toBeTruthy();
    expect(plateInputElement).toBeTruthy();
    expect(matchSelectElement).toBeTruthy();
  });

  it('should not show edit fields when editing but editingSetting is null', () => {
    fixture.componentRef.setInput('isEditing', true);
    fixture.componentRef.setInput('editingSetting', null);

    fixture.detectChanges();

    const editElement = fixture.debugElement.query(By.css('.plate-edit'));
    expect(editElement).toBeFalsy();
  });

  it('should bind editingSetting plateNumber to input', async () => {
    const editingSetting = createMockSetting({ plateNumber: 'XYZ789' });
    fixture.componentRef.setInput('isEditing', true);
    fixture.componentRef.setInput('editingSetting', editingSetting);

    fixture.detectChanges();

    // Wait for ngModel to complete its binding
    await fixture.whenStable();
    fixture.detectChanges();

    const inputElement = fixture.debugElement.query(By.css('input'));
    expect(inputElement.nativeElement.value).toBe('XYZ789');
  });

  it('should update editingSetting when plate number input changes', async () => {
    const editingSetting = createMockSetting({ plateNumber: 'ABC123' });
    fixture.componentRef.setInput('isEditing', true);
    fixture.componentRef.setInput('editingSetting', editingSetting);

    fixture.detectChanges();
    await fixture.whenStable();

    const inputElement = fixture.debugElement.query(By.css('input'));

    // Simulate user input
    inputElement.nativeElement.value = 'DEF456';
    inputElement.nativeElement.dispatchEvent(new Event('input'));
    inputElement.triggerEventHandler('input', { target: inputElement.nativeElement });

    fixture.detectChanges();
    await fixture.whenStable();

    expect(component.editingSetting()?.plateNumber).toBe('DEF456');
  });

  it('should have correct input attributes', () => {
    fixture.componentRef.setInput('isEditing', true);
    fixture.componentRef.setInput('editingSetting', createMockSetting());
    fixture.detectChanges();

    const inputElement = fixture.debugElement.query(By.css('input'));
    expect(inputElement.nativeElement.maxLength).toBe(20);
    expect(inputElement.nativeElement.autocomplete).toBe('off');
    expect(inputElement.nativeElement.placeholder).toBe('Enter plate number');
  });

  it('should show correct select options', async () => {
    fixture.componentRef.setInput('isEditing', true);
    fixture.componentRef.setInput('editingSetting', createMockSetting());
    fixture.detectChanges();
    await fixture.whenStable();

    // Find the mat-select element
    const selectElement = fixture.debugElement.query(By.css('mat-select'));
    expect(selectElement).toBeTruthy();

    // Try multiple ways to open the select
    try {
      // Method 1: Use component method
      selectElement.componentInstance.open();
    } catch {
      try {
        // Method 2: Click the select element
        selectElement.nativeElement.click();
      } catch {
        // Method 3: Dispatch a click event
        selectElement.nativeElement.dispatchEvent(new Event('click'));
      }
    }

    fixture.detectChanges();
    await fixture.whenStable();

    // Query for options in the overlay
    const overlayContainer = document.querySelector('.cdk-overlay-container');
    const optionElements = overlayContainer?.querySelectorAll('mat-option');

    // If no options found in overlay, skip the specific text checks but verify the component has the right data
    if (optionElements && optionElements.length > 0) {
      expect(optionElements.length).toBe(2);
      expect(optionElements[0]?.textContent?.trim()).toBe('Strict Match');
      expect(optionElements[1]?.textContent?.trim()).toBe('Lenient Match');
    } else {
      // Fallback: Just verify the select has the correct options by checking the component
      const options = selectElement.queryAll(By.css('mat-option'));
      expect(options.length).toBe(2);
      expect(options[0].nativeElement.textContent.trim()).toBe('Strict Match');
      expect(options[1].nativeElement.textContent.trim()).toBe('Lenient Match');
    }

    // Clean up
    try {
      selectElement.componentInstance.close?.();
    } catch {
      // Ignore cleanup errors
    }
  });

  it('should bind strictMatch value to select', async () => {
    const editingSetting = createMockSetting({ strictMatch: false });
    fixture.componentRef.setInput('isEditing', true);
    fixture.componentRef.setInput('editingSetting', editingSetting);
    fixture.detectChanges();
    await fixture.whenStable();

    const selectElement = fixture.debugElement.query(By.css('mat-select'));
    expect(selectElement.componentInstance.value).toBe(false);
  });

  it('should update editingSetting when select value changes', async () => {
    const editingSetting = createMockSetting({ strictMatch: true });
    fixture.componentRef.setInput('isEditing', true);
    fixture.componentRef.setInput('editingSetting', editingSetting);
    fixture.detectChanges();
    await fixture.whenStable();

    const selectElement = fixture.debugElement.query(By.css('mat-select'));

    // Simulate selection change
    selectElement.componentInstance.value = false;
    selectElement.componentInstance.selectionChange.emit({
      value: false,
      source: selectElement.componentInstance,
    });

    fixture.detectChanges();
    await fixture.whenStable();

    expect(component.editingSetting()?.strictMatch).toBe(false);
  });
});
