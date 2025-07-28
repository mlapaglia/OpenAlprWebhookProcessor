import { PlateSettingBase, IPlateSetting } from '../shared/plate-setting.interface'

export class Alert extends PlateSettingBase {
  constructor(init?: Partial<IPlateSetting>) {
    super(init)
  }
}
