export class Enricher {
    id: string;
    isEnabled: boolean;
    apiKey: string;
    enrichAlways: boolean;
    enrichInNightMode: boolean;
    enrichManually: boolean;
    
    constructor(init?:Partial<Enricher>) {
        Object.assign(this, init);
    }
}