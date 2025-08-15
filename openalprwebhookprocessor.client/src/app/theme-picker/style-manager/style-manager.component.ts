import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class StyleManager {

  async setStyle(key: string, href: string): Promise<void> {
    const linkEl = await this.getLinkElementForKey(key);
    linkEl.href = href;

    return new Promise((resolve, reject) => {
      linkEl.onload = () => resolve();
      linkEl.onerror = () => reject(new Error(`Failed to load stylesheet: ${href}`));

      if (linkEl.href === href) {
        resolve();
      }
    });
  }

  removeStyle(key: string) {
    const existingLinkElement = this.getExistingLinkElementByKey(key);
    if (existingLinkElement) {
      document.head.removeChild(existingLinkElement);
    }
  }

  private getLinkElementForKey(key: string): Promise<HTMLLinkElement> {
    return new Promise((resolve) => {
      const existingLinkElement = this.getExistingLinkElementByKey(key);

      if (existingLinkElement) {
        resolve(existingLinkElement);
      } else {
        const linkEl = document.createElement('link');
        linkEl.type = 'text/css';
        linkEl.rel = 'stylesheet';
        linkEl.setAttribute('data-style-key', key);
        document.head.appendChild(linkEl);
        resolve(linkEl);
      }
    });
  }

  private getExistingLinkElementByKey(key: string): HTMLLinkElement | null {
    return document.head.querySelector(
      `link[rel="stylesheet"][data-style-key="${key}"]`,
    );
  }
}
