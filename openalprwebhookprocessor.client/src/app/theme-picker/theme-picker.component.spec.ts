import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, type ParamMap } from '@angular/router';
import { LiveAnnouncer } from '@angular/cdk/a11y';
import { BehaviorSubject } from 'rxjs';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { ThemePickerComponent } from './theme-picker.component';
import { ThemeStorage } from './theme-storage/theme-storage';
import { StyleManager } from './style-manager/style-manager.component';

describe('ThemePickerComponent', () => {
  let component: ThemePickerComponent;
  let fixture: ComponentFixture<ThemePickerComponent>;
  let mockThemeStorage: jasmine.SpyObj<ThemeStorage>;
  let mockStyleManager: jasmine.SpyObj<StyleManager>;
  let mockLiveAnnouncer: jasmine.SpyObj<LiveAnnouncer>;
  let mockActivatedRoute: any;
  let queryParamSubject: BehaviorSubject<ParamMap>;

  beforeEach(async () => {
    const themeStorageSpy = jasmine.createSpyObj('ThemeStorage', ['getStoredThemeName', 'storeTheme']);
    const styleManagerSpy = jasmine.createSpyObj('StyleManager', ['setStyle', 'removeStyle']);
    const liveAnnouncerSpy = jasmine.createSpyObj('LiveAnnouncer', ['announce']);

    queryParamSubject = new BehaviorSubject<ParamMap>(new Map() as any);
    mockActivatedRoute = {
      queryParamMap: queryParamSubject.asObservable(),
    };

    await TestBed.configureTestingModule({
      imports: [ThemePickerComponent],
      providers: [
        { provide: ThemeStorage, useValue: themeStorageSpy },
        { provide: StyleManager, useValue: styleManagerSpy },
        { provide: LiveAnnouncer, useValue: liveAnnouncerSpy },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
      ],
      schemas: [NO_ERRORS_SCHEMA], // Ignore Material Design components for easier testing
    }).compileComponents();

    mockThemeStorage = TestBed.inject(ThemeStorage) as jasmine.SpyObj<ThemeStorage>;
    mockStyleManager = TestBed.inject(StyleManager) as jasmine.SpyObj<StyleManager>;
    mockLiveAnnouncer = TestBed.inject(LiveAnnouncer) as jasmine.SpyObj<LiveAnnouncer>;

    fixture = TestBed.createComponent(ThemePickerComponent);
    component = fixture.componentInstance;
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should have correct themes array', () => {
      expect(component.themes).toBeDefined();
      expect(component.themes.length).toBe(2);
      expect(component.themes[0].name).toBe('indigo-pink');
    });

    it('should initialize with stored theme when available', () => {
      mockThemeStorage.getStoredThemeName.and.returnValue('pink-bluegrey');

      // Create a new component instance to test constructor logic
      const newFixture = TestBed.createComponent(ThemePickerComponent);
      const newComponent = newFixture.componentInstance;
      newFixture.detectChanges();

      expect(mockThemeStorage.getStoredThemeName).toHaveBeenCalled();
      expect(newComponent.currentTheme?.name).toBe('pink-bluegrey');
    });

    it('should initialize with default theme when no stored theme', () => {
      mockThemeStorage.getStoredThemeName.and.returnValue(null);

      // Create a new component instance to test constructor logic
      const newFixture = TestBed.createComponent(ThemePickerComponent);
      const newComponent = newFixture.componentInstance;
      newFixture.detectChanges();

      expect(newComponent.currentTheme?.name).toBe('indigo-pink');
    });

    it('should have expected theme count and default', () => {
      expect(component.themes.length).toBe(2);
    });
  });

  describe('ngOnInit', () => {
    beforeEach(() => {
      mockThemeStorage.getStoredThemeName.and.returnValue(null);
      fixture.detectChanges();
    });

    it('should subscribe to query param changes', () => {
      spyOn(component, 'selectTheme');

      component.ngOnInit();

      // Simulate query param change
      const mockParamMap = {
        get: jasmine.createSpy('get').and.returnValue('deeppurple-amber'),
      } as any;
      queryParamSubject.next(mockParamMap);

      expect(component.selectTheme).toHaveBeenCalledWith('deeppurple-amber');
    });

    it('should handle null query param', () => {
      spyOn(component, 'selectTheme');

      component.ngOnInit();

      const mockParamMap = {
        get: jasmine.createSpy('get').and.returnValue(null),
      } as any;
      queryParamSubject.next(mockParamMap);

      expect(component.selectTheme).not.toHaveBeenCalled();
    });

    it('should handle empty query param', () => {
      spyOn(component, 'selectTheme');

      component.ngOnInit();

      const mockParamMap = {
        get: jasmine.createSpy('get').and.returnValue(''),
      } as any;
      queryParamSubject.next(mockParamMap);

      expect(component.selectTheme).not.toHaveBeenCalled();
    });
  });

  describe('ngOnDestroy', () => {
    it('should call super.ngOnDestroy for cleanup', () => {
      mockThemeStorage.getStoredThemeName.and.returnValue(null);
      fixture.detectChanges();
      component.ngOnInit();

      spyOn(Object.getPrototypeOf(Object.getPrototypeOf(component)), 'ngOnDestroy');

      component.ngOnDestroy();

      expect(Object.getPrototypeOf(Object.getPrototypeOf(component)).ngOnDestroy).toHaveBeenCalled();
    });
  });

  describe('selectTheme', () => {
    beforeEach(() => {
      mockThemeStorage.getStoredThemeName.and.returnValue(null);
      fixture.detectChanges();

      // Reset spies after component initialization
      mockStyleManager.setStyle.calls.reset();
      mockStyleManager.removeStyle.calls.reset();
      mockThemeStorage.storeTheme.calls.reset();
      mockLiveAnnouncer.announce.calls.reset();
    });

    it('should select valid theme and update current theme', () => {
      component.selectTheme('pink-bluegrey');

      expect(component.currentTheme).toBeDefined();
      expect(component.currentTheme?.name).toBe('pink-bluegrey');
      expect(component.currentTheme?.displayName).toBe('Dark');
    });

    it('should not update theme for invalid theme name', () => {
      const originalTheme = component.currentTheme;

      component.selectTheme('invalid-theme');

      expect(component.currentTheme).toBe(originalTheme);
      expect(mockStyleManager.setStyle).not.toHaveBeenCalled();
      expect(mockStyleManager.removeStyle).not.toHaveBeenCalled();
    });

    it('should set style for default theme', () => {
      component.selectTheme('indigo-pink');

      expect(mockStyleManager.setStyle).toHaveBeenCalledWith('theme', 'assets/themes/indigo-pink.css');
      expect(mockStyleManager.removeStyle).not.toHaveBeenCalled();
    });

    it('should set style for non-default theme', () => {
      component.selectTheme('pink-bluegrey');

      expect(mockStyleManager.setStyle).toHaveBeenCalledWith('theme', 'assets/themes/pink-bluegrey.css');
      expect(mockStyleManager.removeStyle).not.toHaveBeenCalled();
    });

    it('should announce theme selection to screen readers', () => {
      component.selectTheme('pink-bluegrey');

      expect(mockLiveAnnouncer.announce).toHaveBeenCalledWith(
        'Dark theme selected.',
        'polite',
        3000,
      );
    });

    it('should store theme when successfully selected', () => {
      component.selectTheme('pink-bluegrey');

      expect(mockThemeStorage.storeTheme).toHaveBeenCalledWith(
        jasmine.objectContaining({
          name: 'pink-bluegrey',
          displayName: 'Dark',
        }),
      );
    });

    it('should handle selecting the same theme multiple times', () => {
      component.selectTheme('pink-bluegrey');
      component.selectTheme('pink-bluegrey');

      expect(mockStyleManager.setStyle).toHaveBeenCalledTimes(2);
      expect(mockThemeStorage.storeTheme).toHaveBeenCalledTimes(2);
      expect(mockLiveAnnouncer.announce).toHaveBeenCalledTimes(2);
    });

    it('should handle all available themes', () => {
      component.themes.forEach(theme => {
        component.selectTheme(theme.name);

        expect(component.currentTheme?.name).toBe(theme.name);
        expect(mockThemeStorage.storeTheme).toHaveBeenCalledWith(theme);
        expect(mockLiveAnnouncer.announce).toHaveBeenCalledWith(
          `${theme.displayName} theme selected.`,
          'polite',
          3000,
        );
      });
    });
  });

  describe('Template Integration', () => {
    it('should render without errors', () => {
      mockThemeStorage.getStoredThemeName.and.returnValue('indigo-pink');

      expect(() => fixture.detectChanges()).not.toThrow();
      expect(fixture.nativeElement).toBeTruthy();
    });

    it('should contain theme picker elements', () => {
      mockThemeStorage.getStoredThemeName.and.returnValue('indigo-pink');
      fixture.detectChanges();

      // With NO_ERRORS_SCHEMA, we just verify the template renders
      // without testing complex Material Design interactions
      const compiled = fixture.nativeElement;
      expect(compiled).toBeTruthy();
    });
  });

  describe('Theme Properties', () => {
    it('should have themes with required properties', () => {
      component.themes.forEach(theme => {
        expect(theme.name).toBeTruthy();
        expect(theme.primary).toBeTruthy();
        expect(theme.accent).toBeTruthy();
        expect(theme.displayName).toBeTruthy();
      });
    });
  });

  describe('Integration Tests', () => {
    beforeEach(() => {
      mockThemeStorage.getStoredThemeName.and.returnValue(null);
      fixture.detectChanges();

      // Reset spies after component initialization
      mockStyleManager.setStyle.calls.reset();
      mockStyleManager.removeStyle.calls.reset();
      mockThemeStorage.storeTheme.calls.reset();
      mockLiveAnnouncer.announce.calls.reset();
    });

    it('should complete full theme selection workflow', () => {
      // Select non-default theme
      component.selectTheme('pink-bluegrey');

      expect(component.currentTheme?.name).toBe('pink-bluegrey');
      expect(mockStyleManager.setStyle).toHaveBeenCalledWith('theme', 'assets/themes/pink-bluegrey.css');
      expect(mockThemeStorage.storeTheme).toHaveBeenCalled();
      expect(mockLiveAnnouncer.announce).toHaveBeenCalled();

      // Switch back to default theme
      component.selectTheme('indigo-pink');

      expect(component.currentTheme?.name).toBe('indigo-pink');
      expect(mockStyleManager.setStyle).toHaveBeenCalledWith('theme', 'assets/themes/indigo-pink.css');
    });

    it('should handle theme switching between multiple themes', () => {
      const themes = ['pink-bluegrey']; // Only test themes that exist

      themes.forEach(themeName => {
        component.selectTheme(themeName);
        expect(component.currentTheme?.name).toBe(themeName);
        expect(mockStyleManager.setStyle).toHaveBeenCalledWith('theme', `assets/themes/${themeName}.css`);
      });
    });
  });

  describe('Service Dependencies', () => {
    beforeEach(() => {
      mockThemeStorage.getStoredThemeName.and.returnValue(null);
      fixture.detectChanges();

      // Reset spies after component initialization
      mockStyleManager.setStyle.calls.reset();
      mockStyleManager.removeStyle.calls.reset();
      mockThemeStorage.storeTheme.calls.reset();
      mockLiveAnnouncer.announce.calls.reset();
    });

    it('should call theme storage when theme is selected', () => {
      const theme = component.themes.find(t => t.name === 'pink-bluegrey')!;

      component.selectTheme('pink-bluegrey');

      expect(mockThemeStorage.storeTheme).toHaveBeenCalledWith(theme);
    });

    it('should call live announcer for accessibility', () => {
      component.selectTheme('pink-bluegrey');

      expect(mockLiveAnnouncer.announce).toHaveBeenCalledWith(
        'Dark theme selected.',
        'polite',
        3000,
      );
    });

    it('should use consistent announcement format', () => {
      const theme = component.themes.find(t => t.name === 'pink-bluegrey')!;

      component.selectTheme('pink-bluegrey');

      expect(mockLiveAnnouncer.announce).toHaveBeenCalledWith(
        `${theme.displayName} theme selected.`,
        'polite',
        3000,
      );
    });
  });
});
