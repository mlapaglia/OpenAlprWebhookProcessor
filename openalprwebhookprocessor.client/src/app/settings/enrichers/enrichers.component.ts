import { ChangeDetectionStrategy, Component, inject, type OnDestroy, type OnInit } from '@angular/core';
import { type MatSlideToggleChange, MatSlideToggleModule } from '@angular/material/slide-toggle';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import type { Enricher } from './enricher';
import { EnrichersService } from './enrichers.service';
import { MatButtonModule } from '@angular/material/button';
import { MatRadioModule } from '@angular/material/radio';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

@Component({
  selector: 'app-enrichers',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './enrichers.component.html',
  styleUrls: ['./enrichers.component.less'],
  imports: [
    MatCardModule, MatSlideToggleModule, ReactiveFormsModule, FormsModule, MatFormFieldModule,
    MatInputModule, MatIconModule, MatTooltipModule, MatRadioModule, MatButtonModule,
  ],
})
export class EnrichersComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  private readonly enricherService = inject(EnrichersService);
  private readonly snackbarService = inject(SnackbarService);

  public isTesting: boolean;
  public isSaving: boolean;

  public enricher: Enricher;

  ngOnInit(): void {
    this.getEnricher();
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  private getEnricher() {
    this.subscribeAndMarkForCheck(
      this.enricherService.getEnricher(),
      (result) => {
        this.enricher = result;
      },
      () => {
        // Error loading enricher configuration - component will handle undefined state
      });
  }

  public testEnricher() {
    this.isTesting = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.enricherService.testEnricher(this.enricher.id),
      (result) => {
        this.isTesting = false;

        if (result) {
          this.snackbarService.create('Enricher test succeeded.', SnackBarType.Saved);
        } else {
          this.snackbarService.create('Enricher test failed, check the logs.', SnackBarType.Error);
        }
      }, () => {
        this.isTesting = false;
        this.snackbarService.create('Enricher test failed, check the logs.', SnackBarType.Error);
      });
  }

  public saveEnricher() {
    this.isSaving = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.enricherService.upsertEnricher(this.enricher),
      () => {
        this.isSaving = false;
        this.snackbarService.create('Enricher client saved.', SnackBarType.Successful);
        this.getEnricher();
      },
      () => {
        this.isSaving = false;
        this.snackbarService.create('Enricher client save failed, check the logs.', SnackBarType.Error);
      });
  }

  public onEnricherToggle(event: MatSlideToggleChange) {
    if (!event.checked) {
      this.enricher.isEnabled = event.checked;
      this.isSaving = true;
      this.markForCheck();

      this.subscribeAndMarkForCheck(
        this.enricherService.upsertEnricher(this.enricher),
        () => {
          this.isSaving = false;
        },
        () => {
          this.isSaving = false;
        });
    }
  }
}
