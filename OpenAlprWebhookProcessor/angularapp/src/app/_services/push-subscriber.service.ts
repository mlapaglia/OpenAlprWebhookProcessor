import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { SwPush } from '@angular/service-worker';

@Injectable({ providedIn: 'root' })
export class PushSubscriberService {
    private _subscription: PushSubscription;
    private baseUrl: string = document.getElementsByTagName('base')[0].href;
    public operationName: string;

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
                this.operationName = (this._subscription === null) ? 'Subscribe' : 'Unsubscribe';
                console.log(subscription);
              });
        }

        public subscribe() {
            

            this.httpClient.get(this.baseUrl + 'api/PublicKey', { responseType: 'text' }).subscribe(publicKey => {
              this.swPush.requestSubscription({
                serverPublicKey: publicKey
              })
                .then(subscription => this.httpClient.post(this.baseUrl + 'api/PushSubscriptions', subscription, this.httpOptions).subscribe(
                  success => {
                    console.log(success);
                },
                  error => console.error(error)
                ))
                .catch(error => console.error(error));
            }, error => console.error(error));
          };

        public unsubscribe() {
            this.swPush.unsubscribe()
                .then(() => this.httpClient.delete(this.baseUrl + 'api/PushSubscriptions/' + encodeURIComponent(this._subscription.endpoint)).subscribe(
                () => { },
                error => console.error(error)
                ))
                .catch(error => console.error(error));
        }
}