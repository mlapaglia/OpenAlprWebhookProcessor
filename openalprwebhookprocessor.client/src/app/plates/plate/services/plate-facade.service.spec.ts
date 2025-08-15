import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { PlateFacadeService } from './plate-facade.service';
import { PlateTabStateService } from './plate-tab-state.service';
import { PlateStatisticsStateService } from './plate-statistics-state.service';
import { PlateNotesStateService } from './plate-notes-state.service';
import { VehicleLogoService } from '../../vehicle-logo.service';
import type { PlateData } from '../../plate-item/plate-item.component';
import { Plate } from '../plate';

describe('PlateFacadeService', () => {
  let service: PlateFacadeService;
  let tabState: jasmine.SpyObj<PlateTabStateService>;
  let statisticsState: jasmine.SpyObj<PlateStatisticsStateService>;
  let notesState: jasmine.SpyObj<PlateNotesStateService>;
  let vehicleLogoService: jasmine.SpyObj<VehicleLogoService>;

  const mockPlateData: PlateData = {
    id: '1',
    plateNumber: 'ABC123',
    processedPlateConfidence: 95.5,
    receivedOn: new Date('2025-01-01T12:00:00Z'),
    openAlprCameraId: 1,
    vehicleDescription: 'sedan',
    direction: 1,
    isAlert: false,
    isIgnore: false,
    isOpen: true,
    imageUrl: 'test.jpg',
    cropImageUrl: 'crop.jpg',
  };

  beforeEach(() => {
    const tabStateSpy = jasmine.createSpyObj('PlateTabStateService', ['setActiveTab', 'reset'], {
      activeTab: jasmine.createSpy().and.returnValue('overview'),
      shouldShowImages: false,
      shouldShowNotes: false,
      shouldShowStatistics: false,
    });

    const statisticsStateSpy = jasmine.createSpyObj('PlateStatisticsStateService', ['loadStatistics', 'reset'], {
      plateStatistics: jasmine.createSpy().and.returnValue([]),
      loadingStatistics: jasmine.createSpy().and.returnValue(false),
      loadingStatisticsFailed: jasmine.createSpy().and.returnValue(false),
      statisticsLoaded: jasmine.createSpy().and.returnValue(false),
    });

    const notesStateSpy = jasmine.createSpyObj('PlateNotesStateService', ['saveNotes', 'clearNotes'], {
      isSavingNotes: jasmine.createSpy().and.returnValue(false),
    });

    const vehicleLogoServiceSpy = jasmine.createSpyObj('VehicleLogoService', ['getVehicleMakeInfo']);

    TestBed.configureTestingModule({
      providers: [
        PlateFacadeService,
        { provide: PlateTabStateService, useValue: tabStateSpy },
        { provide: PlateStatisticsStateService, useValue: statisticsStateSpy },
        { provide: PlateNotesStateService, useValue: notesStateSpy },
        { provide: VehicleLogoService, useValue: vehicleLogoServiceSpy },
      ],
    });

    service = TestBed.inject(PlateFacadeService);
    tabState = TestBed.inject(PlateTabStateService) as jasmine.SpyObj<PlateTabStateService>;
    statisticsState = TestBed.inject(PlateStatisticsStateService) as jasmine.SpyObj<PlateStatisticsStateService>;
    notesState = TestBed.inject(PlateNotesStateService) as jasmine.SpyObj<PlateNotesStateService>;
    vehicleLogoService = TestBed.inject(VehicleLogoService) as jasmine.SpyObj<VehicleLogoService>;
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should initialize with a default plate signal', () => {
    expect(service.plateSignal()).toEqual(jasmine.any(Plate));
  });

  describe('setTab', () => {
    it('should delegate to tab state service', () => {
      service.setTab('images', mockPlateData);

      expect(tabState.setActiveTab).toHaveBeenCalledWith('images');
    });

    it('should delegate to tab state service for stats tab', () => {
      service.setTab('stats', mockPlateData);

      expect(tabState.setActiveTab).toHaveBeenCalledWith('stats');
    });
  });

  describe('setPlate', () => {
    it('should set the plate signal with converted plate data', () => {
      service.setPlate(mockPlateData);

      const plateSignalValue = service.plateSignal();
      expect(plateSignalValue.plateNumber).toBe('ABC123');
      expect(plateSignalValue.id).toBe('1');
    });
  });

  describe('saveNotes', () => {
    it('should delegate to notes state service', () => {
      const mockPlate = new Plate({ plateNumber: 'ABC123' });
      const callback = jasmine.createSpy('callback');
      notesState.saveNotes.and.returnValue(of(mockPlate));

      service.saveNotes(mockPlate, callback);

      expect(notesState.saveNotes).toHaveBeenCalledWith(mockPlate);
    });
  });

  describe('clearNotes', () => {
    it('should delegate to notes state service and call callback', () => {
      const mockPlate = new Plate({ plateNumber: 'ABC123', notes: 'test' });
      const callback = jasmine.createSpy('callback');
      const clearedPlate = new Plate({ plateNumber: 'ABC123', notes: '' });
      notesState.clearNotes.and.returnValue(clearedPlate);

      service.clearNotes(mockPlate, callback);

      expect(notesState.clearNotes).toHaveBeenCalledWith(mockPlate);
      expect(callback).toHaveBeenCalledWith(clearedPlate);
    });
  });

  describe('getVehicleMakeInfo', () => {
    it('should delegate to vehicle logo service', () => {
      const mockInfo = { logo: 'toyota.png', displayName: 'Toyota' };
      vehicleLogoService.getVehicleMakeInfo.and.returnValue(mockInfo);

      const result = service.getVehicleMakeInfo('Toyota Camry');

      expect(vehicleLogoService.getVehicleMakeInfo).toHaveBeenCalledWith('Toyota Camry');
      expect(result).toBe(mockInfo);
    });

    it('should handle null vehicle description', () => {
      const mockInfo = { logo: '', displayName: '' };
      vehicleLogoService.getVehicleMakeInfo.and.returnValue(mockInfo);

      const result = service.getVehicleMakeInfo(null);

      expect(vehicleLogoService.getVehicleMakeInfo).toHaveBeenCalledWith('');
      expect(result).toBe(mockInfo);
    });
  });

  describe('reset', () => {
    it('should reset all child services', () => {
      service.reset();

      expect(tabState.reset).toHaveBeenCalled();
      expect(statisticsState.reset).toHaveBeenCalled();
    });
  });

  describe('computed properties', () => {
    it('should expose readonly signals from child services', () => {
      expect(service.activeTab).toBeDefined();
      expect(service.plateStatistics).toBeDefined();
      expect(service.loadingStatistics).toBeDefined();
      expect(service.loadingStatisticsFailed).toBeDefined();
      expect(service.isSavingNotes).toBeDefined();
      expect(service.shouldShowImages).toBeDefined();
      expect(service.shouldShowNotes).toBeDefined();
      expect(service.shouldShowStatistics).toBeDefined();
    });
  });
});
