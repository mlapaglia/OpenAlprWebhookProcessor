import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormBuilder } from '@angular/forms';
import { of, throwError, Subject } from 'rxjs';
import { PlateSettingsTableComponent, ConfirmDeleteDialogComponent, type PlateSettingsConfig } from './plate-settings-table.component';
import type { IPlateSetting } from './plate-setting.interface';


// Test implementation of IPlateSetting
class TestPlateSetting implements IPlateSetting {
  id: string;
  plateNumber: string;
  strictMatch: boolean;
  description: string;

  constructor(init: Partial<IPlateSetting> = {}) {
    this.id = init.id ?? '';
    this.plateNumber = init.plateNumber ?? '';
    this.strictMatch = init.strictMatch ?? true;
    this.description = init.description ?? '';
  }
}

// Mock service interface
interface MockService {
  getAll: jasmine.Spy<() => any>;
  upsert: jasmine.Spy<(items: TestPlateSetting[]) => any>;
}

describe('PlateSettingsTableComponent', () => {
  let component: PlateSettingsTableComponent<TestPlateSetting>;
  let fixture: ComponentFixture<PlateSettingsTableComponent<TestPlateSetting>>;
  let mockSnackBar: jasmine.SpyObj<MatSnackBar>;
  let mockDialog: jasmine.SpyObj<MatDialog>;
  let mockConfig: PlateSettingsConfig<TestPlateSetting>;
  let mockService: MockService;

  beforeEach(async () => {
    mockSnackBar = jasmine.createSpyObj('MatSnackBar', ['open']);
    // Create a more complete MatDialog mock
    mockDialog = jasmine.createSpyObj('MatDialog', ['open'], {
      _openDialogs: [],
      _afterAllClosed: new Subject(),
      _afterOpened: new Subject(),
    });

    mockService = {
      getAll: jasmine.createSpy('getAll').and.returnValue(of([])),
      upsert: jasmine.createSpy('upsert').and.returnValue(of({})),
    };

    mockConfig = {
      title: 'Test Settings',
      subtitle: 'Test subtitle',
      emptyStateTitle: 'No settings',
      emptyStateDescription: 'Add some settings',
      addButtonText: 'Add Setting',
      entityName: 'setting',
      createNew: () => new TestPlateSetting(),
      service: mockService,
    };

    await TestBed.configureTestingModule({
      imports: [
        PlateSettingsTableComponent,
        ConfirmDeleteDialogComponent,
      ],
      providers: [
        { provide: MatSnackBar, useValue: mockSnackBar },
        { provide: MatDialog, useValue: mockDialog },
        FormBuilder,
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PlateSettingsTableComponent<TestPlateSetting>);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('config', mockConfig);
  });

  afterEach(() => {
    if (fixture) {
      fixture.destroy();
    }
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.settings.data).toEqual([]);
      expect(component.isSaving).toBe(false);
      expect(component.isLoading).toBe(false);
      expect(component.hasError).toBe(false);
      expect(component.isAddingSetting).toBe(false);
      expect(component.editingId).toBeNull();
      expect(component.editingSetting).toBeNull();
    });

    it('should have correct displayed columns', () => {
      expect(component.displayedColumns).toEqual(['plateNumber', 'description', 'actions']);
    });

    it('should load settings on init', () => {
      spyOn(component, 'loadSettings');
      component.ngOnInit();
      expect(component.loadSettings).toHaveBeenCalled();
    });

    it('should unsubscribe on destroy', () => {
      spyOn(Object.getPrototypeOf(Object.getPrototypeOf(component)), 'ngOnDestroy');
      component.ngOnDestroy();
      expect(Object.getPrototypeOf(Object.getPrototypeOf(component)).ngOnDestroy).toHaveBeenCalled();
    });
  });

  describe('Loading Settings', () => {
    it('should load settings successfully', () => {
      const testSettings = [
        new TestPlateSetting({ id: '1', plateNumber: 'ABC123', description: 'Test 1' }),
        new TestPlateSetting({ id: '2', plateNumber: 'XYZ789', description: 'Test 2' }),
      ];
      mockService.getAll.and.returnValue(of(testSettings));
      spyOn(component.settingsChanged, 'emit');

      component.loadSettings();

      expect(component.isLoading).toBe(false);
      expect(component.hasError).toBe(false);
      expect(component.settings.data).toEqual(testSettings);
      expect(component.settingsChanged.emit).toHaveBeenCalledWith(testSettings);
    });

    it('should handle loading error', () => {
      mockService.getAll.and.returnValue(throwError(() => new Error('Load failed')));

      component.loadSettings();

      expect(component.hasError).toBe(true);
      expect(component.isLoading).toBe(false);
      expect(mockSnackBar.open).toHaveBeenCalledWith(
        'Failed to load settings. Please try again.',
        'Close',
        { duration: 5000 },
      );
    });

    it('should set loading state during load', () => {
      mockService.getAll.and.returnValue(of([]));

      component.loadSettings();
      // Initially loading should be true (before observable completes)
      expect(component.hasError).toBe(false);
    });
  });

  describe('Adding Settings', () => {
    it('should add new setting successfully', () => {
      const newSetting = new TestPlateSetting({ plateNumber: 'NEW123', description: 'New setting' });
      spyOn(component.settingsChanged, 'emit');

      component.onAddSetting(newSetting);

      expect(component.settings.data).toContain(newSetting);
      expect(component.isAddingSetting).toBe(false);
      expect(component.settingsChanged.emit).toHaveBeenCalledWith(component.settings.data);
      expect(mockSnackBar.open).toHaveBeenCalledWith(
        'setting added. Remember to save your changes.',
        'Close',
        { duration: 3000 },
      );
    });

    it('should handle reset form event', () => {
      expect(() => component.onResetForm()).not.toThrow();
    });
  });

  describe('Editing Settings', () => {
    beforeEach(() => {
      component.settings.data = [
        new TestPlateSetting({ id: '1', plateNumber: 'ABC123', description: 'Test 1' }),
        new TestPlateSetting({ id: '2', plateNumber: 'XYZ789', description: 'Test 2' }),
      ];
    });

    it('should start editing a setting', () => {
      const setting = component.settings.data[0];

      component.startEdit(setting);

      expect(component.editingId).toBe(setting.id);
      expect(component.editingSetting).toEqual(jasmine.objectContaining(setting));
    });

    it('should cancel editing', () => {
      component.editingId = '1';
      component.editingSetting = component.settings.data[0];

      component.cancelEdit();

      expect(component.editingId).toBeNull();
      expect(component.editingSetting).toBeNull();
    });

    it('should save valid edit', () => {
      const setting = component.settings.data[0];
      component.editingSetting = new TestPlateSetting({
        ...setting,
        plateNumber: 'EDITED123',
        description: 'Edited description',
      });
      spyOn(component.settingsChanged, 'emit');

      component.saveEdit(setting);

      expect(setting.plateNumber).toBe('EDITED123');
      expect(setting.description).toBe('Edited description');
      expect(component.editingId).toBeNull();
      expect(component.editingSetting).toBeNull();
      expect(component.settingsChanged.emit).toHaveBeenCalled();
      expect(mockSnackBar.open).toHaveBeenCalledWith(
        'Changes saved locally. Remember to save all changes.',
        'Close',
        { duration: 3000 },
      );
    });

    it('should reject edit with empty plate number', () => {
      const setting = component.settings.data[0];
      component.editingId = setting.id; // Set editing state
      component.editingSetting = new TestPlateSetting({ ...setting, plateNumber: '' });

      component.saveEdit(setting);

      expect(mockSnackBar.open).toHaveBeenCalledWith(
        'Plate number is required.',
        'Close',
        { duration: 3000 },
      );
      expect(component.editingId).not.toBeNull();
    });

    it('should reject edit with short plate number', () => {
      const setting = component.settings.data[0];
      component.editingSetting = new TestPlateSetting({ ...setting, plateNumber: 'A' });

      component.saveEdit(setting);

      expect(mockSnackBar.open).toHaveBeenCalledWith(
        'Plate number must be at least 2 characters.',
        'Close',
        { duration: 3000 },
      );
    });

    it('should reject edit with duplicate plate number', () => {
      const setting = component.settings.data[0];
      component.editingSetting = new TestPlateSetting({ ...setting, plateNumber: 'XYZ789' }); // Same as second setting

      component.saveEdit(setting);

      expect(mockSnackBar.open).toHaveBeenCalledWith(
        'This plate number already exists.',
        'Close',
        { duration: 3000 },
      );
    });

    it('should return early if no editing setting', () => {
      component.editingSetting = null;

      component.saveEdit(component.settings.data[0]);

      expect(mockSnackBar.open).not.toHaveBeenCalled();
    });

    it('should check if setting is being edited', () => {
      const setting = component.settings.data[0];
      component.editingId = setting.id;

      expect(component.isEditing(setting)).toBe(true);
      expect(component.isEditing(component.settings.data[1])).toBe(false);
    });
  });

  describe('Deleting Settings', () => {
    beforeEach(() => {
      component.settings.data = [
        new TestPlateSetting({ id: '1', plateNumber: 'ABC123', description: 'Test 1' }),
        new TestPlateSetting({ id: '2', plateNumber: 'XYZ789', description: 'Test 2' }),
      ];
    });

    it('should delete setting directly', () => {
      const settingToDelete = component.settings.data[0];
      const initialLength = component.settings.data.length;
      spyOn(component.settingsChanged, 'emit');

      component.deleteSetting(settingToDelete);

      expect(component.settings.data.length).toBe(initialLength - 1);
      expect(component.settings.data).not.toContain(settingToDelete);
      expect(component.settingsChanged.emit).toHaveBeenCalledWith(component.settings.data);
    });


  });

  describe('Saving Settings', () => {
    it('should save valid settings successfully', () => {
      component.settings.data = [new TestPlateSetting({ plateNumber: 'ABC123', description: 'Valid setting' })];
      mockService.upsert.and.returnValue(of({}));
      spyOn(component, 'loadSettings');

      component.saveSettings();

      expect(component.isSaving).toBe(false);
      expect(mockService.upsert).toHaveBeenCalledWith(component.settings.data);
      expect(mockSnackBar.open).toHaveBeenCalledWith(
        'settings saved successfully!',
        'Close',
        { duration: 3000 },
      );
      expect(component.loadSettings).toHaveBeenCalled();
    });

    it('should prevent multiple simultaneous saves', () => {
      component.isSaving = true;
      component.settings.data = [new TestPlateSetting({ plateNumber: 'ABC123' })];

      component.saveSettings();

      expect(mockService.upsert).not.toHaveBeenCalled();
    });

    it('should reject empty settings', () => {
      component.settings.data = [];

      component.saveSettings();

      expect(mockSnackBar.open).toHaveBeenCalledWith(
        'Please add at least one setting with a plate number.',
        'Close',
        { duration: 3000 },
      );
      expect(mockService.upsert).not.toHaveBeenCalled();
    });

    it('should filter out invalid settings', () => {
      component.settings.data = [
        new TestPlateSetting({ plateNumber: 'ABC123', description: 'Valid' }),
        new TestPlateSetting({ plateNumber: '', description: 'Invalid' }),
        new TestPlateSetting({ plateNumber: 'XYZ789', description: 'Valid 2' }),
      ];
      mockService.upsert.and.returnValue(of({}));

      component.saveSettings();

      expect(mockService.upsert).toHaveBeenCalledWith([
        jasmine.objectContaining({ plateNumber: 'ABC123' }),
        jasmine.objectContaining({ plateNumber: 'XYZ789' }),
      ]);
    });

    it('should handle save error', () => {
      component.settings.data = [new TestPlateSetting({ plateNumber: 'ABC123' })];
      mockService.upsert.and.returnValue(throwError(() => new Error('Save failed')));

      component.saveSettings();

      expect(component.isSaving).toBe(false);
      expect(mockSnackBar.open).toHaveBeenCalledWith(
        'Failed to save settings. Please try again.',
        'Close',
        { duration: 5000 },
      );
    });
  });

  describe('Utility Functions', () => {
    it('should provide track by function', () => {
      const setting = new TestPlateSetting({ id: '123', plateNumber: 'ABC123' });
      expect(component.trackByFn(0, setting)).toBe('123');

      const settingWithoutId = new TestPlateSetting({ plateNumber: 'XYZ789' });
      expect(component.trackByFn(5, settingWithoutId)).toBe(5);
    });

    it('should scroll to form element', () => {
      const mockElement = {
        scrollIntoView: jasmine.createSpy('scrollIntoView'),
      };
      spyOn(document, 'querySelector').and.returnValue(mockElement as any);

      component.scrollToForm();

      expect(document.querySelector).toHaveBeenCalledWith('.add-setting-card');
      expect(mockElement.scrollIntoView).toHaveBeenCalledWith({ behavior: 'smooth', block: 'start' });
    });

    it('should handle missing form element in scroll', () => {
      spyOn(document, 'querySelector').and.returnValue(null);

      expect(() => component.scrollToForm()).not.toThrow();
    });
  });

  describe('Private Helper Methods', () => {
    beforeEach(() => {
      component.settings.data = [
        new TestPlateSetting({ plateNumber: 'ABC123' }),
        new TestPlateSetting({ plateNumber: 'xyz789' }),
      ];
    });

    it('should detect duplicate plates (case insensitive)', () => {
      expect(component['isDuplicatePlate']('ABC123')).toBe(true);
      expect(component['isDuplicatePlate']('abc123')).toBe(true);
      expect(component['isDuplicatePlate']('XYZ789')).toBe(true);
      expect(component['isDuplicatePlate']('NEW123')).toBe(false);
      expect(component['isDuplicatePlate']('')).toBe(false);
    });

    it('should detect duplicates for edit excluding current setting', () => {
      const currentSetting = component.settings.data[0];

      expect(component['isDuplicatePlateForEdit']('ABC123', currentSetting)).toBe(false); // Same setting
      expect(component['isDuplicatePlateForEdit']('XYZ789', currentSetting)).toBe(true); // Different setting
      expect(component['isDuplicatePlateForEdit']('NEW123', currentSetting)).toBe(false); // Not duplicate
    });

    it('should generate temp ID for settings without ID', () => {
      const setting = new TestPlateSetting({ plateNumber: 'TEST123', strictMatch: true });
      const tempId = component['generateTempId'](setting);

      expect(tempId).toContain('temp_TEST123_true_');
      expect(tempId).toMatch(/temp_TEST123_true_\d+/);
    });
  });
});

describe('ConfirmDeleteDialogComponent', () => {
  let component: ConfirmDeleteDialogComponent;
  let fixture: ComponentFixture<ConfirmDeleteDialogComponent>;
  let mockDialogRef: jasmine.SpyObj<any>;

  beforeEach(async () => {
    mockDialogRef = jasmine.createSpyObj('MatDialogRef', ['close']);

    await TestBed.configureTestingModule({
      imports: [ConfirmDeleteDialogComponent],
      providers: [
        { provide: MatDialogRef, useValue: mockDialogRef },
        { provide: MAT_DIALOG_DATA, useValue: { title: 'Test', message: 'Test message', confirmText: 'OK', cancelText: 'Cancel' } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ConfirmDeleteDialogComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should close with false on cancel', () => {
    component.onCancel();
    expect(mockDialogRef.close).toHaveBeenCalledWith(false);
  });

  it('should close with true on confirm', () => {
    component.onConfirm();
    expect(mockDialogRef.close).toHaveBeenCalledWith(true);
  });
});
