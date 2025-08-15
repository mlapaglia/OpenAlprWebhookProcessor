import { animate, style, transition, trigger } from '@angular/animations';
import { ChangeDetectionStrategy, Component, inject, type OnDestroy, type OnInit } from '@angular/core';
import { MatSlideToggleModule, type MatSlideToggleChange } from '@angular/material/slide-toggle';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import type { Pushover } from './pushover';
import { PushoverService } from './pushover.service';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';
import { RefreshButtonComponent } from 'app/shared/refresh-button/refresh-button.component';

@Component({
  selector: 'app-pushover',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './pushover.component.html',
  styleUrls: ['./pushover.component.less'],
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
    MatCheckboxModule, MatButtonModule,
    RefreshButtonComponent,
  ],
})
export class PushoverComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  private readonly pushoverService = inject(PushoverService);
  private readonly snackbarService = inject(SnackbarService);

  public client: Pushover;
  public isSaving: boolean;
  public isTesting: boolean;

  ngOnInit(): void {
    this.subscribeAndMarkForCheck(
      this.pushoverService.getPushover(),
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
      this.pushoverService.upsertPushover(this.client),
      () => {
        this.snackbarService.create('Pushover client saved.', SnackBarType.Saved);
        this.isSaving = false;
      },
      (error) => {
        this.snackbarService.create('Pushover client save failed.', SnackBarType.Error, error as string);
        this.isSaving = false;
      },
    );
  }

  public testClient() {
    this.isTesting = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.pushoverService.testPushover(),
      () => {
        this.snackbarService.create('Pushover client test successful.', SnackBarType.Successful);
        this.isTesting = false;
      },
      () => {
        this.snackbarService.create('Pushover client test failed.', SnackBarType.Error);
        this.isTesting = false;
      },
    );
  }

  public onPushoverToggle(event: MatSlideToggleChange) {
    if (!event.checked) {
      this.client.isEnabled = event.checked;
      this.isSaving = true;
      this.markForCheck();

      this.subscribeAndMarkForCheck(
        this.pushoverService.upsertPushover(this.client),
        () => {
          this.isSaving = false;
        },
      );
    }
  }
}
