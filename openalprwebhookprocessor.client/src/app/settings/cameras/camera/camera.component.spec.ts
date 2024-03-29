import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CameraComponent } from './camera.component';
import { Camera } from '../camera';

describe('CameraComponent', () => {
    let component: CameraComponent;
    let fixture: ComponentFixture<CameraComponent>;

    beforeEach(async() => {
        await TestBed.configureTestingModule({
            imports: [CameraComponent]
        }).compileComponents();
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(CameraComponent);
        component = fixture.componentInstance;
        component.camera = new Camera();
        
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
