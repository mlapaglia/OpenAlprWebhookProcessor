import type { ComponentFixture } from '@angular/core/testing';
import { TestBed } from '@angular/core/testing';
import { Component, NO_ERRORS_SCHEMA, input, output } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import type { PageEvent } from '@angular/material/paginator';
import { PlateListComponent } from './plate-list.component';
import type { PlateData } from '../plate-item/plate-item.component';

// Mock PlateItemComponent (standalone for testing)
@Component({
  selector: 'app-plate-item',
  template: '<div data-testid="plate-item">{{ plate.plateNumber }}</div>',
  standalone: true,
})
class MockPlateItemComponent {
  readonly plate = input<PlateData>();
  readonly plateOpened = output<number>();
  readonly plateClosed = output<number>();
  readonly enrichPlate = output<number>();
  readonly editPlate = output<number>();
  readonly alertPlate = output<number>();
  readonly ignorePlate = output<number>();
  readonly searchForPlate = output<number>();
}

describe('PlateListComponent', () => {
  let component: PlateListComponent;
  let fixture: ComponentFixture<PlateListComponent>;

  const mockPlates: PlateData[] = [
    {
      id: '1',
      plateNumber: 'ABC123',
      openAlprCameraId: 1,
      vehicleDescription: 'Toyota Camry',
      direction: 90,
      receivedOn: new Date('2023-01-01'),
      isAlert: false,
      isIgnore: false,
      isOpen: false,
      imageUrl: 'http://example.com/image1.jpg',
      cropImageUrl: 'http://example.com/crop1.jpg',
      processedPlateConfidence: 95,
      notes: 'Test note 1',
      canBeEnriched: true,
    },
    {
      id: '2',
      plateNumber: 'XYZ789',
      openAlprCameraId: 2,
      vehicleDescription: 'Honda Civic',
      direction: 180,
      receivedOn: new Date('2023-01-02'),
      isAlert: true,
      isIgnore: false,
      isOpen: true,
      imageUrl: 'http://example.com/image2.jpg',
      cropImageUrl: 'http://example.com/crop2.jpg',
      processedPlateConfidence: 87,
      canBeEnriched: false,
    },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        PlateListComponent,
        BrowserAnimationsModule,
        MockPlateItemComponent,
      ],
      schemas: [NO_ERRORS_SCHEMA],
    })
      .compileComponents();

    fixture = TestBed.createComponent(PlateListComponent);
    component = fixture.componentInstance;

    // Set required inputs with default values
    fixture.componentRef.setInput('plates', []);
    fixture.componentRef.setInput('isLoading', false);
    fixture.componentRef.setInput('pageSize', 25);
    fixture.componentRef.setInput('totalNumberOfPlates', 0);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Input Properties', () => {
    it('should have default input values', () => {
      expect(component.plates()).toEqual([]);
      expect(component.isLoading()).toBe(false);
      expect(component.pageSize()).toBe(25);
      expect(component.totalNumberOfPlates()).toBe(0);
    });

    it('should accept plates input', () => {
      fixture.componentRef.setInput('plates', mockPlates);
      expect(component.plates()).toEqual(mockPlates);
      expect(component.plates().length).toBe(2);
    });

    it('should accept isLoading input', () => {
      fixture.componentRef.setInput('isLoading', true);
      expect(component.isLoading()).toBe(true);
    });

    it('should accept pageSize input', () => {
      fixture.componentRef.setInput('pageSize', 50);
      expect(component.pageSize()).toBe(50);
    });

    it('should accept totalNumberOfPlates input', () => {
      fixture.componentRef.setInput('totalNumberOfPlates', 100);
      expect(component.totalNumberOfPlates()).toBe(100);
    });
  });

  describe('Event Handlers', () => {
    it('should emit plateOpened when onPlateOpened is called', () => {
      spyOn(component.plateOpened, 'emit');

      component.onPlateOpened('123');

      expect(component.plateOpened.emit).toHaveBeenCalledWith('123');
    });

    it('should emit plateClosed when onPlateClosed is called', () => {
      spyOn(component.plateClosed, 'emit');

      component.onPlateClosed('456');

      expect(component.plateClosed.emit).toHaveBeenCalledWith('456');
    });

    it('should emit enrichPlate when onEnrichPlate is called', () => {
      spyOn(component.enrichPlate, 'emit');

      component.onEnrichPlate('789');

      expect(component.enrichPlate.emit).toHaveBeenCalledWith('789');
    });

    it('should emit editPlate when onEditPlate is called', () => {
      spyOn(component.editPlate, 'emit');

      component.onEditPlate('101');

      expect(component.editPlate.emit).toHaveBeenCalledWith('101');
    });

    it('should emit alertPlate when onAlertPlate is called', () => {
      spyOn(component.alertPlate, 'emit');

      component.onAlertPlate('202');

      expect(component.alertPlate.emit).toHaveBeenCalledWith('202');
    });

    it('should emit ignorePlate when onIgnorePlate is called', () => {
      spyOn(component.ignorePlate, 'emit');

      component.onIgnorePlate('303');

      expect(component.ignorePlate.emit).toHaveBeenCalledWith('303');
    });

    it('should emit searchForPlate when onSearchForPlate is called', () => {
      spyOn(component.searchForPlate, 'emit');

      component.onSearchForPlate('404');

      expect(component.searchForPlate.emit).toHaveBeenCalledWith('404');
    });

    it('should emit paginatorChange when onPaginatorPage is called', () => {
      spyOn(component.paginatorChange, 'emit');

      const pageEvent: PageEvent = {
        pageIndex: 1,
        pageSize: 25,
        length: 100,
      };

      component.onPaginatorPage(pageEvent);

      expect(component.paginatorChange.emit).toHaveBeenCalledWith(pageEvent);
    });
  });

  describe('Template Rendering', () => {
    it('should show loading overlay when isLoading is true', () => {
      fixture.componentRef.setInput('isLoading', true);
      fixture.detectChanges();

      const loadingOverlay = fixture.nativeElement.querySelector('.loading-overlay');

      expect(loadingOverlay).toBeTruthy();
    });

    it('should hide loading spinner when isLoading is false', () => {
      fixture.componentRef.setInput('isLoading', false);
      fixture.detectChanges();

      const loadingOverlay = fixture.nativeElement.querySelector('.loading-overlay');

      expect(loadingOverlay).toBeFalsy();
    });

    it('should render header columns', () => {
      fixture.detectChanges();

      const headers = fixture.nativeElement.querySelectorAll('mat-panel-title > div');

      expect(headers.length).toBe(5);
      expect(headers[0].textContent.trim()).toBe('Camera Id');
      expect(headers[1].textContent.trim()).toBe('Plate Number');
      expect(headers[2].textContent.trim()).toBe('Vehicle Description');
      expect(headers[3].textContent.trim()).toBe('Vehicle Direction');
      expect(headers[4].textContent.trim()).toBe('Date');
    });



    it('should render no plate items when plates array is empty', () => {
      fixture.componentRef.setInput('plates', []);
      fixture.detectChanges();

      const plateItems = fixture.nativeElement.querySelectorAll('app-plate-item');

      expect(plateItems.length).toBe(0);
    });

    it('should render paginator with correct properties', () => {
      fixture.componentRef.setInput('pageSize', 50);
      fixture.componentRef.setInput('totalNumberOfPlates', 200);
      fixture.detectChanges();

      const paginator = fixture.nativeElement.querySelector('mat-paginator');

      expect(paginator).toBeTruthy();
      // Verify component properties are bound correctly
      expect(component.pageSize()).toBe(50);
      expect(component.totalNumberOfPlates()).toBe(200);
    });
  });



  describe('Paginator Integration', () => {
    it('should handle page event from paginator', () => {
      spyOn(component, 'onPaginatorPage');

      const paginator = fixture.debugElement.query(selector => selector.name === 'mat-paginator');
      const pageEvent: PageEvent = {
        pageIndex: 2,
        pageSize: 50,
        length: 150,
      };

      paginator.triggerEventHandler('page', pageEvent);

      expect(component.onPaginatorPage).toHaveBeenCalledWith(pageEvent);
    });
  });
});
