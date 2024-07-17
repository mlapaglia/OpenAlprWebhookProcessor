import { TestBed } from '@angular/core/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { SystemLogsService } from './system-logs.service';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe(SystemLogsService.name, () => {
    let service: SystemLogsService;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [],
            providers: [SystemLogsService, provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
        });
        service = TestBed.inject(SystemLogsService);
    });

    it('should be created', () => {
        service = TestBed.get(SystemLogsService);
        expect(service).toBeTruthy();
    });
});
