export class Pushover {
    apiToken: string;
    userKey: string;
    isEnabled: boolean;
    sendPlatePreviewEnabled: boolean;
    sendEveryPlateEnabled: boolean;
    
    constructor(init?:Partial<Pushover>) {
        Object.assign(this, init);
    }
}