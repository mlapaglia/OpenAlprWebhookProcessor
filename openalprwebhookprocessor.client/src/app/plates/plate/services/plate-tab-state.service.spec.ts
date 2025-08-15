import { TestBed } from '@angular/core/testing';
import { PlateTabStateService } from './plate-tab-state.service';

describe('PlateTabStateService', () => {
  let service: PlateTabStateService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [PlateTabStateService],
    });
    service = TestBed.inject(PlateTabStateService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should initialize with overview tab and no loaded flags', () => {
    expect(service.activeTab()).toBe('overview');
    expect(service.imagesLoaded()).toBe(false);
    expect(service.statisticsLoaded()).toBe(false);
    expect(service.notesLoaded()).toBe(false);
  });

  describe('setActiveTab', () => {
    it('should set active tab and mark images as loaded when images tab is selected', () => {
      service.setActiveTab('images');

      expect(service.activeTab()).toBe('images');
      expect(service.imagesLoaded()).toBe(true);
      expect(service.shouldShowImages).toBe(true);
    });

    it('should set active tab and mark statistics as loaded when stats tab is selected', () => {
      service.setActiveTab('stats');

      expect(service.activeTab()).toBe('stats');
      expect(service.statisticsLoaded()).toBe(true);
      expect(service.shouldShowStatistics).toBe(true);
    });

    it('should set active tab and mark notes as loaded when notes tab is selected', () => {
      service.setActiveTab('notes');

      expect(service.activeTab()).toBe('notes');
      expect(service.notesLoaded()).toBe(true);
      expect(service.shouldShowNotes).toBe(true);
    });

    it('should not set loaded flags for overview tab', () => {
      service.setActiveTab('overview');

      expect(service.activeTab()).toBe('overview');
      expect(service.imagesLoaded()).toBe(false);
      expect(service.statisticsLoaded()).toBe(false);
      expect(service.notesLoaded()).toBe(false);
    });

    it('should not reset loaded flags when switching between tabs', () => {
      service.setActiveTab('images');
      service.setActiveTab('stats');
      service.setActiveTab('notes');

      expect(service.imagesLoaded()).toBe(true);
      expect(service.statisticsLoaded()).toBe(true);
      expect(service.notesLoaded()).toBe(true);
    });
  });

  describe('reset', () => {
    it('should reset all state to initial values', () => {
      service.setActiveTab('images');
      service.setActiveTab('stats');
      service.setActiveTab('notes');

      service.reset();

      expect(service.activeTab()).toBe('overview');
      expect(service.imagesLoaded()).toBe(false);
      expect(service.statisticsLoaded()).toBe(false);
      expect(service.notesLoaded()).toBe(false);
      expect(service.shouldShowImages).toBe(false);
      expect(service.shouldShowStatistics).toBe(false);
      expect(service.shouldShowNotes).toBe(false);
    });
  });
});
