import type { ComponentFixture } from '@angular/core/testing';
import { TestBed } from '@angular/core/testing';
import { HomeComponent } from './home.component';
import { LayoutService } from './layout.service';
import { HomeDataService } from './home-data.service';
import { AccountService } from 'app/_services';
import { of, throwError } from 'rxjs';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe('HomeComponent', () => {
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;
  let mockLayoutService: jasmine.SpyObj<LayoutService>;
  let mockHomeDataService: jasmine.SpyObj<HomeDataService>;

  beforeEach(async () => {
    const layoutServiceSpy = jasmine.createSpyObj('LayoutService', ['getLayoutState']);
    const homeDataServiceSpy = jasmine.createSpyObj('HomeDataService', ['loadAllData']);
    const accountServiceSpy = jasmine.createSpyObj('AccountService', [], {
      userValue: {
        id: '1',
        username: 'testuser',
        firstName: 'Test',
        lastName: 'User',
        password: 'password',
        isDeleting: false,
        twoFactorEnabled: false,
      },
    });

    await TestBed.configureTestingModule({
      imports: [HomeComponent, NoopAnimationsModule],
      providers: [
        { provide: LayoutService, useValue: layoutServiceSpy },
        { provide: HomeDataService, useValue: homeDataServiceSpy },
        { provide: AccountService, useValue: accountServiceSpy },
      ],
    }).compileComponents();

    mockLayoutService = TestBed.inject(LayoutService) as jasmine.SpyObj<LayoutService>;
    mockHomeDataService = TestBed.inject(HomeDataService) as jasmine.SpyObj<HomeDataService>;

    // Setup default return values
    mockLayoutService.getLayoutState.and.returnValue(of({
      isMobile: false,
      isTablet: false,
      quickStatCols: 4,
    }));

    mockHomeDataService.loadAllData.and.returnValue(of({
      quickStats: {
        todayCount: 10,
        weekCount: 50,
        monthCount: 200,
        uniquePlatesThisWeek: 25,
        activeCameras: 5,
        averageDailyPlates: 15,
      },
      nextExpected: null,
      upcomingPredictions: [],
      predictablePlates: [],
      mostSeenCounts: [],
      hourlyStats: [],
      dailyStats: [],
    }));

    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with default values', () => {
    expect(component.mostSeenCounts).toEqual([]);
    expect(component.nextExpected).toBeNull();
    expect(component.upcomingPredictions).toEqual([]);
    expect(component.predictablePlates).toEqual([]);
    expect(component.quickStats).toBeNull();
    expect(component.isLoadingPredictions).toBeFalse();
    expect(component.isLoadingStats).toBeFalse();
    expect(component.isLoadingCharts).toBeFalse();
  });

  it('should have user from account service', () => {
    expect(component.user).toBeDefined();
    expect(component.user.username).toBe('testuser');
  });

  it('should setup responsive layout after view init', () => {
    component.ngAfterViewInit();
    expect(mockLayoutService.getLayoutState).toHaveBeenCalled();
  });

  it('should update layout properties when layout state changes', (done) => {
    const mockLayoutState = {
      isMobile: true,
      isTablet: false,
      quickStatCols: 2,
    };

    mockLayoutService.getLayoutState.and.returnValue(of(mockLayoutState));

    component.ngAfterViewInit();

    // Wait for subscription to process
    setTimeout(() => {
      expect(component.isMobile).toBe(true);
      expect(component.isTablet).toBe(false);
      expect(component.quickStatCols).toBe(2);
      done();
    }, 20);
  });

  it('should load data after view init', (done) => {
    const mockData = {
      quickStats: {
        todayCount: 15,
        weekCount: 75,
        monthCount: 300,
        uniquePlatesThisWeek: 30,
        activeCameras: 8,
        averageDailyPlates: 20,
      },
      nextExpected: null,
      upcomingPredictions: [],
      predictablePlates: [],
      mostSeenCounts: [{ name: 'ABC123', value: 5 }],
      hourlyStats: [],
      dailyStats: [{ date: '2024-01-01', count: 10 }, { date: '2024-01-02', count: 15 }],
    };

    mockHomeDataService.loadAllData.and.returnValue(of(mockData));

    component.ngAfterViewInit();

    setTimeout(() => {
      expect(mockHomeDataService.loadAllData).toHaveBeenCalled();
      expect(component.quickStats).toEqual(mockData.quickStats);
      expect(component.mostSeenCounts).toEqual(mockData.mostSeenCounts);
      expect(component.isLoadingStats).toBeFalse();
      expect(component.isLoadingPredictions).toBeFalse();
      expect(component.isLoadingCharts).toBeFalse();
      done();
    }, 10);
  });

  it('should handle data loading errors gracefully', (done) => {
    mockHomeDataService.loadAllData.and.returnValue(throwError(() => new Error('Test error')));

    component.ngAfterViewInit();

    setTimeout(() => {
      expect(component.isLoadingStats).toBeFalse();
      expect(component.isLoadingPredictions).toBeFalse();
      expect(component.isLoadingCharts).toBeFalse();
      done();
    }, 10);
  });

  it('should call super.ngOnDestroy for cleanup', () => {
    component.ngAfterViewInit();

    spyOn(Object.getPrototypeOf(Object.getPrototypeOf(component)), 'ngOnDestroy');

    component.ngOnDestroy();

    expect(Object.getPrototypeOf(Object.getPrototypeOf(component)).ngOnDestroy).toHaveBeenCalled();
  });

  it('should set loading states correctly', (done) => {
    // Test the private method indirectly by triggering data load
    component.ngAfterViewInit();

    // Initially loading should be set to true, then false after data loads
    setTimeout(() => {
      expect(component.isLoadingStats).toBeFalse();
      expect(component.isLoadingPredictions).toBeFalse();
      expect(component.isLoadingCharts).toBeFalse();
      done();
    }, 10);
  });
});
