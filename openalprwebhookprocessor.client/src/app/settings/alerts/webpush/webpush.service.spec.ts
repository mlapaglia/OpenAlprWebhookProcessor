import { TestBed } from '@angular/core/testing';

import { WebpushService } from './webpush.service';
import { HttpClientTestingModule } from '@angular/common/http/testing';

describe('WebpushService', () => {
    let service: WebpushService;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule]
        });
        service = TestBed.inject(WebpushService);
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });
});
