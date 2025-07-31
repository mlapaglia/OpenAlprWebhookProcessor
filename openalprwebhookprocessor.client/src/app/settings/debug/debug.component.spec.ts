import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { DebugComponent } from './debug.component';
import { SettingsService } from '../settings.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { of } from 'rxjs';

describe('DebugComponent', () => {
  let component: DebugComponent;
  let fixture: ComponentFixture<DebugComponent>;
  let mockSettingsService: jasmine.SpyObj<SettingsService>;

  beforeEach(async () => {
    const settingsServiceSpy = jasmine.createSpyObj('SettingsService', ['cleanupDatabase']);
    const snackbarServiceSpy = jasmine.createSpyObj('SnackbarService', ['create']);

    await TestBed.configureTestingModule({
      imports: [DebugComponent],
      providers: [
        { provide: SettingsService, useValue: settingsServiceSpy },
        { provide: SnackbarService, useValue: snackbarServiceSpy },
      ],
    })
      .compileComponents();

    fixture = TestBed.createComponent(DebugComponent);
    component = fixture.componentInstance;
    mockSettingsService = TestBed.inject(SettingsService) as jasmine.SpyObj<SettingsService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with isCleaningDatabase false', () => {
    expect(component.isCleaningDatabase).toBeFalse();
  });

  it('should call cleanupDatabase service when cleanupDatabase is called', async () => {
    mockSettingsService.cleanupDatabase.and.returnValue(of(null));
    spyOn(window, 'confirm').and.returnValue(true);

    await component.cleanupDatabase();

    expect(mockSettingsService.cleanupDatabase).toHaveBeenCalled();
  });

  it('should not call service if user cancels confirmation', async () => {
    spyOn(window, 'confirm').and.returnValue(false);

    await component.cleanupDatabase();

    expect(mockSettingsService.cleanupDatabase).not.toHaveBeenCalled();
  });
});
