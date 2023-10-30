import { TestBed } from '@angular/core/testing';

import { SystemLogsService } from './systemLogs.service';

describe('LogsService', () => {
  let service: SystemLogsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SystemLogsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
