import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';

@Component({
  selector: 'app-recovery-codes-display',
  templateUrl: 'recovery-codes-display.component.html',
  styleUrl: 'recovery-codes-display.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatListModule
],
})
export class RecoveryCodesDisplayComponent {
  readonly recoveryCodes = input<string[]>([]);
  readonly canGenerate = input<boolean>(false);
  readonly generateNewCodes = output<void>();

  onGenerateNewCodes() {
    this.generateNewCodes.emit();
  }

  downloadCodes() {
    const content = this.recoveryCodes().join('\n');
    const blob = new Blob([content], { type: 'text/plain' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = 'recovery-codes.txt';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }

  printCodes() {
    const printContent = `
      <h2>Two-Factor Authentication Recovery Codes</h2>
      <p>Keep these codes safe. Each code can only be used once.</p>
      <ul>
        ${this.recoveryCodes().map(code => `<li>${code}</li>`).join('')}
      </ul>
    `;

    const printWindow = window.open('', '_blank');
    if (printWindow) {
      printWindow.document.write(`
        <html>
          <head><title>Recovery Codes</title></head>
          <body>${printContent}</body>
        </html>
      `);
      printWindow.document.close();
      printWindow.print();
    }
  }
}
