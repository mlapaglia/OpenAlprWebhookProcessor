export class Agent {
    endpointUrl: string;
    hostname: string;
    uid: string;
    openAlprWebServerApiKey: string;
    openAlprWebServerUrl: string;
    version: string;

    constructor(init?:Partial<Agent>) {
        Object.assign(this, init);
    }
}
