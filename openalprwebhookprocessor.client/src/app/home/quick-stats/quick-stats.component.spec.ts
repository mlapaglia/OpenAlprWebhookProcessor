import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { QuickStatsComponent } from './quick-stats.component';
import type { QuickStats } from './../home.service';

describe('QuickStatsComponent', () => {
  let component: QuickStatsComponent;
  let fixture: ComponentFixture<QuickStatsComponent>;

  const createMockQuickStats = (overrides: Partial<QuickStats> = {}): QuickStats => ({
    todayCount: 45,
    weekCount: 320,
    monthCount: 1200,
    uniquePlatesThisWeek: 85,
    activeCameras: 4,
    averageDailyPlates: 40,
    ...overrides,
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        QuickStatsComponent,
        NoopAnimationsModule,
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(QuickStatsComponent);
    component = fixture.componentInstance;
  });

  describe('component initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.quickStats()).toBe(null);
      expect(component.isLoadingStats()).toBe(false);
      expect(component.quickStatCols()).toBe(4);
    });
  });

  describe('input properties', () => {
    it('should accept quickStats input', () => {
      const mockStats = createMockQuickStats();
      fixture.componentRef.setInput('quickStats', mockStats);

      expect(component.quickStats()).toEqual(mockStats);
    });

    it('should accept isLoadingStats input', () => {
      fixture.componentRef.setInput('isLoadingStats', true);


      expect(component.isLoadingStats()).toBe(true);
    });

    it('should accept quickStatCols input', () => {
      fixture.componentRef.setInput('quickStatCols', 2);

      expect(component.quickStatCols()).toBe(2);
    });

    it('should handle null quickStats', () => {
      fixture.componentRef.setInput('quickStats', null);

      expect(component.quickStats()).toBe(null);
    });
  });

  describe('template rendering', () => {
    it('should render grid with correct number of columns', () => {
      fixture.componentRef.setInput('quickStatCols', 3);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const gridList = compiled.querySelector('mat-grid-list');

      expect(gridList).toBeTruthy();
      // The cols binding should be reflected in the component property
      expect(component.quickStatCols()).toBe(3);
    });

    it('should render all stat cards', () => {
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const statCards = compiled.querySelectorAll('.stat-card');

      expect(statCards.length).toBe(4);
    });

    it('should render loading spinners when isLoadingStats is true', () => {
      fixture.componentRef.setInput('isLoadingStats', true);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const spinners = compiled.querySelectorAll('mat-spinner');

      expect(spinners.length).toBe(4);
    });

    it('should not render spinners when isLoadingStats is false', () => {
      fixture.componentRef.setInput('isLoadingStats', false);
      fixture.componentRef.setInput('quickStats', createMockQuickStats());
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const spinners = compiled.querySelectorAll('mat-spinner');

      expect(spinners.length).toBe(0);
    });

    it('should display stat values when quickStats is provided', () => {
      const mockStats = createMockQuickStats({
        todayCount: 25,
        weekCount: 180,
        uniquePlatesThisWeek: 42,
        activeCameras: 3,
      });
      fixture.componentRef.setInput('quickStats', mockStats);
      fixture.componentRef.setInput('isLoadingStats', false);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const statValues = compiled.querySelectorAll('.stat-value');

      expect(statValues[0]?.textContent?.trim()).toBe('25');
      expect(statValues[1]?.textContent?.trim()).toBe('180');
      expect(statValues[2]?.textContent?.trim()).toBe('42');
      expect(statValues[3]?.textContent?.trim()).toBe('3');
    });

    it('should not display stat values when quickStats is null', () => {
      fixture.componentRef.setInput('quickStats', null);
      fixture.componentRef.setInput('isLoadingStats', false);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const statValues = compiled.querySelectorAll('.stat-value');

      expect(statValues.length).toBe(0);
    });

    it('should render correct stat labels', () => {
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const statTitles = compiled.querySelectorAll('.stat-content h3');

      expect(statTitles[0]?.textContent?.trim()).toBe('Today');
      expect(statTitles[1]?.textContent?.trim()).toBe('This Week');
      expect(statTitles[2]?.textContent?.trim()).toBe('Unique Plates');
      expect(statTitles[3]?.textContent?.trim()).toBe('Active Cameras');
    });

    it('should render correct icons', () => {
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const icons = compiled.querySelectorAll('.stat-icon mat-icon');

      expect(icons[0]?.textContent?.trim()).toBe('today');
      expect(icons[1]?.textContent?.trim()).toBe('date_range');
      expect(icons[2]?.textContent?.trim()).toBe('directions_car');
      expect(icons[3]?.textContent?.trim()).toBe('videocam');
    });
  });

  describe('grid layout', () => {
    it('should use default column count of 4', () => {
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const gridList = compiled.querySelector('mat-grid-list');

      expect(gridList).toBeTruthy();
      expect(component.quickStatCols()).toBe(4);
    });

    it('should adjust column count based on input', () => {
      fixture.componentRef.setInput('quickStatCols', 2);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const gridList = compiled.querySelector('mat-grid-list');

      expect(gridList).toBeTruthy();
      expect(component.quickStatCols()).toBe(2);
    });

    it('should render all grid tiles', () => {
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const gridTiles = compiled.querySelectorAll('mat-grid-tile');

      expect(gridTiles.length).toBe(4);
    });
  });

  describe('data handling', () => {
    it('should handle zero values', () => {
      const mockStats = createMockQuickStats({
        todayCount: 0,
        weekCount: 0,
        uniquePlatesThisWeek: 0,
        activeCameras: 0,
      });
      fixture.componentRef.setInput('quickStats', mockStats);
      fixture.componentRef.setInput('isLoadingStats', false);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const statValues = compiled.querySelectorAll('.stat-value');

      expect(statValues[0]?.textContent?.trim()).toBe('0');
      expect(statValues[1]?.textContent?.trim()).toBe('0');
      expect(statValues[2]?.textContent?.trim()).toBe('0');
      expect(statValues[3]?.textContent?.trim()).toBe('0');
    });

    it('should handle large numbers', () => {
      const mockStats = createMockQuickStats({
        todayCount: 9999,
        weekCount: 50000,
        uniquePlatesThisWeek: 15000,
        activeCameras: 100,
      });
      fixture.componentRef.setInput('quickStats', mockStats);
      fixture.componentRef.setInput('isLoadingStats', false);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const statValues = compiled.querySelectorAll('.stat-value');

      expect(statValues[0]?.textContent?.trim()).toBe('9999');
      expect(statValues[1]?.textContent?.trim()).toBe('50000');
      expect(statValues[2]?.textContent?.trim()).toBe('15000');
      expect(statValues[3]?.textContent?.trim()).toBe('100');
    });

    it('should handle negative numbers', () => {
      const mockStats = createMockQuickStats({
        todayCount: -1,
        weekCount: -5,
        uniquePlatesThisWeek: -10,
        activeCameras: -2,
      });
      fixture.componentRef.setInput('quickStats', mockStats);
      fixture.componentRef.setInput('isLoadingStats', false);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const statValues = compiled.querySelectorAll('.stat-value');

      expect(statValues[0]?.textContent?.trim()).toBe('-1');
      expect(statValues[1]?.textContent?.trim()).toBe('-5');
      expect(statValues[2]?.textContent?.trim()).toBe('-10');
      expect(statValues[3]?.textContent?.trim()).toBe('-2');
    });
  });

  describe('loading states', () => {
    it('should show loading state for all stats when isLoadingStats is true', () => {
      fixture.componentRef.setInput('isLoadingStats', true);
      fixture.componentRef.setInput('quickStats', null);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const spinners = compiled.querySelectorAll('mat-spinner');
      const statValues = compiled.querySelectorAll('.stat-value');

      expect(spinners.length).toBe(4);
      expect(statValues.length).toBe(0);
    });

    it('should show stats when loading is complete', () => {
      fixture.componentRef.setInput('isLoadingStats', false);
      fixture.componentRef.setInput('quickStats', createMockQuickStats());
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const spinners = compiled.querySelectorAll('mat-spinner');
      const statValues = compiled.querySelectorAll('.stat-value');

      expect(spinners.length).toBe(0);
      expect(statValues.length).toBe(4);
    });

    it('should handle transition from loading to loaded state', () => {
      // Start with loading state
      fixture.componentRef.setInput('isLoadingStats', true);
      fixture.componentRef.setInput('quickStats', null);
      fixture.detectChanges();

      let compiled = fixture.nativeElement as HTMLElement;
      let spinners = compiled.querySelectorAll('mat-spinner');
      expect(spinners.length).toBe(4);

      // Transition to loaded state
      fixture.componentRef.setInput('isLoadingStats', false);
      fixture.componentRef.setInput('quickStats', createMockQuickStats());
      fixture.detectChanges();

      compiled = fixture.nativeElement as HTMLElement;
      spinners = compiled.querySelectorAll('mat-spinner');
      const statValues = compiled.querySelectorAll('.stat-value');

      expect(spinners.length).toBe(0);
      expect(statValues.length).toBe(4);
    });
  });
});
