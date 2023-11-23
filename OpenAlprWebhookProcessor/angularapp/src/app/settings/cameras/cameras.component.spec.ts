import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CamerasComponent } from './cameras.component';
import { HttpClientTestingModule } from '@angular/common/http/testing';

describe('CamerasComponent', () => {
    let component: CamerasComponent;
    let fixture: ComponentFixture<CamerasComponent>;

    beforeEach(async() => {
        await TestBed.configureTestingModule({
            imports: [CamerasComponent, HttpClientTestingModule],
            providers: []
        }).compileComponents();
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(CamerasComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
