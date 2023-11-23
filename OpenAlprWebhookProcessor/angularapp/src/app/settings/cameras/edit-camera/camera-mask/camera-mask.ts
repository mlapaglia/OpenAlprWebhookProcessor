import { Coordinate } from './coordinate';

export class CameraMask {
    coordinates: Coordinate[];
    imageMask: string;
    cameraId: string;
    
    constructor(init?: Partial<Coordinate>) {
        Object.assign(this, init);
    }
}