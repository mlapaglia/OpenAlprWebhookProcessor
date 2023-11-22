import { animate, style, transition, trigger } from '@angular/animations';
import { Component, OnInit } from '@angular/core';
import { MatSlideToggleChange, MatSlideToggleModule } from '@angular/material/slide-toggle';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { Webpush } from './webpush';
import { WebpushService } from './webpush.service';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { NgIf } from '@angular/common';
import { MatCheckboxModule } from '@angular/material/checkbox';

@Component({
    selector: 'app-webpush',
    templateUrl: './webpush.component.html',
    styleUrls: ['./webpush.component.css'],
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
    imports: [NgIf, MatCardModule, MatSlideToggleModule, ReactiveFormsModule, FormsModule, MatFormFieldModule, MatInputModule, MatIconModule, MatTooltipModule, MatButtonModule, MatCheckboxModule]
})
export class WebpushComponent implements OnInit {
  public client: Webpush;
  public isSaving: boolean;
  public isTesting: boolean;
  public hidePrivateKey: boolean = true;

  constructor(
    private webpushService: WebpushService,
    private snackbarService: SnackbarService) { }

  ngOnInit(): void {
    this.webpushService.getWebpush().subscribe(result => {
      this.client = result;
    })
  }

  public saveClient() {
    this.isSaving = true;
    this.webpushService.upsertWebpush(this.client).subscribe(_ => {
      this.snackbarService.create("WebPush client saved.", SnackBarType.Saved);
      this.isSaving = false;
    });
  }

  public testClient() {
    this.isTesting = true;
    this.webpushService.testWebpush().subscribe(_ => {
      this.snackbarService.create("WebPush client test successful.", SnackBarType.Successful);
      this.isTesting = false;
    },
      _ => {
        this.snackbarService.create("WebPush client test failed.", SnackBarType.Error);
        this.isTesting = false;
      });
  }

  public onWebPushToggle(event: MatSlideToggleChange) {
    if (!event.checked) {
      this.client.isEnabled = event.checked;
      this.isSaving = true;
      this.webpushService.upsertWebpush(this.client).subscribe(_ => {
        this.isSaving = false;
      });
    }
  }
}
