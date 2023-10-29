import { animate, style, transition, trigger } from '@angular/animations';
import { Component, OnInit } from '@angular/core';
import { MatSlideToggleChange } from '@angular/material/slide-toggle';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { Pushover } from './pushover';
import { PushoverService } from './pushover.service';

@Component({
  selector: 'app-pushover',
  templateUrl: './pushover.component.html',
  styleUrls: ['./pushover.component.less'],
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
export class PushoverComponent implements OnInit {
  public client: Pushover;
  public isSaving: boolean;
  public isTesting: boolean;

  constructor(
    private pushoverService: PushoverService,
    private snackbarService: SnackbarService) { }

  ngOnInit(): void {
    this.pushoverService.getPushover().subscribe(result => {
      this.client = result;
    })
  }

  public saveClient() {
    this.isSaving = true;
    this.pushoverService.upsertPushover(this.client).subscribe(_ => {
      this.snackbarService.create("Pushover client saved.", SnackBarType.Saved);
      this.isSaving = false;
    });
  }

  public testClient() {
    this.isTesting = true;
    this.pushoverService.testPushover().subscribe(_ => {
      this.snackbarService.create("Pushover client test successful.", SnackBarType.Successful);
      this.isTesting = false;
    },
    _ => {
      this.snackbarService.create("Pushover client test failed.", SnackBarType.Error);
      this.isTesting = false;
    });
  }

  public onPushoverToggle(event: MatSlideToggleChange) {
    if (!event.checked) {
      this.client.isEnabled = event.checked;
      this.isSaving = true;
      this.pushoverService.upsertPushover(this.client).subscribe(_ => {
        this.isSaving = false;
      });
    }
  }
}
