import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { SystemLogsService } from './system-logs.service';

describe(SystemLogsService.name, () => {
    let service: SystemLogsService;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [SystemLogsService]
        });
        service = TestBed.inject(SystemLogsService);
    });

    it('should be created', () => {
        service = TestBed.get(SystemLogsService);
        expect(service).toBeTruthy();
    });
});
