import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SnackbarComponent } from './snackbar.component';
import { MAT_SNACK_BAR_DATA } from '@angular/material/snack-bar';
import { SnackBar } from './snackbar';
import { SnackBarType } from './snackbartype';

describe('SnackbarComponent', () => {
    let component: SnackbarComponent;
    let fixture: ComponentFixture<SnackbarComponent>;

    beforeEach(async() => {
        await TestBed.configureTestingModule({
            imports: [SnackbarComponent],
            providers: [
                {
                    provide: MAT_SNACK_BAR_DATA,
                    useValue: new SnackBar(
                        {
                            message: 'test',
                            message2: 'test123',
                            snackType: SnackBarType.Info
                        })
                }
            ]
        })
            .compileComponents();
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(SnackbarComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
