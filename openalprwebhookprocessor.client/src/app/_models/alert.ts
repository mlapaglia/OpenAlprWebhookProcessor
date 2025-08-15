export class Alert {
  id: string;
  type: AlertType;
  message: string;
  autoClose: boolean;
  keepAfterRouteChange?: boolean;
  fade: boolean;

  constructor(init?: Partial<Alert>) {
    Object.assign(this, init);
  }
}

export enum AlertType {
  Success = 0,
  Error = 1,
  Info = 2,
  Warning = 3,
}
