export class Plate {
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

    imageUrl: URL;

    cropImageUrl: URL;

    notes: string;

    canBeEnriched: boolean;

    constructor(init?: Partial<Plate>) {
        Object.assign(this, init);
    }
}