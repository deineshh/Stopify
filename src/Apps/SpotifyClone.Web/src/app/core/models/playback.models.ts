export interface PlayRequest {
  deviceId: string;
  contextType: string;
  contextExternalId: string;
  startTrackId: string | null;
}

export interface PlayResponse {
  hlsUrl: string | null;
  dashUrl: string | null;
  startPositionMs: number;
  trackId: string;
}

export interface ShuffleResponse {
  isShuffled: boolean;
}

export interface RepeatResponse {
  repeatMode: 'Off' | 'All' | 'Track';
}
