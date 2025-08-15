import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { ChangeDetectorRef } from '@angular/core';
import { MatTooltip } from '@angular/material/tooltip';
import { ActionsCellComponent } from './actions-cell.component';
import type { PlateSettingsConfig } from './plate-settings-table.component';
import type { IPlateSetting } from './plate-setting.interface';

interface TestSetting extends IPlateSetting {
  testProp: string;
}

describe('ActionsCellComponent', () => {
  let component: ActionsCellComponent<TestSetting>;
  let fixture: ComponentFixture<ActionsCellComponent<TestSetting>>;
  let cdr: ChangeDetectorRef;

  const mockSetting: TestSetting = {
    id: '1',
    plateNumber: 'ABC123',
    strictMatch: true,
    description: 'Test description',
    testProp: 'test',
  };

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
      imports: [ActionsCellComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ActionsCellComponent<TestSetting>);
    component = fixture.componentInstance;
    cdr = fixture.componentRef.injector.get(ChangeDetectorRef);
    fixture.componentRef.setInput('setting', mockSetting);
    fixture.componentRef.setInput('config', mockConfig);
    fixture.componentRef.setInput('isEditing', false);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should show display actions when not editing', () => {
    fixture.componentRef.setInput('isEditing', false);
    fixture.detectChanges();

    const displayActions = fixture.debugElement.query(By.css('.display-actions'));
    const editActions = fixture.debugElement.query(By.css('.edit-actions'));

    expect(displayActions).toBeTruthy();
    expect(editActions).toBeFalsy();
  });

  it('should show edit actions when editing', () => {
    fixture.componentRef.setInput('isEditing', true);
    cdr.markForCheck();
    fixture.detectChanges();

    const displayActions = fixture.debugElement.query(By.css('.display-actions'));
    const editActions = fixture.debugElement.query(By.css('.edit-actions'));

    expect(displayActions).toBeFalsy();
    expect(editActions).toBeTruthy();
  });

  it('should show edit and delete buttons in display mode', () => {
    fixture.componentRef.setInput('isEditing', false);
    fixture.detectChanges();

    const buttons = fixture.debugElement.queryAll(By.css('.display-actions button'));
    const icons = fixture.debugElement.queryAll(By.css('.display-actions mat-icon'));

    expect(buttons.length).toBe(2);
    expect(icons[0].nativeElement.textContent.trim()).toBe('edit');
    expect(icons[1].nativeElement.textContent.trim()).toBe('delete');
  });

  it('should show save and cancel buttons in edit mode', () => {
    fixture.componentRef.setInput('isEditing', true);
    cdr.markForCheck();
    fixture.detectChanges();

    const buttons = fixture.debugElement.queryAll(By.css('.edit-actions button'));
    const icons = fixture.debugElement.queryAll(By.css('.edit-actions mat-icon'));

    expect(buttons.length).toBe(2);
    expect(icons[0].nativeElement.textContent.trim()).toBe('check');
    expect(icons[1].nativeElement.textContent.trim()).toBe('close');
  });

  it('should emit startEdit event when edit button is clicked', () => {
    spyOn(component.startEdit, 'emit');
    fixture.componentRef.setInput('isEditing', false);
    fixture.detectChanges();

    const editButton = fixture.debugElement.query(By.css('.display-actions button'));
    editButton.nativeElement.click();

    expect(component.startEdit.emit).toHaveBeenCalled();
  });

  it('should emit confirmDelete event when delete button is clicked', () => {
    spyOn(component.confirmDelete, 'emit');
    fixture.componentRef.setInput('isEditing', false);
    fixture.detectChanges();

    const deleteButton = fixture.debugElement.queryAll(By.css('.display-actions button'))[1];
    deleteButton.nativeElement.click();

    expect(component.confirmDelete.emit).toHaveBeenCalled();
  });

  it('should emit saveEdit event when save button is clicked', () => {
    spyOn(component.saveEdit, 'emit');
    fixture.componentRef.setInput('isEditing', true);
    cdr.markForCheck();
    fixture.detectChanges();

    const saveButton = fixture.debugElement.query(By.css('.edit-actions button'));
    saveButton.nativeElement.click();

    expect(component.saveEdit.emit).toHaveBeenCalled();
  });

  it('should emit cancelEdit event when cancel button is clicked', () => {
    spyOn(component.cancelEdit, 'emit');
    fixture.componentRef.setInput('isEditing', true);
    cdr.markForCheck();
    fixture.detectChanges();

    const cancelButton = fixture.debugElement.queryAll(By.css('.edit-actions button'))[1];
    cancelButton.nativeElement.click();

    expect(component.cancelEdit.emit).toHaveBeenCalled();
  });

  it('should have correct tooltips for display mode buttons', () => {
    fixture.componentRef.setInput('isEditing', false);
    fixture.detectChanges();

    const buttons = fixture.debugElement.queryAll(By.css('.display-actions button'));
    const editTooltip = buttons[0].injector.get(MatTooltip);
    const deleteTooltip = buttons[1].injector.get(MatTooltip);

    expect(editTooltip.message).toBe('Edit this test rule');
    expect(deleteTooltip.message).toBe('Delete this test rule');
  });

  it('should have correct aria labels for display mode buttons', () => {
    fixture.componentRef.setInput('isEditing', false);
    fixture.detectChanges();

    const buttons = fixture.debugElement.queryAll(By.css('.display-actions button'));

    expect(buttons[0].nativeElement.getAttribute('aria-label')).toBe('Edit test rule for ABC123');
    expect(buttons[1].nativeElement.getAttribute('aria-label')).toBe('Delete test rule for ABC123');
  });

  it('should have correct aria labels for edit mode buttons', () => {
    fixture.componentRef.setInput('isEditing', true);
    cdr.markForCheck();
    fixture.detectChanges();

    const buttons = fixture.debugElement.queryAll(By.css('.edit-actions button'));

    expect(buttons[0].nativeElement.getAttribute('aria-label')).toBe('Save changes for ABC123');
    expect(buttons[1].nativeElement.getAttribute('aria-label')).toBe('Cancel editing for ABC123');
  });

  it('should call event emitters when methods are called directly', () => {
    spyOn(component.startEdit, 'emit');
    spyOn(component.confirmDelete, 'emit');
    spyOn(component.saveEdit, 'emit');
    spyOn(component.cancelEdit, 'emit');

    component.onStartEdit();
    component.onConfirmDelete();
    component.onSaveEdit();
    component.onCancelEdit();

    expect(component.startEdit.emit).toHaveBeenCalled();
    expect(component.confirmDelete.emit).toHaveBeenCalled();
    expect(component.saveEdit.emit).toHaveBeenCalled();
    expect(component.cancelEdit.emit).toHaveBeenCalled();
  });
});
