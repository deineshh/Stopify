import { Injectable, inject } from '@angular/core';
import { Observable, catchError, forkJoin, map, of, switchMap } from 'rxjs';
import { imageUrl } from '@core/api/media';
import { CatalogApi } from '../api/catalog-api';
import { AlbumDetailsDto, TrackDetailsDto } from '../api/catalog.dtos';
import { albumToCard, albumTracks, albumTypeLabel, trackToViewModel } from '../api/catalog.mappers';
import { AlbumView, ArtistView, PlaylistView } from '../models/collection.models';
import { Track } from '../models/catalog.models';

const MORE_BY_LIMIT = 10;
const POPULAR_TRACKS_LIMIT = 10;

/** Playlist, album and artist pages: one call per page, composed from the API's endpoints. */
@Injectable({ providedIn: 'root' })
export class CollectionService {
  private readonly api = inject(CatalogApi);

  /**
   * Playlist details only list track ids, so each track is fetched, then each distinct album
   * (for its title and cover). Tracks that fail to load (deleted, unpublished) are skipped.
   */
  getPlaylist(id: string): Observable<PlaylistView> {
    return this.api.playlist(id).pipe(
      switchMap((playlist) => {
        const ordered = [...playlist.tracks].sort((a, b) => a.position - b.position);
        return this.resolveTracks(ordered.map((track) => track.id)).pipe(
          map((tracks): PlaylistView => ({
            id: playlist.id,
            title: playlist.name,
            description: playlist.description,
            cover: imageUrl(playlist.customCoverImageId?.imageId ?? playlist.generatedCoverImageIds[0]),
            isPublic: playlist.isPublic,
            ownerId: playlist.ownerId,
            tracks,
            totalDurationSeconds: sumDurations(tracks),
          })),
        );
      }),
    );
  }

  getAlbum(id: string): Observable<AlbumView> {
    return this.api.album(id).pipe(
      switchMap((album) => {
        const tracks = albumTracks(album);
        const mainArtist = album.mainArtists[0];
        const moreBy$ = mainArtist
          ? this.api.albums({ mainArtistIds: [mainArtist.id] }, { pageSize: MORE_BY_LIMIT + 1 }).pipe(
              map((page) => page.items.filter((other) => other.id !== album.id).slice(0, MORE_BY_LIMIT)),
              catchError(() => of([])),
            )
          : of([]);
        return moreBy$.pipe(
          map((others): AlbumView => ({
            id: album.id,
            title: album.title,
            typeLabel: albumTypeLabel(album.type),
            cover: imageUrl(album.cover?.imageId),
            artists: album.mainArtists.map(({ id: artistId, name }) => ({ id: artistId, name })),
            releaseDate: album.releaseDate,
            tracks,
            totalDurationSeconds: sumDurations(tracks),
            moreBy: others.length
              ? { id: 'more-by', title: `More by ${mainArtist!.name}`, showAllLabel: 'See discography', items: others.map(albumToCard) }
              : null,
          })),
        );
      }),
    );
  }

  getArtist(id: string): Observable<ArtistView> {
    return this.api.artist(id).pipe(
      switchMap((artist) =>
        forkJoin({
          tracks: this.api.tracks({ mainArtistIds: [id] }, { pageSize: POPULAR_TRACKS_LIMIT }).pipe(
            map((page) => page.items.map((track) => trackToViewModel(track))),
            catchError(() => of<Track[]>([])),
          ),
          albums: this.api.albums({ mainArtistIds: [id] }, { pageSize: 20 }).pipe(
            map((page) => page.items),
            catchError(() => of([])),
          ),
        }).pipe(
          map(({ tracks, albums }): ArtistView => {
            // Album covers double as track artwork in the "Popular" list.
            const covers = new Map(albums.map((album) => [album.id, album]));
            return {
              id: artist.id,
              name: artist.name,
              bio: artist.bio,
              avatar: imageUrl(artist.avatar?.imageId),
              verified: artist.status.toLowerCase() === 'verified',
              popularTracks: tracks.map((track) => {
                const album = covers.get(track.album.id);
                return album
                  ? { ...track, album: { id: album.id, name: album.title }, cover: imageUrl(album.cover?.imageId) }
                  : track;
              }),
              discography: albums.length
                ? {
                    id: 'discography',
                    title: 'Discography',
                    showAllLabel: 'Show all',
                    filters: ['Popular releases', 'Albums', 'Singles and EPs'],
                    items: albums.map(albumToCard),
                  }
                : null,
            };
          }),
        ),
      ),
    );
  }

  private resolveTracks(ids: readonly string[]): Observable<Track[]> {
    if (!ids.length) {
      return of([]);
    }
    return forkJoin(ids.map((trackId) => this.api.track(trackId).pipe(catchError(() => of(null))))).pipe(
      switchMap((details) => {
        const tracks = details.filter((track): track is TrackDetailsDto => track !== null);
        const albumIds = [...new Set(tracks.map((track) => track.albumId).filter((value): value is string => !!value))];
        const albums$ = albumIds.length
          ? forkJoin(albumIds.map((albumId) => this.api.album(albumId).pipe(catchError(() => of(null)))))
          : of<(AlbumDetailsDto | null)[]>([]);
        return albums$.pipe(
          map((albums) => {
            const byId = new Map(albums.filter((a): a is AlbumDetailsDto => a !== null).map((a) => [a.id, a]));
            return tracks.map((track) => {
              const album = track.albumId ? byId.get(track.albumId) : undefined;
              return trackToViewModel(
                track,
                album ? { id: album.id, title: album.title, coverImageId: album.cover?.imageId } : undefined,
              );
            });
          }),
        );
      }),
    );
  }
}

function sumDurations(tracks: readonly Track[]): number {
  return tracks.reduce((total, track) => total + track.durationSeconds, 0);
}
