import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { FormBuilder, Validators } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ConfigurationActionsComponent } from './configuration-actions.component';

describe('ConfigurationActionsComponent', () => {
  let component: ConfigurationActionsComponent;
  let fixture: ComponentFixture<ConfigurationActionsComponent>;
  let formBuilder: FormBuilder;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        ConfigurationActionsComponent,
        NoopAnimationsModule,
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ConfigurationActionsComponent);
    component = fixture.componentInstance;
    formBuilder = TestBed.inject(FormBuilder);

    // Set up a mock form using signal input
    const testForm = formBuilder.group({
      testField: ['', Validators.required],
    });
    fixture.componentRef.setInput('configForm', testForm);
  });

  describe('component initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.isEditingConfiguration()).toBe(false);
      expect(component.isLoadingConfiguration()).toBe(false);
      expect(component.isSavingConfiguration()).toBe(false);
    });
  });

  describe('template rendering', () => {
    it('should show edit button when not editing', () => {
      fixture.componentRef.setInput('isEditingConfiguration', false);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const editButton = compiled.querySelector('button[color="accent"]');
      const saveButton = compiled.querySelector('button[color="primary"]');
      const cancelButton = compiled.querySelector('button[mat-button]:not([color])');

      expect(editButton).toBeTruthy();
      expect(editButton?.textContent?.trim()).toContain('Edit Configuration');
      expect(saveButton).toBeFalsy();
      expect(cancelButton).toBeFalsy();
    });

    it('should show save and cancel buttons when editing', () => {
      fixture.componentRef.setInput('isEditingConfiguration', true);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const editButton = compiled.querySelector('button[color="accent"]');
      const saveButton = compiled.querySelector('app-refresh-button');
      const cancelButton = compiled.querySelector('button[mat-button]:not([color])');

      expect(editButton).toBeFalsy();
      expect(saveButton).toBeTruthy();
      expect(saveButton?.textContent?.trim()).toContain('Save Notes');
      expect(cancelButton).toBeTruthy();
      expect(cancelButton?.textContent?.trim()).toContain('Cancel');
    });

    it('should disable edit button when loading configuration', () => {
      fixture.componentRef.setInput('isEditingConfiguration', false);
      fixture.componentRef.setInput('isLoadingConfiguration', true);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const editButton = compiled.querySelector('button[color="accent"]') as HTMLButtonElement;

      expect(editButton.disabled).toBe(true);
    });

    it('should disable save button when form is invalid', () => {
      fixture.componentRef.setInput('isEditingConfiguration', true);
      component.configForm().patchValue({ testField: '' }); // Invalid - required field empty
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const saveButton = compiled.querySelector('app-refresh-button button') as HTMLButtonElement;

      expect(saveButton.disabled).toBe(true);
    });

    it('should disable save button when saving configuration', () => {
      fixture.componentRef.setInput('isEditingConfiguration', true);
      fixture.componentRef.setInput('isSavingConfiguration', true);
      component.configForm().patchValue({ testField: 'valid value' });
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const saveButton = compiled.querySelector('app-refresh-button button') as HTMLButtonElement;

      expect(saveButton.disabled).toBe(true);
    });

    it('should disable cancel button when saving configuration', () => {
      fixture.componentRef.setInput('isEditingConfiguration', true);
      fixture.componentRef.setInput('isSavingConfiguration', true);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const cancelButton = compiled.querySelector('button[mat-button]:not([color])') as HTMLButtonElement;

      expect(cancelButton.disabled).toBe(true);
    });

    it('should show loading spinner when saving', () => {
      fixture.componentRef.setInput('isEditingConfiguration', true);
      fixture.componentRef.setInput('isSavingConfiguration', true);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const spinner = compiled.querySelector('app-refresh-button mat-icon');
      const saveButton = compiled.querySelector('app-refresh-button');

      expect(spinner).toBeTruthy();
      expect(saveButton?.textContent?.trim()).toContain('Saving...');
    });

    it('should show normal save button text when not saving', () => {
      fixture.componentRef.setInput('isEditingConfiguration', true);
      fixture.componentRef.setInput('isSavingConfiguration', false);
      component.configForm().patchValue({ testField: 'valid value' });
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const spinner = compiled.querySelector('app-refresh-button mat-icon.spinning');
      const saveButton = compiled.querySelector('app-refresh-button');

      expect(spinner).toBeFalsy();
      expect(saveButton?.textContent?.trim()).toContain('Save Notes');
    });
  });

  describe('button interactions', () => {
    it('should emit editConfiguration when edit button is clicked', () => {
      spyOn(component.editConfiguration, 'emit');
      fixture.componentRef.setInput('isEditingConfiguration', false);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const editButton = compiled.querySelector('button[color="accent"]') as HTMLButtonElement;

      editButton.click();

      expect(component.editConfiguration.emit).toHaveBeenCalled();
    });

    it('should emit saveConfiguration when save button is clicked with valid form', () => {
      spyOn(component.saveConfiguration, 'emit');
      fixture.componentRef.setInput('isEditingConfiguration', true);
      component.configForm().patchValue({ testField: 'valid value' });
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const saveButton = compiled.querySelector('app-refresh-button button') as HTMLButtonElement;

      saveButton.click();

      expect(component.saveConfiguration.emit).toHaveBeenCalled();
    });

    it('should emit cancelConfigurationEdit when cancel button is clicked', () => {
      spyOn(component.cancelConfigurationEdit, 'emit');
      fixture.componentRef.setInput('isEditingConfiguration', true);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const cancelButton = compiled.querySelector('button[mat-button]:not([color])') as HTMLButtonElement;

      cancelButton.click();

      expect(component.cancelConfigurationEdit.emit).toHaveBeenCalled();
    });
  });

  describe('method behaviors', () => {
    it('should not emit saveConfiguration when form is invalid', () => {
      spyOn(component.saveConfiguration, 'emit');
      component.configForm().patchValue({ testField: '' }); // Invalid

      component.onSaveConfiguration();

      expect(component.saveConfiguration.emit).not.toHaveBeenCalled();
    });

    it('should not emit saveConfiguration when saving is in progress', () => {
      spyOn(component.saveConfiguration, 'emit');
      component.configForm().patchValue({ testField: 'valid value' });
      fixture.componentRef.setInput('isSavingConfiguration', true);

      component.onSaveConfiguration();

      expect(component.saveConfiguration.emit).not.toHaveBeenCalled();
    });

    it('should emit saveConfiguration when form is valid and not saving', () => {
      spyOn(component.saveConfiguration, 'emit');
      component.configForm().patchValue({ testField: 'valid value' });
      fixture.componentRef.setInput('isSavingConfiguration', false);

      component.onSaveConfiguration();

      expect(component.saveConfiguration.emit).toHaveBeenCalled();
    });

    it('should emit cancelConfigurationEdit when onCancelConfiguration is called', () => {
      spyOn(component.cancelConfigurationEdit, 'emit');

      component.onCancelConfiguration();

      expect(component.cancelConfigurationEdit.emit).toHaveBeenCalled();
    });

    it('should emit editConfiguration when onEditConfiguration is called', () => {
      spyOn(component.editConfiguration, 'emit');

      component.onEditConfiguration();

      expect(component.editConfiguration.emit).toHaveBeenCalled();
    });
  });
});
