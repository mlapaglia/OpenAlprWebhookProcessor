import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PushoverComponent } from './pushover.component';
import { PushoverService } from './pushover.service';
import { of } from 'rxjs';
import { Pushover } from './pushover';

describe(PushoverComponent.name, () => {
    let component: PushoverComponent;
    let fixture: ComponentFixture<PushoverComponent>;
    const pushoverServiceSpy = jasmine.createSpyObj(PushoverService.name, ['getPushover']);
    
    beforeEach(async() => {
        await TestBed.configureTestingModule({
            imports: [PushoverComponent],
            providers: [
                { provide: PushoverService, useValue: pushoverServiceSpy }
            ]
        }).compileComponents();
    });

    beforeEach(() => {
        pushoverServiceSpy.getPushover.and.returnValue(of(new Pushover()));
        fixture = TestBed.createComponent(PushoverComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
        expect(pushoverServiceSpy.getPushover).toHaveBeenCalled();
    });
});
