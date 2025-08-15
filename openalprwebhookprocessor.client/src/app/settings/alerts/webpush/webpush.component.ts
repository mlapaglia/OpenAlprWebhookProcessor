import { animate, style, transition, trigger } from '@angular/animations';
import { ChangeDetectionStrategy, Component, inject, type OnDestroy, type OnInit } from '@angular/core';
import { MatSlideToggleModule, type MatSlideToggleChange } from '@angular/material/slide-toggle';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import type { Webpush } from './webpush';
import { WebpushService } from './webpush.service';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';

import { MatCheckboxModule } from '@angular/material/checkbox';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';
import { RefreshButtonComponent } from 'app/shared/refresh-button/refresh-button.component';

@Component({
  selector: 'app-webpush',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './webpush.component.html',
  styleUrls: ['./webpush.component.css'],
  animations: [
    trigger('inOutAnimation', [
      transition(':enter', [
        style({ height: 0, opacity: 0 }),
        animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)', style({ height: '*', opacity: 1 })),
      ]),
      transition(':leave', [
        style({ height: '*', opacity: 1 }),
        animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)', style({ height: 0, opacity: 0 })),
      ]),
    ]),
  ],
  imports: [
    MatCardModule, MatSlideToggleModule, ReactiveFormsModule, FormsModule,
    MatFormFieldModule, MatInputModule, MatIconModule, MatTooltipModule,
    MatButtonModule, MatCheckboxModule,
    RefreshButtonComponent,
  ],
})
export class WebpushComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  private readonly webpushService = inject(WebpushService);
  private readonly snackbarService = inject(SnackbarService);

  public client: Webpush;
  public isSaving: boolean;
  public isTesting: boolean;
  public hidePrivateKey = true;

  ngOnInit(): void {
    this.subscribeAndMarkForCheck(
      this.webpushService.getWebpush(),
      (result) => {
        this.client = result;
      },
    );
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  public saveClient() {
    this.isSaving = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.webpushService.upsertWebpush(this.client),
      () => {
        this.snackbarService.create('WebPush client saved.', SnackBarType.Saved);
        this.isSaving = false;
      },
    );
  }

  public testClient() {
    this.isTesting = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.webpushService.testWebpush(),
      () => {
        this.snackbarService.create('WebPush client test successful.', SnackBarType.Successful);
        this.isTesting = false;
      },
      () => {
        this.snackbarService.create('WebPush client test failed.', SnackBarType.Error);
        this.isTesting = false;
      },
    );
  }

  public onWebPushToggle(event: MatSlideToggleChange) {
    if (!event.checked) {
      this.client.isEnabled = event.checked;
      this.isSaving = true;
      this.markForCheck();

      this.subscribeAndMarkForCheck(
        this.webpushService.upsertWebpush(this.client),
        () => {
          this.isSaving = false;
        },
      );
    }
  }
}
