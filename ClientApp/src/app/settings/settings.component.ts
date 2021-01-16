import { Component, OnInit, ViewChild } from '@angular/core';
import { MatSidenav } from '@angular/material/sidenav';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css']
})
export class SettingsComponent implements OnInit {
  @ViewChild('sidenav') sidenav: MatSidenav;
  isExpanded = true;
  isShowing = true;
  showSubmenu = true;
  visibleSetting: string;

  constructor() { }

  ngOnInit(): void {
  }

  mouseenter() {
    if (!this.isExpanded) {
      this.isShowing = true;
    }
  }

  mouseleave() {
    if (!this.isExpanded) {
      this.isShowing = false;
    }
  }

  public openSettings(settingName: string) {
    this.visibleSetting = settingName;
  }
}
