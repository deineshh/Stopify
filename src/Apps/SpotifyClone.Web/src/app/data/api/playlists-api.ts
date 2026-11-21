import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { injectApiUrl } from '@core/api/api.config';

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
 * The API only lets the owner edit or delete; the owner and collaborators add songs.
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
}
