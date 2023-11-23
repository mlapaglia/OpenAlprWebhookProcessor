import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PlateComponent } from './plate.component';
import { Lightbox, LightboxConfig, LightboxEvent } from 'ngx-lightbox';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { DatePipe } from '@angular/common';
import { Plate } from './plate';
import { PlateService } from '../plate.service';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

describe('PlateComponent', () => {
    let component: PlateComponent;
    let fixture: ComponentFixture<PlateComponent>;

    beforeEach(async() => {
        await TestBed.configureTestingModule({
            imports: [
                BrowserAnimationsModule,
                HttpClientTestingModule,
                PlateComponent
            ],
            providers: [
                DatePipe,
                Lightbox,
                LightboxConfig,
                LightboxEvent,
                PlateService
            ]
        })
            .compileComponents();
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(PlateComponent);
        component = fixture.componentInstance;
        component.plate = new Plate({
            cropImageUrl: new URL('http://google.com'),
            imageUrl: new URL('http://google.com') 
        });

        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
