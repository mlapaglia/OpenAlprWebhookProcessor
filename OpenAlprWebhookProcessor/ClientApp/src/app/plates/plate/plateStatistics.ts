export interface PlateStatistics {
    firstSeen: Date;
    lastSeen: Date;
    last90Days: number;
}

export interface PlateStatisticsData { 
    key: string;
    value: string;
}