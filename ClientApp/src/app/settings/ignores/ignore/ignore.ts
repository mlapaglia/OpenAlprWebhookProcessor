export class Ignore {
    plateNumber: string;
    strictMatch: boolean;

    constructor(init?:Partial<Ignore>) {
        Object.assign(this, init);
    }
}