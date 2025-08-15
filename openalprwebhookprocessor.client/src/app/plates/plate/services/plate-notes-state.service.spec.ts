import { TestBed } from '@angular/core/testing';
import { Subject } from 'rxjs';
import { PlateNotesStateService } from './plate-notes-state.service';
import { PlateService } from '../../plate.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { Plate } from '../plate';

describe('PlateNotesStateService', () => {
  let service: PlateNotesStateService;
  let plateService: jasmine.SpyObj<PlateService>;
  let snackbarService: jasmine.SpyObj<SnackbarService>;

  beforeEach(() => {
    const plateServiceSpy = jasmine.createSpyObj('PlateService', ['upsertPlate']);
    const snackbarServiceSpy = jasmine.createSpyObj('SnackbarService', ['create']);

    TestBed.configureTestingModule({
      providers: [
        PlateNotesStateService,
        { provide: PlateService, useValue: plateServiceSpy },
        { provide: SnackbarService, useValue: snackbarServiceSpy },
      ],
    });

    service = TestBed.inject(PlateNotesStateService);
    plateService = TestBed.inject(PlateService) as jasmine.SpyObj<PlateService>;
    snackbarService = TestBed.inject(SnackbarService) as jasmine.SpyObj<SnackbarService>;
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should initialize with isSavingNotes as false', () => {
    expect(service.isSavingNotes()).toBe(false);
  });

  describe('saveNotes', () => {
    const mockPlate = new Plate({
      id: '1',
      plateNumber: 'ABC123',
      notes: 'Test notes',
    });

    it('should save notes successfully', (done) => {
      const subject = new Subject<null>();
      plateService.upsertPlate.and.returnValue(subject.asObservable());

      expect(service.isSavingNotes()).toBe(false);

      service.saveNotes(mockPlate).subscribe({
        next: (result) => {
          expect(result).toBe(mockPlate);
          expect(service.isSavingNotes()).toBe(false);
          expect(snackbarService.create).toHaveBeenCalledWith(
            'Notes saved for: ABC123',
            SnackBarType.Saved,
          );
          expect(plateService.upsertPlate).toHaveBeenCalledWith(mockPlate);
          done();
        },
      });

      expect(service.isSavingNotes()).toBe(true);

      subject.next(null);
      subject.complete();
    });

    it('should handle save errors', (done) => {
      const subject = new Subject<null>();
      plateService.upsertPlate.and.returnValue(subject.asObservable());

      expect(service.isSavingNotes()).toBe(false);

      service.saveNotes(mockPlate).subscribe({
        error: () => {
          expect(service.isSavingNotes()).toBe(false);
          expect(snackbarService.create).toHaveBeenCalledWith(
            'Failed to save notes for: ABC123',
            SnackBarType.Error,
          );
          done();
        },
      });

      expect(service.isSavingNotes()).toBe(true);

      subject.error(new Error('Save failed'));
    });
  });

  describe('clearNotes', () => {
    it('should clear notes from plate', () => {
      const mockPlate = new Plate({
        id: '1',
        plateNumber: 'ABC123',
        notes: 'Some notes to clear',
      });

      const result = service.clearNotes(mockPlate);

      expect(result.notes).toBe('');
      expect(result.plateNumber).toBe('ABC123');
      expect(result.id).toBe('1');
    });

    it('should not modify original plate object', () => {
      const mockPlate = new Plate({
        id: '1',
        plateNumber: 'ABC123',
        notes: 'Original notes',
      });

      const result = service.clearNotes(mockPlate);

      expect(mockPlate.notes).toBe('Original notes');
      expect(result.notes).toBe('');
      expect(result).not.toBe(mockPlate);
    });
  });
});
