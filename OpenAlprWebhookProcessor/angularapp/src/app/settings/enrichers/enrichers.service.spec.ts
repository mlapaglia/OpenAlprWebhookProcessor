import { TestBed } from '@angular/core/testing';

import { EnrichersService } from './enrichers.service';

describe('EnrichersService', () => {
  let service: EnrichersService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(EnrichersService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
