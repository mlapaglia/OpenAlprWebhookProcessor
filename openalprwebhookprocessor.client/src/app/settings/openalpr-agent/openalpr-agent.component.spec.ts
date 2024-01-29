import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OpenalprAgentComponent } from './openalpr-agent.component';
import { SettingsService } from '../settings.service';
import { of } from 'rxjs';
import { Agent } from './agent';
import { AgentStatus } from './agentStatus';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

describe(OpenalprAgentComponent.name, () => {
    let component: OpenalprAgentComponent;
    let fixture: ComponentFixture<OpenalprAgentComponent>;
    const settingsServiceSpy = jasmine.createSpyObj(SettingsService.name, ['getAgent', 'getAgentStatus']);
    
    beforeEach(async() => {
        await TestBed.configureTestingModule({
            imports: [OpenalprAgentComponent, BrowserAnimationsModule],
            providers: [
                { provide: SettingsService, useValue: settingsServiceSpy }
            ]
        }).compileComponents();
    });

    beforeEach(() => {
        settingsServiceSpy.getAgent.and.returnValue(of(new Agent()));
        settingsServiceSpy.getAgentStatus.and.returnValue(of(new AgentStatus()));
        fixture = TestBed.createComponent(OpenalprAgentComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
        expect(settingsServiceSpy.getAgent).toHaveBeenCalled();
        expect(settingsServiceSpy.getAgentStatus).toHaveBeenCalled();
    });
});
