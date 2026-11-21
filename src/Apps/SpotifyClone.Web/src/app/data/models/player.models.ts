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
