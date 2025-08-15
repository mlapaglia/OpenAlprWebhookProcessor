import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MatCardModule } from '@angular/material/card';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';

import { PredictionsSectionComponent } from './predictions-section.component';
import type { PredictionResult } from './../prediction-response';

describe('PredictionsSectionComponent', () => {
  let component: PredictionsSectionComponent;
  let fixture: ComponentFixture<PredictionsSectionComponent>;

  const createMockPrediction = (overrides: Partial<PredictionResult> = {}): PredictionResult => ({
    licensePlate: 'ABC123',
    predictedNextSeen: new Date('2025-01-01T12:00:00Z'),
    predictedHours: 2,
    confidenceScore: 0.8,
    totalHistoricalVisits: 10,
    averageTimeBetweenVisits: 24,
    lastSeen: new Date('2025-01-01T10:00:00Z'),
    modelVersion: '1.0',
    predictionMadeAt: new Date('2025-01-01T09:00:00Z'),
    ...overrides,
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        PredictionsSectionComponent,
        NoopAnimationsModule,
        MatCardModule,
        MatListModule,
        MatIconModule,
        MatProgressSpinnerModule,
        MatChipsModule,
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PredictionsSectionComponent);
    component = fixture.componentInstance;
  });

  describe('component initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.nextExpected()).toBe(null);
      expect(component.upcomingPredictions()).toEqual([]);
      expect(component.predictablePlates()).toEqual([]);
      expect(component.isLoadingPredictions()).toBe(false);
    });
  });

  describe('input properties', () => {
    it('should accept nextExpected input', () => {
      const mockPrediction = createMockPrediction();
      fixture.componentRef.setInput('nextExpected', mockPrediction);

      expect(component.nextExpected()).toEqual(mockPrediction);
    });

    it('should accept upcomingPredictions input', () => {
      const mockPredictions = [createMockPrediction(), createMockPrediction({ licensePlate: 'XYZ789' })];
      fixture.componentRef.setInput('upcomingPredictions', mockPredictions);

      expect(component.upcomingPredictions()).toEqual(mockPredictions);
    });

    it('should accept predictablePlates input', () => {
      const mockPredictions = [createMockPrediction({ confidenceScore: 0.9 })];
      fixture.componentRef.setInput('predictablePlates', mockPredictions);

      expect(component.predictablePlates()).toEqual(mockPredictions);
    });

    it('should accept isLoadingPredictions input', () => {
      fixture.componentRef.setInput('isLoadingPredictions', true);

      expect(component.isLoadingPredictions()).toBe(true);
    });
  });

  describe('formatTimeUntil method', () => {
    beforeEach(() => {
      jasmine.clock().install();
      jasmine.clock().mockDate(new Date('2025-01-01T10:00:00Z'));
    });

    afterEach(() => {
      jasmine.clock().uninstall();
    });

    it('should return "Now" for past times', () => {
      const pastTime = new Date('2025-01-01T09:30:00Z');
      const result = component.formatTimeUntil(pastTime);

      expect(result).toBe('Now');
    });

    it('should return minutes for times less than 1 hour', () => {
      const futureTime = new Date('2025-01-01T10:15:00Z'); // 15 minutes = 0.25 hours, rounds to 0
      const result = component.formatTimeUntil(futureTime);

      expect(result).toBe('15m');
    });

    it('should return hours for times less than 24 hours', () => {
      const futureTime = new Date('2025-01-01T13:00:00Z');
      const result = component.formatTimeUntil(futureTime);

      expect(result).toBe('3h');
    });

    it('should return days for times 24 hours or more', () => {
      const futureTime = new Date('2025-01-02T10:00:00Z');
      const result = component.formatTimeUntil(futureTime);

      expect(result).toBe('1d');
    });

    it('should handle multiple days', () => {
      const futureTime = new Date('2025-01-03T10:00:00Z');
      const result = component.formatTimeUntil(futureTime);

      expect(result).toBe('2d');
    });
  });

  describe('getConfidenceColor method', () => {
    it('should return green for high confidence (>= 0.7)', () => {
      expect(component.getConfidenceColor(0.8)).toBe('#4CAF50');
      expect(component.getConfidenceColor(0.7)).toBe('#4CAF50');
    });

    it('should return orange for medium confidence (>= 0.4)', () => {
      expect(component.getConfidenceColor(0.5)).toBe('#FF9800');
      expect(component.getConfidenceColor(0.4)).toBe('#FF9800');
    });

    it('should return red for low confidence (< 0.4)', () => {
      expect(component.getConfidenceColor(0.3)).toBe('#F44336');
      expect(component.getConfidenceColor(0.1)).toBe('#F44336');
    });
  });

  describe('getConfidenceText method', () => {
    it('should return "High" for high confidence (>= 0.7)', () => {
      expect(component.getConfidenceText(0.8)).toBe('High');
      expect(component.getConfidenceText(0.7)).toBe('High');
    });

    it('should return "Medium" for medium confidence (>= 0.4)', () => {
      expect(component.getConfidenceText(0.5)).toBe('Medium');
      expect(component.getConfidenceText(0.4)).toBe('Medium');
    });

    it('should return "Low" for low confidence (< 0.4)', () => {
      expect(component.getConfidenceText(0.3)).toBe('Low');
      expect(component.getConfidenceText(0.1)).toBe('Low');
    });
  });

  describe('shouldShow getter', () => {
    it('should return true when nextExpected is present', () => {
      fixture.componentRef.setInput('nextExpected', createMockPrediction());

      expect(component.shouldShow).toBe(true);
    });

    it('should return true when isLoadingPredictions is true', () => {
      fixture.componentRef.setInput('isLoadingPredictions', true);

      expect(component.shouldShow).toBe(true);
    });

    it('should return true when upcomingPredictions has items', () => {
      fixture.componentRef.setInput('upcomingPredictions', [createMockPrediction()]);

      expect(component.shouldShow).toBe(true);
    });

    it('should return true when predictablePlates has items', () => {
      fixture.componentRef.setInput('predictablePlates', [createMockPrediction()]);

      expect(component.shouldShow).toBe(true);
    });

    it('should return false when all conditions are false/empty', () => {
      fixture.componentRef.setInput('nextExpected', null);
      fixture.componentRef.setInput('isLoadingPredictions', false);
      fixture.componentRef.setInput('upcomingPredictions', []);
      fixture.componentRef.setInput('predictablePlates', []);

      expect(component.shouldShow).toBe(false);
    });
  });

  describe('nextExpectedWithFormatting getter', () => {
    beforeEach(() => {
      jasmine.clock().install();
      jasmine.clock().mockDate(new Date('2025-01-01T10:00:00Z'));
    });

    afterEach(() => {
      jasmine.clock().uninstall();
    });

    it('should return null when nextExpected is null', () => {
      fixture.componentRef.setInput('nextExpected', null);

      expect(component.nextExpectedWithFormatting).toBe(null);
    });

    it('should return formatted object when nextExpected is present', () => {
      const mockPrediction = createMockPrediction({
        predictedNextSeen: new Date('2025-01-01T12:00:00Z'),
        confidenceScore: 0.8,
      });
      fixture.componentRef.setInput('nextExpected', mockPrediction);

      const result = component.nextExpectedWithFormatting;

      expect(result).toEqual({
        ...mockPrediction,
        formattedTime: '2h',
        confidenceColor: '#4CAF50',
        confidenceText: 'High',
      });
    });
  });

  describe('upcomingPredictionsWithFormatting getter', () => {
    beforeEach(() => {
      jasmine.clock().install();
      jasmine.clock().mockDate(new Date('2025-01-01T10:00:00Z'));
    });

    afterEach(() => {
      jasmine.clock().uninstall();
    });

    it('should return empty array when upcomingPredictions is empty', () => {
      fixture.componentRef.setInput('upcomingPredictions', []);

      expect(component.upcomingPredictionsWithFormatting).toEqual([]);
    });

    it('should return formatted array when upcomingPredictions has items', () => {
      const mockPredictions = [
        createMockPrediction({
          licensePlate: 'ABC123',
          predictedNextSeen: new Date('2025-01-01T11:00:00Z'),
          confidenceScore: 0.6,
        }),
        createMockPrediction({
          licensePlate: 'XYZ789',
          predictedNextSeen: new Date('2025-01-01T14:00:00Z'),
          confidenceScore: 0.9,
        }),
      ];
      fixture.componentRef.setInput('upcomingPredictions', mockPredictions);

      const result = component.upcomingPredictionsWithFormatting;

      expect(result).toEqual([
        {
          ...mockPredictions[0],
          formattedTime: '1h',
          confidenceColor: '#FF9800',
          confidencePercentage: '60',
        },
        {
          ...mockPredictions[1],
          formattedTime: '4h',
          confidenceColor: '#4CAF50',
          confidencePercentage: '90',
        },
      ]);
    });
  });

  describe('predictablePlatesWithFormatting getter', () => {
    it('should return empty array when predictablePlates is empty', () => {
      fixture.componentRef.setInput('predictablePlates', []);

      expect(component.predictablePlatesWithFormatting).toEqual([]);
    });

    it('should return formatted array when predictablePlates has items', () => {
      const mockPredictions = [
        createMockPrediction({
          licensePlate: 'HIGH123',
          confidenceScore: 0.95,
        }),
        createMockPrediction({
          licensePlate: 'MED456',
          confidenceScore: 0.5,
        }),
      ];
      fixture.componentRef.setInput('predictablePlates', mockPredictions);

      const result = component.predictablePlatesWithFormatting;

      expect(result).toEqual([
        {
          ...mockPredictions[0],
          confidenceColor: '#4CAF50',
          confidencePercentage: '95',
        },
        {
          ...mockPredictions[1],
          confidenceColor: '#FF9800',
          confidencePercentage: '50',
        },
      ]);
    });
  });

  describe('template rendering', () => {
    it('should not render anything when shouldShow is false', () => {
      fixture.componentRef.setInput('nextExpected', null);
      fixture.componentRef.setInput('isLoadingPredictions', false);
      fixture.componentRef.setInput('upcomingPredictions', []);
      fixture.componentRef.setInput('predictablePlates', []);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const predictionsSection = compiled.querySelector('.predictions-section');

      expect(predictionsSection).toBeFalsy();
    });

    it('should render next expected section when present', () => {
      fixture.componentRef.setInput('nextExpected', createMockPrediction({
        licensePlate: 'NEXT123',
        confidenceScore: 0.8,
      }));
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const featuredCard = compiled.querySelector('.featured-prediction-card');
      const plateNumber = compiled.querySelector('.plate-number');

      expect(featuredCard).toBeTruthy();
      expect(plateNumber?.textContent?.trim()).toBe('NEXT123');
    });

    it('should render loading spinner when isLoadingPredictions is true', () => {
      fixture.componentRef.setInput('isLoadingPredictions', true);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const spinner = compiled.querySelector('mat-progress-spinner');

      expect(spinner).toBeTruthy();
    });

    it('should render upcoming predictions when present', () => {
      fixture.componentRef.setInput('upcomingPredictions', [
        createMockPrediction({ licensePlate: 'UP001' }),
        createMockPrediction({ licensePlate: 'UP002' }),
      ]);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const listItems = compiled.querySelectorAll('mat-list-item');

      expect(listItems.length).toBeGreaterThanOrEqual(2);
    });

    it('should render predictable plates when present', () => {
      fixture.componentRef.setInput('predictablePlates', [createMockPrediction({ licensePlate: 'PRED001', confidenceScore: 0.9 })]);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const predictableCard = compiled.querySelector('.predictions-card');

      expect(predictableCard).toBeTruthy();
    });
  });

  describe('confidence percentage calculation', () => {
    it('should calculate confidence percentage correctly', () => {
      fixture.componentRef.setInput('upcomingPredictions', [createMockPrediction({ confidenceScore: 0.753 })]);

      const result = component.upcomingPredictionsWithFormatting;

      expect(result[0].confidencePercentage).toBe('75');
    });

    it('should handle zero confidence', () => {
      fixture.componentRef.setInput('predictablePlates', [createMockPrediction({ confidenceScore: 0 })]);

      const result = component.predictablePlatesWithFormatting;

      expect(result[0].confidencePercentage).toBe('0');
    });

    it('should handle perfect confidence', () => {
      fixture.componentRef.setInput('predictablePlates', [createMockPrediction({ confidenceScore: 1 })]);

      const result = component.predictablePlatesWithFormatting;

      expect(result[0].confidencePercentage).toBe('100');
    });
  });
});
