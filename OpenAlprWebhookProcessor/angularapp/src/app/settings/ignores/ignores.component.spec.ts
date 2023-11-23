import { ComponentFixture, TestBed } from '@angular/core/testing';

import { IgnoresComponent } from './ignores.component';
import { SettingsService } from '../settings.service';
import { Ignore } from './ignore';
import { of } from 'rxjs';

describe(IgnoresComponent.name, () => {
    let component: IgnoresComponent;
    let fixture: ComponentFixture<IgnoresComponent>;
    const settingsServiceSpy = jasmine.createSpyObj(SettingsService.name, ['getIgnores']);

    beforeEach(async() => {
        await TestBed.configureTestingModule({
            imports: [IgnoresComponent],
            providers: [
                { provide: SettingsService, useValue: settingsServiceSpy }
            ]
        })
            .compileComponents();
    });

    beforeEach(() => {
        const ignores: Ignore[] = [];
        settingsServiceSpy.getIgnores.and.returnValue(of(ignores));

        fixture = TestBed.createComponent(IgnoresComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
        expect(settingsServiceSpy.getIgnores).toHaveBeenCalled();
    });
});
