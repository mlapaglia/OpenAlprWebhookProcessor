import { ChangeDetectionStrategy, Component, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

@Component({
  selector: 'app-add-camera',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './add-camera.component.html',
  styleUrls: ['./add-camera.component.less'],
  imports: [MatCardModule, MatIconModule, MatButtonModule],
})
export class AddCameraComponent extends OnPushBaseComponent {
  readonly add = output<void>();

  public addCamera() {
    this.add.emit();
    this.markForCheck();
  }

  public onKeyDown(event: KeyboardEvent) {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      this.addCamera();
    }
  }
}
