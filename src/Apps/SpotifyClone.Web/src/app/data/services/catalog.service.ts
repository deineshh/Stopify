import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { Genre, MediaCard, MediaShelf, ShortcutItem, Track } from '../models/catalog.models';
import { HOME_SHELVES, RECENTLY_PLAYED, SEARCH_RESULTS } from '../mock/catalog.mock';
import { GENRES } from '../mock/genres.mock';

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
}
