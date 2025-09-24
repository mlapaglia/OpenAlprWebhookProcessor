import { type ComponentFixture, TestBed } from '@angular/core/testing';

import { MostSeenPlatesComponent } from './most-seen-plates.component';

describe('MostSeenPlatesComponent', () => {
  let component: MostSeenPlatesComponent;
  let fixture: ComponentFixture<MostSeenPlatesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MostSeenPlatesComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(MostSeenPlatesComponent);
    component = fixture.componentInstance;
  });

  describe('component initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with empty mostSeenCounts array', () => {
      expect(component.mostSeenCounts()).toEqual([]);
    });
  });

  describe('input properties', () => {
    it('should accept mostSeenCounts input', () => {
      const mockData = [
        { name: 'ABC123', value: 15 },
        { name: 'XYZ789', value: 12 },
        { name: 'DEF456', value: 8 },
      ];

      fixture.componentRef.setInput('mostSeenCounts', mockData);
      fixture.detectChanges();

      expect(component.mostSeenCounts()).toEqual(mockData);
    });

    it('should handle empty mostSeenCounts', () => {
      fixture.componentRef.setInput('mostSeenCounts', []);
      fixture.detectChanges();

      expect(component.mostSeenCounts()).toEqual([]);
    });
  });

  describe('template rendering', () => {
    it('should render card title and subtitle', () => {
      fixture.detectChanges();
      const compiled = fixture.nativeElement as HTMLElement;

      const title = compiled.querySelector('mat-card-title');
      const subtitle = compiled.querySelector('mat-card-subtitle');

      expect(title?.textContent?.trim()).toBe('Most Frequently Seen');
      expect(subtitle?.textContent?.trim()).toBe('Top plates by occurrence');
    });

    it('should render list items for each plate', () => {
      const mockData = [
        { name: 'ABC123', value: 15 },
        { name: 'XYZ789', value: 12 },
        { name: 'DEF456', value: 8 },
      ];

      fixture.componentRef.setInput('mostSeenCounts', mockData);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const listItems = compiled.querySelectorAll('mat-list-item');

      expect(listItems.length).toBe(3);
    });

    it('should display correct ranking numbers', () => {
      const mockData = [
        { name: 'ABC123', value: 15 },
        { name: 'XYZ789', value: 12 },
        { name: 'DEF456', value: 8 },
      ];

      fixture.componentRef.setInput('mostSeenCounts', mockData);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const ranks = compiled.querySelectorAll('.rank');

      expect(ranks[0]?.textContent?.trim()).toBe('#1');
      expect(ranks[1]?.textContent?.trim()).toBe('#2');
      expect(ranks[2]?.textContent?.trim()).toBe('#3');
    });

    it('should display plate names correctly', () => {
      const mockData = [
        { name: 'ABC123', value: 15 },
        { name: 'XYZ789', value: 12 },
      ];

      fixture.componentRef.setInput('mostSeenCounts', mockData);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const listItems = compiled.querySelectorAll('mat-list-item');

      expect(listItems[0]?.textContent).toContain('ABC123');
      expect(listItems[1]?.textContent).toContain('XYZ789');
    });

    it('should display count chips with correct values', () => {
      const mockData = [
        { name: 'ABC123', value: 15 },
        { name: 'XYZ789', value: 12 },
      ];

      fixture.componentRef.setInput('mostSeenCounts', mockData);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const chips = compiled.querySelectorAll('mat-chip');

      expect(chips[0]?.textContent?.trim()).toBe('15 times');
      expect(chips[1]?.textContent?.trim()).toBe('12 times');
    });

    it('should handle empty list correctly', () => {
      fixture.componentRef.setInput('mostSeenCounts', []);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const listItems = compiled.querySelectorAll('mat-list-item');

      expect(listItems.length).toBe(0);
    });

    it('should handle single item list', () => {
      const mockData = [{ name: 'SINGLE', value: 5 }];

      fixture.componentRef.setInput('mostSeenCounts', mockData);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const listItems = compiled.querySelectorAll('mat-list-item');
      const rank = compiled.querySelector('.rank');
      const chip = compiled.querySelector('mat-chip');

      expect(listItems.length).toBe(1);
      expect(rank?.textContent?.trim()).toBe('#1');
      expect(chip?.textContent?.trim()).toBe('5 times');
      expect(listItems[0]?.textContent).toContain('SINGLE');
    });
  });

  describe('data handling', () => {
    it('should handle large count values', () => {
      const mockData = [{ name: 'HIGH123', value: 1000 }];

      fixture.componentRef.setInput('mostSeenCounts', mockData);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const chip = compiled.querySelector('mat-chip');

      expect(chip?.textContent?.trim()).toBe('1000 times');
    });

    it('should handle zero count values', () => {
      const mockData = [{ name: 'ZERO123', value: 0 }];

      fixture.componentRef.setInput('mostSeenCounts', mockData);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const chip = compiled.querySelector('mat-chip');

      expect(chip?.textContent?.trim()).toBe('0 times');
    });

    it('should handle special characters in plate names', () => {
      const mockData = [{ name: 'ABC-123', value: 5 }];

      fixture.componentRef.setInput('mostSeenCounts', mockData);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const listItem = compiled.querySelector('mat-list-item');

      expect(listItem?.textContent).toContain('ABC-123');
    });
  });
});
