import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { EditCameraComponent } from './edit-camera.component';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { EditCameraService } from './edit-camera.service';
import { SettingsService } from '../../settings.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { Camera } from '../camera';
import { ZoomFocus } from './zoomfocus';
import { of } from 'rxjs';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

describe(EditCameraComponent.name, () => {
  let component: EditCameraComponent;
  let fixture: ComponentFixture<EditCameraComponent>;
  const editCameraServiceSpy = jasmine.createSpyObj(EditCameraService.name, ['getZoomAndFocus']);
  const settingsServiceSpy = jasmine.createSpyObj(SettingsService.name, ['updateCamera']);
  const snackbarServiceSpy = jasmine.createSpyObj(SnackbarService.name, ['create']);

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditCameraComponent, BrowserAnimationsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: MatDialogRef, useValue: {} },
        { provide: MAT_DIALOG_DATA, useValue: new Camera() },
        { provide: EditCameraService, useValue: editCameraServiceSpy },
        { provide: SettingsService, useValue: settingsServiceSpy },
        { provide: SnackbarService, useValue: snackbarServiceSpy },
      ],
    }).compileComponents();
  });

  beforeEach(() => {
    editCameraServiceSpy.getZoomAndFocus.and.returnValue(of(new ZoomFocus()));
    fixture = TestBed.createComponent(EditCameraComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
