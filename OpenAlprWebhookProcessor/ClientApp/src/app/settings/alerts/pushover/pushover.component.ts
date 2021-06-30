import { animate, style, transition, trigger } from '@angular/animations';
import { Component, OnInit } from '@angular/core';

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
  public isEnabled: string;
  public userKey: string;
  public apiToken: string;
  public sendPlatePreviewEnabled: boolean;

  constructor() { }

  ngOnInit(): void {
  }

}
