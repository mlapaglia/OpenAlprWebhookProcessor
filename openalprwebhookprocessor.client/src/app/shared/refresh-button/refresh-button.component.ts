import { Component, input, output, ChangeDetectionStrategy, type OnChanges, HostBinding } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';
import { timer, type Subscription } from 'rxjs';

export type ButtonType = 'basic' | 'raised' | 'stroked' | 'flat' | 'icon' | 'fab' | 'mini-fab';
export type ButtonColor = 'primary' | 'accent' | 'warn' | undefined;

@Component({
  selector: 'app-refresh-button',
  templateUrl: './refresh-button.component.html',
  styleUrls: ['./refresh-button.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MatButtonModule,
    MatIconModule,
  ],
})
export class RefreshButtonComponent extends OnPushBaseComponent implements OnChanges {
  /** Whether the button is loading. */
  readonly isLoading = input<boolean>(false);

  /** Whether the button is disabled. */
  readonly disabled = input<boolean>();

  /** The text to display when the button is not loading. */
  readonly buttonText = input<string>('Refresh');

  /** The text to display when the button is loading. */
  readonly buttonRefreshingText = input<string>('Refreshing...');

  /** The type of button to display. */
  readonly buttonType = input<ButtonType>('basic');

  /** The color of the button. */
  readonly color = input<ButtonColor>(undefined);

  /** The icon to display when not refreshing. */
  readonly icon = input<string>('refresh');

  /** Whether the button should take full width. */
  readonly fullWidth = input<boolean>(false);

  /** Emits when the button is clicked. */
  readonly refreshStarted = output<void>();

  @HostBinding('class.full-width-host')
  get isFullWidth(): boolean {
    return this.fullWidth();
  }

  public isSpinning = false;
  public isStopping = false;

  private stoppingTimerSubscription: Subscription | null = null;

  ngOnChanges(): void {
    this.handleLoadingState();
  }

  override ngOnDestroy(): void {
    this.cancelStoppingTimer();
    super.ngOnDestroy();
  }

  protected isDisabled(): boolean {
    const userRequestedDisabled = this.disabled();
    if (userRequestedDisabled !== undefined) {
      return userRequestedDisabled;
    }

    return (this.isSpinning || this.isStopping);
  }

  protected onRefresh(): void {
    this.refreshStarted.emit();
  }

  private handleLoadingState(): void {
    const loading = this.isLoading();

    if (loading && !this.isSpinning) {
      // Start spinning
      this.cancelStoppingTimer();
      this.isSpinning = true;
      this.isStopping = false;
      this.markForCheck();
    } else if (!loading && this.isSpinning && !this.isStopping) {
      // Start stopping animation
      this.isStopping = true;
      this.markForCheck();

      // Wait for the stopping animation to complete
      this.stoppingTimerSubscription = timer(1000).subscribe(() => {
        this.isSpinning = false;
        this.isStopping = false;
        this.stoppingTimerSubscription = null;
        this.markForCheck();
      });
    } else if (loading && this.isStopping) {
      // If loading starts again while stopping, immediately go back to spinning
      this.cancelStoppingTimer();
      this.isSpinning = true;
      this.isStopping = false;
      this.markForCheck();
    }
  }

  private cancelStoppingTimer(): void {
    if (this.stoppingTimerSubscription) {
      this.stoppingTimerSubscription.unsubscribe();
      this.stoppingTimerSubscription = null;
    }
  }
}
