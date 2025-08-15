import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { MatTableDataSource } from '@angular/material/table';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SettingsListComponent } from './settings-list.component';
import type { PlateSettingsConfig } from './plate-settings-table.component';
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

// Create mock config for testing
const createMockConfig = (): PlateSettingsConfig<TestPlateSetting> => ({
  title: 'Test Settings',
  subtitle: 'Test subtitle',
  emptyStateTitle: 'No settings',
  emptyStateDescription: 'Add some settings',
  addButtonText: 'Add Setting',
  entityName: 'setting',
  createNew: () => new TestPlateSetting(),
  service: {
    getAll: jasmine.createSpy('getAll'),
    upsert: jasmine.createSpy('upsert'),
  },
});

describe('SettingsListComponent', () => {
  let component: SettingsListComponent<TestPlateSetting>;
  let fixture: ComponentFixture<SettingsListComponent<TestPlateSetting>>;
  let mockSnackBar: jasmine.SpyObj<MatSnackBar>;
  let mockConfig: PlateSettingsConfig<TestPlateSetting>;

  beforeEach(async () => {
    mockSnackBar = jasmine.createSpyObj('MatSnackBar', ['open']);
    mockConfig = createMockConfig();

    await TestBed.configureTestingModule({
      imports: [
        SettingsListComponent,
        NoopAnimationsModule,
      ],
      providers: [{ provide: MatSnackBar, useValue: mockSnackBar }],
    }).compileComponents();

    fixture = TestBed.createComponent(SettingsListComponent<TestPlateSetting>);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('config', mockConfig);
    fixture.componentRef.setInput('trackByFn', (index: number, item: TestPlateSetting) => item.id || index);
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
      expect(component.settings()).toBeInstanceOf(MatTableDataSource);
      expect(component.settings().data).toEqual([]);
      expect(component.isSaving()).toBe(false);
      expect(component.editingId()).toBeNull();
      expect(component.editingSetting()).toBeNull();
    });

    it('should have correct displayed columns', () => {
      expect(component.displayedColumns).toEqual(['plateNumber', 'description', 'actions']);
    });
  });

  describe('Input Properties', () => {
    it('should accept config input', () => {
      const testConfig = { ...mockConfig, title: 'Updated Title' };
      fixture.componentRef.setInput('config', testConfig);

      expect(component.config()?.title).toBe('Updated Title');
    });

    it('should accept settings data source input', () => {
      const testSettings = new MatTableDataSource([
        new TestPlateSetting({ id: '1', plateNumber: 'ABC123' }),
        new TestPlateSetting({ id: '2', plateNumber: 'XYZ789' }),
      ]);
      fixture.componentRef.setInput('settings', testSettings);

      expect(component.settings().data.length).toBe(2);
      expect(component.settings().data[0].plateNumber).toBe('ABC123');
    });

    it('should accept editing state inputs', () => {
      const testSetting = new TestPlateSetting({ id: '1', plateNumber: 'ABC123' });
      fixture.componentRef.setInput('editingId', '1');
      fixture.componentRef.setInput('editingSetting', testSetting);

      expect(component.editingId()).toBe('1');
      expect(component.editingSetting()).toBe(testSetting);
    });

    it('should accept saving state input', () => {
      fixture.componentRef.setInput('isSaving', true);

      expect(component.isSaving()).toBe(true);
    });
  });

  describe('Computed Data', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('settings', new MatTableDataSource([
        new TestPlateSetting({ id: '1', plateNumber: 'ABC123', description: 'First setting' }),
        new TestPlateSetting({ id: '2', plateNumber: 'XYZ789', description: 'Second setting' }),
        new TestPlateSetting({ plateNumber: 'NO-ID', description: 'No ID setting' }), // No ID
      ]));
    });

    it('should compute data with editing state and element IDs', () => {
      fixture.componentRef.setInput('editingId', '1');

      const { computedData } = component;

      expect(computedData.length).toBe(3);

      // First item should be marked as editing
      expect(computedData[0].isCurrentlyEditing).toBe(true);
      expect(computedData[0].elementId).toBe('1');
      expect(computedData[0].plateNumber).toBe('ABC123');

      // Second item should not be editing
      expect(computedData[1].isCurrentlyEditing).toBe(false);
      expect(computedData[1].elementId).toBe('2');

      // Third item without ID should have generated temp ID
      expect(computedData[2].isCurrentlyEditing).toBe(false);
      expect(computedData[2].elementId).toContain('temp_NO-ID_strict');
    });

    it('should handle empty data', () => {
      fixture.componentRef.setInput('settings', new MatTableDataSource<TestPlateSetting>([]));

      const { computedData } = component;

      expect(computedData).toEqual([]);
    });

    it('should generate temp IDs consistently', () => {
      const settingWithoutId = new TestPlateSetting({
        plateNumber: 'TEST123',
        strictMatch: false,
      });
      fixture.componentRef.setInput('settings', new MatTableDataSource([settingWithoutId]));

      const computedData1 = component.computedData;
      const computedData2 = component.computedData;

      expect(computedData1[0].elementId).toBe(computedData2[0].elementId);
      expect(computedData1[0].elementId).toBe('temp_TEST123_lenient');
    });
  });

  describe('Event Emissions', () => {
    it('should emit saveSettings event', () => {
      spyOn(component.saveSettings, 'emit');

      component.onSaveSettings();

      expect(component.saveSettings.emit).toHaveBeenCalled();
    });

    it('should emit scrollToForm event', () => {
      spyOn(component.scrollToForm, 'emit');

      component.onScrollToForm();

      expect(component.scrollToForm.emit).toHaveBeenCalled();
    });

    it('should emit startEdit event with element', () => {
      spyOn(component.startEdit, 'emit');
      const testSetting = new TestPlateSetting({ plateNumber: 'ABC123' });

      component.onStartEdit(testSetting);

      expect(component.startEdit.emit).toHaveBeenCalledWith(testSetting);
    });

    it('should emit confirmDelete event with element', () => {
      spyOn(component.confirmDelete, 'emit');
      const testSetting = new TestPlateSetting({ plateNumber: 'ABC123' });

      component.onConfirmDelete(testSetting);

      expect(component.confirmDelete.emit).toHaveBeenCalledWith(testSetting);
    });

    it('should emit saveEdit event with element', () => {
      spyOn(component.saveEdit, 'emit');
      const testSetting = new TestPlateSetting({ plateNumber: 'ABC123' });

      component.onSaveEdit(testSetting);

      expect(component.saveEdit.emit).toHaveBeenCalledWith(testSetting);
    });

    it('should emit cancelEdit event', () => {
      spyOn(component.cancelEdit, 'emit');

      component.onCancelEdit();

      expect(component.cancelEdit.emit).toHaveBeenCalled();
    });
  });

  describe('Temp ID Generation', () => {
    it('should generate temp ID for strict match setting', () => {
      const setting = new TestPlateSetting({
        plateNumber: 'TEST123',
        strictMatch: true,
      });

      const tempId = component['generateTempId'](setting);

      expect(tempId).toBe('temp_TEST123_strict');
    });

    it('should generate temp ID for lenient match setting', () => {
      const setting = new TestPlateSetting({
        plateNumber: 'TEST456',
        strictMatch: false,
      });

      const tempId = component['generateTempId'](setting);

      expect(tempId).toBe('temp_TEST456_lenient');
    });

    it('should handle empty plate number', () => {
      const setting = new TestPlateSetting({
        plateNumber: '',
        strictMatch: true,
      });

      const tempId = component['generateTempId'](setting);

      expect(tempId).toBe('temp__strict');
    });

    it('should handle special characters in plate number', () => {
      const setting = new TestPlateSetting({
        plateNumber: 'TEST-123_ABC',
        strictMatch: false,
      });

      const tempId = component['generateTempId'](setting);

      expect(tempId).toBe('temp_TEST-123_ABC_lenient');
    });
  });

  describe('Data Integration', () => {
    it('should properly map settings to computed data with mixed ID states', () => {
      const settingsWithMixedIds = [
        new TestPlateSetting({ id: 'existing-1', plateNumber: 'ABC123', strictMatch: true }),
        new TestPlateSetting({ plateNumber: 'NO-ID-1', strictMatch: false }), // No ID
        new TestPlateSetting({ id: 'existing-2', plateNumber: 'XYZ789', strictMatch: true }),
        new TestPlateSetting({ plateNumber: 'NO-ID-2', strictMatch: true }), // No ID
      ];

      fixture.componentRef.setInput('settings', new MatTableDataSource(settingsWithMixedIds));
      fixture.componentRef.setInput('editingId', 'temp_NO-ID-1_lenient'); // Editing a no-ID item

      const { computedData } = component;

      expect(computedData.length).toBe(4);

      // Check first item (has ID, not editing)
      expect(computedData[0].elementId).toBe('existing-1');
      expect(computedData[0].isCurrentlyEditing).toBe(false);

      // Check second item (no ID, should be editing)
      expect(computedData[1].elementId).toBe('temp_NO-ID-1_lenient');
      expect(computedData[1].isCurrentlyEditing).toBe(true);

      // Check third item (has ID, not editing)
      expect(computedData[2].elementId).toBe('existing-2');
      expect(computedData[2].isCurrentlyEditing).toBe(false);

      // Check fourth item (no ID, not editing)
      expect(computedData[3].elementId).toBe('temp_NO-ID-2_strict');
      expect(computedData[3].isCurrentlyEditing).toBe(false);
    });

    it('should preserve original setting properties in computed data', () => {
      const originalSetting = new TestPlateSetting({
        id: '1',
        plateNumber: 'TEST123',
        strictMatch: false,
        description: 'Test description',
      });

      fixture.componentRef.setInput('settings', new MatTableDataSource([originalSetting]));

      const { computedData } = component;

      expect(computedData[0].id).toBe('1');
      expect(computedData[0].plateNumber).toBe('TEST123');
      expect(computedData[0].strictMatch).toBe(false);
      expect(computedData[0].description).toBe('Test description');
      expect(computedData[0].elementId).toBe('1');
      expect(computedData[0].isCurrentlyEditing).toBe(false);
    });
  });

  describe('TrackBy Function', () => {
    it('should use provided trackByFn correctly', () => {
      const testSetting = new TestPlateSetting({ id: '123', plateNumber: 'ABC123' });

      const result = component.trackByFn()!(0, testSetting);

      expect(result).toBe('123');
    });

    it('should handle trackByFn with index fallback', () => {
      const testSetting = new TestPlateSetting({ plateNumber: 'ABC123' }); // No ID
      fixture.componentRef.setInput('trackByFn', (index: number, item: TestPlateSetting) => item.id || index);

      const result = component.trackByFn()!(5, testSetting);

      expect(result).toBe(5);
    });
  });

  describe('Event Flow Integration', () => {
    it('should handle complete edit workflow', () => {
      spyOn(component.startEdit, 'emit');
      spyOn(component.saveEdit, 'emit');
      spyOn(component.cancelEdit, 'emit');

      const testSetting = new TestPlateSetting({ id: '1', plateNumber: 'ABC123' });

      // Start edit
      component.onStartEdit(testSetting);
      expect(component.startEdit.emit).toHaveBeenCalledWith(testSetting);

      // Save edit
      component.onSaveEdit(testSetting);
      expect(component.saveEdit.emit).toHaveBeenCalledWith(testSetting);

      // Cancel edit
      component.onCancelEdit();
      expect(component.cancelEdit.emit).toHaveBeenCalled();
    });

    it('should handle delete workflow', () => {
      spyOn(component.confirmDelete, 'emit');

      const testSetting = new TestPlateSetting({ id: '1', plateNumber: 'ABC123' });

      component.onConfirmDelete(testSetting);

      expect(component.confirmDelete.emit).toHaveBeenCalledWith(testSetting);
    });

    it('should handle save workflow', () => {
      spyOn(component.saveSettings, 'emit');

      component.onSaveSettings();

      expect(component.saveSettings.emit).toHaveBeenCalled();
    });
  });
});
