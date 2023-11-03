import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class NavBarService {
    public settingsButtonClicked: BehaviorSubject<boolean> = new BehaviorSubject(false);

    public settingsClicked() {
        this.settingsButtonClicked.next(true);
    }
}