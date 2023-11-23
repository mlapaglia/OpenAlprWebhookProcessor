import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EditPlateComponent } from './edit-plate.component';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

describe('EditPlateComponent', () => {
    let component: EditPlateComponent;
    let fixture: ComponentFixture<EditPlateComponent>;

    beforeEach(async() => {
        await TestBed.configureTestingModule({
            imports: [
                EditPlateComponent,
                BrowserAnimationsModule
            ],
            providers: [
                { provide: MAT_DIALOG_DATA, useValue: {}}
            ]
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
