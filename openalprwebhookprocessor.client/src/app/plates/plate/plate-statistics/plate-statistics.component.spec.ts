import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { PlateStatisticsComponent } from './plate-statistics.component';
import type { PlateStatisticsData } from '../plateStatistics';

describe('PlateStatisticsComponent', () => {
  let component: PlateStatisticsComponent;
  let fixture: ComponentFixture<PlateStatisticsComponent>;

  const mockStatistics: PlateStatisticsData[] = [
    { key: 'Confidence', value: '95.5%' },
    { key: 'Total Seen', value: '25' },
    { key: 'First seen', value: 'Jan 1, 2024, 12:00:00 PM' },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        MatTableModule,
        MatProgressSpinnerModule,
        MatIconModule,
        MatCardModule,
        PlateStatisticsComponent,
      ],
    })
      .compileComponents();

    fixture = TestBed.createComponent(PlateStatisticsComponent);
    component = fixture.componentInstance;

    fixture.componentRef.setInput('plateStatistics', mockStatistics);
    fixture.componentRef.setInput('loadingStatistics', false);
    fixture.componentRef.setInput('loadingStatisticsFailed', false);

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with input signals', () => {
    expect(component.plateStatistics()).toEqual(mockStatistics);
    expect(component.loadingStatistics()).toBe(false);
    expect(component.loadingStatisticsFailed()).toBe(false);
  });

  it('should have correct displayed columns', () => {
    expect(component.displayedColumns).toEqual(['key', 'value']);
  });

  it('should handle loading state', () => {
    fixture.componentRef.setInput('loadingStatistics', true);
    fixture.detectChanges();

    expect(component.loadingStatistics()).toBe(true);
  });

  it('should handle failed loading state', () => {
    fixture.componentRef.setInput('loadingStatisticsFailed', true);
    fixture.detectChanges();

    expect(component.loadingStatisticsFailed()).toBe(true);
  });

  it('should handle empty statistics', () => {
    fixture.componentRef.setInput('plateStatistics', []);
    fixture.detectChanges();

    expect(component.plateStatistics()).toEqual([]);
  });
});
