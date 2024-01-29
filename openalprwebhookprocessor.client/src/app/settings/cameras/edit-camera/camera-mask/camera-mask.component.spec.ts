import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CameraMaskComponent } from './camera-mask.component';
import { CameraMaskService } from './camera-mask.service';
import { of } from 'rxjs';
import { CameraMask } from './camera-mask';
import { Camera } from '../../camera';

describe(CameraMaskComponent.name, () => {
    let component: CameraMaskComponent;
    let fixture: ComponentFixture<CameraMaskComponent>;
    const cameraMaskServiceSpy = jasmine.createSpyObj(CameraMaskService.name, ['getMask', 'getPlateCaptures']);

    beforeEach(async() => {
        await TestBed.configureTestingModule({
            imports: [CameraMaskComponent],
            providers: [
                { provide: CameraMaskService, useValue: cameraMaskServiceSpy }
            ]
        }).compileComponents();
    });

    beforeEach(() => {
        cameraMaskServiceSpy.getMask.and.returnValue(of(new CameraMask()));

        const plateIds: string[] = [];
        cameraMaskServiceSpy.getPlateCaptures.and.returnValue(of(plateIds));
        fixture = TestBed.createComponent(CameraMaskComponent);
        component = fixture.componentInstance;
        component.camera = new Camera();
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
