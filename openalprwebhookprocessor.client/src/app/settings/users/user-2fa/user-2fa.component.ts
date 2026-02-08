import { Component, inject, type OnInit, ChangeDetectionStrategy } from '@angular/core';
import { first } from 'rxjs/operators';
import { AccountService, AlertService } from 'app/_services';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

interface DialogData {
  userId: string;
  userName: string;
}

import { QrCodeDisplayComponent } from '../../../shared/qr-code-display/qr-code-display.component';
import { ConfirmationDialogComponent, type ConfirmationDialogData } from '../../../shared/confirmation-dialog/confirmation-dialog.component';
import { TwoFactorCodeInputComponent } from '../../../shared/two-factor-code-input/two-factor-code-input.component';
import { RecoveryCodesDisplayComponent } from '../../../shared/recovery-codes-display/recovery-codes-display.component';

@Component({
  selector: 'app-user-2fa',
  templateUrl: 'user-2fa.component.html',
  styleUrl: 'user-2fa.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MatCardModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatExpansionModule,
    MatIconModule,
    QrCodeDisplayComponent,
    TwoFactorCodeInputComponent,
    RecoveryCodesDisplayComponent
],
})
export class User2FAComponent extends OnPushBaseComponent implements OnInit {
  data = inject<DialogData>(MAT_DIALOG_DATA);

  private readonly accountService = inject(AccountService);
  private readonly alertService = inject(AlertService);
  private readonly dialog = inject(MatDialog);
  private readonly dialogRef = inject(MatDialogRef<User2FAComponent>);

  userId = '';
  loading = false;
  setupLoading = false;

  qrCodeUri = '';
  sharedKey = '';
  recoveryCodes: string[] = [];
  twoFactorEnabled = false;
  hasAuthenticator = false;
  userName = '';

  ngOnInit() {
    this.userId = this.data.userId;
    this.userName = this.data.userName;

    this.loadTwoFactorStatus();
  }

  loadTwoFactorStatus() {
    this.subscribeAndMarkForCheck(
      this.accountService.getTwoFactorStatusForUser(this.userId).pipe(first()),
      (status) => {
        this.twoFactorEnabled = status.isTwoFactorEnabled;
        this.hasAuthenticator = status.hasAuthenticator;
        if (!this.twoFactorEnabled) {
          this.setupTwoFactor();
        }
      },
      (error) => {
        this.alertService.error(String(error));
      },
    );
  }

  setupTwoFactor() {
    this.setupLoading = true;
    this.markForCheck();
    this.subscribeAndMarkForCheck(
      this.accountService.setupTwoFactorForUser(this.userId).pipe(first()),
      (setup) => {
        this.qrCodeUri = setup.qrCodeUri;
        this.sharedKey = setup.sharedKey;
        this.setupLoading = false;
      },
      (error) => {
        this.alertService.error(String(error));
        this.setupLoading = false;
      },
    );
  }

  onCodeSubmitted(code: string) {
    this.loading = true;
    this.markForCheck();
    this.subscribeAndMarkForCheck(
      this.accountService.enableTwoFactorForUser(this.userId, code).pipe(first()),
      (result) => {
        this.recoveryCodes = result.recoveryCodes;
        this.twoFactorEnabled = true;
        this.loading = false;
        this.alertService.success('Two-factor authentication has been enabled successfully for this user!');
      },
      (error) => {
        this.alertService.error(String(error));
        this.loading = false;
      },
    );
  }

  disableTwoFactor() {
    const dialogData: ConfirmationDialogData = {
      title: 'Disable Two-Factor Authentication',
      message: 'Are you sure you want to disable two-factor authentication for this user? This will make their account less secure.',
      confirmText: 'Disable',
      cancelText: 'Cancel',
      color: 'warn',
    };

    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width: '400px',
      data: dialogData,
    });

    this.subscribeAndMarkForCheck(
      dialogRef.afterClosed().pipe(first()),
      (confirmed) => {
        if (confirmed) {
          this.subscribeAndMarkForCheck(
            this.accountService.disableTwoFactorForUser(this.userId).pipe(first()),
            () => {
              this.twoFactorEnabled = false;
              this.hasAuthenticator = false;
              this.recoveryCodes = [];
              this.qrCodeUri = '';
              this.sharedKey = '';
              this.alertService.success('Two-factor authentication has been disabled for this user.');
              this.setupTwoFactor();
            },
            (error) => {
              this.alertService.error(String(error));
            },
          );
        }
      },
    );
  }

  onGenerateNewRecoveryCodes() {
    this.subscribeAndMarkForCheck(
      this.accountService.getRecoveryCodesForUser(this.userId).pipe(first()),
      (result) => {
        this.recoveryCodes = result.recoveryCodes;
        this.alertService.success('New recovery codes have been generated for this user.');
      },
      (error) => {
        this.alertService.error(String(error));
      },
    );
  }

  onClose() {
    this.dialogRef.close();
  }
}
