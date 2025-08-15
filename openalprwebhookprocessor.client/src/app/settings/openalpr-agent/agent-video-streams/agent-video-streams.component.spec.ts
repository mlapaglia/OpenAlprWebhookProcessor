import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { AgentVideoStreamsComponent } from './agent-video-streams.component';
import { SettingsService } from '../../settings.service';
import { SignalrService } from 'app/signalr/signalr.service';
import { of, Subject, throwError } from 'rxjs';
import { AgentVideoStreams, VideoStream } from '../videoStream';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('AgentVideoStreamsComponent', () => {
  let component: AgentVideoStreamsComponent;
  let fixture: ComponentFixture<AgentVideoStreamsComponent>;
  let mockSettingsService: jasmine.SpyObj<SettingsService>;
  let mockSignalrService: jasmine.SpyObj<SignalrService>;
  let connectionStatusSubject: Subject<any>;

  beforeEach(async () => {
    connectionStatusSubject = new Subject<any>();

    mockSettingsService = jasmine.createSpyObj('SettingsService', ['getAgentVideoStreams']);
    mockSignalrService = jasmine.createSpyObj('SignalrService', [], {
      openAlprAgentConnectionStatusChanged: connectionStatusSubject.asObservable(),
    });

    await TestBed.configureTestingModule({
      imports: [AgentVideoStreamsComponent, BrowserAnimationsModule],
      providers: [
        { provide: SettingsService, useValue: mockSettingsService },
        { provide: SignalrService, useValue: mockSignalrService },
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AgentVideoStreamsComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    mockSettingsService.getAgentVideoStreams.and.returnValue(of(new AgentVideoStreams()));
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load video streams on init', () => {
    const mockVideoStreams = new AgentVideoStreams({
      isConnected: true,
      videoStreams: [
        new VideoStream({
          cameraId: 1,
          cameraName: 'Test Camera',
          fps: 15.0,
          isStreaming: true,
          lastPlateRead: 1640995200000,
          lastUpdate: 1640995260000,
          totalPlateReads: 50,
          url: 'rtsp://test/stream',
        }),
      ],
    });

    mockSettingsService.getAgentVideoStreams.and.returnValue(of(mockVideoStreams));
    fixture.detectChanges();

    expect(mockSettingsService.getAgentVideoStreams).toHaveBeenCalled();
    expect(component.agentVideoStreams).toEqual(mockVideoStreams);
    expect(component.isLoading).toBeFalse();
  });

  it('should handle error when loading video streams', () => {
    mockSettingsService.getAgentVideoStreams.and.returnValue(throwError(() => new Error('Test error')));
    fixture.detectChanges();

    expect(component.isLoading).toBeFalse();
    expect(component.agentVideoStreams?.isConnected).toBeFalse();
  });

  it('should reload video streams when connection status changes', () => {
    const mockVideoStreams = new AgentVideoStreams({ isConnected: true, videoStreams: [] });
    mockSettingsService.getAgentVideoStreams.and.returnValue(of(mockVideoStreams));

    fixture.detectChanges();
    expect(mockSettingsService.getAgentVideoStreams).toHaveBeenCalledTimes(1);

    connectionStatusSubject.next({});
    expect(mockSettingsService.getAgentVideoStreams).toHaveBeenCalledTimes(2);
  });

  it('should format epoch time correctly', () => {
    mockSettingsService.getAgentVideoStreams.and.returnValue(of(new AgentVideoStreams()));
    fixture.detectChanges();

    expect(component.formatEpochTime(0)).toBe('Never');
    expect(component.formatEpochTime(undefined as any)).toBe('Never');

    const testTime = 1640995200000; // 2022-01-01 00:00:00 UTC
    const result = component.formatEpochTime(testTime);
    expect(result).toBe(new Date(testTime).toLocaleString());
  });

  it('should display correct columns', () => {
    expect(component.displayedColumns).toEqual(['cameraName', 'url', 'isStreaming', 'fps', 'totalPlateReads', 'lastPlateRead', 'lastUpdate']);
  });

  it('should cleanup subscriptions on destroy', () => {
    mockSettingsService.getAgentVideoStreams.and.returnValue(of(new AgentVideoStreams()));
    fixture.detectChanges();

    spyOn(component['destroy$'], 'next');
    spyOn(component['destroy$'], 'complete');
    component.ngOnDestroy();
    expect(component['destroy$'].next).toHaveBeenCalled();
    expect(component['destroy$'].complete).toHaveBeenCalled();
  });
});
