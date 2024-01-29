import { TestBed } from '@angular/core/testing';
import { AlertsService } from './alerts.service';
import { HttpClientTestingModule } from '@angular/common/http/testing';

describe(AlertsService.name, () => {
    let service: AlertsService;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule]
        });

        service = TestBed.inject(AlertsService);
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });
});
