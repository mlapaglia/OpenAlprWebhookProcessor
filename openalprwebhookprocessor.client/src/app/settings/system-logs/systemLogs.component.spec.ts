import { ComponentFixture, TestBed } from '@angular/core/testing'
import { provideHttpClientTesting } from '@angular/common/http/testing'
import { SystemLogsComponent } from './system-logs.component'
import { SystemLogsService, ApiLogLevel } from './system-logs.service'
import { of } from 'rxjs'
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http'
import { HIGHLIGHT_OPTIONS } from 'ngx-highlightjs'

describe(SystemLogsComponent.name, () => {
  let component: SystemLogsComponent
  let fixture: ComponentFixture<SystemLogsComponent>
  const systemLogsServiceSpy = jasmine.createSpyObj(SystemLogsService.name, ['getLogs'])

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SystemLogsComponent],
      providers: [
        { provide: SystemLogsService, useValue: systemLogsServiceSpy },
        {
          provide: HIGHLIGHT_OPTIONS,
          useValue: {
            coreLibraryLoader: () => Promise.resolve({
              highlight: (text: string) => ({ 
                value: text,
                language: 'plaintext',
                relevance: 0,
                top: null,
                secondBest: null
              }),
              highlightAuto: (text: string) => ({
                value: text,
                language: 'plaintext',
                relevance: 0,
                top: null,
                secondBest: null
              }),
              configure: () => {},
              listLanguages: () => ['plaintext'],
              registerLanguage: () => {},
              getLanguage: () => ({ name: 'plaintext' }),
              highlightAll: () => {},
              debugMode: () => {},
              safeMode: () => {},
              versionString: '11.0.0'
            }),
            languages: {
              plaintext: () => Promise.resolve({})
            }
          }
        },
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
      ],
    }).compileComponents()
  })

  beforeEach(() => {
    const logs: string[] = []
    systemLogsServiceSpy.getLogs.and.returnValue(of(logs))

    fixture = TestBed.createComponent(SystemLogsComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
    expect(systemLogsServiceSpy.getLogs).toHaveBeenCalledWith(ApiLogLevel.Information)
  })
})
