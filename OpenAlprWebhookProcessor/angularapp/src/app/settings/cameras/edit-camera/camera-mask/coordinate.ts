export class Coordinate {
    x: number;
    y: number;
    
    constructor(init?: Partial<Coordinate>) {
        Object.assign(this, init);
    }
}