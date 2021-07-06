export class Tag {
    id: string;
    name: string;

    constructor(init?:Partial<Tag>) {
        Object.assign(this, init);
    }
}
