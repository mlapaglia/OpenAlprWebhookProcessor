import { TestBed } from '@angular/core/testing';
import { BreakpointObserver } from '@angular/cdk/layout';
import { of } from 'rxjs';
import { LayoutService } from './layout.service';

describe('LayoutService', () => {
  let service: LayoutService;
  let mockBreakpointObserver: jasmine.SpyObj<BreakpointObserver>;

  beforeEach(() => {
    const breakpointObserverSpy = jasmine.createSpyObj('BreakpointObserver', ['observe', 'isMatched']);

    TestBed.configureTestingModule({
      providers: [
        LayoutService,
        { provide: BreakpointObserver, useValue: breakpointObserverSpy },
      ],
    });

    service = TestBed.inject(LayoutService);
    mockBreakpointObserver = TestBed.inject(BreakpointObserver) as jasmine.SpyObj<BreakpointObserver>;
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should return mobile layout state', (done) => {
    mockBreakpointObserver.observe.and.returnValue(of({ matches: true, breakpoints: {} }));
    mockBreakpointObserver.isMatched.and.returnValues(true, false); // mobile: true, tablet: false

    let emissionCount = 0;
    service.getLayoutState().subscribe(state => {
      emissionCount++;
      if (emissionCount === 2) { // Second emission after debounce is the actual state
        expect(state.isMobile).toBe(true);
        expect(state.isTablet).toBe(false);
        expect(state.quickStatCols).toBe(2);
        done();
      }
    });
  });

  it('should return tablet layout state', (done) => {
    mockBreakpointObserver.observe.and.returnValue(of({ matches: true, breakpoints: {} }));
    mockBreakpointObserver.isMatched.and.returnValues(false, true); // mobile: false, tablet: true

    let emissionCount = 0;
    service.getLayoutState().subscribe(state => {
      emissionCount++;
      if (emissionCount === 2) { // Second emission is the actual state
        expect(state.isMobile).toBe(false);
        expect(state.isTablet).toBe(true);
        expect(state.quickStatCols).toBe(4);
        done();
      }
    });
  });

  it('should return desktop layout state', (done) => {
    mockBreakpointObserver.observe.and.returnValue(of({ matches: false, breakpoints: {} }));
    mockBreakpointObserver.isMatched.and.returnValues(false, false); // mobile: false, tablet: false

    let emissionCount = 0;
    service.getLayoutState().subscribe(state => {
      emissionCount++;
      if (emissionCount === 2) { // Second emission is the actual state
        expect(state.isMobile).toBe(false);
        expect(state.isTablet).toBe(false);
        expect(state.quickStatCols).toBe(4);
        done();
      }
    });
  });
});
