export class VideoStream {
  cameraId: number;
  cameraName: string;
  fps: number;
  isStreaming: boolean;
  lastPlateRead: number;
  lastUpdate: number;
  totalPlateReads: number;
  url: string;

  constructor(init?: Partial<VideoStream>) {
    Object.assign(this, init);
  }
}

export class AgentVideoStreams {
  isConnected: boolean;
  videoStreams: VideoStream[];

  constructor(init?: Partial<AgentVideoStreams>) {
    Object.assign(this, init);
    if (!(this.videoStreams as VideoStream[] | undefined)) {
      this.videoStreams = [];
    }
  }
}
