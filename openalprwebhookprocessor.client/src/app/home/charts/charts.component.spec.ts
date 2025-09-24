import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { ChartsComponent } from './charts.component';
import { ChartService } from './charts.service';

describe('ChartsComponent', () => {
  let component: ChartsComponent;
  let fixture: ComponentFixture<ChartsComponent>;
  let mockChartService: jasmine.SpyObj<ChartService>;
  let mockChart: any;

  beforeEach(async () => {
    mockChart = {
      destroy: jasmine.createSpy('destroy'),
      update: jasmine.createSpy('update'),
      data: { labels: [], datasets: [{ data: [] }] },
    };

    mockChartService = jasmine.createSpyObj('ChartService', [
      'createDailyChart',
      'createHourlyChart',
      'updateChart',
      'destroyChart',
    ]);

    mockChartService.createDailyChart.and.returnValue(mockChart);
    mockChartService.createHourlyChart.and.returnValue(mockChart);

    await TestBed.configureTestingModule({
      imports: [ChartsComponent],
      providers: [{ provide: ChartService, useValue: mockChartService }],
    }).compileComponents();

    fixture = TestBed.createComponent(ChartsComponent);
    component = fixture.componentInstance;
  });

  describe('component initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default loading state', () => {
      expect(component.isLoadingCharts()).toBe(false);
    });

    it('should accept isLoadingCharts input', () => {
      fixture.componentRef.setInput('isLoadingCharts', true);
      fixture.detectChanges();
      expect(component.isLoadingCharts()).toBe(true);
    });
  });

  describe('ngAfterViewInit', () => {
    beforeEach(() => {
      // Mock ViewChild elements
      const mockDailyCanvas = document.createElement('canvas');
      const mockHourlyCanvas = document.createElement('canvas');

      spyOn(component, 'dailyChartRef').and.returnValue({
        nativeElement: mockDailyCanvas,
      } as any);
      spyOn(component, 'hourlyChartRef').and.returnValue({
        nativeElement: mockHourlyCanvas,
      } as any);
    });

    it('should initialize both charts', () => {
      component.ngAfterViewInit();

      expect(mockChartService.createDailyChart).toHaveBeenCalled();
      expect(mockChartService.createHourlyChart).toHaveBeenCalled();
    });
  });

  describe('ngOnDestroy', () => {
    it('should destroy both charts', () => {
      component['dailyChart'] = mockChart;
      component['hourlyChart'] = mockChart;

      component.ngOnDestroy();

      expect(mockChartService.destroyChart).toHaveBeenCalledWith(mockChart);
      expect(mockChartService.destroyChart).toHaveBeenCalledTimes(2);
    });

    it('should handle null charts gracefully', () => {
      component['dailyChart'] = null;
      component['hourlyChart'] = null;

      expect(() => component.ngOnDestroy()).not.toThrow();
      expect(mockChartService.destroyChart).toHaveBeenCalledWith(null);
      expect(mockChartService.destroyChart).toHaveBeenCalledTimes(2);
    });
  });

  describe('updateDailyChart', () => {
    it('should call chart service to update daily chart', () => {
      const labels = ['Day 1', 'Day 2', 'Day 3'];
      const data = [10, 20, 30];

      component.updateDailyChart(labels, data);

      expect(mockChartService.updateChart).toHaveBeenCalledWith(
        component['dailyChart'],
        { labels, data },
      );
    });

    it('should handle empty data', () => {
      const labels: string[] = [];
      const data: number[] = [];

      component.updateDailyChart(labels, data);

      expect(mockChartService.updateChart).toHaveBeenCalledWith(
        component['dailyChart'],
        { labels, data },
      );
    });
  });

  describe('updateHourlyChart', () => {
    it('should call chart service to update hourly chart', () => {
      const labels = ['Hour 1', 'Hour 2', 'Hour 3'];
      const data = [5, 15, 25];

      component.updateHourlyChart(labels, data);

      expect(mockChartService.updateChart).toHaveBeenCalledWith(
        component['hourlyChart'],
        { labels, data },
      );
    });

    it('should handle empty data', () => {
      const labels: string[] = [];
      const data: number[] = [];

      component.updateHourlyChart(labels, data);

      expect(mockChartService.updateChart).toHaveBeenCalledWith(
        component['hourlyChart'],
        { labels, data },
      );
    });
  });

  describe('component lifecycle integration', () => {
    beforeEach(() => {
      const mockDailyCanvas = document.createElement('canvas');
      const mockHourlyCanvas = document.createElement('canvas');

      spyOn(component, 'dailyChartRef').and.returnValue({
        nativeElement: mockDailyCanvas,
      } as any);
      spyOn(component, 'hourlyChartRef').and.returnValue({
        nativeElement: mockHourlyCanvas,
      } as any);
    });

    it('should initialize charts and then be able to update them', () => {
      component.ngAfterViewInit();

      expect(mockChartService.createDailyChart).toHaveBeenCalled();
      expect(mockChartService.createHourlyChart).toHaveBeenCalled();

      // Now update charts
      component.updateDailyChart(['Day 1'], [10]);
      component.updateHourlyChart(['Hour 1'], [5]);

      expect(mockChartService.updateChart).toHaveBeenCalledTimes(2);
    });

    it('should initialize and destroy charts in proper lifecycle', () => {
      component.ngAfterViewInit();
      component.ngOnDestroy();

      expect(mockChartService.createDailyChart).toHaveBeenCalled();
      expect(mockChartService.createHourlyChart).toHaveBeenCalled();
      expect(mockChartService.destroyChart).toHaveBeenCalledTimes(2);
    });
  });

  describe('public methods', () => {
    it('should have updateDailyChart method', () => {
      expect(typeof component.updateDailyChart).toBe('function');
    });

    it('should have updateHourlyChart method', () => {
      expect(typeof component.updateHourlyChart).toBe('function');
    });
  });

  describe('chart service integration', () => {
    it('should pass correct canvas elements to chart service', () => {
      const dailyCanvas = document.createElement('canvas');
      const hourlyCanvas = document.createElement('canvas');

      spyOn(component, 'dailyChartRef').and.returnValue({
        nativeElement: dailyCanvas,
      } as any);
      spyOn(component, 'hourlyChartRef').and.returnValue({
        nativeElement: hourlyCanvas,
      } as any);

      component.ngAfterViewInit();

      expect(mockChartService.createDailyChart).toHaveBeenCalledWith(dailyCanvas);
      expect(mockChartService.createHourlyChart).toHaveBeenCalledWith(hourlyCanvas);
    });

    it('should pass chart data in correct format to service', () => {
      // Initialize the chart first
      spyOn(component, 'dailyChartRef').and.returnValue({
        nativeElement: document.createElement('canvas'),
      } as any);
      spyOn(component, 'hourlyChartRef').and.returnValue({
        nativeElement: document.createElement('canvas'),
      } as any);
      component.ngAfterViewInit();

      const labels = ['Test Label'];
      const data = [42];

      component.updateDailyChart(labels, data);

      expect(mockChartService.updateChart).toHaveBeenCalledWith(
        mockChart, // Now it should be the mock chart, not null
        { labels, data },
      );
    });
  });
});
