export class Ignore {
    id: string;
    plateNumber: string;
    strictMatch: boolean;
    description: string;
    
    constructor(init?: Partial<Ignore>) {
        Object.assign(this, init);
    }
}