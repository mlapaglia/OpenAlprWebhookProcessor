import {Injectable, EventEmitter} from '@angular/core';

export interface DocsSiteTheme {
    name: string;
    displayName?: string;
    accent: string;
    primary: string;
    isDark?: boolean;
    isDefault?: boolean;
}


@Injectable({providedIn: 'root'})
export class ThemeStorage {
    static storageKey = 'docs-theme-storage-current-name';

    onThemeUpdate: EventEmitter<DocsSiteTheme> = new EventEmitter<DocsSiteTheme>();

    storeTheme(theme: DocsSiteTheme) {
        try {
            window.localStorage[ThemeStorage.storageKey] = theme.name;
        } catch {
            undefined; 
        }

        this.onThemeUpdate.emit(theme);
    }

    getStoredThemeName(): string | null {
        try {
            return window.localStorage[ThemeStorage.storageKey] || null;
        } catch {
            return null;
        }
    }

    clearStorage() {
        try {
            window.localStorage.removeItem(ThemeStorage.storageKey);
        } catch {
            undefined; 
        }
    }
}