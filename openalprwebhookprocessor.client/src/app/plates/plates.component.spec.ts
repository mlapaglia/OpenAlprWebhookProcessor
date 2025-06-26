import { TestBed } from '@angular/core/testing'
import { PlatesComponent } from './plates.component'
import { provideHttpClientTesting } from '@angular/common/http/testing'
import { RouterTestingModule } from '@angular/router/testing'
import { BrowserAnimationsModule } from '@angular/platform-browser/animations'
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http'

describe(PlatesComponent.name, () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [RouterTestingModule,
        BrowserAnimationsModule],
      providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()],
    })
  })

  it('should create the app', () => {
    const fixture = TestBed.createComponent(PlatesComponent)
    const app = fixture.componentInstance
    expect(app).toBeTruthy()
  })
})
