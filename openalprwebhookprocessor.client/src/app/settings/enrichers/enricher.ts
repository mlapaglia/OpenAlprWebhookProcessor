import { EnrichmentType } from './enrichmentType';

export class Enricher {
    id: string;
    isEnabled: boolean;
    apiKey: string;
    enrichmentType: EnrichmentType;
    
    constructor(init?: Partial<Enricher>) {
        Object.assign(this, init);
    }
}