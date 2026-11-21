import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { injectApiUrl } from '@core/api/api.config';
import { UploadedImage } from './media-api';

export interface PlaylistDetailsChange {
  name: string;
  description: string | null;
  isPublic: boolean;
}

/** Name and description limits from the Playlists module (PlaylistNameRules / PlaylistDescriptionRules). */
export const PLAYLIST_NAME_MAX_LENGTH = 100;
export const PLAYLIST_DESCRIPTION_MAX_LENGTH = 300;

/**
 * Typed client for changing playlists. Reading them stays in `CatalogApi`.
 * The API only lets the owner edit, delete and manage collaborators; the owner and collaborators
 * add and reorder songs, and a song can be removed by the owner or whoever added it.
 */
@Injectable({ providedIn: 'root' })
export class PlaylistsApi {
  private readonly http = inject(HttpClient);
  private readonly url = injectApiUrl();

  /** Creates a private playlist named "My Playlist #N" and returns its id. */
  create(): Observable<string> {
    return this.http.post<{ playlistId: string }>(this.url('/playlists'), null).pipe(map((response) => response.playlistId));
  }

  editDetails(id: string, change: PlaylistDetailsChange): Observable<void> {
    return this.http.put<void>(this.url(`/playlists/${encodeURIComponent(id)}`), change);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(this.url(`/playlists/${encodeURIComponent(id)}`));
  }

  /** Fails with `Playlist.InvalidTrack` when the song is already in the playlist. */
  addTrack(id: string, trackId: string): Observable<void> {
    return this.http.post<void>(this.url(`/playlists/${encodeURIComponent(id)}/tracks`), { trackId });
  }

  removeTrack(id: string, trackId: string): Observable<void> {
    return this.http.delete<void>(
      this.url(`/playlists/${encodeURIComponent(id)}/tracks/${encodeURIComponent(trackId)}`),
    );
  }

  /** Owner only: makes an uploaded image the playlist's cover (see `MediaApi`). */
  linkCover(id: string, image: UploadedImage): Observable<void> {
    return this.http.put<void>(this.url(`/playlists/${encodeURIComponent(id)}/cover`), {
      imageId: image.imageId,
      imageWidth: image.width,
      imageHeight: image.height,
      imageFileType: image.fileType,
      imageSizeInBytes: image.sizeInBytes,
    });
  }

  /**
   * Owner only: drops the uploaded photo, so the cover is made from the songs again.
   * `DELETE /playlists/{id}/cover` mirrors the genre and mood endpoints; until the API has it,
   * it answers 405 (the route only allows PUT).
   */
  removeCover(id: string): Observable<void> {
    return this.http.delete<void>(this.url(`/playlists/${encodeURIComponent(id)}/cover`));
  }

  /** Owner only. Fails with `Playlist.InvalidCollaborator` for an unknown user or one already added. */
  addCollaborator(id: string, collaboratorId: string): Observable<void> {
    return this.http.post<void>(this.url(`/playlists/${encodeURIComponent(id)}/collaborators`), { collaboratorId });
  }

  removeCollaborator(id: string, collaboratorId: string): Observable<void> {
    return this.http.delete<void>(
      this.url(`/playlists/${encodeURIComponent(id)}/collaborators/${encodeURIComponent(collaboratorId)}`),
    );
  }

  /**
   * Moves a song so it ends up at `targetPositionIndex` (0-based) of the playlist's full track
   * list. The API clamps the index to the list.
   */
  moveTrack(id: string, trackId: string, targetPositionIndex: number): Observable<void> {
    return this.http.post<void>(
      this.url(`/playlists/${encodeURIComponent(id)}/tracks/${encodeURIComponent(trackId)}/move`),
      { targetPositionIndex },
    );
  }
}
