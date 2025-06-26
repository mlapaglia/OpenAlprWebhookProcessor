import { TestBed } from '@angular/core/testing';
import { AlertsService } from './alerts.service';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe(AlertsService.name, () => {
    let service: AlertsService;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [],
            providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
        });

        service = TestBed.inject(AlertsService);
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });
});
