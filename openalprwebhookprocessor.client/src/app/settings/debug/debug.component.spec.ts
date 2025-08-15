import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { MatDialog } from '@angular/material/dialog';
import { DebugComponent } from './debug.component';
import { SettingsService } from '../settings.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { of } from 'rxjs';

describe('DebugComponent', () => {
  let component: DebugComponent;
  let fixture: ComponentFixture<DebugComponent>;
  let mockSettingsService: jasmine.SpyObj<SettingsService>;
  let mockDialog: jasmine.SpyObj<MatDialog>;

  beforeEach(async () => {
    const settingsServiceSpy = jasmine.createSpyObj('SettingsService', ['cleanupDatabase', 'getVersion']);
    const snackbarServiceSpy = jasmine.createSpyObj('SnackbarService', ['create']);
    const dialogSpy = jasmine.createSpyObj('MatDialog', ['open']);

    await TestBed.configureTestingModule({
      imports: [DebugComponent],
      providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        { provide: SettingsService, useValue: settingsServiceSpy },
        { provide: SnackbarService, useValue: snackbarServiceSpy },
        { provide: MatDialog, useValue: dialogSpy },
      ],
    })
      .compileComponents();

    fixture = TestBed.createComponent(DebugComponent);
    component = fixture.componentInstance;
    mockSettingsService = TestBed.inject(SettingsService) as jasmine.SpyObj<SettingsService>;
    mockDialog = TestBed.inject(MatDialog) as jasmine.SpyObj<MatDialog>;

    // Setup default return values
    mockSettingsService.getVersion.and.returnValue(of({
      version: '1.0.0',
      assemblyVersion: '1.0.0.0',
      fileVersion: '1.0.0.0',
      informationalVersion: '1.0.0+abc123',
    }));
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with isCleaningDatabase false', () => {
    expect(component.isCleaningDatabase).toBeFalse();
  });

  it('should call cleanupDatabase service when user confirms dialog', () => {
    const mockDialogRef = jasmine.createSpyObj('MatDialogRef', ['afterClosed']);
    mockDialogRef.afterClosed.and.returnValue(of(true));
    mockDialog.open.and.returnValue(mockDialogRef);
    mockSettingsService.cleanupDatabase.and.returnValue(of(null));

    component.cleanupDatabase();

    expect(mockDialog.open).toHaveBeenCalled();
    expect(mockSettingsService.cleanupDatabase).toHaveBeenCalled();
  });

  it('should not call service if user cancels confirmation dialog', () => {
    const mockDialogRef = jasmine.createSpyObj('MatDialogRef', ['afterClosed']);
    mockDialogRef.afterClosed.and.returnValue(of(false));
    mockDialog.open.and.returnValue(mockDialogRef);

    component.cleanupDatabase();

    expect(mockDialog.open).toHaveBeenCalled();
    expect(mockSettingsService.cleanupDatabase).not.toHaveBeenCalled();
  });
});
