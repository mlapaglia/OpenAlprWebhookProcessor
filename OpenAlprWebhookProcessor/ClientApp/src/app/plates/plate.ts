import { Url } from "url";

export interface Plate {
    id: string;
    
    openAlprCameraId: number;

    vehicleDescription: string;

    plateNumber: string;

    penAlprProcessingTimeMs: number;

    processedPlateConfidence: number;

    licensePlateJpegBase64: number;

    isAlert: boolean;

    isIgnore: boolean;

    alertDescription: string;

    receivedOn: Date;

    direction: number;

    imageUrl: Url;

    cropImageUrl: Url;
}