import { Injectable, inject } from '@angular/core';
import { Observable, catchError, forkJoin, map, of, shareReplay, switchMap } from 'rxjs';
import { CatalogApi } from '../api/catalog-api';
import { AlbumDetailsDto, TrackDetailsDto, TrackSummaryDto } from '../api/catalog.dtos';
import { trackToViewModel } from '../api/catalog.mappers';
import { Track } from '../models/catalog.models';

/**
 * Turns track ids and track DTOs into `Track`s with their album's title and cover. The player and
 * the queue mostly stay on a handful of albums, so albums are fetched once and cached.
 */
@Injectable({ providedIn: 'root' })
export class TrackLookup {
  private readonly catalog = inject(CatalogApi);
  private readonly albums = new Map<string, Observable<AlbumDetailsDto | null>>();

  track(id: string): Observable<Track> {
    return this.catalog.track(id).pipe(switchMap((dto) => this.withAlbum(dto)));
  }

  /** Keeps the order; albums are looked up once each. */
  tracks(dtos: readonly (TrackDetailsDto | TrackSummaryDto)[]): Observable<Track[]> {
    return dtos.length ? forkJoin(dtos.map((dto) => this.withAlbum(dto))) : of([]);
  }

  private withAlbum(dto: TrackDetailsDto | TrackSummaryDto): Observable<Track> {
    return (dto.albumId ? this.album(dto.albumId) : of(null)).pipe(
      map((album) =>
        trackToViewModel(dto, album ? { id: album.id, title: album.title, coverImageId: album.cover?.imageId } : undefined),
      ),
    );
  }

  private album(id: string): Observable<AlbumDetailsDto | null> {
    let album = this.albums.get(id);
    if (!album) {
      album = this.catalog.album(id).pipe(
        catchError(() => {
          this.albums.delete(id);
          return of(null);
        }),
        shareReplay(1),
      );
      this.albums.set(id, album);
    }
    return album;
  }
}
