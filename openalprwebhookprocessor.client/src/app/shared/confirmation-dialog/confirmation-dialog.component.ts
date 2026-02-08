import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

export interface ConfirmationDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  icon?: string;
  color?: 'primary' | 'accent' | 'warn';
}

@Component({
  selector: 'app-confirmation-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    MatDialogModule,
    MatButtonModule,
    MatIconModule
],
  template: `
    <div class="confirmation-dialog">
      <h2 mat-dialog-title>
        @if (data.icon) {
          <mat-icon [class]="'text-' + (data.color || 'primary')">
            {{ data.icon }}
          </mat-icon>
        }
        {{ data.title }}
      </h2>

      <mat-dialog-content>
        <div class="dialog-message" [innerHTML]="data.message"></div>
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button
          mat-button
          (click)="onCancel()"
          type="button">
          {{ data.cancelText || 'Cancel' }}
        </button>
        <button
          mat-raised-button
          [color]="data.color || 'primary'"
          (click)="onConfirm()"
          type="button">
          {{ data.confirmText || 'Confirm' }}
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles:
  [
    `
    .confirmation-dialog {
      min-width: 300px;
      max-width: 500px;
    }

    .dialog-message {
      white-space: pre-line;
      line-height: 1.5;
    }

    h2[mat-dialog-title] {
      display: flex;
      align-items: center;
      gap: 8px;
      margin: 0 0 16px 0;
    }

    mat-dialog-content {
      margin: 0 0 16px 0;
    }

    mat-dialog-actions {
      margin: 0;
      padding: 0;
    }

    .text-primary {
      color: #1976d2;
    }

    .text-accent {
      color: #ff4081;
    }

    .text-warn {
      color: #f44336;
    }
  `,
  ],
})
export class ConfirmationDialogComponent extends OnPushBaseComponent {
  dialogRef = inject(MatDialogRef<ConfirmationDialogComponent>);
  data = inject<ConfirmationDialogData>(MAT_DIALOG_DATA);

  onConfirm(): void {
    this.dialogRef.close(true);
    this.markForCheck();
  }

  onCancel(): void {
    this.dialogRef.close(false);
    this.markForCheck();
  }
}
