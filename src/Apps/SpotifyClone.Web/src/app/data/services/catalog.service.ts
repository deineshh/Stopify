import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { Genre, MediaCard, MediaShelf, ShortcutItem, Track } from '../models/catalog.models';
import {
  HOME_SHELVES,
  PLAYLIST_SHELVES,
  PLAYLIST_SHELVES_AFTER_ABOUT,
  PLAYLIST_TRACKS,
  RECENTLY_PLAYED,
  SEARCH_RESULTS,
} from '../mock/catalog.mock';
import { GENRES } from '../mock/genres.mock';
import { COVER } from '../mock/catalog.mock';

export interface HomeFeed {
  shortcuts: readonly ShortcutItem[];
  shelves: readonly MediaShelf[];
}

export interface SearchResults {
  query: string;
  topResult: { id: string; name: string; type: string; cover: string };
  songs: readonly Track[];
  artists: readonly MediaCard[];
  albums: readonly MediaCard[];
  playlists: readonly MediaCard[];
  profiles: readonly MediaCard[];
  genres: readonly { id: string; name: string; cover: string }[];
}

export interface PlaylistDetails {
  id: string;
  title: string;
  description: string;
  cover: string;
  verified: boolean;
  owner: { id: string; name: string };
  releaseDate: string;
  trackCount: number;
  totalDurationSeconds: number;
  publicPlaylists: number;
  monthlyListeners: number;
  about: string;
  copyright: string;
  tracks: readonly Track[];
  shelves: readonly MediaShelf[];
  shelvesAfterAbout: readonly MediaShelf[];
}

/**
 * Read-only catalog queries. Returns static mock data for now; every method is
 * shaped like an HttpClient call so it can be pointed at the API without touching the UI.
 */
@Injectable({ providedIn: 'root' })
export class CatalogService {
  getHomeFeed(): Observable<HomeFeed> {
    return of({ shortcuts: RECENTLY_PLAYED, shelves: HOME_SHELVES });
  }

  getGenres(): Observable<readonly Genre[]> {
    return of(GENRES);
  }

  search(query: string): Observable<SearchResults> {
    return of({ query, ...SEARCH_RESULTS });
  }

  getPlaylist(id: string): Observable<PlaylistDetails> {
    return of({
      id,
      title: 'cali',
      description: 'asd',
      cover: COVER,
      verified: true,
      owner: { id: 'aqua-studios', name: 'Aqua Studios' },
      releaseDate: '2025-10-18',
      trackCount: 81,
      totalDurationSeconds: 3 * 3600 + 30 * 60,
      publicPlaylists: 18,
      monthlyListeners: 668_849,
      about: 'creator from hungary',
      copyright: '© 2025 Supermanagement',
      tracks: PLAYLIST_TRACKS,
      shelves: PLAYLIST_SHELVES,
      shelvesAfterAbout: PLAYLIST_SHELVES_AFTER_ABOUT,
    });
  }
}
