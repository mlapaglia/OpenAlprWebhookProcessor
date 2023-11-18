import { Injectable } from '@angular/core';
import { OverlayContainer } from '@angular/cdk/overlay';

@Injectable({
  providedIn: 'root',
})
export class ThemeService {
  constructor(private overlayContainer: OverlayContainer) {}

  setTheme(theme: string): void {
    this.overlayContainer.getContainerElement().classList.remove("app-theme-dark");
    this.overlayContainer.getContainerElement().classList.add("app-theme-default");


    // Add the selected theme class
    
  }
}