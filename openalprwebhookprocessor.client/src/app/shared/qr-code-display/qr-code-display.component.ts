import { Component, ChangeDetectionStrategy, type OnInit, input } from '@angular/core';

import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';
import { generate } from 'lean-qr';

@Component({
  selector: 'app-qr-code-display',
  templateUrl: 'qr-code-display.component.html',
  styleUrl: 'qr-code-display.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [MatProgressSpinnerModule, MatIconModule, MatCardModule, MatButtonModule, MatTooltipModule],
})
export class QrCodeDisplayComponent extends OnPushBaseComponent implements OnInit {
  readonly qrCodeData = input<string>('');
  readonly sharedKey = input<string>('');

  qrCodeImageUrl = '';
  loading = false;
  error = '';

  ngOnInit(): void {
    if (this.qrCodeData()) {
      void this.generateQrCode();
    }
  }

  async generateQrCode() {
    if (!this.qrCodeData()) return;

    this.loading = true;
    this.error = '';
    this.markForCheck();

    try {
      const qrCode = generate(this.qrCodeData());
      this.qrCodeImageUrl = qrCode.toDataURL();
    } catch {
      this.error = 'Failed to generate QR code';
    } finally {
      this.loading = false;
      this.markForCheck();
    }
  }

  async copyToClipboard() {
    if (!this.sharedKey()) return;

    try {
      await navigator.clipboard.writeText(this.sharedKey());
    } catch {
      this.fallbackCopyToClipboard();
    }
  }

  private fallbackCopyToClipboard() {
    const textArea = document.createElement('textarea');
    textArea.value = this.sharedKey();
    textArea.style.position = 'fixed';
    textArea.style.left = '-999999px';
    textArea.style.top = '-999999px';
    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();

    try {
      document.execCommand('copy');
    } catch (error) {
      console.error('Fallback copy failed:', error);
    }

    document.body.removeChild(textArea);
  }
}
