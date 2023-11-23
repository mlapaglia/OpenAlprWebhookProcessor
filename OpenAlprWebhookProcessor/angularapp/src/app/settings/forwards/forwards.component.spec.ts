import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ForwardsComponent } from './forwards.component';
import { ForwardsService } from './forwards.service';
import { of } from 'rxjs';
import { Forward } from './forward';

describe(ForwardsComponent.name, () => {
    let component: ForwardsComponent;
    let fixture: ComponentFixture<ForwardsComponent>;
    const forwardsServiceSpy = jasmine.createSpyObj(ForwardsService.name, ['getForwards']);

    beforeEach(async() => {
        await TestBed.configureTestingModule({
            imports: [ForwardsComponent],
            providers: [ 
                { provide: ForwardsService, useValue: forwardsServiceSpy }
            ]
        }).compileComponents();
    });

    beforeEach(() => {
        const forwards: Forward[] = [];
        forwardsServiceSpy.getForwards.and.returnValue(of(forwards));
        fixture = TestBed.createComponent(ForwardsComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
        expect(forwardsServiceSpy.getForwards).toHaveBeenCalled();
    });
});
