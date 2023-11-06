export class Webpush {
  isEnabled: boolean;
  emailAddress: string;
  publicKey: string;
  privateKey: boolean;

  constructor(init?: Partial<Webpush>) {
    Object.assign(this, init);
  }
}
