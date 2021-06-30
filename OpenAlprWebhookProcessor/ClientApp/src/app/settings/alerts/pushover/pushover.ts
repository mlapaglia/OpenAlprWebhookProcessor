export class Pushover {
    apiToken: string;
    userKey: string;
    isEnabled: boolean;
    sendPlatePreview: boolean;
    
    constructor(init?:Partial<Pushover>) {
        Object.assign(this, init);
    }
}