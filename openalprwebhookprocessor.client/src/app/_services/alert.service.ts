import { Injectable } from '@angular/core';
import { Subject, type Observable } from 'rxjs';
import { filter } from 'rxjs/operators';
import { Alert, AlertType } from 'app/_models';

@Injectable({ providedIn: 'root' })
export class AlertService {
  private readonly subject = new Subject<Alert>();
  private readonly defaultId = 'default-alert';

  onAlert(id = this.defaultId): Observable<Alert> {
    return this.subject.asObservable().pipe(filter(x => x.id === id));
  }

  success(message: string, keepAfterRouteChange?: boolean) {
    this.alert(new Alert({ keepAfterRouteChange, type: AlertType.Success, message }));
  }

  error(message: string, keepAfterRouteChange?: boolean) {
    this.alert(new Alert({ keepAfterRouteChange, type: AlertType.Error, message }));
  }

  info(message: string, keepAfterRouteChange?: boolean) {
    this.alert(new Alert({ keepAfterRouteChange, type: AlertType.Info, message }));
  }

  warn(message: string, keepAfterRouteChange?: boolean) {
    this.alert(new Alert({ keepAfterRouteChange, type: AlertType.Warning, message }));
  }

  // main alert method
  alert(alert: Alert) {
    alert.id = (alert.id as string | undefined) ?? this.defaultId;
    this.subject.next(alert);
  }

  // clear alerts
  clear(id = this.defaultId) {
    this.subject.next(new Alert({ id }));
  }
}
