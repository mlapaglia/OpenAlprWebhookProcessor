import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WebpushComponent } from './webpush.component';
import { WebpushService } from './webpush.service';
import { Webpush } from './webpush';
import { of } from 'rxjs';

describe(WebpushComponent.name, () => {
    let component: WebpushComponent;
    let fixture: ComponentFixture<WebpushComponent>;
    const webpushServiceSpy = jasmine.createSpyObj(WebpushService.name, ['getWebpush']);

    beforeEach(async() => {
        await TestBed.configureTestingModule({
            imports: [WebpushComponent],
            providers: [
                { provide: WebpushService, useValue: webpushServiceSpy }
            ]
        }).compileComponents();
    });

    beforeEach(() => {
        webpushServiceSpy.getWebpush.and.returnValue(of(new Webpush()));
        fixture = TestBed.createComponent(WebpushComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
        expect(webpushServiceSpy.getWebpush).toHaveBeenCalled();
    });
});
