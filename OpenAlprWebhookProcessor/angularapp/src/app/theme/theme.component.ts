import { Component } from '@angular/core';
import { ThemeService } from './theme.service';

@Component({
  selector: 'app-root',
  template: `
    <div [class.mat-light-theme]="!themeService.isDarkTheme" [class.mat-dark-theme]="themeService.isDarkTheme">
      <h1>{{ title }}</h1>
      <button (click)="toggleTheme()">Toggle Theme</button>
    </div>
  `,
  standalone: true,
  styles: [``],
})
export class AppComponent {
  title = 'My Angular App';

  constructor(public themeService: ThemeService) {}

  toggleTheme() {
    this.themeService.toggleTheme();
  }
}
