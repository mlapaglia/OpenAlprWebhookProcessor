export class Forward {
    Id: string;
    destination: string;
    ignoreSslErrors: boolean;

    constructor(init?:Partial<Forward>) {
        Object.assign(this, init);
    }
}
