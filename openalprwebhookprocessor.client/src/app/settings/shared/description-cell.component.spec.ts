import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { DescriptionCellComponent } from './description-cell.component';
import type { IPlateSetting } from '../shared/plate-setting.interface';

interface TestSetting extends IPlateSetting {
  testProp: string;
}

describe('DescriptionCellComponent', () => {
  let component: DescriptionCellComponent<TestSetting>;
  let fixture: ComponentFixture<DescriptionCellComponent<TestSetting>>;

  const mockSetting: TestSetting = {
    id: '1',
    plateNumber: 'ABC123',
    strictMatch: true,
    description: 'Test description',
    testProp: 'test',
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DescriptionCellComponent, FormsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(DescriptionCellComponent<TestSetting>);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('setting', mockSetting);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display description text when not editing', () => {
    fixture.componentRef.setInput('isEditing', false);
    fixture.detectChanges();

    const descriptionElement = fixture.debugElement.query(By.css('.description-text'));
    expect(descriptionElement).toBeTruthy();
    expect(descriptionElement.nativeElement.textContent.trim()).toBe('Test description');
  });

  it('should display "No description provided" when description is empty and not editing', () => {
    fixture.componentRef.setInput('setting', { ...mockSetting, description: '' });
    fixture.componentRef.setInput('isEditing', false);
    fixture.detectChanges();

    const descriptionElement = fixture.debugElement.query(By.css('.description-text'));
    expect(descriptionElement.nativeElement.textContent.trim()).toBe('No description provided');
    expect(descriptionElement.nativeElement.classList).toContain('no-description');
  });

  it('should display "No description provided" when description is undefined and not editing', () => {
    fixture.componentRef.setInput('setting', { ...mockSetting, description: undefined as any });
    fixture.componentRef.setInput('isEditing', false);
    fixture.detectChanges();

    const descriptionElement = fixture.debugElement.query(By.css('.description-text'));
    expect(descriptionElement.nativeElement.textContent.trim()).toBe('No description provided');
    expect(descriptionElement.nativeElement.classList).toContain('no-description');
  });

  it('should not show edit field when not editing', () => {
    fixture.componentRef.setInput('isEditing', false);
    fixture.detectChanges();

    const editFieldElement = fixture.debugElement.query(By.css('mat-form-field'));
    expect(editFieldElement).toBeFalsy();
  });

  it('should show edit field when editing and editingSetting is provided', () => {
    fixture.componentRef.setInput('isEditing', true);
    fixture.componentRef.setInput('editingSetting', { ...mockSetting });
    fixture.detectChanges();

    const editFieldElement = fixture.debugElement.query(By.css('mat-form-field'));
    const textareaElement = fixture.debugElement.query(By.css('textarea'));

    expect(editFieldElement).toBeTruthy();
    expect(textareaElement).toBeTruthy();
  });

  it('should not show edit field when editing but editingSetting is null', () => {
    fixture.componentRef.setInput('isEditing', true);
    fixture.componentRef.setInput('editingSetting', null);
    fixture.detectChanges();

    const editFieldElement = fixture.debugElement.query(By.css('mat-form-field'));
    expect(editFieldElement).toBeFalsy();
  });

  it('should bind editingSetting description to textarea', async () => {
    const editingSetting = { ...mockSetting, description: 'Editable description' };
    fixture.componentRef.setInput('isEditing', true);
    fixture.componentRef.setInput('editingSetting', editingSetting);
    fixture.detectChanges();

    // Wait for ngModel to complete its binding
    await fixture.whenStable();
    fixture.detectChanges();

    const textareaElement = fixture.debugElement.query(By.css('textarea'));
    expect(textareaElement.nativeElement.value).toBe('Editable description');
  });

  it('should update editingSetting when textarea value changes', () => {
    const editingSetting = { ...mockSetting, description: 'Original description' };
    fixture.componentRef.setInput('isEditing', true);
    fixture.componentRef.setInput('editingSetting', editingSetting);
    fixture.detectChanges();

    const textareaElement = fixture.debugElement.query(By.css('textarea'));
    textareaElement.nativeElement.value = 'Updated description';
    textareaElement.nativeElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    expect(component.editingSetting()?.description).toBe('Updated description');
  });

  it('should have correct textarea attributes', () => {
    fixture.componentRef.setInput('isEditing', true);
    fixture.componentRef.setInput('editingSetting', mockSetting);
    fixture.detectChanges();

    const textareaElement = fixture.debugElement.query(By.css('textarea'));
    expect(textareaElement.nativeElement.rows).toBe(2);
    expect(textareaElement.nativeElement.maxLength).toBe(200);
    expect(textareaElement.nativeElement.placeholder).toBe('Add description...');
  });
});
