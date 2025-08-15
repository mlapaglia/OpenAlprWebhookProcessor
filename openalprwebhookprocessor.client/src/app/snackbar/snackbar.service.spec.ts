import { TestBed } from '@angular/core/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SnackbarService } from './snackbar.service';
import { SnackbarComponent } from './snackbar.component';
import { SnackBarType } from './snackbartype';
import { SnackBar } from './snackbar';

describe('SnackbarService', () => {
  let service: SnackbarService;
  let matSnackBarSpy: jasmine.SpyObj<MatSnackBar>;

  beforeEach(() => {
    const spy = jasmine.createSpyObj('MatSnackBar', ['openFromComponent']);

    TestBed.configureTestingModule({
      providers: [
        SnackbarService,
        { provide: MatSnackBar, useValue: spy },
      ],
    });

    service = TestBed.inject(SnackbarService);
    matSnackBarSpy = TestBed.inject(MatSnackBar) as jasmine.SpyObj<MatSnackBar>;
  });

  describe('Service Creation', () => {
    it('should be created', () => {
      expect(service).toBeTruthy();
    });
  });

  describe('create method', () => {
    it('should call MatSnackBar.openFromComponent with correct parameters for basic message', () => {
      const message = 'Test message';
      const snackBarType = SnackBarType.Info;

      service.create(message, snackBarType);

      expect(matSnackBarSpy.openFromComponent).toHaveBeenCalledWith(
        SnackbarComponent,
        {
          horizontalPosition: 'right',
          verticalPosition: 'bottom',
          duration: 3000,
          data: new SnackBar({
            message,
            message2: undefined,
            snackType: snackBarType,
          }),
        },
      );
    });

    it('should call MatSnackBar.openFromComponent with message2 when provided', () => {
      const message = 'Primary message';
      const message2 = 'Secondary message';
      const snackBarType = SnackBarType.Alert;

      service.create(message, snackBarType, message2);

      expect(matSnackBarSpy.openFromComponent).toHaveBeenCalledWith(
        SnackbarComponent,
        {
          horizontalPosition: 'right',
          verticalPosition: 'bottom',
          duration: 4000,
          data: new SnackBar({
            message,
            message2,
            snackType: snackBarType,
          }),
        },
      );
    });

    it('should create snackbar with Alert type', () => {
      service.create('Alert message', SnackBarType.Alert);

      expect(matSnackBarSpy.openFromComponent).toHaveBeenCalledWith(
        SnackbarComponent,
        jasmine.objectContaining({
          data: jasmine.objectContaining({
            snackType: SnackBarType.Alert,
          }),
        }),
      );
    });

    it('should create snackbar with Info type', () => {
      service.create('Info message', SnackBarType.Info);

      expect(matSnackBarSpy.openFromComponent).toHaveBeenCalledWith(
        SnackbarComponent,
        jasmine.objectContaining({
          data: jasmine.objectContaining({
            snackType: SnackBarType.Info,
          }),
        }),
      );
    });

    it('should create snackbar with Connected type', () => {
      service.create('Connected message', SnackBarType.Connected);

      expect(matSnackBarSpy.openFromComponent).toHaveBeenCalledWith(
        SnackbarComponent,
        jasmine.objectContaining({
          data: jasmine.objectContaining({
            snackType: SnackBarType.Connected,
          }),
        }),
      );
    });

    it('should create snackbar with Disconnected type', () => {
      service.create('Disconnected message', SnackBarType.Disconnected);

      expect(matSnackBarSpy.openFromComponent).toHaveBeenCalledWith(
        SnackbarComponent,
        jasmine.objectContaining({
          data: jasmine.objectContaining({
            snackType: SnackBarType.Disconnected,
          }),
        }),
      );
    });

    it('should create snackbar with Saved type', () => {
      service.create('Saved message', SnackBarType.Saved);

      expect(matSnackBarSpy.openFromComponent).toHaveBeenCalledWith(
        SnackbarComponent,
        jasmine.objectContaining({
          data: jasmine.objectContaining({
            snackType: SnackBarType.Saved,
          }),
        }),
      );
    });

    it('should create snackbar with Deleted type', () => {
      service.create('Deleted message', SnackBarType.Deleted);

      expect(matSnackBarSpy.openFromComponent).toHaveBeenCalledWith(
        SnackbarComponent,
        jasmine.objectContaining({
          data: jasmine.objectContaining({
            snackType: SnackBarType.Deleted,
          }),
        }),
      );
    });

    it('should create snackbar with Successful type', () => {
      service.create('Successful message', SnackBarType.Successful);

      expect(matSnackBarSpy.openFromComponent).toHaveBeenCalledWith(
        SnackbarComponent,
        jasmine.objectContaining({
          data: jasmine.objectContaining({
            snackType: SnackBarType.Successful,
          }),
        }),
      );
    });

    it('should create snackbar with Error type', () => {
      service.create('Error message', SnackBarType.Error);

      expect(matSnackBarSpy.openFromComponent).toHaveBeenCalledWith(
        SnackbarComponent,
        jasmine.objectContaining({
          data: jasmine.objectContaining({
            snackType: SnackBarType.Error,
          }),
        }),
      );
    });
  });

  describe('Snackbar Configuration', () => {
    beforeEach(() => {
      service.create('Test message', SnackBarType.Info);
    });

    it('should set correct horizontal position', () => {
      expect(matSnackBarSpy.openFromComponent).toHaveBeenCalledWith(
        SnackbarComponent,
        jasmine.objectContaining({
          horizontalPosition: 'right',
        }),
      );
    });

    it('should set correct vertical position', () => {
      expect(matSnackBarSpy.openFromComponent).toHaveBeenCalledWith(
        SnackbarComponent,
        jasmine.objectContaining({
          verticalPosition: 'bottom',
        }),
      );
    });

    it('should set correct duration', () => {
      expect(matSnackBarSpy.openFromComponent).toHaveBeenCalledWith(
        SnackbarComponent,
        jasmine.objectContaining({
          duration: 3000,
        }),
      );
    });

    it('should use SnackbarComponent', () => {
      expect(matSnackBarSpy.openFromComponent).toHaveBeenCalledWith(
        SnackbarComponent,
        jasmine.any(Object),
      );
    });
  });

  describe('Data Object Creation', () => {
    it('should create SnackBar data object with correct structure', () => {
      const message = 'Test message';
      const message2 = 'Test secondary message';
      const snackType = SnackBarType.Info;

      service.create(message, snackType, message2);

      const expectedData = new SnackBar({
        message,
        message2,
        snackType,
      });

      expect(matSnackBarSpy.openFromComponent).toHaveBeenCalledWith(
        SnackbarComponent,
        jasmine.objectContaining({
          data: expectedData,
        }),
      );
    });

    it('should handle undefined message2 correctly', () => {
      const message = 'Test message';
      const snackType = SnackBarType.Info;

      service.create(message, snackType);

      expect(matSnackBarSpy.openFromComponent).toHaveBeenCalledWith(
        SnackbarComponent,
        jasmine.objectContaining({
          data: jasmine.objectContaining({
            message,
            message2: undefined,
            snackType,
          }),
        }),
      );
    });
  });

  describe('Method Call Count', () => {
    it('should call openFromComponent exactly once per create call', () => {
      service.create('Message 1', SnackBarType.Info);
      service.create('Message 2', SnackBarType.Alert);
      service.create('Message 3', SnackBarType.Error);

      expect(matSnackBarSpy.openFromComponent).toHaveBeenCalledTimes(3);
    });
  });
});
