import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EditCameraComponent } from './edit-camera.component';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { EditCameraService } from './edit-camera.service';
import { Camera } from '../camera';
import { of } from 'rxjs';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

describe(EditCameraComponent.name, () => {
    let component: EditCameraComponent;
    let fixture: ComponentFixture<EditCameraComponent>;
    const editCameraServiceSpy = jasmine.createSpyObj(EditCameraService.name, ['getZoomAndFocus']);

    beforeEach(async() => {
        await TestBed.configureTestingModule({
            imports: [EditCameraComponent, BrowserAnimationsModule],
            providers: [
                { provide: MatDialogRef, useValue: {} },
                { provide: MAT_DIALOG_DATA, useValue: {} },
                { provide: EditCameraService, useValue: editCameraServiceSpy }
            ]
        }).compileComponents();
    });

    beforeEach(() => {
        editCameraServiceSpy.getZoomAndFocus.and.returnValue(of(new Camera()));
        fixture = TestBed.createComponent(EditCameraComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
