import { animate, style, transition, trigger } from '@angular/animations';
import { Component, OnInit } from '@angular/core';
import { MatSlideToggleChange } from '@angular/material/slide-toggle';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { Enricher } from './enricher';
import { EnrichersService } from './enrichers.service';

@Component({
  selector: 'app-enrichers',
  templateUrl: './enrichers.component.html',
  styleUrls: ['./enrichers.component.less'],
  animations: [
    trigger(
      'inOutAnimation', 
      [
        transition(
          ':enter', [
            style({ height: 0, opacity: 0 }),
            animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)', 
                    style({ height: '*', opacity: 1 }))]),
        transition(
          ':leave', [
            style({ height: '*', opacity: 1 }),
            animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)', 
                    style({ height: 0, opacity: 0 }))])
      ])]
})
export class EnrichersComponent implements OnInit {
  public isTesting: boolean;
  public isSaving: boolean;

  public enricher: Enricher;
  
  constructor(
    private enricherService: EnrichersService,
    private snackbarService: SnackbarService) { }

  ngOnInit(): void {
    this.getEnricher();
  }

  private getEnricher() {
    this.enricherService.getEnricher().subscribe(result => {
      this.enricher = result;
    })
  }

  public testEnricher() {
    this.isTesting = true;

    this.enricherService.testEnricher(this.enricher.id).subscribe(result => {
      this.isTesting = false;

      if (result) {
        this.snackbarService.create("Enricher test succeeded.", SnackBarType.Saved);
      }
      else {
        this.snackbarService.create("Enricher test failed, check the logs.", SnackBarType.Error);
      }
    }, _ => {
      this.isSaving = false;
      this.snackbarService.create("Enricher test failed, check the logs.", SnackBarType.Error);
    });
  }

  public saveEnricher() {
    this.isSaving = true;

    this.enricherService.upsertEnricher(this.enricher).subscribe(_ => {
      this.isSaving = false;
      this.snackbarService.create("Enricher client saved.", SnackBarType.Successful);
      this.getEnricher();
    },
    _ => {
      this.isSaving = false;
      this.snackbarService.create("Enricher client test failed, check the logs.", SnackBarType.Error);
    });
  }

  public onEnricherToggle(event: MatSlideToggleChange) {
    if (!event.checked) {
      this.enricher.isEnabled = event.checked;
      this.isSaving = true;
      this.enricherService.upsertEnricher(this.enricher).subscribe(_ => {
        this.isSaving = false;
      });
    }
  }
}