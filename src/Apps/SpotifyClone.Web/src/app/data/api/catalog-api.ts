import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { injectApiUrl } from '@core/api/api.config';
import { Page, PageRequest, toPage } from '@core/api/pagination';
import {
  AlbumDetailsDto,
  AlbumSummaryDto,
  ArtistDetailsDto,
  ArtistSummaryDto,
  GenreSummaryDto,
  MoodSummaryDto,
  PlaylistDetailsDto,
  PlaylistSummaryDto,
  SearchResultsDto,
  TrackDetailsDto,
  TrackSummaryDto,
} from './catalog.dtos';

type QueryValue = string | number | boolean | readonly string[] | null | undefined;
type Query = Record<string, QueryValue>;

export interface AlbumFilters {
  title?: string;
  type?: string;
  mainArtistIds?: readonly string[];
  genreIds?: readonly string[];
}

export interface ArtistFilters {
  name?: string;
  genreIds?: readonly string[];
}

export interface TrackFilters {
  title?: string;
  albumId?: string;
  mainArtistIds?: readonly string[];
  genreIds?: readonly string[];
}

export interface PlaylistFilters {
  name?: string;
  ownerId?: string;
  isPublic?: boolean;
}

/**
 * Typed HTTP client for the read side of the Catalog, Playlists and Search modules.
 * Returns API DTOs; `CatalogService` turns them into view models.
 */
@Injectable({ providedIn: 'root' })
export class CatalogApi {
  private readonly http = inject(HttpClient);
  private readonly url = injectApiUrl();

  albums(filters: AlbumFilters = {}, page: PageRequest = {}): Observable<Page<AlbumSummaryDto>> {
    return this.list<AlbumSummaryDto>('/albums', { ...filters }, page);
  }

  album(id: string): Observable<AlbumDetailsDto> {
    return this.http.get<AlbumDetailsDto>(this.url(`/albums/${encodeURIComponent(id)}`));
  }

  artists(filters: ArtistFilters = {}, page: PageRequest = {}): Observable<Page<ArtistSummaryDto>> {
    return this.list<ArtistSummaryDto>('/artists', { ...filters }, page);
  }

  artist(id: string): Observable<ArtistDetailsDto> {
    return this.http.get<ArtistDetailsDto>(this.url(`/artists/${encodeURIComponent(id)}`));
  }

  tracks(filters: TrackFilters = {}, page: PageRequest = {}): Observable<Page<TrackSummaryDto>> {
    return this.list<TrackSummaryDto>('/tracks', { ...filters }, page);
  }

  track(id: string): Observable<TrackDetailsDto> {
    return this.http.get<TrackDetailsDto>(this.url(`/tracks/${encodeURIComponent(id)}`));
  }

  genres(page: PageRequest = {}): Observable<Page<GenreSummaryDto>> {
    return this.list<GenreSummaryDto>('/genres', {}, page);
  }

  moods(page: PageRequest = {}): Observable<Page<MoodSummaryDto>> {
    return this.list<MoodSummaryDto>('/moods', {}, page);
  }

  playlists(filters: PlaylistFilters = {}, page: PageRequest = {}): Observable<Page<PlaylistSummaryDto>> {
    return this.list<PlaylistSummaryDto>('/playlists', { ...filters }, page);
  }

  playlist(id: string): Observable<PlaylistDetailsDto> {
    return this.http.get<PlaylistDetailsDto>(this.url(`/playlists/${encodeURIComponent(id)}`));
  }

  /** The signed-in user's playlists (requires a session). */
  myPlaylists(page: PageRequest = {}): Observable<Page<PlaylistSummaryDto>> {
    return this.list<PlaylistSummaryDto>('/me/playlists', {}, page);
  }

  search(term: string, limit = 20): Observable<SearchResultsDto> {
    return this.http.get<SearchResultsDto>(this.url('/search'), {
      params: toParams({ searchTerm: term, limit }),
    });
  }

  private list<T>(path: string, filters: Query, page: PageRequest): Observable<Page<T>> {
    return this.http
      .get<T[]>(this.url(path), { params: toParams({ ...filters, ...page }), observe: 'response' })
      .pipe(map((response) => toPage(response.body, response.headers, page)));
  }
}

/** ASP.NET binds arrays from repeated keys (`mainArtistIds=a&mainArtistIds=b`). */
function toParams(query: Query): HttpParams {
  let params = new HttpParams();
  for (const [key, value] of Object.entries(query)) {
    if (value === undefined || value === null || value === '') {
      continue;
    }
    if (Array.isArray(value)) {
      for (const item of value) {
        params = params.append(key, item);
      }
    } else {
      params = params.set(key, String(value));
    }
  }
  return params;
}
