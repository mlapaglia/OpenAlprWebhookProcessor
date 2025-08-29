import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
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
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatListModule,
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
    const printWindow = window.open('', '_blank');
    if (printWindow?.document) {
      const doc = printWindow.document;

      const html = doc.createElement('html');
      const head = doc.createElement('head');
      const title = doc.createElement('title');
      title.textContent = 'Recovery Codes';
      head.appendChild(title);

      const body = doc.createElement('body');
      const h2 = doc.createElement('h2');
      h2.textContent = 'Two-Factor Authentication Recovery Codes';
      const p = doc.createElement('p');
      p.textContent = 'Keep these codes safe. Each code can only be used once.';
      const ul = doc.createElement('ul');

      this.recoveryCodes().forEach(code => {
        const li = doc.createElement('li');
        li.textContent = code;
        ul.appendChild(li);
      });

      body.appendChild(h2);
      body.appendChild(p);
      body.appendChild(ul);
      html.appendChild(head);
      html.appendChild(body);

      doc.appendChild(html);
      printWindow.print();
    }
  }
}
