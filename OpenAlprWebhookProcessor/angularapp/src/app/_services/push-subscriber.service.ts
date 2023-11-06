import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { SwPush } from '@angular/service-worker';

@Injectable({ providedIn: 'root' })
export class PushSubscriberService {
    private _subscription: PushSubscription;
    private baseUrl: string = document.getElementsByTagName('base')[0].href;

    readonly httpOptions = {
        headers: new HttpHeaders({
          'Content-Type': 'application/json'
        })
      };
      
    constructor(
        private swPush: SwPush,
        private httpClient: HttpClient) {
            swPush.subscription.subscribe((subscription) => {
                this._subscription = subscription!;
              });
    }

        public subscribe() {
            this.httpClient.get(this.baseUrl + 'api/WebPushPublicKey', { responseType: 'text' }).subscribe(publicKey => {
              this.swPush.requestSubscription({
                serverPublicKey: publicKey
              })
                .then(subscription => this.httpClient.post(this.baseUrl + 'api/WebPushSubscriptions', subscription, this.httpOptions).subscribe(
                  success => {
                    console.log("sent subscription to server.");
                },
                  error => console.error(error)
                ))
                .catch(error => {
                    this.resetSubscription();
                });
            }, error => console.error(error));
          };

        public unsubscribe() {
            this.swPush.unsubscribe()
                .then(() => this.httpClient.delete(this.baseUrl + 'api/WebPushSubscriptions/' + encodeURIComponent(this._subscription.endpoint)).subscribe(
                () => { },
                error => console.error(error)
                ))
                .catch(error => console.error(error));
        }

        public resetSubscription() {
            navigator.serviceWorker.ready.then(registration => {
                registration.pushManager.getSubscription()
                .then(pushSubscription => {
                    if(pushSubscription){
                        pushSubscription.unsubscribe().then(successful => {
                            // You've successfully unsubscribed
                            this.subscribe();
                        }).catch(e => {
                            // Unsubscription failed
                        })
                    }
                });
        });
    }
}