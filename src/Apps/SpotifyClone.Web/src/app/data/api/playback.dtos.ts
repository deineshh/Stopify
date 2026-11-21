import { TrackSummaryDto } from './catalog.dtos';

/** Contexts the API can build a queue from. ("search" exists too but can't be started yet.) */
export type PlaybackContextType = 'album' | 'playlist';

export interface StartPlaybackRequestDto {
  deviceId: string;
  contextType: PlaybackContextType;
  contextExternalId: string;
  startTrackId: string | null;
}

/** Returned by play, next and previous. `trackId` is null when there is nothing (more) to play. */
export interface PlaybackStreamDto {
  /** Relative to the site, e.g. `/audio/{assetId}/media_0.m3u8` (free) or `master.m3u8` (premium). */
  hlsUrl: string | null;
  dashUrl: string | null;
  startPositionMs: number | null;
  trackId: string | null;
}

export interface PlaybackSessionDto {
  id: string;
  userId: string;
  trackId: string | null;
  deviceId: string;
  contextType: string;
  contextExternalId: string | null;
  currentPositionMs: number;
  isPlaying: boolean;
  isShuffled: boolean;
  /** `Off`, `All` or `Track`. */
  repeatMode: string;
  updatedAtUtc: string;
}

export interface PlaybackQueueDto {
  currentTrack: TrackSummaryDto | null;
  tracksInQueue: TrackSummaryDto[];
}
