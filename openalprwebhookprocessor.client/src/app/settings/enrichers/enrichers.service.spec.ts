import { TestBed } from '@angular/core/testing';

import { EnrichersService } from './enrichers.service';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('EnrichersService', () => {
    let service: EnrichersService;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [],
            providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
        });
        service = TestBed.inject(EnrichersService);
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });
});
