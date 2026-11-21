import type { PlaybackTarget } from './player.models';

/**
 * View models for the catalog UI. They describe what the screens render,
 * so the API layer can map backend DTOs onto them later without touching components.
 */

export interface EntityRef {
  id: string;
  name: string;
}

export type CoverShape = 'rounded' | 'circle';

export type MediaKind = 'album' | 'single' | 'ep' | 'playlist' | 'artist' | 'profile' | 'mix';

/** A card in a horizontal shelf (album, playlist, artist, mix, ...). */
export interface MediaCard {
  id: string;
  kind: MediaKind;
  /** Mixes ("Made For") have no title, only a description. */
  title?: string;
  cover: string;
  shape: CoverShape;
  /** Values followed by a bullet, e.g. the release year. */
  meta?: readonly string[];
  /** Plain description segments, e.g. "Artist" or "2 Followers". */
  text?: readonly string[];
  /** Linked artists, rendered as a comma separated list. */
  authors?: readonly EntityRef[];
}

/** A titled, single-row group of cards ("Jump back in", "Fans also like", ...). */
export interface MediaShelf {
  id: string;
  eyebrow?: string;
  title: string;
  showAllLabel?: string;
  filters?: readonly string[];
  items: readonly MediaCard[];
}

export interface Track {
  id: string;
  title: string;
  artists: readonly EntityRef[];
  album: EntityRef;
  cover: string;
  durationSeconds: number;
  /** ISO date the track was added to the playlist. */
  addedAt?: string;
  saved?: boolean;
  explicit?: boolean;
  /** Who is credited and how (only known for tracks loaded on their own, not album listings). */
  credits?: readonly TrackCredit[];
}

export interface TrackCredit {
  artist: EntityRef;
  role: 'Main Artist' | 'Featured Artist';
}

export interface Genre {
  id: string;
  name: string;
  image: string;
  /** Background colour of the browse tile. */
  color: string;
}

export interface ShortcutItem {
  id: string;
  title: string;
  cover: string;
  link: readonly string[];
  /** What its play button starts. */
  target: PlaybackTarget;
}
