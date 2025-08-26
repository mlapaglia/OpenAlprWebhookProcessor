export interface PlateStatistics {
  firstSeen: Date
  lastSeen: Date
  last90Days: number
  totalSeen: number
  possiblePlates: string[]
  region: string
}

export interface PlateStatisticsData {
  key: string
  value: string
}
