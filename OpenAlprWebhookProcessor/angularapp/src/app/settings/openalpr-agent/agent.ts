export class Agent {
    endpointUrl: string;
    id: string;
    isDebugEnabled: boolean;
    isImageCompressionEnabled: boolean;
    lastHeartbeatEpochMs: number;
    latitude: number;
    longitude: number;
    uid: string;
    openAlprWebServerUrl: string;
    nextScrapeInMinutes: Date;
    scheduledScrapingIntervalMinutes: number;
    sunriseOffset: number;
    sunsetOffset: number;
    timezoneOffset: number;

    constructor(init?: Partial<Agent>) {
        Object.assign(this, init);
    }
}
