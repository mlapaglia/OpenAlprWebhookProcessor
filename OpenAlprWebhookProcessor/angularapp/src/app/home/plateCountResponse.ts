export interface DayCounts {
  counts: DayCount[];
  weeklyUniqueCounts: DayCount[];
}

export interface DayCount {
  date: Date;
  count: number;
}
