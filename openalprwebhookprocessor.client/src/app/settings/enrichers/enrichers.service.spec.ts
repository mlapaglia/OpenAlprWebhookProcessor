import { TestBed } from '@angular/core/testing';

import { EnrichersService } from './enrichers.service';
import { HttpClientTestingModule } from '@angular/common/http/testing';

describe('EnrichersService', () => {
    let service: EnrichersService;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule]
        });
        service = TestBed.inject(EnrichersService);
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });
});
