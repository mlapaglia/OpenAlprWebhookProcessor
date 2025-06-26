import { TestBed } from '@angular/core/testing';

import { PushoverService } from './pushover.service';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe(PushoverService.name, () => {
    let service: PushoverService;

    beforeEach(() => {
        TestBed.configureTestingModule({
    imports: [],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
        service = TestBed.inject(PushoverService);
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });
});
