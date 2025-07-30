import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';

import { ThemePickerComponent } from './theme-picker.component';
import { ThemeStorage } from './theme-storage/theme-storage';
import { StyleManager } from './style-manager/style-manager.component';

/**
 * SIMPLER APPROACH TO ANGULAR TESTING
 *
 * This demonstrates easier ways to test Angular components:
 * 1. NO_ERRORS_SCHEMA - Ignores unknown elements and attributes
 * 2. Minimal mocking - Only mock what we actually test
 * 3. Focus on component logic, not DOM complexity
 * 4. Use real services where possible
 */
describe('ThemePickerComponent (Simplified Testing)', () => {
  let component: ThemePickerComponent;
  let fixture: ComponentFixture<ThemePickerComponent>;
  let mockThemeStorage: jasmine.SpyObj<ThemeStorage>;
  let mockStyleManager: jasmine.SpyObj<StyleManager>;

  beforeEach(async () => {
    // Create minimal spies - only what we actually need
    const themeStorageSpy = jasmine.createSpyObj('ThemeStorage', ['getStoredThemeName', 'storeTheme']);
    const styleManagerSpy = jasmine.createSpyObj('StyleManager', ['setStyle', 'removeStyle']);

    await TestBed.configureTestingModule({
      imports: [ThemePickerComponent],
      providers: [
        { provide: ThemeStorage, useValue: themeStorageSpy },
        { provide: StyleManager, useValue: styleManagerSpy },
        {
          provide: ActivatedRoute,
          useValue: { queryParamMap: of(new Map()) },
        },
      ],
      schemas: [NO_ERRORS_SCHEMA],  // This ignores Material Design components we don't need to test
    }).compileComponents();

    mockThemeStorage = TestBed.inject(ThemeStorage) as jasmine.SpyObj<ThemeStorage>;
    mockStyleManager = TestBed.inject(StyleManager) as jasmine.SpyObj<StyleManager>;

    fixture = TestBed.createComponent(ThemePickerComponent);
    component = fixture.componentInstance;
  });

  describe('Component Logic (Focus on Business Logic)', () => {
    it('should create component', () => {
      expect(component).toBeTruthy();
    });

    it('should have predefined themes', () => {
      expect(component.themes.length).toBe(4);
      expect(component.themes.find(t => t.isDefault)?.name).toBe('indigo-pink');
    });

    it('should initialize with stored theme when available', () => {
      mockThemeStorage.getStoredThemeName.and.returnValue('pink-bluegrey');

      // Create new fixture to test initialization
      const testFixture = TestBed.createComponent(ThemePickerComponent);
      const testComponent = testFixture.componentInstance;
      testFixture.detectChanges();

      expect(testComponent.currentTheme?.name).toBe('pink-bluegrey');
      expect(mockThemeStorage.getStoredThemeName).toHaveBeenCalled();
    });

    it('should initialize with default theme when no stored theme', () => {
      mockThemeStorage.getStoredThemeName.and.returnValue(null);

      // Create new fixture to test initialization
      const testFixture = TestBed.createComponent(ThemePickerComponent);
      const testComponent = testFixture.componentInstance;
      testFixture.detectChanges();

      expect(testComponent.currentTheme?.name).toBe('indigo-pink');
    });

    it('should select theme and update dependencies', () => {
      component.selectTheme('pink-bluegrey');

      expect(component.currentTheme?.name).toBe('pink-bluegrey');
      expect(mockStyleManager.setStyle).toHaveBeenCalledWith('theme', 'pink-bluegrey.css');
      expect(mockThemeStorage.storeTheme).toHaveBeenCalled();
    });

    it('should handle default theme by removing styles', () => {
      component.selectTheme('indigo-pink');

      expect(mockStyleManager.removeStyle).toHaveBeenCalledWith('theme');
      expect(mockStyleManager.setStyle).not.toHaveBeenCalled();
    });

    it('should ignore invalid theme names', () => {
      const originalTheme = component.currentTheme;

      component.selectTheme('invalid-theme');

      expect(component.currentTheme).toBe(originalTheme);
      expect(mockStyleManager.setStyle).not.toHaveBeenCalled();
    });
  });

  describe('Theme Management', () => {
    it('should have correct theme properties', () => {
      const theme = component.themes[0];
      expect(theme.name).toBeDefined();
      expect(theme.primary).toBeDefined();
      expect(theme.accent).toBeDefined();
      expect(theme.displayName).toBeDefined();
    });

    it('should distinguish light and dark themes', () => {
      const lightThemes = component.themes.filter(t => !t.isDark);
      const darkThemes = component.themes.filter(t => t.isDark);

      expect(lightThemes.length).toBeGreaterThan(0);
      expect(darkThemes.length).toBeGreaterThan(0);
    });
  });

  describe('Service Integration', () => {
    it('should store selected theme', () => {
      const theme = component.themes[2]; // pink-bluegrey

      component.selectTheme(theme.name);

      expect(mockThemeStorage.storeTheme).toHaveBeenCalledWith(theme);
    });

    it('should apply correct CSS file for non-default themes', () => {
      component.themes.filter(t => !t.isDefault).forEach(theme => {
        component.selectTheme(theme.name);
        expect(mockStyleManager.setStyle).toHaveBeenCalledWith('theme', `${theme.name}.css`);
      });
    });
  });
});

