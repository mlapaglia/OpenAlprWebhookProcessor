import type { SnackBarType } from './snackbartype';

export class SnackBar {
  message: string;
  message2: string;
  snackType: SnackBarType;

  constructor(init?: Partial<SnackBar>) {
    Object.assign(this, init);
  }
}
