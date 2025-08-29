import { Injectable, inject } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { map, startWith, type Observable } from 'rxjs';

export interface LayoutState {
  isMobile: boolean;
  isTablet: boolean;
  quickStatCols: number;
}

@Injectable({
  providedIn: 'root',
})
export class LayoutService {
  private readonly breakpointObserver = inject(BreakpointObserver);

  getLayoutState(): Observable<LayoutState> {
    const defaultState: LayoutState = {
      isMobile: false,
      isTablet: false,
      quickStatCols: 4,
    };

    return this.breakpointObserver.observe([
      Breakpoints.XSmall,
      Breakpoints.Small,
      Breakpoints.Medium,
    ]).pipe(
      map(_result => {
        const isMobile = this.breakpointObserver.isMatched(Breakpoints.XSmall);
        const isTablet = this.breakpointObserver.isMatched(Breakpoints.Small);

        let quickStatCols = 4;
        if (isMobile) {
          quickStatCols = 2;
        } else if (isTablet) {
          quickStatCols = 4;
        } else {
          quickStatCols = 4;
        }

        return {
          isMobile,
          isTablet,
          quickStatCols,
        };
      }),
      // Start with default state to prevent layout shift
      startWith(defaultState),
    );
  }
}
