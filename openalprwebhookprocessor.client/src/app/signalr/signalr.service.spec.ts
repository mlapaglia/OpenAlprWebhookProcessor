import { TestBed } from '@angular/core/testing'
import { provideHttpClientTesting } from '@angular/common/http/testing'
import { SignalrService } from './signalr.service'
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http'

describe('SignalrService', () => {
  let service: SignalrService

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()],
    })
    service = TestBed.inject(SignalrService)
  })

  it('should be created', () => {
    expect(service).toBeTruthy()
  })
})
