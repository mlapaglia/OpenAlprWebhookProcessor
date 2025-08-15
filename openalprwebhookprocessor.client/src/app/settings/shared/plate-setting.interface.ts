export interface IPlateSetting {
  id: string
  plateNumber: string
  strictMatch: boolean
  description: string
}

export abstract class PlateSettingBase implements IPlateSetting {
  id: string;
  plateNumber: string;
  strictMatch: boolean;
  description: string;

  constructor(init?: Partial<IPlateSetting>) {
    Object.assign(this, init);
  }
}
