import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { PlateNotesComponent } from './plate-notes.component';
import { Plate } from '../plate';

describe('PlateNotesComponent', () => {
  let component: PlateNotesComponent;
  let fixture: ComponentFixture<PlateNotesComponent>;

  const mockPlate = new Plate({
    id: '1',
    plateNumber: 'ABC123',
    notes: 'Test notes',
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        FormsModule,
        MatButtonModule,
        MatCardModule,
        MatFormFieldModule,
        MatInputModule,
        MatIconModule,
        PlateNotesComponent,
      ],
    })
      .compileComponents();

    fixture = TestBed.createComponent(PlateNotesComponent);
    component = fixture.componentInstance;

    fixture.componentRef.setInput('plate', mockPlate);
    fixture.componentRef.setInput('isSavingNotes', false);
    fixture.componentRef.setInput('saveNotes', jasmine.createSpy('saveNotes'));
    fixture.componentRef.setInput('clearNotes', jasmine.createSpy('clearNotes'));

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with input signals', () => {
    expect(component.plate()).toEqual(mockPlate);
    expect(component.isSavingNotes()).toBe(false);
    expect(component.saveNotes()).toBeDefined();
    expect(component.clearNotes()).toBeDefined();
  });

  it('should call saveNotes callback when onSaveNotes is called', () => {
    const saveNotesSpy = jasmine.createSpy('saveNotes');
    fixture.componentRef.setInput('saveNotes', saveNotesSpy);
    fixture.detectChanges();

    component.onSaveNotes();

    expect(saveNotesSpy).toHaveBeenCalledWith(mockPlate);
  });

  it('should call clearNotes callback when onClearNotes is called', () => {
    const clearNotesSpy = jasmine.createSpy('clearNotes');
    fixture.componentRef.setInput('clearNotes', clearNotesSpy);
    fixture.detectChanges();

    component.onClearNotes();

    expect(clearNotesSpy).toHaveBeenCalledWith(mockPlate);
  });
});
