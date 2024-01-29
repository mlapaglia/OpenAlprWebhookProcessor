import { TestBed } from '@angular/core/testing';

import { PushoverService } from './pushover.service';
import { HttpClientTestingModule } from '@angular/common/http/testing';

describe(PushoverService.name, () => {
    let service: PushoverService;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule]
        });
        service = TestBed.inject(PushoverService);
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });
});
