import { Component, OnInit, ViewChild } from '@angular/core';
import { MatSidenav } from '@angular/material/sidenav';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css']
})
export class SettingsComponent implements OnInit {
  visibleSetting: string;

  constructor() { }

  ngOnInit(): void {
  }

  public openSettings(settingName: string) {
    this.visibleSetting = settingName;
  }
}
