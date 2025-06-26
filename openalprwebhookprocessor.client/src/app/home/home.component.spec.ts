import { ComponentFixture, TestBed } from '@angular/core/testing'
import { HomeComponent } from './home.component'
import { HomeService } from './home.service'
import { of } from 'rxjs'
import { BrowserAnimationsModule } from '@angular/platform-browser/animations'
import { MatCardModule } from '@angular/material/card'
import { BarChartModule } from '@swimlane/ngx-charts'
import { AccountService } from 'app/_services'
import { User } from 'app/_models'
import { DayCounts } from './plateCountResponse'
import { MostSeenCounts } from './mostSeenResponse'

describe('HomeComponent', () => {
  let component: HomeComponent
  let fixture: ComponentFixture<HomeComponent>
  let mockHomeService: jasmine.SpyObj<HomeService>
  let mockAccountService: Partial<AccountService>

  const testPlateData: DayCounts = {
    counts: [
      { date: new Date('2025-06-20'), count: 5 },
      { date: new Date('2025-06-21'), count: 8 },
    ],
    weeklyUniqueCounts: [
      { date: new Date('2025-06-20'), count: 5 },
      { date: new Date('2025-06-21'), count: 8 },
    ],
  }

  const testMostSeenData: MostSeenCounts = {
    counts: [
      { plateNumber: 'ABC123', count: 12 },
      { plateNumber: 'XYZ789', count: 7 },
    ],
  }

  beforeEach(async () => {
    mockHomeService = jasmine.createSpyObj('HomeService', ['getPlatesCount', 'getMostSeenPlates'])
    mockHomeService.getPlatesCount.and.returnValue(of(testPlateData))
    mockHomeService.getMostSeenPlates.and.returnValue(of(testMostSeenData))

    mockAccountService = {
      userValue: {
        username: 'testuser',
      } as User,
    }

    await TestBed.configureTestingModule({
      imports: [
        HomeComponent,
        MatCardModule,
        BarChartModule,
        BrowserAnimationsModule,
      ],
      providers: [
        { provide: HomeService, useValue: mockHomeService },
        { provide: AccountService, useValue: mockAccountService },
      ],
    }).compileComponents()
  })

  beforeEach(() => {
    fixture = TestBed.createComponent(HomeComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })

  it('should load plateCounts on init', () => {
    expect(component.plateCounts.length).toBe(2)
    expect(component.plateCounts[0]).toEqual({
      name: new Date('2025-06-20'),
      value: 5,
    })
  })

  it('should load mostSeenCounts on init', () => {
    expect(component.mostSeenCounts.length).toBe(2)
    expect(component.mostSeenCounts[0]).toEqual({
      name: 'ABC123',
      value: 12,
    })
  })

  it('should call homeService methods once each', () => {
    expect(mockHomeService.getPlatesCount).toHaveBeenCalledTimes(1)
    expect(mockHomeService.getMostSeenPlates).toHaveBeenCalledTimes(1)
  })
})
