import { Injectable, inject } from '@angular/core';
import { Observable, catchError, forkJoin, map, of } from 'rxjs';
import { Page } from '@core/api/pagination';
import { CatalogApi } from '../api/catalog-api';
import { albumToCard, artistToCard, genreToTile, playlistToCard, searchToViewModels } from '../api/catalog.mappers';
import { Genre, MediaCard, MediaShelf, ShortcutItem, Track } from '../models/catalog.models';

export type ShelfCategory = 'music' | 'podcasts';

export interface HomeShelf extends MediaShelf {
  category: ShelfCategory;
}

export interface TopResult {
  id: string;
  name: string;
  type: string;
  cover: string;
  shape: 'rounded' | 'circle';
  link: readonly string[];
}

export interface SearchResults {
  query: string;
  topResult: TopResult | null;
  songs: readonly Track[];
  artists: readonly MediaCard[];
  albums: readonly MediaCard[];
  playlists: readonly MediaCard[];
  profiles: readonly MediaCard[];
  genres: readonly { id: string; name: string; cover: string }[];
}

const SHELF_SIZE = 12;
const SHORTCUT_COUNT = 8;
const SONGS_IN_SEARCH = 4;

/**
 * Home, browse and search. The API has no recommendations endpoint yet, so the home feed is
 * composed from the list endpoints; each shelf fails (and disappears) on its own.
 */
@Injectable({ providedIn: 'root' })
export class CatalogService {
  private readonly api = inject(CatalogApi);

  getHomeShelves(): Observable<readonly HomeShelf[]> {
    return forkJoin({
      albums: this.api.albums({}, { pageSize: SHELF_SIZE }).pipe(orEmptyPage()),
      artists: this.api.artists({}, { pageSize: SHELF_SIZE }).pipe(orEmptyPage()),
      playlists: this.api.playlists({ isPublic: true }, { pageSize: SHELF_SIZE }).pipe(orEmptyPage()),
    }).pipe(
      map(({ albums, artists, playlists }) =>
        [
          shelf('new-releases', 'New releases', albums.items.map(albumToCard), 'music', 'Fresh music on Stopify'),
          shelf('artists', 'Popular artists', artists.items.map(artistToCard), 'music'),
          shelf('playlists', 'Playlists for you', playlists.items.map(playlistToCard), 'music'),
        ].filter((item) => item.items.length > 0),
      ),
    );
  }

  /** The top grid: the user's own playlists when signed in, otherwise the newest albums. */
  getShortcuts(signedIn: boolean): Observable<readonly ShortcutItem[]> {
    const source$: Observable<ShortcutItem[]> = signedIn
      ? this.api.myPlaylists({ pageSize: SHORTCUT_COUNT }).pipe(
          map((page) => page.items.map(playlistToCard).map((card) => toShortcut(card, ['/playlist', card.id]))),
        )
      : this.api.albums({}, { pageSize: SHORTCUT_COUNT }).pipe(
          map((page) => page.items.map(albumToCard).map((card) => toShortcut(card, ['/album', card.id]))),
        );
    return source$.pipe(catchError(() => of([])));
  }

  /** Browse tiles: genres first, then moods. */
  getGenres(): Observable<readonly Genre[]> {
    return forkJoin([
      this.api.genres({ pageSize: 50 }).pipe(orEmptyPage()),
      this.api.moods({ pageSize: 50 }).pipe(orEmptyPage()),
    ]).pipe(map(([genres, moods]) => [...genres.items, ...moods.items].map((genre) => genreToTile(genre, ''))));
  }

  search(query: string): Observable<SearchResults> {
    return this.api.search(query, 20).pipe(
      map((dto) => {
        const view = searchToViewModels(dto);
        return {
          query,
          topResult: pickTopResult(view),
          songs: view.tracks.slice(0, SONGS_IN_SEARCH),
          artists: view.artists,
          albums: view.albums,
          playlists: view.playlists,
          profiles: view.profiles,
          genres: view.genres,
        };
      }),
    );
  }
}

function shelf(id: string, title: string, items: MediaCard[], category: ShelfCategory, eyebrow?: string): HomeShelf {
  return { id, title, eyebrow, items, category, showAllLabel: 'Show all' };
}

function toShortcut(card: MediaCard, link: string[]): ShortcutItem {
  return { id: card.id, title: card.title ?? '', cover: card.cover, link };
}

/** Artists beat albums beat songs, like the search page of the mockup. */
function pickTopResult(view: ReturnType<typeof searchToViewModels>): TopResult | null {
  const [artist] = view.artists;
  if (artist) {
    return { id: artist.id, name: artist.title ?? '', type: 'Artist', cover: artist.cover, shape: 'circle', link: ['/artist', artist.id] };
  }
  const [album] = view.albums;
  if (album) {
    return { id: album.id, name: album.title ?? '', type: 'Album', cover: album.cover, shape: 'rounded', link: ['/album', album.id] };
  }
  const [track] = view.tracks;
  if (track) {
    return {
      id: track.id,
      name: track.title,
      type: 'Song',
      cover: track.cover,
      shape: 'rounded',
      link: track.album.id ? ['/album', track.album.id] : [],
    };
  }
  return null;
}

function orEmptyPage<T>() {
  return catchError<Page<T>, Observable<Page<T>>>(() =>
    of({ items: [], page: 1, pageSize: 0, hasNextPage: false, hasPreviousPage: false, totalCount: 0 }),
  );
}
