import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

@Component({
  selector: 'app-camera-image',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './camera-image.component.html',
  styleUrls: ['./camera-image.component.less'],
  imports: [MatProgressSpinnerModule, MatIconModule, MatTooltipModule],
})
export class CameraImageComponent extends OnPushBaseComponent {
  readonly imageDataUrl = input<string | null>(null);
  readonly isLoadingImage = input<boolean>(false);
  readonly isLoadingFailed = input<boolean>(false);
  readonly sampleImageUrl = input<string | undefined>(undefined);
  readonly hasOpenAlprIntegration = input<boolean>(false);

  public get tooltipText(): string {
    return this.hasOpenAlprIntegration()
      ? 'Image will display after first plate capture'
      : 'Unable to get snapshot image from camera';
  }
}
