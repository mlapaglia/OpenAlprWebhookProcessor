import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SystemLogsComponent } from './system-logs.component';
import { SystemLogsService } from './system-logs.service';
import { of } from 'rxjs';

describe(SystemLogsComponent.name, () => {
    let component: SystemLogsComponent;
    let fixture: ComponentFixture<SystemLogsComponent>;
    const systemLogsServiceSpy = jasmine.createSpyObj(SystemLogsService.name, ['getLogs']);

    beforeEach(async() => {
        await TestBed.configureTestingModule({
            imports: [SystemLogsComponent],
            providers: [
                { provide: SystemLogsService, useValue: systemLogsServiceSpy }
            ]
        }).compileComponents();
    });

    beforeEach(() => {
        const logs: string[] = [];
        systemLogsServiceSpy.getLogs.and.returnValue(of(logs));

        fixture = TestBed.createComponent(SystemLogsComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
        expect(systemLogsServiceSpy.getLogs).toHaveBeenCalled();
    });
});
