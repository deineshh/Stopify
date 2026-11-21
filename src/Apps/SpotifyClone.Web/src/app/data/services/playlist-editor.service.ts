import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { EMPTY, Observable, Subject, catchError, concatMap, finalize, map, switchMap, tap } from 'rxjs';
import { toApiError } from '@core/api/api-error';
import { AuthStore } from '@core/auth/auth.store';
import { ToastService } from '@core/notifications/toast.service';
import { PlaylistDetailsChange, PlaylistsApi } from '../api/playlists-api';
import { Collaborator, PlaylistRef } from '../models/playlist.models';
import { LibraryService } from './library.service';

/**
 * Every change to a playlist goes through here: it calls the API, refreshes Your Library and
 * bumps the playlist's revision so an open playlist page reloads, and tells the user how it went.
 *
 * Methods return cold observables that emit once on success. Failures are reported as a toast
 * and complete without emitting, except `editDetails`, whose dialog shows the error inline.
 * Guests are sent to log in instead.
 */
@Injectable({ providedIn: 'root' })
export class PlaylistEditor {
  private readonly api = inject(PlaylistsApi);
  private readonly library = inject(LibraryService);
  private readonly toast = inject(ToastService);
  private readonly auth = inject(AuthStore);
  private readonly router = inject(Router);

  private readonly revisions = signal<ReadonlyMap<string, number>>(new Map());

  /** Moves not answered yet, per playlist. */
  private readonly pendingMoves = signal<ReadonlyMap<string, number>>(new Map());
  /** Playlists whose last move failed: their queued moves were worked out from a wrong order. */
  private readonly failedMoves = new Set<string>();
  /**
   * Moves are sent one at a time, in order: each is worked out from the order the previous one
   * left. The queue lives here rather than on the page so leaving the page doesn't cancel them.
   */
  private readonly moves = new Subject<QueuedMove>();

  constructor() {
    this.moves.pipe(concatMap((move) => this.sendMove(move))).subscribe();
  }

  /** Changes whenever this playlist was changed from here; pages key their data on it. */
  revision(playlistId: string): number {
    return this.revisions().get(playlistId) ?? 0;
  }

  /** Creates "My Playlist #N" and opens it. */
  create(): Observable<string> {
    return this.signedIn(() =>
      this.api.create().pipe(
        tap((id) => {
          this.changed(id);
          void this.router.navigate(['/playlist', id]);
        }),
        this.reportFailure("Couldn't create a playlist."),
      ),
    );
  }

  /** Creates a playlist with this song in it (Add to playlist → New playlist) and opens it. */
  createWith(trackId: string): Observable<string> {
    return this.signedIn(() =>
      this.api.create().pipe(
        switchMap((id) => this.api.addTrack(id, trackId).pipe(map(() => id))),
        tap((id) => {
          this.changed(id);
          this.toast.show('Added to a new playlist.');
          void this.router.navigate(['/playlist', id]);
        }),
        this.reportFailure("Couldn't create a playlist."),
      ),
    );
  }

  /** Errors are passed on (as ApiError) so the edit dialog can show them next to the fields. */
  editDetails(playlist: PlaylistRef, change: PlaylistDetailsChange): Observable<void> {
    return this.api.editDetails(playlist.id, change).pipe(
      tap(() => this.changed(playlist.id)),
      catchError((error: unknown) => {
        throw toApiError(error);
      }),
    );
  }

  setPublic(playlist: PlaylistRef, isPublic: boolean): Observable<void> {
    return this.signedIn(() =>
      this.api.editDetails(playlist.id, { name: playlist.name, description: playlist.description, isPublic }).pipe(
        tap(() => {
          this.changed(playlist.id);
          this.toast.show(isPublic ? `${playlist.name} is now public.` : `${playlist.name} is now private.`);
        }),
        this.reportFailure("Couldn't change who can see this playlist."),
      ),
    );
  }

