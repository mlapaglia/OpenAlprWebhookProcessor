import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { EditPlateComponent } from './edit-plate.component';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { PlateService } from '../plate.service';

describe('EditPlateComponent', () => {
  let component: EditPlateComponent;
  let fixture: ComponentFixture<EditPlateComponent>;
  let mockDialogRef: jasmine.SpyObj<MatDialogRef<EditPlateComponent>>;
  let mockPlateService: jasmine.SpyObj<PlateService>;

  beforeEach(async () => {
    mockDialogRef = jasmine.createSpyObj('MatDialogRef', ['close']);
    mockPlateService = jasmine.createSpyObj('PlateService', ['upsertPlate']);

    await TestBed.configureTestingModule({
      imports: [
        EditPlateComponent,
        BrowserAnimationsModule,
      ],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: { plateId: '1', currentPlateNumber: 'ABC123' } },
        { provide: MatDialogRef, useValue: mockDialogRef },
        { provide: PlateService, useValue: mockPlateService },
      ],
    })
      .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(EditPlateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
