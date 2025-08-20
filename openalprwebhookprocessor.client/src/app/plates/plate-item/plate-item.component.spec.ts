import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { Component, input, NO_ERRORS_SCHEMA } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { DatePipe } from '@angular/common';
import { PlateItemComponent, type PlateData } from './plate-item.component';
import { VehicleLogoService } from '../vehicle-logo.service';

// Mock PlateComponent
@Component({
  selector: 'app-plate',
  template: '<div>Mock Plate Component</div>',
  standalone: true,
})
class MockPlateComponent {
  readonly isVisible = input(false);
  readonly plate = input<PlateData>();
}

describe('PlateItemComponent', () => {
  let component: PlateItemComponent;
  let fixture: ComponentFixture<PlateItemComponent>;
  let mockVehicleLogoService: jasmine.SpyObj<VehicleLogoService>;

  const mockPlateData: PlateData = {
    id: '123',
    plateNumber: 'ABC123',
    openAlprCameraId: 1,
    vehicleDescription: 'Toyota Camry',
    direction: 90,
    receivedOn: new Date('2023-01-01T12:00:00Z'),
    isAlert: false,
    isIgnore: false,
    isOpen: false,
    imageUrl: 'http://example.com/image.jpg',
    cropImageUrl: 'http://example.com/crop.jpg',
    processedPlateConfidence: 95,
    notes: 'Test notes',
    canBeEnriched: true,
  };

  beforeEach(async () => {
    const spy = jasmine.createSpyObj('VehicleLogoService', ['getVehicleMakeInfo', 'formatVehicleDescription']);

    await TestBed.configureTestingModule({
      imports: [
        PlateItemComponent,
        BrowserAnimationsModule,
        MockPlateComponent,
      ],
      providers: [
        DatePipe,
        { provide: VehicleLogoService, useValue: spy },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    mockVehicleLogoService = TestBed.inject(VehicleLogoService) as jasmine.SpyObj<VehicleLogoService>;
    fixture = TestBed.createComponent(PlateItemComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.componentRef.setInput('plate', mockPlateData);
    expect(component).toBeTruthy();
  });

  describe('Input Properties', () => {
    it('should accept plate input', () => {
      fixture.componentRef.setInput('plate', mockPlateData);
      expect(component.plate()).toEqual(mockPlateData);
    });

    it('should require plate input', () => {
      // This test verifies that plate is a required input
      expect(() => component.plate()).toThrow();
    });
  });

  describe('Event Handlers', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('plate', mockPlateData);
    });

    it('should emit plateOpened when onPlateOpened is called', () => {
      spyOn(component.plateOpened, 'emit');

      component.onPlateOpened();

      expect(component.plateOpened.emit).toHaveBeenCalledWith(mockPlateData.id);
    });

    it('should emit plateClosed when onPlateClosed is called', () => {
      spyOn(component.plateClosed, 'emit');

      component.onPlateClosed();

      expect(component.plateClosed.emit).toHaveBeenCalledWith(mockPlateData.id);
    });

    it('should emit enrichPlate when onEnrichPlate is called', () => {
      spyOn(component.enrichPlate, 'emit');

      component.onEnrichPlate();

      expect(component.enrichPlate.emit).toHaveBeenCalledWith(mockPlateData.id);
    });

    it('should emit editPlate when onEditPlate is called', () => {
      spyOn(component.editPlate, 'emit');

      component.onEditPlate();

      expect(component.editPlate.emit).toHaveBeenCalledWith(mockPlateData.id);
    });

    it('should emit alertPlate when onAlertPlate is called', () => {
      spyOn(component.alertPlate, 'emit');

      component.onAlertPlate();

      expect(component.alertPlate.emit).toHaveBeenCalledWith(mockPlateData.id);
    });

    it('should emit ignorePlate when onIgnorePlate is called', () => {
      spyOn(component.ignorePlate, 'emit');

      component.onIgnorePlate();

      expect(component.ignorePlate.emit).toHaveBeenCalledWith(mockPlateData.id);
    });

    it('should emit searchForPlate when onSearchForPlate is called', () => {
      spyOn(component.searchForPlate, 'emit');

      component.onSearchForPlate();

      expect(component.searchForPlate.emit).toHaveBeenCalledWith(mockPlateData.plateNumber);
    });
  });

  describe('Computed Properties', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('plate', mockPlateData);
    });

    describe('vehicleMakeInfo', () => {
      it('should return vehicle make info from service', () => {
        const mockMakeInfo = {
          logo: 'assets/icons/vehicle-logos/toyota.png',
          displayName: 'Toyota',
        };
        mockVehicleLogoService.getVehicleMakeInfo.and.returnValue(mockMakeInfo);

        const result = component.vehicleMakeInfo;

        expect(result).toEqual(mockMakeInfo);
        expect(mockVehicleLogoService.getVehicleMakeInfo).toHaveBeenCalledWith('Toyota Camry');
      });

      it('should handle null plate data', () => {
        fixture.componentRef.setInput('plate', null as any);
        mockVehicleLogoService.getVehicleMakeInfo.and.returnValue(null);

        const result = component.vehicleMakeInfo;

        expect(result).toBeNull();
        expect(mockVehicleLogoService.getVehicleMakeInfo).toHaveBeenCalledWith('');
      });

      it('should handle undefined vehicle description', () => {
        const plateWithoutDescription = { ...mockPlateData };
        delete (plateWithoutDescription as any).vehicleDescription;
        fixture.componentRef.setInput('plate', plateWithoutDescription);
        mockVehicleLogoService.getVehicleMakeInfo.and.returnValue(null);

        const result = component.vehicleMakeInfo;

        expect(result).toBeNull();
        expect(mockVehicleLogoService.getVehicleMakeInfo).toHaveBeenCalledWith('');
      });
    });

    describe('formattedVehicleDescription', () => {
      it('should return formatted vehicle description from service', () => {
        const formattedDescription = '2018 Toyota Camry';
        mockVehicleLogoService.formatVehicleDescription.and.returnValue(formattedDescription);

        const result = component.formattedVehicleDescription;

        expect(result).toBe(formattedDescription);
        expect(mockVehicleLogoService.formatVehicleDescription).toHaveBeenCalledWith('Toyota Camry');
      });

      it('should handle null plate data', () => {
        fixture.componentRef.setInput('plate', null as any);
        mockVehicleLogoService.formatVehicleDescription.and.returnValue('');

        const result = component.formattedVehicleDescription;

        expect(result).toBe('');
        expect(mockVehicleLogoService.formatVehicleDescription).toHaveBeenCalledWith('');
      });

      it('should handle undefined vehicle description', () => {
        const plateWithoutDescription = { ...mockPlateData };
        delete (plateWithoutDescription as any).vehicleDescription;
        fixture.componentRef.setInput('plate', plateWithoutDescription);
        mockVehicleLogoService.formatVehicleDescription.and.returnValue('');

        const result = component.formattedVehicleDescription;

        expect(result).toBe('');
        expect(mockVehicleLogoService.formatVehicleDescription).toHaveBeenCalledWith('');
      });
    });
  });

  describe('Image Error Handling', () => {
    it('should hide image on error', () => {
      const mockImg = document.createElement('img');
      mockImg.style.display = 'block';

      const mockEvent = {
        target: mockImg,
      } as unknown as Event;

      component.onImageError(mockEvent);

      expect(mockImg.style.display).toBe('none');
    });

    it('should handle null target', () => {
      const mockEvent = {
        target: null,
      } as unknown as Event;

      expect(() => component.onImageError(mockEvent)).not.toThrow();
    });

    it('should handle event with non-image target', () => {
      const mockDiv = document.createElement('div');
      const mockEvent = {
        target: mockDiv,
      } as unknown as Event;

      expect(() => component.onImageError(mockEvent)).not.toThrow();
    });
  });

  describe('Template Integration', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('plate', mockPlateData);
      mockVehicleLogoService.getVehicleMakeInfo.and.returnValue({
        logo: 'assets/icons/vehicle-logos/toyota.png',
        displayName: 'Toyota',
      });
      mockVehicleLogoService.formatVehicleDescription.and.returnValue('2018 Toyota Camry');
      fixture.detectChanges();
    });

    it('should display plate information in header', () => {
      const compiled = fixture.nativeElement;

      expect(compiled.textContent).toContain('1');
      expect(compiled.textContent).toContain('ABC123');
      expect(compiled.textContent).toContain('2018 Toyota Camry');
    });

    it('should apply alert styling when plate is alert', () => {
      fixture.componentRef.setInput('plate', { ...mockPlateData, isAlert: true });
      fixture.detectChanges();

      const header = fixture.nativeElement.querySelector('mat-expansion-panel-header');
      expect(header.classList.contains('alertPlate')).toBe(true);
    });

    it('should apply ignore styling when plate is ignored', () => {
      fixture.componentRef.setInput('plate', { ...mockPlateData, isIgnore: true });
      fixture.detectChanges();

      const header = fixture.nativeElement.querySelector('mat-expansion-panel-header');
      const title = fixture.nativeElement.querySelector('mat-panel-title');

      expect(header.classList.contains('ignorePlate')).toBe(true);
      expect(title.classList.contains('ignorePlateText')).toBe(true);
    });

    it('should disable ignore button when plate is already ignored', () => {
      fixture.componentRef.setInput('plate', { ...mockPlateData, isIgnore: true });
      fixture.detectChanges();

      // Find the ignore button within the refresh button component
      const refreshButtons = fixture.nativeElement.querySelectorAll('app-refresh-button');
      const ignoreRefreshButton = Array.from(refreshButtons).find((btn: any) =>
        btn.textContent.includes('Add to ignore list'),
      ) as HTMLElement;
      const ignoreButton = ignoreRefreshButton?.querySelector('button') as HTMLButtonElement;
      expect(ignoreButton.disabled).toBe(true);
    });

    it('should disable alert button when plate is already alerted', () => {
      fixture.componentRef.setInput('plate', { ...mockPlateData, isAlert: true });
      fixture.detectChanges();

      // Find the alert button within the refresh button component
      const refreshButtons = fixture.nativeElement.querySelectorAll('app-refresh-button');
      const alertRefreshButton = Array.from(refreshButtons).find((btn: any) =>
        btn.textContent.includes('Add to alert list'),
      ) as HTMLElement;
      const alertButton = alertRefreshButton?.querySelector('button') as HTMLButtonElement;
      expect(alertButton.disabled).toBe(true);
    });

    it('should trigger enrich event when enrich button is clicked', () => {
      // Set canBeEnriched to true so the button is visible but disabled
      fixture.componentRef.setInput('plate', { ...mockPlateData, canBeEnriched: true });
      spyOn(component, 'onEnrichPlate');
      fixture.detectChanges();

      // Find the enrich button by looking for the button with "Enrich plate" text
      const buttons = fixture.nativeElement.querySelectorAll('button[mat-raised-button]');
      const enrichButton = Array.from(buttons).find((btn: any) =>
        btn.textContent.includes('Enrich plate'),
      ) as HTMLButtonElement;

      // Simulate click even though button is disabled (for testing purposes)
      enrichButton?.click();

      expect(component.onEnrichPlate).toHaveBeenCalled();
    });

    it('should trigger edit event when edit button is clicked', () => {
      spyOn(component, 'onEditPlate');
      fixture.detectChanges();

      // Find the edit button by looking for the button with "Edit plate" text
      const buttons = fixture.nativeElement.querySelectorAll('button[mat-raised-button]');
      const editButton = Array.from(buttons).find((btn: any) =>
        btn.textContent.includes('Edit plate'),
      ) as HTMLButtonElement;
      editButton.click();

      expect(component.onEditPlate).toHaveBeenCalled();
    });

    // Note: View functionality is handled at a higher level, not through a button in this component



    it('should display formatted date', () => {
      const compiled = fixture.nativeElement;
      // The date should be formatted using Angular's DatePipe with 'medium' format
      expect(compiled.textContent).toContain('Jan 1, 2023');
    });

    it('should rotate direction icon based on plate direction', () => {
      fixture.componentRef.setInput('plate', { ...mockPlateData, direction: 180 });
      fixture.detectChanges();

      const directionIcon = fixture.nativeElement.querySelector('mat-icon');
      const { transform } = directionIcon.style;
      expect(transform).toContain('rotate(90deg)'); // 180 - 90 = 90
    });
  });

  describe('Vehicle Logo Integration', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('plate', mockPlateData);
    });



    it('should not display vehicle logo when make info is not available', () => {
      mockVehicleLogoService.getVehicleMakeInfo.and.returnValue(null);
      fixture.detectChanges();

      const logoImg = fixture.nativeElement.querySelector('.vehicle-logo');
      expect(logoImg).toBeFalsy();
    });

    it('should handle image error event', () => {
      const mockMakeInfo = {
        logo: 'assets/icons/vehicle-logos/toyota.png',
        displayName: 'Toyota',
      };
      mockVehicleLogoService.getVehicleMakeInfo.and.returnValue(mockMakeInfo);
      spyOn(component, 'onImageError');
      fixture.detectChanges();

      const logoImg = fixture.nativeElement.querySelector('.vehicle-logo');
      logoImg.dispatchEvent(new Event('error'));

      expect(component.onImageError).toHaveBeenCalled();
    });
  });
});
