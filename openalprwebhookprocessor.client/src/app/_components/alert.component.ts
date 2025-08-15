import { Component, inject, type OnInit, type OnDestroy, ChangeDetectionStrategy, input } from '@angular/core';
import { Router, NavigationStart } from '@angular/router';
import { type Alert, AlertType } from 'app/_models';
import { AlertService } from 'app/_services';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

interface AlertWithCssClass extends Alert {
  cssClass?: string;
}

@Component({
  selector: 'app-alert',
  templateUrl: 'alert.component.html',
  styleUrls: ['alert.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AlertComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  private readonly router = inject(Router);
  private readonly alertService = inject(AlertService);

  readonly id = input<string>('default-alert');
  readonly fade = input<boolean>(true);
  alerts: AlertWithCssClass[] = [];

  ngOnInit() {
    this.subscribeAndMarkForCheck(
      this.alertService.onAlert(this.id()),
      (alert) => {
        if (!alert.message) {
          this.alerts = this.alerts.filter(x => x.keepAfterRouteChange);

          this.alerts.forEach(x => delete x.keepAfterRouteChange);
          return;
        }

        const alertWithClass = { ...alert, cssClass: this.cssClass(alert) };
        this.alerts.push(alertWithClass);

        if (alert.autoClose) {
          this.setTimeoutWithCheck(() => this.removeAlert(alert), 3000);
        }
      },
    );

    this.subscribeAndMarkForCheck(
      this.router.events,
      (event) => {
        if (event instanceof NavigationStart) {
          this.alertService.clear(this.id());
        }
      },
    );
  }

  override ngOnDestroy() {
    super.ngOnDestroy();
  }

  removeAlert(alert: Alert) {
    if (!this.alerts.includes(alert)) return;

    if (this.fade()) {
      alert.fade = true;
      (alert as AlertWithCssClass).cssClass = this.cssClass(alert);

      this.setTimeoutWithCheck(() => {
        this.alerts = this.alerts.filter(x => x !== alert);
      }, 250);
    } else {
      this.alerts = this.alerts.filter(x => x !== alert);
      this.markForCheck();
    }
  }

  cssClass(alert: Alert | null | undefined) {
    if (!alert) return;

    const classes = ['alert', 'alert-dismissable', 'mt-4', 'container'];

    const alertTypeClass = {
      [AlertType.Success]: 'alert alert-success',
      [AlertType.Error]: 'alert alert-danger',
      [AlertType.Info]: 'alert alert-info',
      [AlertType.Warning]: 'alert alert-warning',
    };

    classes.push(alertTypeClass[alert.type]);

    if (alert.fade) {
      classes.push('fade');
    }

    return classes.join(' ');
  }
}
