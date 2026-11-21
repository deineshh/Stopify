import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, of, throwError } from 'rxjs';
import { injectApiUrl } from '@core/api/api.config';
import { toApiError } from '@core/api/api-error';
import {
  PlaybackQueueDto,
  PlaybackSessionDto,
  PlaybackStreamDto,
  StartPlaybackRequestDto,
} from './playback.dtos';

/**
 * Typed client for the Streaming module's playback session (`/me/playback`). The session lives
 * on the server, so every control is also sent there; the browser only plays the stream it returns.
 */
@Injectable({ providedIn: 'root' })
export class PlaybackApi {
  private readonly http = inject(HttpClient);
  private readonly url = injectApiUrl();

  /** The user's session, or null when they have never played anything. */
  session(): Observable<PlaybackSessionDto | null> {
    return this.http.get<PlaybackSessionDto>(this.url('/me/playback')).pipe(
      catchError((error: unknown) => {
        const apiError = toApiError(error);
        return apiError.status === 404 ? of(null) : throwError(() => apiError);
      }),
    );
  }

  play(request: StartPlaybackRequestDto): Observable<PlaybackStreamDto> {
    return this.http.post<PlaybackStreamDto>(this.url('/me/playback/play'), request);
  }

  resume(deviceId: string): Observable<void> {
    return this.http.post<void>(this.url('/me/playback/resume'), { deviceId });
  }

  pause(deviceId: string): Observable<void> {
    return this.http.post<void>(this.url('/me/playback/pause'), { deviceId });
  }

  /** Records progress without moving playback (ignored unless this device is the active one). */
  sync(deviceId: string, positionMs: number): Observable<void> {
    return this.http.patch<void>(this.url('/me/playback/sync'), { deviceId, positionMs: Math.round(positionMs) });
  }

  seek(deviceId: string, positionMs: number): Observable<void> {
    return this.http.patch<void>(this.url('/me/playback/seek'), { deviceId, positionMs: Math.round(positionMs) });
  }

  next(deviceId: string): Observable<PlaybackStreamDto> {
    return this.http.post<PlaybackStreamDto>(this.url('/me/playback/next'), { deviceId });
  }

  previous(deviceId: string): Observable<PlaybackStreamDto> {
    return this.http.post<PlaybackStreamDto>(this.url('/me/playback/previous'), { deviceId });
  }

  toggleShuffle(deviceId: string): Observable<boolean> {
    return this.http
      .patch<{ isShuffled: boolean }>(this.url('/me/playback/shuffle'), { deviceId })
      .pipe(map((response) => response.isShuffled));
  }

  /** Cycles Off → All → Track and returns the new mode. */
  toggleRepeat(deviceId: string): Observable<string> {
    return this.http
      .patch<{ repeatMode: string }>(this.url('/me/playback/repeat'), { deviceId })
      .pipe(map((response) => response.repeatMode));
  }

  queue(): Observable<PlaybackQueueDto> {
    return this.http.get<PlaybackQueueDto>(this.url('/me/playback/queue'));
  }

  addToQueue(trackId: string): Observable<void> {
    return this.http.post<void>(this.url('/me/playback/queue'), { trackId });
  }

  removeFromQueue(trackId: string): Observable<void> {
    return this.http.delete<void>(this.url(`/me/playback/queue/${encodeURIComponent(trackId)}`));
  }
}
