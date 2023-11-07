import { Component, OnInit, ViewChild } from '@angular/core';
import { MatSidenav } from '@angular/material/sidenav';
import { NavBarService } from 'app/_services/nav-bar.service';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css']
})
export class SettingsComponent implements OnInit {
  visibleSetting: string;
  navBarVisible: boolean = false;

  constructor(private navBarService: NavBarService) { }

  ngOnInit(): void {
    this.navBarService.settingsButtonClicked.subscribe(() => {
      this.navBarVisible = !this.navBarVisible;
    })
  }

  public openSettings(settingName: string) {
    this.visibleSetting = settingName;
    this.navBarVisible = !this.navBarVisible;
  }
}
