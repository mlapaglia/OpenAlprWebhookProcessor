export class Alert {
    id: string;
    plateNumber: string;
    strictMatch: boolean;
    description: string;
    
    constructor(init?:Partial<Alert>) {
        Object.assign(this, init);
    }
}