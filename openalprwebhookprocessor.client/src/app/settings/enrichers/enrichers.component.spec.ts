import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EnrichersComponent } from './enrichers.component';
import { EnrichersService } from './enrichers.service';
import { Enricher } from './enricher';
import { of } from 'rxjs';

describe(EnrichersComponent.name, () => {
    let component: EnrichersComponent;
    let fixture: ComponentFixture<EnrichersComponent>;
    const enrichersServiceSpy = jasmine.createSpyObj(EnrichersComponent.name, ['getEnricher']);

    beforeEach(async() => {
        await TestBed.configureTestingModule({
            imports: [EnrichersComponent],
            providers: [
                { provide: EnrichersService, useValue: enrichersServiceSpy }
            ]
        })
            .compileComponents();
    });

    beforeEach(() => {
        enrichersServiceSpy.getEnricher.and.returnValue(of(new Enricher()));
        fixture = TestBed.createComponent(EnrichersComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
        expect(enrichersServiceSpy.getEnricher).toHaveBeenCalled();
    });
});
