import { TestBed } from '@angular/core/testing';

import { ForwardsService } from './forwards.service';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('ForwardsService', () => {
  let service: ForwardsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [],
      providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()],
    });
    service = TestBed.inject(ForwardsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
