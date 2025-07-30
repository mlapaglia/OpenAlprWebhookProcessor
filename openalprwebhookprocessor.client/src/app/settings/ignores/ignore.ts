import type { IPlateSetting } from '../shared/plate-setting.interface';
import { PlateSettingBase } from '../shared/plate-setting.interface';

export class Ignore extends PlateSettingBase {
  constructor(init?: Partial<IPlateSetting>) {
    super(init);
  }
}
