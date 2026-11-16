import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ImageViewerDialogData {
  src: string;
  caption: string;
}

@Component({
  selector: 'app-image-viewer-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <div class="viewer">
      <button mat-icon-button mat-dialog-close class="close-button"
        aria-label="Close image" i18n-aria-label>
        <mat-icon>close</mat-icon>
      </button>
      <!-- eslint-disable-next-line @angular-eslint/template/prefer-ngsrc -->
      <img [src]="data.src" [alt]="data.caption" mat-dialog-close />
      @if (data.caption) {
        <div class="caption">{{ data.caption }}</div>
      }
    </div>
  `,
  styles: `
    .viewer {
      position: relative;
      display: flex;
      flex-direction: column;
      align-items: center;
      background: #000;
    }
    img {
      display: block;
      max-width: 90vw;
      max-height: 85vh;
      object-fit: contain;
      cursor: zoom-out;
    }
    .caption {
      width: 100%;
      padding: 8px 12px;
      color: #fff;
      text-align: center;
      font-weight: 500;
      box-sizing: border-box;
    }
    .close-button {
      position: absolute;
      top: 4px;
      right: 4px;
      color: #fff;
      background: rgb(0 0 0 / 40%);
      z-index: 1;
    }
  `,
})
export class ImageViewerDialogComponent {
  readonly data = inject<ImageViewerDialogData>(MAT_DIALOG_DATA);
}
