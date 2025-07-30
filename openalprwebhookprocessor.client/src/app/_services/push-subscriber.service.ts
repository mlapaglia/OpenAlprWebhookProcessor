import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { SwPush } from '@angular/service-worker';

@Injectable({ providedIn: 'root' })
export class PushSubscriberService {
  private readonly swPush = inject(SwPush);
  private readonly httpClient = inject(HttpClient);
  private readonly router = inject(Router);

  private _subscription: PushSubscription;
  private readonly baseUrl: string = document.getElementsByTagName('base')[0].href;

  readonly httpOptions = {
    headers: new HttpHeaders({
      'Content-Type': 'application/json',
    }),
  };

  constructor() {
    const { swPush } = this;

    swPush.subscription.subscribe((subscription) => {
      if (subscription) {
        this._subscription = subscription;
      }
    });

    swPush.notificationClicks.subscribe((clicked) => {
      this.router.navigate([clicked.notification.data.url as string]);
    });
  }

  public subscribe() {
    this.httpClient.get(`${this.baseUrl}api/WebPushPublicKey`, { responseType: 'text' }).subscribe((publicKey) => {
      this.swPush.requestSubscription({
        serverPublicKey: publicKey,
      })
        .then(subscription => this.httpClient.post(`${this.baseUrl}api/WebPushSubscriptions`, subscription, this.httpOptions).subscribe(
          () => {
          },
          () => {
          },
        ))
        .catch(() => {
          this.resetSubscription();
        });
    }, _ => {
    });
  }

  public unsubscribe() {
    this.swPush.unsubscribe()
      .then(() => this.httpClient.delete(`${this.baseUrl}api/WebPushSubscriptions/${encodeURIComponent(this._subscription.endpoint)}`).subscribe(
        () => { },
        _ => { },
      ))
      .catch(_ => { });
  }

  public resetSubscription() {
    navigator.serviceWorker.ready.then((registration) => {
      registration.pushManager.getSubscription()
        .then((pushSubscription) => {
          if (pushSubscription) {
            pushSubscription.unsubscribe().then(() => {
              this.subscribe();
            }).catch(() => { });
          }
        });
    });
  }
}
