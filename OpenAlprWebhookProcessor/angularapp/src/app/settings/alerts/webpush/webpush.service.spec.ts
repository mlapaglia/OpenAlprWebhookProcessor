import { TestBed } from '@angular/core/testing';

import { WebpushService } from './webpush.service';

describe('WebpushService', () => {
  let service: WebpushService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(WebpushService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
