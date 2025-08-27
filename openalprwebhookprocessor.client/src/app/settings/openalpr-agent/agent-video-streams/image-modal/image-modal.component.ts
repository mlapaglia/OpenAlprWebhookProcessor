import { ChangeDetectionStrategy, Component, inject, type OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { SettingsService } from '../../../settings.service';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

interface DialogData {
  agentId: string;
  cameraId: number;
  cameraName: string;
}

@Component({
  selector: 'app-image-modal',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './image-modal.component.html',
  styleUrls: ['./image-modal.component.less'],
  imports: [
    CommonModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    MatIconModule,
  ],
})
export class ImageModalComponent extends OnPushBaseComponent implements OnInit {
  data = inject<DialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<ImageModalComponent>);
  private readonly settingsService = inject(SettingsService);

  public isLoading = true;
  public imageUrl: string | null = null;
  public errorMessage: string | null = null;

  ngOnInit(): void {
    this.loadCameraSnapshot();
  }

  private loadCameraSnapshot(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.settingsService.getCameraSnapshot(this.data.agentId, this.data.cameraId),
      (blob: Blob) => {
        this.imageUrl = URL.createObjectURL(blob);
        this.isLoading = false;
      },
      _ => {
        this.errorMessage = 'Failed to load camera snapshot';
        this.isLoading = false;
      },
    );
  }

  public onClose(): void {
    if (this.imageUrl) {
      URL.revokeObjectURL(this.imageUrl);
    }
    this.dialogRef.close();
  }

  public onRetry(): void {
    this.loadCameraSnapshot();
  }
}
