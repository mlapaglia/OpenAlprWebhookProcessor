import { Component, Input, OnInit } from '@angular/core';
import { Alert } from './alert';

@Component({
  selector: 'app-alert',
  templateUrl: './alert.component.html',
  styleUrls: ['./alert.component.less']
})
export class AlertComponent implements OnInit {
  @Input() alert: Alert;
  
  constructor() { }

  ngOnInit(): void {
  }

  public deleteAlert(alert: Alert) {
  }
}
