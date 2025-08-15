import { TestBed } from '@angular/core/testing';
import type { ComponentFixture } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { SettingsHeaderComponent } from './settings-header.component';
import type { PlateSettingsConfig } from './plate-settings-table.component';
import type { IPlateSetting } from '../shared/plate-setting.interface';

interface TestSetting extends IPlateSetting {
  testProp: string;
}

describe('SettingsHeaderComponent', () => {
  let component: SettingsHeaderComponent<TestSetting>;
  let fixture: ComponentFixture<SettingsHeaderComponent<TestSetting>>;

  const mockConfig: PlateSettingsConfig<TestSetting> = {
    title: 'Test Rules',
    subtitle: 'Manage your test rules',
    entityName: 'test rule',
    emptyStateTitle: 'No Test Rules',
    emptyStateDescription: 'Description',
    addButtonText: 'Add Test Rule',
    createNew: () => ({ id: '', plateNumber: '', strictMatch: true, description: '', testProp: '' }),
    service: {
      getAll: () => ({ subscribe: () => {/*empty*/} } as any),
      upsert: () => ({ subscribe: () => {/*empty*/} } as any),
    },
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SettingsHeaderComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(SettingsHeaderComponent<TestSetting>);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('config', mockConfig);
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display correct title from config', () => {
    fixture.detectChanges();
    const titleElement = fixture.debugElement.query(By.css('mat-card-title'));
    expect(titleElement.nativeElement.textContent.trim()).toBe('Current Test Rules');
  });

  it('should display singular entity name when itemCount is 1', () => {
    fixture.componentRef.setInput('itemCount', 1);
    fixture.detectChanges();

    const subtitleElement = fixture.debugElement.query(By.css('mat-card-subtitle'));
    expect(subtitleElement.nativeElement.textContent.trim()).toBe('1 test rule configured');
  });

  it('should display plural entity name when itemCount is greater than 1', () => {
    fixture.componentRef.setInput('itemCount', 3);
    fixture.detectChanges();

    const subtitleElement = fixture.debugElement.query(By.css('mat-card-subtitle'));
    expect(subtitleElement.nativeElement.textContent.trim()).toBe('3 test rules configured');
  });

  it('should display plural entity name when itemCount is 0', () => {
    fixture.componentRef.setInput('itemCount', 0);
    fixture.detectChanges();

    const subtitleElement = fixture.debugElement.query(By.css('mat-card-subtitle'));
    expect(subtitleElement.nativeElement.textContent.trim()).toBe('0 test rules configured');
  });

  it('should disable save button when itemCount is 0', () => {
    fixture.componentRef.setInput('itemCount', 0);
    fixture.detectChanges();

    const buttonElement = fixture.debugElement.query(By.css('button'));
    expect(buttonElement.nativeElement.disabled).toBe(true);
  });

  it('should disable save button when isSaving is true', () => {
    fixture.componentRef.setInput('isSaving', true);
    fixture.componentRef.setInput('itemCount', 1);
    fixture.detectChanges();

    const buttonElement = fixture.debugElement.query(By.css('button'));
    expect(buttonElement.nativeElement.disabled).toBe(true);
  });

  it('should enable save button when itemCount > 0 and not saving', () => {
    fixture.componentRef.setInput('itemCount', 1);
    fixture.componentRef.setInput('isSaving', false);
    fixture.detectChanges();

    const buttonElement = fixture.debugElement.query(By.css('button'));
    expect(buttonElement.nativeElement.disabled).toBe(false);
  });

  it('should show save icon when not saving', () => {
    fixture.componentRef.setInput('isSaving', false);
    fixture.detectChanges();

    const iconElement = fixture.debugElement.query(By.css('mat-icon'));
    const spinnerElement = fixture.debugElement.query(By.css('mat-spinner'));

    expect(iconElement).toBeTruthy();
    expect(iconElement.nativeElement.textContent.trim()).toBe('save');
    expect(spinnerElement).toBeFalsy();
  });

  it('should show spinner when saving', () => {
    fixture.componentRef.setInput('isSaving', true);
    fixture.detectChanges();

    const iconElement = fixture.debugElement.query(By.css('mat-icon'));
    const spinnerElement = fixture.debugElement.query(By.css('mat-spinner'));

    expect(iconElement).toBeFalsy();
    expect(spinnerElement).toBeTruthy();
  });

  it('should show correct button text when not saving', () => {
    fixture.componentRef.setInput('isSaving', false);
    fixture.detectChanges();

    const buttonElement = fixture.debugElement.query(By.css('button'));
    expect(buttonElement.nativeElement.textContent).toContain('Save All Changes');
  });

  it('should show correct button text when saving', () => {
    fixture.componentRef.setInput('isSaving', true);
    fixture.detectChanges();

    const buttonElement = fixture.debugElement.query(By.css('button'));
    expect(buttonElement.nativeElement.textContent).toContain('Saving...');
  });

  it('should emit saveSettings event when button is clicked', () => {
    spyOn(component.saveSettings, 'emit');
    fixture.componentRef.setInput('itemCount', 1);
    fixture.componentRef.setInput('isSaving', false);
    fixture.detectChanges();

    const buttonElement = fixture.debugElement.query(By.css('button'));
    expect(buttonElement.nativeElement.disabled).toBeFalsy();

    buttonElement.nativeElement.click();

    expect(component.saveSettings.emit).toHaveBeenCalled();
  });

  it('should emit saveSettings event when onSaveSettings is called', () => {
    spyOn(component.saveSettings, 'emit');

    component.onSaveSettings();

    expect(component.saveSettings.emit).toHaveBeenCalled();
  });
});
