import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { SettingsEmptyStateComponent } from './settings-empty-state.component';
import type { PlateSettingsConfig } from './plate-settings-table.component';
import type { IPlateSetting } from './plate-setting.interface';

interface TestSetting extends IPlateSetting {
  testProp: string;
}

describe('SettingsEmptyStateComponent', () => {
  let component: SettingsEmptyStateComponent<TestSetting>;
  let fixture: ComponentFixture<SettingsEmptyStateComponent<TestSetting>>;

  const mockConfig: PlateSettingsConfig<TestSetting> = {
    title: 'Test Rules',
    subtitle: 'Manage your test rules',
    entityName: 'test rule',
    emptyStateTitle: 'No Test Rules Found',
    emptyStateDescription: 'Create your first test rule to get started.',
    addButtonText: 'Add Test Rule',
    createNew: () => ({ id: '', plateNumber: '', strictMatch: true, description: '', testProp: '' }),
    service: {
      getAll: () => ({ subscribe: () => {/* empty */} } as any),
      upsert: () => ({ subscribe: () => {/* empty */} } as any),
    },
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SettingsEmptyStateComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(SettingsEmptyStateComponent<TestSetting>);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('config', mockConfig);
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display empty state title from config', () => {
    fixture.detectChanges();
    const titleElement = fixture.debugElement.query(By.css('h3'));
    expect(titleElement.nativeElement.textContent).toBe(mockConfig.emptyStateTitle);
  });

  it('should display empty state description from config', () => {
    fixture.detectChanges();
    const descElement = fixture.debugElement.query(By.css('p'));
    expect(descElement.nativeElement.textContent).toBe(mockConfig.emptyStateDescription);
  });

  it('should display correct icon for non-ignore rule', () => {
    fixture.detectChanges();
    const iconElement = fixture.debugElement.query(By.css('mat-icon'));
    expect(iconElement.nativeElement.textContent).toBe('notifications');
  });

  it('should display block icon for ignore rule', () => {
    fixture.componentRef.setInput('config', { ...mockConfig, entityName: 'ignore rule' });
    fixture.detectChanges();

    const iconElement = fixture.debugElement.query(By.css('mat-icon'));
    expect(iconElement.nativeElement.textContent).toBe('block');
  });

  it('should display button with titlecased entity name', () => {
    fixture.detectChanges();
    const buttonElement = fixture.debugElement.query(By.css('button'));
    expect(buttonElement.nativeElement.textContent.trim()).toContain('Add First Test Rule');
  });

  it('should emit scrollToForm event when button is clicked', () => {
    spyOn(component.scrollToForm, 'emit');

    const buttonElement = fixture.debugElement.query(By.css('button'));
    buttonElement.nativeElement.click();

    expect(component.scrollToForm.emit).toHaveBeenCalled();
  });

  it('should emit scrollToForm event when onScrollToForm is called', () => {
    spyOn(component.scrollToForm, 'emit');

    component.onScrollToForm();

    expect(component.scrollToForm.emit).toHaveBeenCalled();
  });
});