  delete(playlist: PlaylistRef): Observable<void> {
    return this.signedIn(() =>
      this.api.delete(playlist.id).pipe(
        tap(() => {
          // No revision bump: an open page for it would only reload into "not found".
          this.library.reload();
          this.toast.show(`Removed ${playlist.name} from Your Library.`);
          if (this.router.url.split(/[?#]/)[0] === `/playlist/${playlist.id}`) {
            void this.router.navigateByUrl('/');
          }
        }),
        this.reportFailure("Couldn't delete this playlist."),
      ),
    );
  }

  addTrack(playlist: Pick<PlaylistRef, 'id' | 'name'>, trackId: string): Observable<void> {
    return this.signedIn(() =>
      this.api.addTrack(playlist.id, trackId).pipe(
        tap(() => {
          this.changed(playlist.id);
          this.toast.show(`Added to ${playlist.name}.`);
        }),
        catchError((error: unknown) => {
          const apiError = toApiError(error);
          this.toast.show(
            apiError.has('Playlist.InvalidTrack')
              ? `This song is already in ${playlist.name}.`
              : failureMessage(apiError, `Couldn't add this song to ${playlist.name}.`),
          );
          return EMPTY;
        }),
      ),
    );
  }

  removeTrack(playlist: Pick<PlaylistRef, 'id' | 'name'>, trackId: string): Observable<void> {
    return this.signedIn(() =>
      this.api.removeTrack(playlist.id, trackId).pipe(
        tap(() => {
          this.changed(playlist.id);
          this.toast.show(`Removed from ${playlist.name}.`);
        }),
        catchError((error: unknown) => {
          const apiError = toApiError(error);
          // Collaborators may only remove the songs they added themselves.
          this.toast.show(
            apiError.has('Playlist.InvalidCollaborator')
              ? 'You can only remove songs you added.'
              : failureMessage(apiError, `Couldn't remove this song from ${playlist.name}.`),
          );
          return EMPTY;
        }),
      ),
    );
  }

  /** Owner only: lets someone add, remove and reorder songs. */
  addCollaborator(playlist: Pick<PlaylistRef, 'id' | 'name'>, user: Pick<Collaborator, 'id' | 'name'>): Observable<void> {
    return this.signedIn(() =>
      this.api.addCollaborator(playlist.id, user.id).pipe(
        tap(() => {
          this.changed(playlist.id);
          this.toast.show(`${user.name} can now edit ${playlist.name}.`);
        }),
        catchError((error: unknown) => {
          const apiError = toApiError(error);
          this.toast.show(
            apiError.has('Playlist.InvalidCollaborator') && apiError.status !== 403
              ? `Couldn't add ${user.name}. They may already be a collaborator.`
              : failureMessage(apiError, `Couldn't add ${user.name}.`),
          );
          return EMPTY;
        }),
      ),
    );
  }

  removeCollaborator(playlist: Pick<PlaylistRef, 'id' | 'name'>, user: Pick<Collaborator, 'id' | 'name'>): Observable<void> {
    return this.signedIn(() =>
      this.api.removeCollaborator(playlist.id, user.id).pipe(
        tap(() => {
          this.changed(playlist.id);
          this.toast.show(`${user.name} can no longer edit ${playlist.name}.`);
        }),
        this.reportFailure(`Couldn't remove ${user.name}.`),
      ),
    );
  }

  /**
   * Moves a song to `targetIndex` of the playlist's full list (see `moveInPlaylist`). The page
   * shows the new order straight away; once every queued move of the playlist is answered it
   * reloads, which also brings back the real order after a failure.
   */
  moveTrack(playlist: Pick<PlaylistRef, 'id' | 'name'>, trackId: string, targetIndex: number): void {
    if (!this.auth.isAuthenticated()) {
      this.signedIn(() => EMPTY).subscribe();
      return;
    }
    this.pendingMoves.update((pending) => new Map(pending).set(playlist.id, (pending.get(playlist.id) ?? 0) + 1));
    this.moves.next({ playlist, trackId, targetIndex });
  }

  /** Whether moves of this playlist are still on their way (its page keeps its own order meanwhile). */
  isMoving(playlistId: string): boolean {
    return (this.pendingMoves().get(playlistId) ?? 0) > 0;
  }

  private sendMove({ playlist, trackId, targetIndex }: QueuedMove): Observable<void> {
    const request = this.failedMoves.has(playlist.id)
      ? EMPTY
      : this.api.moveTrack(playlist.id, trackId, targetIndex).pipe(
          catchError((error: unknown) => {
            this.failedMoves.add(playlist.id);
            this.toast.show(failureMessage(toApiError(error), "Couldn't move this song."));
            return EMPTY;
          }),
        );
    return request.pipe(
      finalize(() => {
        const left = (this.pendingMoves().get(playlist.id) ?? 1) - 1;
        this.pendingMoves.update((pending) => {
          const next = new Map(pending);
          if (left > 0) {
            next.set(playlist.id, left);
          } else {
            next.delete(playlist.id);
          }
          return next;
        });
        if (left === 0) {
          this.failedMoves.delete(playlist.id);
          this.changed(playlist.id);
        }
      }),
    );
  }

  private changed(playlistId: string): void {
    this.revisions.update((revisions) => new Map(revisions).set(playlistId, (revisions.get(playlistId) ?? 0) + 1));
    // Names, covers (generated from the songs) and the list itself can all change.
    this.library.reload();
  }

  private signedIn<T>(run: () => Observable<T>): Observable<T> {
    if (this.auth.isAuthenticated()) {
      return run();
    }
    void this.router.navigate(['/login'], { queryParams: { redirect: this.router.url } });
    return EMPTY;
  }

  private reportFailure<T>(fallback: string) {
    return catchError<T, Observable<never>>((error: unknown) => {
      this.toast.show(failureMessage(toApiError(error), fallback));
      return EMPTY;
    });
  }
}

interface QueuedMove {
  playlist: Pick<PlaylistRef, 'id' | 'name'>;
  trackId: string;
  targetIndex: number;
}

function failureMessage(error: ReturnType<typeof toApiError>, fallback: string): string {
  if (error.isNetworkError) {
    return "Couldn't reach Stopify. Check your connection.";
  }
  if (error.status === 403 || error.has('Playlist.NotOwned') || error.has('Playlist.InvalidCollaborator')) {
    return "You can't change this playlist.";
  }
  if (error.status === 404) {
    return 'This playlist no longer exists.';
  }
  return fallback;
}
