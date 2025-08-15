import { ChangeDetectorRef, inject, type OnDestroy, Directive } from '@angular/core';
import { Subject, type Observable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

/**
 * Base class for OnPush components that provides common patterns and utilities
 */
@Directive()
export abstract class OnPushBaseComponent implements OnDestroy {
  protected readonly cdr = inject(ChangeDetectorRef);
  protected readonly destroy$ = new Subject<void>();

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Marks the component for check and optionally runs detectChanges
   * @param detectChanges - Whether to run detectChanges immediately
   */
  protected markForCheck(detectChanges = false): void {
    this.cdr.markForCheck();
    if (detectChanges) {
      this.cdr.detectChanges();
    }
  }

  /**
   * Helper method for subscribing to observables with automatic cleanup and change detection
   * @param observable - The observable to subscribe to
   * @param next - The next callback
   * @param error - Optional error callback
   */
  protected subscribeAndMarkForCheck<T>(
    observable: Observable<T>,
    next: (value: T) => void,
    error?: (error: unknown) => void,
  ): void {
    observable
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (value: T) => {
          next(value);
          this.markForCheck();
        },
        error: (err: unknown) => {
          if (error) {
            error(err);
          } else {
            console.error('Observable error:', err);
          }
          this.markForCheck();
        },
      });
  }

  /**
   * Helper for setTimeout with automatic change detection
   * @param callback - Function to execute
   * @param delay - Delay in milliseconds
   */
  protected setTimeoutWithCheck(callback: () => void, delay: number): void {
    setTimeout(() => {
      callback();
      this.markForCheck();
    }, delay);
  }
}
