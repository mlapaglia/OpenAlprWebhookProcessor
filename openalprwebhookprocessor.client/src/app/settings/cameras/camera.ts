export class Camera {
    id: string;
    latitude: number;
    longitude: number;
    ipAddress: string;
    manufacturer: Manufacturer;
    modelNumber: string;
    openAlprName: string;
    openAlprCameraId: number;
    cameraPassword: string;
    cameraUsername: string;
    updateOverlayEnabled: boolean;
    updateOverlayTextUrl: string;
    nightZoom: string;
    nightFocus: string;
    dayZoom: string;
    dayFocus: string;
    dayNightModeEnabled: boolean;
    dayNightModeUrl: string;
    dayNightNextScheduledCommand: Date;
    openAlprEnabled: boolean;
    sunriseOffset: number;
    sunsetOffset: number;
    platesSeen: number;
    sampleImageUrl: string;
    timezoneOffset: number;
    
    constructor(init?: Partial<Camera>) {
        Object.assign(this, init);
    }
}

export enum Manufacturer {
    Hikvision = 'Hikvision',
    Dahua = 'Dahua'
}
