import { ChangeDetectionStrategy, Component, inject, type OnDestroy, type OnInit } from '@angular/core';
import { ThemeStorage, type DocsSiteTheme } from './theme-storage/theme-storage';
import { StyleManager } from './style-manager/style-manager.component';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule, MatIconRegistry } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute, type ParamMap } from '@angular/router';

import { map } from 'rxjs/operators';
import { DomSanitizer } from '@angular/platform-browser';
import { LiveAnnouncer } from '@angular/cdk/a11y';
import { MatRadioModule } from '@angular/material/radio';
import { MatListModule } from '@angular/material/list';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

@Component({
  selector: 'app-theme-picker',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatButtonModule, MatTooltipModule, MatMenuModule, MatIconModule, MatRadioModule, MatListModule],
  templateUrl: './theme-picker.component.html',
  styleUrls: ['./theme-picker.component.scss'],
})
export class ThemePickerComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  private readonly _themeStorage = inject(ThemeStorage);
  private readonly _styleManager = inject(StyleManager);
  private readonly _activatedRoute = inject(ActivatedRoute);
  private readonly liveAnnouncer = inject(LiveAnnouncer);

  currentTheme: DocsSiteTheme | undefined;

  themes: DocsSiteTheme[] = [
    {
      primary: '#3F51B5',
      accent: '#E91E63',
      displayName: 'Light',
      name: 'indigo-pink',
    },
    {
      primary: '#E91E63',
      accent: '#607D8B',
      displayName: 'Dark',
      name: 'pink-bluegrey',
    },
  ];

  constructor() {
    super();
    const iconRegistry = inject(MatIconRegistry);
    const sanitizer = inject(DomSanitizer);
    iconRegistry.addSvgIcon('theme-example',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/img/theme-demo-icon.svg'));
    this.initializeTheme();
  }

  ngOnInit() {
    this.subscribeAndMarkForCheck(
      this._activatedRoute.queryParamMap.pipe(map((params: ParamMap) => params.get('theme'))),
      (themeName: string | null) => {
        if (themeName) {
          this.selectTheme(themeName);
        }
      },
    );
  }

  override ngOnDestroy() {
    super.ngOnDestroy();
  }

  private initializeTheme() {
    const storedThemeName = this._themeStorage.getStoredThemeName();
    const themeName = storedThemeName ?? 'indigo-pink'; // Default to indigo-pink if no theme is stored

    this.selectTheme(themeName);
  }

  selectTheme(themeName: string) {
    const theme = this.themes.find(currentTheme => currentTheme.name === themeName);
    if (!theme) {
      return;
    }

    this.currentTheme = theme;
    this.markForCheck();

    // Always load the theme CSS file
    const themeUrl = `assets/themes/${theme.name}.css`;
    void this._styleManager.setStyle('theme', themeUrl);

    void this.liveAnnouncer.announce(`${theme.displayName} theme selected.`, 'polite', 3000);

    this._themeStorage.storeTheme(this.currentTheme);
  }
}
