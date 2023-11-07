import { TestBed } from '@angular/core/testing';

import { ForwardsService } from './forwards.service';

describe('ForwardsService', () => {
  let service: ForwardsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ForwardsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
