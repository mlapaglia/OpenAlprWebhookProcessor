export interface PlateStatistics {
    firstSeen: Date;
    lastSeen: Date;
    last90Days: number;
    totalSeen: number;
}

export interface PlateStatisticsData { 
    key: string;
    value: string;
}