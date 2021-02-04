export class Camera {
    id: string;
    latitude: number;
    longitude: number;
    manufacturer: Manufacturer;
    modelNumber: string;
    openAlprName: string;
    openAlprCameraId: number;
    cameraPassword: string;
    cameraUsername: string;
    updateOverlayEnabled: boolean;
    updateOverlayTextUrl: string;
    dayNightModeEnabled: boolean;
    dayNightModeUrl: string;
    sunriseOffset: number;
    sunsetOffset: number;
    platesSeen: number;
    sampleImageUrl: string;
    timezoneOffset: number;
    
    constructor(init?:Partial<Camera>) {
        Object.assign(this, init);
    }
}

export enum Manufacturer {
    Hikvision = "Hikvision",
    Dahua = "Dahua"
}
