export class Forward {
    Id: string;
    destination: string;
    ignoreSslErrors: boolean;
    forwardSinglePlates: boolean;
    forwardGroupPreviews: boolean;
    forwardGroups: boolean;

    constructor(init?: Partial<Forward>) {
        Object.assign(this, init);
    }
}
