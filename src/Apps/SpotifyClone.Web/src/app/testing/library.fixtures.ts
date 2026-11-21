import { HttpRequest } from '@angular/common/http';
import { HttpTestingController } from '@angular/common/http/testing';

/** Your Library's two requests: the user's playlists and the ones they collaborate on. */
export const isOwnPlaylistsRequest = (req: HttpRequest<unknown>) => req.url === '/api/v1/me/playlists';
export const isCollaborativePlaylistsRequest = (req: HttpRequest<unknown>) =>
  req.url === '/api/v1/playlists' && req.params.has('collaboratorIds');

/** Answers one load of Your Library. */
export function flushLibrary(http: HttpTestingController, own: object[] = [], collaborative: object[] = []): void {
  http.expectOne(isCollaborativePlaylistsRequest).flush(collaborative);
  http.expectOne(isOwnPlaylistsRequest).flush(own);
}

/** Answers every pending library load with empty lists (when a test doesn't care about the library). */
export function flushAnyLibrary(http: HttpTestingController): void {
  for (const req of http.match(isCollaborativePlaylistsRequest)) {
    req.flush([]);
  }
  for (const req of http.match(isOwnPlaylistsRequest)) {
    req.flush([]);
  }
}
