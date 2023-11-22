import { Url } from "url";

export interface Plate {
    id: string;
    
    openAlprCameraId: number;

    vehicleDescription: string;

    plateNumber: string;

    region: string;
    
    possiblePlateNumbers: string;

    openAlprProcessingTimeMs: number;

    processedPlateConfidence: number;

    isAlert: boolean;

    isIgnore: boolean;

    isOpen: boolean;

    alertDescription: string;

    receivedOn: Date;

    direction: number;

    imageUrl: Url;

    cropImageUrl: Url;

    notes: string;

    canBeEnriched: boolean;
}