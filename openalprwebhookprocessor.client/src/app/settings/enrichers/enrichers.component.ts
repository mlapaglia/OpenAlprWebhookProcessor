import { animate, style, transition, trigger } from '@angular/animations';
import { Component, OnInit } from '@angular/core';
import { MatSlideToggleChange, MatSlideToggleModule } from '@angular/material/slide-toggle';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { Enricher } from './enricher';
import { EnrichersService } from './enrichers.service';
import { MatButtonModule } from '@angular/material/button';
import { MatRadioModule } from '@angular/material/radio';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { NgIf } from '@angular/common';

@Component({
    selector: 'app-enrichers',
    templateUrl: './enrichers.component.html',
    styleUrls: ['./enrichers.component.less'],
    animations: [
        trigger('inOutAnimation', [
            transition(':enter', [
                style({ height: 0, opacity: 0 }),
                animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)', style({ height: '*', opacity: 1 }))
            ]),
            transition(':leave', [
                style({ height: '*', opacity: 1 }),
                animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)', style({ height: 0, opacity: 0 }))
            ])
        ])
    ],
    standalone: true,
    imports: [NgIf, MatCardModule, MatSlideToggleModule, ReactiveFormsModule, FormsModule, MatFormFieldModule, MatInputModule, MatIconModule, MatTooltipModule, MatRadioModule, MatButtonModule]
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
        });
    }

    public testEnricher() {
        this.isTesting = true;

        this.enricherService.testEnricher(this.enricher.id).subscribe(result => {
            this.isTesting = false;

            if (result) {
                this.snackbarService.create('Enricher test succeeded.', SnackBarType.Saved);
            } else {
                this.snackbarService.create('Enricher test failed, check the logs.', SnackBarType.Error);
            }
        }, () => {
            this.isSaving = false;
            this.snackbarService.create('Enricher test failed, check the logs.', SnackBarType.Error);
        });
    }

    public saveEnricher() {
        this.isSaving = true;

        this.enricherService.upsertEnricher(this.enricher).subscribe(() => {
            this.isSaving = false;
            this.snackbarService.create('Enricher client saved.', SnackBarType.Successful);
            this.getEnricher();
        },
        () => {
            this.isSaving = false;
            this.snackbarService.create('Enricher client test failed, check the logs.', SnackBarType.Error);
        });
    }

    public onEnricherToggle(event: MatSlideToggleChange) {
        if (!event.checked) {
            this.enricher.isEnabled = event.checked;
            this.isSaving = true;
            this.enricherService.upsertEnricher(this.enricher).subscribe(() => {
                this.isSaving = false;
            });
        }
    }
}