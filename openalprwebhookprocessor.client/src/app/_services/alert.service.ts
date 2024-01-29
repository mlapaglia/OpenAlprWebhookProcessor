import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { filter } from 'rxjs/operators';
import { Alert, AlertType } from 'app/_models';

@Injectable({ providedIn: 'root' })
export class AlertService {
    private subject = new Subject<Alert>();
    private defaultId = 'default-alert';

    onAlert(id = this.defaultId): Observable<Alert> {
        return this.subject.asObservable().pipe(filter(x => x && x.id === id));
    }

    success(message: string, keepAfterRouteChange?: boolean) {
        this.alert(new Alert({ keepAfterRouteChange: keepAfterRouteChange, type: AlertType.Success, message }));
    }

    error(message: string, keepAfterRouteChange?: boolean) {
        this.alert(new Alert({ keepAfterRouteChange: keepAfterRouteChange, type: AlertType.Error, message }));
    }

    info(message: string, keepAfterRouteChange?: boolean) {
        this.alert(new Alert({ keepAfterRouteChange: keepAfterRouteChange, type: AlertType.Info, message }));
    }

    warn(message: string, keepAfterRouteChange?: boolean) {
        this.alert(new Alert({ keepAfterRouteChange: keepAfterRouteChange, type: AlertType.Warning, message }));
    }

    // main alert method    
    alert(alert: Alert) {
        alert.id = alert.id || this.defaultId;
        this.subject.next(alert);
    }

    // clear alerts
    clear(id = this.defaultId) {
        this.subject.next(new Alert({ id }));
    }
}