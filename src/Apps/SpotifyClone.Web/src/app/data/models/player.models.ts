import { EntityRef, Track } from './catalog.models';

export type RepeatMode = 'off' | 'all' | 'one';

export interface ArtistSpotlight {
  artist: EntityRef;
  avatar: string;
  monthlyListeners: number;
  bio: string;
  following: boolean;
}

export interface TrackCredit {
  artist: EntityRef;
  role: string;
  following: boolean;
}

/** Everything the right sidebar shows about the current track. */
export interface NowPlayingDetails {
  /** Context the track plays from, shown as the panel title. */
  context: EntityRef;
  track: Track;
  spotlight: ArtistSpotlight;
  credits: readonly TrackCredit[];
}
