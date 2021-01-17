export class Camera {
    latitude: number;
    longitude: number;
    manufacturer: Manufacturer;
    modelNumber: string;
    openAlprName: string;
    openAlprCameraId: number;
    cameraPassword: string;
    cameraUsername: string;
    updateOverlayTextUrl: string;
    platesSeen: number;
    sampleImageUrl: string;

    constructor(init?:Partial<Camera>) {
        Object.assign(this, init);
    }
}

export enum Manufacturer {
    Hikvision = "Hikvision",
    Dahua = "Dahua"
}