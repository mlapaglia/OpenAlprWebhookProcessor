import { Injectable, signal } from '@angular/core';

export type PlateTab = 'overview' | 'images' | 'stats' | 'notes';

@Injectable()
export class PlateTabStateService {
  private readonly _activeTab = signal<PlateTab>('overview');
  private readonly _imagesLoaded = signal(false);
  private readonly _statisticsLoaded = signal(false);
  private readonly _notesLoaded = signal(false);

  readonly activeTab = this._activeTab.asReadonly();
  readonly imagesLoaded = this._imagesLoaded.asReadonly();
  readonly statisticsLoaded = this._statisticsLoaded.asReadonly();
  readonly notesLoaded = this._notesLoaded.asReadonly();

  setActiveTab(tab: PlateTab) {
    this._activeTab.set(tab);

    switch (tab) {
      case 'images':
        if (!this._imagesLoaded()) {
          this._imagesLoaded.set(true);
        }
        break;
      case 'stats':
        if (!this._statisticsLoaded()) {
          this._statisticsLoaded.set(true);
        }
        break;
      case 'notes':
        if (!this._notesLoaded()) {
          this._notesLoaded.set(true);
        }
        break;
    }
  }

  get shouldShowImages() {
    return this._imagesLoaded();
  }

  get shouldShowStatistics() {
    return this._statisticsLoaded();
  }

  get shouldShowNotes() {
    return this._notesLoaded();
  }

  reset() {
    this._activeTab.set('overview');
    this._imagesLoaded.set(false);
    this._statisticsLoaded.set(false);
    this._notesLoaded.set(false);
  }
}
