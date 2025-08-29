import { Component, inject, type OnInit, ChangeDetectionStrategy } from '@angular/core';
import { first } from 'rxjs/operators';
import { AccountService, AlertService } from 'app/_services';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConfirmationDialogComponent, type ConfirmationDialogData } from '../../../shared/confirmation-dialog/confirmation-dialog.component';
import { RefreshButtonComponent } from '../../../shared/refresh-button/refresh-button.component';

interface DialogData {
  userId: string;
  userName: string;
}

interface PasskeyInfo {
  id: number;
  name: string;
  regDate: string;
  aaGuid: string;
}

@Component({
  selector: 'app-user-passkeys',
  templateUrl: 'user-passkeys.component.html',
  styleUrl: 'user-passkeys.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatExpansionModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatTooltipModule,
    RefreshButtonComponent,
  ],
})
export class UserPasskeysComponent extends OnPushBaseComponent implements OnInit {
  data = inject<DialogData>(MAT_DIALOG_DATA);

  private readonly accountService = inject(AccountService);
  private readonly alertService = inject(AlertService);
  private readonly dialog = inject(MatDialog);
  private readonly dialogRef = inject(MatDialogRef<UserPasskeysComponent>);
  private readonly formBuilder = inject(FormBuilder);

  userId = '';
  userName = '';
  loading = false;
  registering = false;
  
  passkeys: PasskeyInfo[] = [];
  registrationForm: FormGroup;

  ngOnInit() {
    this.userId = this.data.userId;
    this.userName = this.data.userName;

    this.registrationForm = this.formBuilder.group({
      name: ['', [Validators.required, Validators.maxLength(50)]]
    });

    this.loadPasskeys();
  }

  loadPasskeys() {
    this.loading = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.accountService.getPasskeys().pipe(first()),
      (response) => {
        this.passkeys = response.passkeys || [];
        this.loading = false;
      },
      (error) => {
        this.alertService.error(`Failed to load passkeys: ${error}`);
        this.loading = false;
      }
    );
  }

  async registerPasskey() {
    if (this.registrationForm.invalid) {
      return;
    }

    this.registering = true;
    this.markForCheck();

    try {
      // Check if WebAuthn is supported
      if (!window.navigator.credentials || !window.PublicKeyCredential) {
        throw new Error('WebAuthn is not supported in this browser');
      }

      const passkeyName = this.registrationForm.get('name')?.value;

      // Step 1: Get registration options from server
      const optionsResponse = await this.accountService.registerPasskey(passkeyName).pipe(first()).toPromise();
      
      if (!optionsResponse?.options) {
        throw new Error('Failed to get registration options');
      }

      // Step 2: Create credential using WebAuthn API
      const credential = await navigator.credentials.create({
        publicKey: this.convertRegistrationOptions(optionsResponse.options)
      }) as PublicKeyCredential;

      if (!credential) {
        throw new Error('Failed to create credential');
      }

      // Step 3: Send credential to server for verification
      const attestationResponse = this.encodeAttestationResponse(credential);
      
      const result = await this.accountService.completePasskeyRegistration(
        attestationResponse, 
        passkeyName
      ).pipe(first()).toPromise();

      if (result?.success) {
        this.alertService.success('Passkey registered successfully!');
        this.registrationForm.reset();
        this.loadPasskeys(); // Refresh the list
      } else {
        throw new Error(result?.message || 'Failed to register passkey');
      }

    } catch (error) {
      console.error('Passkey registration error:', error);
      this.alertService.error(`Failed to register passkey: ${error instanceof Error ? error.message : 'Unknown error'}`);
    } finally {
      this.registering = false;
      this.markForCheck();
    }
  }

  private convertRegistrationOptions(options: any): PublicKeyCredentialCreationOptions {
    return {
      ...options,
      challenge: this.base64urlToBuffer(options.challenge),
      user: {
        ...options.user,
        id: this.base64urlToBuffer(options.user.id)
      },
      excludeCredentials: options.excludeCredentials?.map((cred: any) => ({
        ...cred,
        id: this.base64urlToBuffer(cred.id)
      }))
    };
  }

  private encodeAttestationResponse(credential: PublicKeyCredential): string {
    const response = credential.response as AuthenticatorAttestationResponse;
    
    return JSON.stringify({
      id: credential.id,
      rawId: this.bufferToBase64url(credential.rawId),
      type: credential.type,
      response: {
        attestationObject: this.bufferToBase64url(response.attestationObject),
        clientDataJSON: this.bufferToBase64url(response.clientDataJSON)
      }
    });
  }

  private base64urlToBuffer(base64url: string): ArrayBuffer {
    const base64 = base64url.replace(/-/g, '+').replace(/_/g, '/');
    const padded = base64.padEnd(base64.length + (4 - base64.length % 4) % 4, '=');
    const binary = atob(padded);
    const buffer = new ArrayBuffer(binary.length);
    const bytes = new Uint8Array(buffer);
    for (let i = 0; i < binary.length; i++) {
      bytes[i] = binary.charCodeAt(i);
    }
    return buffer;
  }

  private bufferToBase64url(buffer: ArrayBuffer): string {
    const bytes = new Uint8Array(buffer);
    let binary = '';
    for (let i = 0; i < bytes.byteLength; i++) {
      binary += String.fromCharCode(bytes[i]);
    }
    return btoa(binary).replace(/\+/g, '-').replace(/\//g, '_').replace(/=/g, '');
  }

  deletePasskey(passkey: PasskeyInfo) {
    const dialogData: ConfirmationDialogData = {
      title: 'Delete Passkey',
      message: `Are you sure you want to delete the passkey "${passkey.name}"? This action cannot be undone.`,
      confirmText: 'Delete',
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
            this.accountService.deletePasskey(passkey.id).pipe(first()),
            (result) => {
              if (result.success) {
                this.alertService.success('Passkey deleted successfully');
                this.loadPasskeys(); // Refresh the list
              } else {
                this.alertService.error(result.message || 'Failed to delete passkey');
              }
            },
            (error) => {
              this.alertService.error(`Failed to delete passkey: ${error}`);
            }
          );
        }
      }
    );
  }

  onClose() {
    this.dialogRef.close();
  }
}
