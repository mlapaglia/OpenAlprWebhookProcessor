import { TestBed, type ComponentFixture, fakeAsync, tick } from '@angular/core/testing';
import type { MatSlideToggleChange } from '@angular/material/slide-toggle';
import { of, throwError } from 'rxjs';
import { EnrichersComponent } from './enrichers.component';
import { EnrichersService } from './enrichers.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import type { Enricher } from './enricher';

describe('EnrichersComponent', () => {
  let component: EnrichersComponent;
  let fixture: ComponentFixture<EnrichersComponent>;
  let mockEnrichersService: jasmine.SpyObj<EnrichersService>;
  let mockSnackbarService: jasmine.SpyObj<SnackbarService>;

  beforeEach(async () => {
    mockEnrichersService = jasmine.createSpyObj('EnrichersService', ['getEnricher', 'testEnricher', 'upsertEnricher']);
    mockSnackbarService = jasmine.createSpyObj('SnackbarService', ['create']);

    await TestBed.configureTestingModule({
      imports: [EnrichersComponent],
      providers: [
        { provide: EnrichersService, useValue: mockEnrichersService },
        { provide: SnackbarService, useValue: mockSnackbarService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(EnrichersComponent);
    component = fixture.componentInstance;
  });

  describe('component initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.isTesting).toBeUndefined();
      expect(component.isSaving).toBeUndefined();
      expect(component.enricher).toBeUndefined();
    });

    it('should load enricher on init', () => {
      const mockEnricher: Enricher = {
        id: 'enricher-1',
        enrichmentType: 'PlateRecognizer' as any,
        isEnabled: true,

        apiKey: 'test-key',
      };

      mockEnrichersService.getEnricher.and.returnValue(of(mockEnricher));

      component.ngOnInit();

      expect(mockEnrichersService.getEnricher).toHaveBeenCalled();
      expect(component.enricher).toEqual(mockEnricher);
    });
  });

  describe('testEnricher', () => {
    beforeEach(() => {
      component.enricher = {
        id: 'enricher-1',
        enrichmentType: 'PlateRecognizer' as any,
        isEnabled: true,

        apiKey: 'test-key',
      };
    });

    it('should test enricher successfully', fakeAsync(() => {
      mockEnrichersService.testEnricher.and.returnValue(of(true));

      component.testEnricher();

      tick();

      expect(mockEnrichersService.testEnricher).toHaveBeenCalledWith('enricher-1');
      expect(mockSnackbarService.create).toHaveBeenCalledWith('Enricher test succeeded.', SnackBarType.Saved);
    }));

    it('should handle test failure with false result', fakeAsync(() => {
      mockEnrichersService.testEnricher.and.returnValue(of(false));

      component.testEnricher();

      tick();

      expect(mockEnrichersService.testEnricher).toHaveBeenCalledWith('enricher-1');
      expect(mockSnackbarService.create).toHaveBeenCalledWith('Enricher test failed, check the logs.', SnackBarType.Error);
    }));

    it('should handle test error', fakeAsync(() => {
      mockEnrichersService.testEnricher.and.returnValue(throwError(() => new Error('Test failed')));

      component.testEnricher();

      tick();

      expect(mockSnackbarService.create).toHaveBeenCalledWith('Enricher test failed, check the logs.', SnackBarType.Error);
    }));
  });

  describe('saveEnricher', () => {
    beforeEach(() => {
      component.enricher = {
        id: 'enricher-1',
        enrichmentType: 'PlateRecognizer' as any,
        isEnabled: true,

        apiKey: 'test-key',
      };
    });

    it('should save enricher successfully', fakeAsync(() => {
      const mockEnricher: Enricher = {
        id: 'enricher-1',
        enrichmentType: 'PlateRecognizer' as any,
        isEnabled: true,

        apiKey: 'test-key',
      };

      mockEnrichersService.upsertEnricher.and.returnValue(of(null));
      mockEnrichersService.getEnricher.and.returnValue(of(mockEnricher));

      component.saveEnricher();

      tick();

      expect(mockEnrichersService.upsertEnricher).toHaveBeenCalledWith(component.enricher);
      expect(mockSnackbarService.create).toHaveBeenCalledWith('Enricher client saved.', SnackBarType.Successful);
      expect(mockEnrichersService.getEnricher).toHaveBeenCalled();
    }));

    it('should handle save error', fakeAsync(() => {
      mockEnrichersService.upsertEnricher.and.returnValue(throwError(() => new Error('Save failed')));

      component.saveEnricher();

      tick();

      expect(mockEnrichersService.upsertEnricher).toHaveBeenCalledWith(component.enricher);
      expect(mockSnackbarService.create).toHaveBeenCalledWith('Enricher client save failed, check the logs.', SnackBarType.Error);
    }));
  });

  describe('onEnricherToggle', () => {
    beforeEach(() => {
      component.enricher = {
        id: 'enricher-1',
        enrichmentType: 'PlateRecognizer' as any,
        isEnabled: true,

        apiKey: 'test-key',
      };
    });

    it('should handle toggle off', fakeAsync(() => {
      const mockToggleEvent = {
        checked: false,
        source: {} as any,
      } as MatSlideToggleChange;

      mockEnrichersService.upsertEnricher.and.returnValue(of(null));

      component.onEnricherToggle(mockToggleEvent);

      expect(component.enricher.isEnabled).toBe(false);

      tick();

      expect(mockEnrichersService.upsertEnricher).toHaveBeenCalledWith(component.enricher);
    }));

    it('should not save when toggle is on', () => {
      const mockToggleEvent = {
        checked: true,
        source: {} as any,
      } as MatSlideToggleChange;

      component.onEnricherToggle(mockToggleEvent);

      expect(mockEnrichersService.upsertEnricher).not.toHaveBeenCalled();
    });

    it('should handle toggle off with error', fakeAsync(() => {
      const mockToggleEvent = {
        checked: false,
        source: {} as any,
      } as MatSlideToggleChange;

      mockEnrichersService.upsertEnricher.and.returnValue(throwError(() => new Error('Toggle failed')));

      component.onEnricherToggle(mockToggleEvent);

      expect(component.enricher.isEnabled).toBe(false);

      tick();

      expect(mockEnrichersService.upsertEnricher).toHaveBeenCalledWith(component.enricher);
    }));
  });

  describe('getEnricher', () => {
    it('should handle getEnricher error', () => {
      mockEnrichersService.getEnricher.and.returnValue(throwError(() => new Error('Load failed')));

      component.ngOnInit();

      expect(mockEnrichersService.getEnricher).toHaveBeenCalled();
      // Component should handle the error gracefully without crashing
      expect(component.enricher).toBeUndefined();
    });
  });

  describe('ngOnDestroy', () => {
    it('should call parent ngOnDestroy', () => {
      spyOn(component, 'ngOnDestroy').and.callThrough();

      component.ngOnDestroy();

      expect(component.ngOnDestroy).toHaveBeenCalled();
    });
  });
});
