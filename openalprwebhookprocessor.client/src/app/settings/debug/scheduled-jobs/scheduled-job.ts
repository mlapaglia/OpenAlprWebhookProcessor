export interface ScheduledJob {
  jobId: string;
  jobType: ScheduledJobType;
  scheduledExecutionTime: string;
  cameraId?: string;
  sunriseSunsetType?: SunriseSunsetType;
  scheduleNextJob?: boolean;
}

export interface ScheduledJobsResponse {
  scheduledJobs: ScheduledJob[];
}

export enum ScheduledJobType {
  ClearOverlay = 0,
  SunriseSunset = 1,
}

export enum SunriseSunsetType {
  Unknown = 0,
  Sunrise = 1,
  Sunset = 2,
}
