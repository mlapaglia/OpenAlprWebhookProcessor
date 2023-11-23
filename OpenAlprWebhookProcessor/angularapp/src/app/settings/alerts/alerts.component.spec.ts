import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AlertsComponent } from './alerts.component';
import { AlertsService } from './alerts.service';
import { PushoverService } from './pushover/pushover.service';
import { WebpushService } from './webpush/webpush.service';
import { of } from 'rxjs';
import { Alert } from './alert';
import { Pushover } from './pushover/pushover';
import { Webpush } from './webpush/webpush';

describe(AlertsComponent.name, () => {
    let fixture: ComponentFixture<AlertsComponent>;
    let component: AlertsComponent;
    
    const pushoverServiceSpy = jasmine.createSpyObj(PushoverService.name, ['getPushover']);
    const webpushServiceSpy = jasmine.createSpyObj('WebpushService', ['getWebpush']);
    const alertsServiceSpy = jasmine.createSpyObj('AlertsService', ['getAlerts']);

    beforeEach(async() => {
        await TestBed.configureTestingModule({
            imports: [AlertsComponent],
            providers: [
                { provide: AlertsService, useValue: alertsServiceSpy },
                { provide: PushoverService, useValue: pushoverServiceSpy },
                { provide: WebpushService, useValue: webpushServiceSpy }
            ]
        }).compileComponents();
    });

    beforeEach(() => {
        const alertsData: Alert[] = [];
        alertsServiceSpy.getAlerts.and.returnValue(of(alertsData));
        pushoverServiceSpy.getPushover.and.returnValue(of(new Pushover()));
        webpushServiceSpy.getWebpush.and.returnValue(of(new Webpush()));

        fixture = TestBed.createComponent(AlertsComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should call getAlerts method with specific return value', () => {
        expect(component).toBeTruthy();
        expect(alertsServiceSpy.getAlerts).toHaveBeenCalled();
        expect(pushoverServiceSpy.getPushover).toHaveBeenCalled();
        expect(webpushServiceSpy.getWebpush).toHaveBeenCalled();
    });
});