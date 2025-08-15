import { Component, type OnInit, inject, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { first } from 'rxjs/operators';

import { AccountService } from '../account.service';
import { SnackbarService } from '../../snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatExpansionModule } from '@angular/material/expansion';

import { QrCodeDisplayComponent } from '../../shared/qr-code-display/qr-code-display.component';
import { TwoFactorCodeInputComponent } from '../../shared/two-factor-code-input/two-factor-code-input.component';
import { RecoveryCodesDisplayComponent } from '../../shared/recovery-codes-display/recovery-codes-display.component';

@Component({
  selector: 'app-setup-2fa',
  templateUrl: 'setup-2fa.component.html',
  styleUrl: 'setup-2fa.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatExpansionModule,
    QrCodeDisplayComponent,
    TwoFactorCodeInputComponent,
    RecoveryCodesDisplayComponent,
  ],
})
export class Setup2FAComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly accountService = inject(AccountService);
  private readonly snackbarService = inject(SnackbarService);
  private readonly cdr = inject(ChangeDetectorRef);

  loading = false;
  setupLoading = false;

  qrCodeUri = '';
  sharedKey = '';
  recoveryCodes: string[] = [];
  twoFactorEnabled = false;

  ngOnInit() {
    this.loadTwoFactorStatus();
  }

  loadTwoFactorStatus() {
    this.accountService.getTwoFactorStatus()
      .pipe(first())
      .subscribe({
        next: (status) => {
          this.twoFactorEnabled = status.isTwoFactorEnabled;
          this.cdr.markForCheck();
          if (!this.twoFactorEnabled) {
            this.setupTwoFactor();
          }
        },
        error: (error) => {
          this.snackbarService.create(error, SnackBarType.Error);
        },
      });
  }

  setupTwoFactor() {
    this.setupLoading = true;
    this.cdr.markForCheck();

    this.accountService.setupTwoFactor()
      .pipe(first())
      .subscribe({
        next: (setup) => {
          this.qrCodeUri = setup.qrCodeUri;
          this.sharedKey = setup.sharedKey;
          this.setupLoading = false;
          this.cdr.markForCheck();
        },
        error: (error) => {
          this.snackbarService.create(error, SnackBarType.Error);
          this.setupLoading = false;
          this.cdr.markForCheck();
        },
      });
  }

  onCodeSubmitted(code: string) {
    this.loading = true;
    this.cdr.markForCheck();

    this.accountService.enableTwoFactor(code)
      .pipe(first())
      .subscribe({
        next: (result) => {
          this.recoveryCodes = result.recoveryCodes;
          this.twoFactorEnabled = true;
          this.loading = false;
          this.cdr.markForCheck();
          this.snackbarService.create('Two-factor authentication has been enabled successfully!', SnackBarType.Successful);
        },
        error: (error) => {
          this.snackbarService.create(error, SnackBarType.Error);
          this.loading = false;
          this.cdr.markForCheck();
        },
      });
  }

  disableTwoFactor() {
    // TODO: Replace with proper Material dialog component
    // Using confirm for now to maintain functionality
    // eslint-disable-next-line no-alert
    if (confirm('Are you sure you want to disable two-factor authentication? This will make your account less secure.')) {
      this.accountService.disableTwoFactor()
        .pipe(first())
        .subscribe({
          next: () => {
            this.twoFactorEnabled = false;
            this.qrCodeUri = '';
            this.sharedKey = '';
            this.recoveryCodes = [];
            this.cdr.markForCheck();
            this.snackbarService.create('Two-factor authentication has been disabled.', SnackBarType.Successful);
            this.setupTwoFactor(); // Setup again for re-enabling
          },
          error: (error) => {
            this.snackbarService.create(error, SnackBarType.Error);
          },
        });
    }
  }

  onGenerateNewRecoveryCodes() {
    this.accountService.getRecoveryCodes()
      .pipe(first())
      .subscribe({
        next: (result) => {
          this.recoveryCodes = result.recoveryCodes;
          this.cdr.markForCheck();
          this.snackbarService.create('New recovery codes have been generated.', SnackBarType.Successful);
        },
        error: (error) => {
          this.snackbarService.create(error, SnackBarType.Error);
        },
      });
  }
}
