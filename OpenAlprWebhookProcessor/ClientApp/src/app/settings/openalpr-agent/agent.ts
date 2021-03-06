export class Agent {
    endpointUrl: string;
    hostname: string;
    latitude: number;
    longitude: number;
    uid: string;
    openAlprWebServerApiKey: string;
    openAlprWebServerUrl: string;
    version: string;
    sunriseOffset: number;
    sunsetOffset: number;
    timezoneOffset: number;

    constructor(init?:Partial<Agent>) {
        Object.assign(this, init);
    }
}
