import { TestBed } from '@angular/core/testing';

import { PushoverService } from './pushover.service';

describe('PushoverService', () => {
  let service: PushoverService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PushoverService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
