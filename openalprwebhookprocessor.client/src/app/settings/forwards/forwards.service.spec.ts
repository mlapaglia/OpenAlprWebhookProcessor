import { TestBed } from '@angular/core/testing';

import { ForwardsService } from './forwards.service';
import { HttpClientTestingModule } from '@angular/common/http/testing';

describe('ForwardsService', () => {
    let service: ForwardsService;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule]
        });
        service = TestBed.inject(ForwardsService);
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });
});