/**
 * Lifecycle and Query Parameter Testing
 */
describe('ThemePickerComponent (Lifecycle & Route Integration)', () => {
  let component: ThemePickerComponent;
  let fixture: ComponentFixture<ThemePickerComponent>;
  let mockThemeStorage: jasmine.SpyObj<ThemeStorage>;
  let mockActivatedRoute: any;

  beforeEach(async () => {
    const themeStorageSpy = jasmine.createSpyObj('ThemeStorage', ['getStoredThemeName', 'storeTheme']);
    const styleManagerSpy = jasmine.createSpyObj('StyleManager', ['setStyle', 'removeStyle']);

    // Create a Subject to control query parameter emissions
    mockActivatedRoute = {
      queryParamMap: of({
        get: jasmine.createSpy('get').and.returnValue(null),
      }),
    };

    await TestBed.configureTestingModule({
      imports: [ThemePickerComponent],
      providers: [
        { provide: ThemeStorage, useValue: themeStorageSpy },
        { provide: StyleManager, useValue: styleManagerSpy },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    mockThemeStorage = TestBed.inject(ThemeStorage) as jasmine.SpyObj<ThemeStorage>;
    fixture = TestBed.createComponent(ThemePickerComponent);
    component = fixture.componentInstance;
  });

  it('should unsubscribe from route params on destroy', () => {
    mockThemeStorage.getStoredThemeName.and.returnValue(null);
    fixture.detectChanges();
    component.ngOnInit();

    spyOn(component['_queryParamSubscription'], 'unsubscribe');

    component.ngOnDestroy();

    expect(component['_queryParamSubscription'].unsubscribe).toHaveBeenCalled();
  });

  it('should handle query parameter theme changes', () => {
    mockThemeStorage.getStoredThemeName.and.returnValue(null);
    fixture.detectChanges();

    spyOn(component, 'selectTheme');

    // Simulate query param with theme
    mockActivatedRoute.queryParamMap = of({
      get: jasmine.createSpy('get').and.returnValue('deeppurple-amber'),
    });

    component.ngOnInit();

    expect(component.selectTheme).toHaveBeenCalledWith('deeppurple-amber');
  });
});

/**
 * SUMMARY: Easier Angular Testing Approaches
 *
 * 1. NO_ERRORS_SCHEMA: Ignores unknown elements, great for focusing on logic
 * 2. Minimal mocking: Only mock what you actually use in tests
 * 3. Focus on component logic: Test business rules, not DOM complexity
 * 4. Pure unit testing: Test component class directly without TestBed
 * 5. ComponentHarness (for Material): Use Angular Material's test harnesses
 * 6. Integration vs Unit: Choose the right level - not everything needs full DOM testing
 *
 * The complex approach we used earlier is good for:
 * - Template rendering testing
 * - User interaction testing
 * - Integration testing
 *
 * This simpler approach is better for:
 * - Business logic testing
 * - Fast test execution
 * - Easier maintenance
 * - Focus on what matters
 */
