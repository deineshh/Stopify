import { EntityRef, Track } from './catalog.models';

export type RepeatMode = 'off' | 'all' | 'one';

/**
 * Something a play button can start. Albums and playlists are server-side playback contexts
 * (optionally starting at one of their tracks); an artist plays their top track's album.
 */
export type PlaybackTarget =
  | { kind: 'album' | 'playlist'; id: string; trackId?: string }
  | { kind: 'artist'; id: string };

/** The context the server builds the queue from. */
export interface PlaybackContextRef {
  kind: 'album' | 'playlist';
  id: string;
}

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
