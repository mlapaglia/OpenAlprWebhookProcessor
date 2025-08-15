import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable, inject, type OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { SwPush } from '@angular/service-worker';
import { firstValueFrom, Subscription } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class PushSubscriberService implements OnDestroy {
  private readonly swPush = inject(SwPush);
  private readonly httpClient = inject(HttpClient);
  private readonly router = inject(Router);
  private _subscription: PushSubscription | null = null;
  private readonly baseUrl: string = document.getElementsByTagName('base')[0].href;

  private readonly subscriptions = new Subscription();

  readonly httpOptions = {
    headers: new HttpHeaders({
      'Content-Type': 'application/json',
    }),
  };

  constructor() {
    const { swPush } = this;

    this.subscriptions.add(
      swPush.subscription.subscribe((subscription) => {
        if (subscription) {
          this._subscription = subscription;
        }
      }),
    );

    this.subscriptions.add(
      swPush.notificationClicks.subscribe((clicked) => {
        const { url } = clicked.notification.data;
        if (url && typeof url === 'string') {
          void this.router.navigate([url]);
        }
      }),
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  public async subscribe() {
    try {
      const publicKey = await firstValueFrom(
        this.httpClient.get(`${this.baseUrl}api/WebPushPublicKey`, { responseType: 'text' }),
      );
      if (!publicKey.trim()) return;

      const subscription = await this.swPush.requestSubscription({
        serverPublicKey: publicKey,
      });

      await firstValueFrom(
        this.httpClient.post(`${this.baseUrl}api/WebPushSubscriptions`, subscription, this.httpOptions),
      );
    } catch {
      this.resetSubscription();
    }
  }

  public unsubscribe() {
    if (!this._subscription) return;

    this.swPush.unsubscribe()
      .then(() => {
        const subscriptionToDelete = this._subscription;
        if (subscriptionToDelete) {
          this.httpClient.delete(
            `${this.baseUrl}api/WebPushSubscriptions/${encodeURIComponent(subscriptionToDelete.endpoint)}`,
          ).subscribe({
            next: () => { /* do nothing */ },
            error: () => { /* do nothing */ },
          });
        }
      })
      .catch(() => { /* do nothing */ });
  }

  public resetSubscription() {
    void navigator.serviceWorker.ready.then((registration) => {
      void registration.pushManager.getSubscription()
        .then((pushSubscription) => {
          if (pushSubscription) {
            pushSubscription.unsubscribe().then(() => {
              void this.subscribe();
            }).catch(() => { /* do nothing */ });
          }
        });
    });
  }
}
