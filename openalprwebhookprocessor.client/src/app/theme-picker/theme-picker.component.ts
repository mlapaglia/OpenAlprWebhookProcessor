import { ChangeDetectionStrategy, Component, ViewEncapsulation, inject, type OnDestroy, type OnInit } from '@angular/core';
import { ThemeStorage, type DocsSiteTheme } from './theme-storage/theme-storage';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule, MatIconRegistry } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute, type ParamMap } from '@angular/router';
import { Subscription } from 'rxjs';
import { map } from 'rxjs/operators';
import { DomSanitizer } from '@angular/platform-browser';
import { LiveAnnouncer } from '@angular/cdk/a11y';
import { StyleManager } from './style-manager/style-manager.component';
import { MatRadioModule } from '@angular/material/radio';
import { MatListModule } from '@angular/material/list';

@Component({
  selector: 'app-theme-picker',
  templateUrl: './theme-picker.component.html',
  styleUrls: ['./theme-picker.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  imports: [MatButtonModule, MatTooltipModule, MatMenuModule, MatIconModule, MatRadioModule, MatListModule],
})
export class ThemePickerComponent implements OnInit, OnDestroy {
  styleManager = inject(StyleManager);
  private readonly _themeStorage = inject(ThemeStorage);
  private readonly _activatedRoute = inject(ActivatedRoute);
  private readonly liveAnnouncer = inject(LiveAnnouncer);

  private _queryParamSubscription = Subscription.EMPTY;
  currentTheme: DocsSiteTheme | undefined;

  // The below colors need to align with the themes defined in theme-picker.scss
  themes: DocsSiteTheme[] = [
    {
      primary: '#673AB7',
      accent: '#FFC107',
      displayName: 'Deep Purple & Amber',
      name: 'deeppurple-amber',
      isDark: false,
    },
    {
      primary: '#3F51B5',
      accent: '#E91E63',
      displayName: 'Indigo & Pink',
      name: 'indigo-pink',
      isDark: false,
      isDefault: true,
    },
    {
      primary: '#E91E63',
      accent: '#607D8B',
      displayName: 'Pink & Blue-grey',
      name: 'pink-bluegrey',
      isDark: true,
    },
    {
      primary: '#9C27B0',
      accent: '#4CAF50',
      displayName: 'Purple & Green',
      name: 'purple-green',
      isDark: true,
    },
  ];

  constructor() {
    const iconRegistry = inject(MatIconRegistry);
    const sanitizer = inject(DomSanitizer);

    iconRegistry.addSvgIcon('theme-example',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/img/theme-demo-icon.svg'));

    const themeName = this._themeStorage.getStoredThemeName();
    if (themeName) {
      this.selectTheme(themeName);
    } else {
      const defaultTheme = this.themes.find(theme => theme.isDefault);
      if (defaultTheme) {
        this.selectTheme(defaultTheme.name);
      }
    }
  }

  ngOnInit() {
    this._queryParamSubscription = this._activatedRoute.queryParamMap
      .pipe(map((params: ParamMap) => params.get('theme')))
      .subscribe((themeName: string | null) => {
        if (themeName) {
          this.selectTheme(themeName);
        }
      });
  }

  ngOnDestroy() {
    this._queryParamSubscription.unsubscribe();
  }

  selectTheme(themeName: string) {
    const theme = this.themes.find(currentTheme => currentTheme.name === themeName);

    if (!theme) {
      return;
    }

    this.currentTheme = theme;

    if (theme.isDefault) {
      this.styleManager.removeStyle('theme');
    } else {
      this.styleManager.setStyle('theme', `${theme.name}.css`);
    }

    void this.liveAnnouncer.announce(`${theme.displayName} theme selected.`, 'polite', 3000);
    this._themeStorage.storeTheme(this.currentTheme);
  }
}
