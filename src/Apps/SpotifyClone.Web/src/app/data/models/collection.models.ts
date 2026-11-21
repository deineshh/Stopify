import { EntityRef, MediaShelf, Track } from './catalog.models';
import { Collaborator } from './playlist.models';

/** A user playlist, ready to render. */
export interface PlaylistView {
  id: string;
  title: string;
  description: string | null;
  cover: string;
  /** The cover is an uploaded photo rather than the one made from the songs. */
  hasCustomCover: boolean;
  isPublic: boolean;
  ownerId: string;
  tracks: readonly Track[];
  /** Every song id in the playlist's order, including songs that no longer load (not in `tracks`). */
  trackIds: readonly string[];
  collaborators: readonly Collaborator[];
  totalDurationSeconds: number;
}

export interface AlbumView {
  id: string;
  title: string;
  /** "Album", "Single" or "EP". */
  typeLabel: string;
  cover: string;
  artists: readonly EntityRef[];
  releaseDate: string | null;
  tracks: readonly Track[];
  totalDurationSeconds: number;
  /** Other releases by the main artist. */
  moreBy: MediaShelf | null;
}

export interface ArtistView {
  id: string;
  name: string;
  bio: string | null;
  avatar: string;
  verified: boolean;
  popularTracks: readonly Track[];
  discography: MediaShelf | null;
}
