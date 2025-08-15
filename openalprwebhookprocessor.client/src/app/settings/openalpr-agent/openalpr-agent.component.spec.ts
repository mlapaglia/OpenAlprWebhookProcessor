import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { OpenalprAgentComponent } from './openalpr-agent.component';
import { SettingsService } from '../settings.service';
import { SignalrService } from 'app/signalr/signalr.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { of, Subject, throwError } from 'rxjs';
import { Agent } from './agent';
import { AgentStatus } from './agentStatus';
import { AgentVideoStreams } from './videoStream';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe(OpenalprAgentComponent.name, () => {
  let component: OpenalprAgentComponent;
  let fixture: ComponentFixture<OpenalprAgentComponent>;
  let mockSettingsService: jasmine.SpyObj<SettingsService>;
  let mockSignalrService: jasmine.SpyObj<SignalrService>;
  let mockSnackbarService: jasmine.SpyObj<SnackbarService>;
  let connectionStatusSubject: Subject<any>;

  beforeEach(async () => {
    connectionStatusSubject = new Subject<any>();

    mockSettingsService = jasmine.createSpyObj('SettingsService', [
      'getAgent', 'getAgentStatus', 'getAgentVideoStreams', 'upsertAgent',
      'startAgentScrape', 'enableAgent', 'disableAgent',
    ]);
    mockSignalrService = jasmine.createSpyObj('SignalrService', [], {
      openAlprAgentConnectionStatusChanged: connectionStatusSubject.asObservable(),
    });
    mockSnackbarService = jasmine.createSpyObj('SnackbarService', ['create']);

    await TestBed.configureTestingModule({
      imports: [OpenalprAgentComponent, BrowserAnimationsModule],
      providers: [
        { provide: SettingsService, useValue: mockSettingsService },
        { provide: SignalrService, useValue: mockSignalrService },
        { provide: SnackbarService, useValue: mockSnackbarService },
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();
  });

  beforeEach(() => {
    mockSettingsService.getAgent.and.returnValue(of(new Agent()));
    mockSettingsService.getAgentStatus.and.returnValue(of(new AgentStatus()));
    mockSettingsService.getAgentVideoStreams.and.returnValue(of(new AgentVideoStreams()));
    fixture = TestBed.createComponent(OpenalprAgentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
    expect(mockSettingsService.getAgent).toHaveBeenCalled();
    expect(mockSettingsService.getAgentStatus).toHaveBeenCalled();
  });

  it('should handle agent status loading state', () => {
    component.isLoadingAgentStatus = true;
    expect(component.isLoadingAgentStatus).toBeTrue();
  });

  it('should save agent', () => {
    const testAgent = new Agent({ id: 'test-id', endpointUrl: 'http://test.com' });
    component.agent = testAgent;
    mockSettingsService.upsertAgent.and.returnValue(of({} as any));

    component.saveAgent();

    expect(mockSettingsService.upsertAgent).toHaveBeenCalledWith(testAgent);
    expect(mockSettingsService.getAgent).toHaveBeenCalled();
  });

  it('should start agent scrape', () => {
    mockSettingsService.startAgentScrape.and.returnValue(of(null));

    component.scrapeAgent();

    expect(mockSettingsService.startAgentScrape).toHaveBeenCalled();
    expect(mockSnackbarService.create).toHaveBeenCalled();
  });

  it('should enable agent', () => {
    const testAgent = new Agent({ id: 'test-id' });
    component.agent = testAgent;
    mockSettingsService.enableAgent.and.returnValue(of(true));

    component.enableAgent();

    expect(mockSettingsService.enableAgent).toHaveBeenCalledWith('test-id');
  });

  it('should disable agent', () => {
    const testAgent = new Agent({ id: 'test-id' });
    component.agent = testAgent;
    mockSettingsService.disableAgent.and.returnValue(of(true));

    component.disableAgent();

    expect(mockSettingsService.disableAgent).toHaveBeenCalledWith('test-id');
  });

  it('should format bytes correctly', () => {
    expect(component['formatBytes'](0)).toBe('0 Bytes');
    expect(component['formatBytes'](1024)).toBe('1 KiB');
    expect(component['formatBytes'](1048576)).toBe('1 MiB');
    expect(component['formatBytes'](1073741824)).toBe('1 GiB');
  });

  it('should handle agent status error', () => {
    mockSettingsService.getAgentStatus.and.returnValue(throwError(() => new Error('Test error')));
    fixture = TestBed.createComponent(OpenalprAgentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.isLoadingAgentStatus).toBeFalse();
    expect(component.agentStatus.isConnected).toBeFalse();
  });

  it('should reload agent status when connection status changes', () => {
    expect(mockSettingsService.getAgentStatus).toHaveBeenCalledTimes(1);

    connectionStatusSubject.next({});
    expect(mockSettingsService.getAgentStatus).toHaveBeenCalledTimes(2);
  });

  it('should call super.ngOnDestroy on destroy', () => {
    spyOn(Object.getPrototypeOf(Object.getPrototypeOf(component)), 'ngOnDestroy');

    component.ngOnDestroy();

    expect(Object.getPrototypeOf(Object.getPrototypeOf(component)).ngOnDestroy).toHaveBeenCalled();
  });
});
