import { DestroyRef, Injectable, PLATFORM_ID, computed, effect, inject, signal, untracked } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';
import { EMPTY, Observable, Subscription, catchError, interval, map, of, switchMap, tap, throwError } from 'rxjs';
import { toApiError } from '@core/api/api-error';
import { AuthStore } from '@core/auth/auth.store';
import { ToastService } from '@core/notifications/toast.service';
import { AudioEngine } from '@core/playback/audio-engine';
import { DEVICE_ID } from '@core/playback/device-id';
import { PlaybackSignals } from '@core/playback/playback-signals';
import { CatalogApi } from '../api/catalog-api';
import { PlaybackApi } from '../api/playback-api';
import { PlaybackSessionDto, PlaybackStreamDto } from '../api/playback.dtos';
import { Track } from '../models/catalog.models';
import { PlaybackContextRef, PlaybackTarget, RepeatMode } from '../models/player.models';
import { TrackLookup } from './track-lookup.service';

/** How often progress is reported while playing, so another device can pick up where this one was. */
const SYNC_INTERVAL_MS = 10_000;
/** "Previous" restarts the song instead once it has played this long (like Spotify). */
const RESTART_THRESHOLD_SECONDS = 3;
const VOLUME_KEY = 'stopify.volume';

/**
 * The player. The playback session (current track, queue, shuffle, repeat, active device) lives
 * on the server; this service sends every control there and plays the HLS stream it hands back
 * through `AudioEngine`. Guests are sent to log in when they press play.
 */
@Injectable({ providedIn: 'root' })
export class PlayerService {
  private readonly api = inject(PlaybackApi);
  private readonly catalog = inject(CatalogApi);
  private readonly lookup = inject(TrackLookup);
  private readonly engine = inject(AudioEngine);
  private readonly auth = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly deviceId = inject(DEVICE_ID);

  /** The current song; null until something has been played (or a session restored). */
  readonly track = signal<Track | null>(null);
  readonly context = signal<PlaybackContextRef | null>(null);
  /** What the user pressed play on (an artist resolves to an album context). */
  private readonly source = signal<PlaybackTarget | null>(null);

  readonly isPlaying = this.engine.playing.asReadonly();
  readonly isBuffering = this.engine.buffering.asReadonly();
  readonly shuffle = signal(false);
  readonly repeat = signal<RepeatMode>('off');
  /** UI only: the API has no Liked Songs yet. */
  readonly saved = signal(false);

  /** 0–100. */
  readonly volume = signal(readStoredVolume());
  readonly muted = signal(false);
  readonly effectiveVolume = computed(() => (this.muted() ? 0 : this.volume()));

  /** Where a restored session stopped, until its stream is loaded. */
  private readonly restoredPosition = signal(0);
  /** Slider position (0–100) while the user drags it. */
  private readonly scrubbing = signal<number | null>(null);

  /** The stream's length, rounded: AAC encoding makes it a few ms shorter than the catalog's (2:26.98 → 2:27). */
  readonly durationSeconds = computed(() => Math.round(this.engine.duration()) || this.track()?.durationSeconds || 0);
  readonly positionSeconds = computed(() => {
    const scrub = this.scrubbing();
    if (scrub !== null) {
      return (scrub / 100) * this.durationSeconds();
    }
    return this.engine.hasSource() ? this.engine.currentTime() : this.restoredPosition();
  });
  /** Playback progress, 0–100. */
  readonly progress = computed(() => {
    const duration = this.durationSeconds();
    return duration > 0 ? Math.min(100, (this.positionSeconds() / duration) * 100) : 0;
  });

  private loading?: Subscription;

  constructor() {
    effect(() => {
      this.engine.setVolume(this.effectiveVolume() / 100);
      storeVolume(this.volume());
    });

    // Restore the session when someone signs in; forget it on sign-out.
    effect(() => {
      const userId = this.auth.user()?.id;
      untracked(() => (userId ? this.restore() : this.reset()));
    });

    const destroyRef = inject(DestroyRef);
    const ended = this.engine.ended.subscribe(() => this.next());
    const failed = this.engine.failed.subscribe(() => this.toast.show("Couldn't play this song."));
    // Another device started playing: stop here (pressing play takes the session back).
    const movedAway = inject(PlaybackSignals).stop.subscribe(() => {
      if (this.isPlaying()) {
        this.engine.pause();
        this.toast.show('Playing on another device.');
      }
    });
    const browser = isPlatformBrowser(inject(PLATFORM_ID));
    const sync = (browser ? interval(SYNC_INTERVAL_MS) : EMPTY).subscribe(() => {
      if (this.isPlaying()) {
        this.api.sync(this.deviceId, this.engine.currentTime() * 1000).subscribe({ error: () => undefined });
      }
    });
    destroyRef.onDestroy(() => {
      ended.unsubscribe();
      failed.unsubscribe();
      movedAway.unsubscribe();
      sync.unsubscribe();
    });
  }

  // ---------------------------------------------------------------- what's playing

  /** True while `target` is what's playing (an album card, a playlist header, one track row…). */
  isPlayingTarget(target: PlaybackTarget): boolean {
    return this.isPlaying() && this.isCurrentTarget(target);
  }

  isCurrentTarget(target: PlaybackTarget): boolean {
    if (target.kind === 'artist') {
      const source = this.source();
      return source?.kind === 'artist' && source.id === target.id;
    }
    const context = this.context();
    if (context?.kind !== target.kind || context.id !== target.id) {
      return false;
    }
    return !target.trackId || this.track()?.id === target.trackId;
  }

  // ---------------------------------------------------------------- controls

  /** Play/pause button of a card, header or row: pauses what's playing, otherwise plays `target`. */
  toggle(target: PlaybackTarget): void {
    if (this.isCurrentTarget(target)) {
      this.togglePlay();
    } else {
      this.play(target);
    }
  }

  play(target: PlaybackTarget): void {
    if (!this.auth.isAuthenticated()) {
      void this.router.navigate(['/login'], { queryParams: { redirect: this.router.url } });
      return;
    }
    this.run(
      this.resolve(target).pipe(
        switchMap((context) => {
          if (!context) {
            return throwError(() => new NothingToPlayError());
          }
          return this.api
            .play({
              deviceId: this.deviceId,
              contextType: context.kind,
              contextExternalId: context.id,
              startTrackId: context.trackId ?? null,
            })
            .pipe(
              tap(() => {
                this.context.set({ kind: context.kind, id: context.id });
                this.source.set(target);
              }),
            );
        }),
      ),
    );
  }

  togglePlay(): void {
    const track = this.track();
    if (!track) {
      return;
    }
    if (this.isPlaying()) {
      this.engine.pause();
      // Pause doesn't carry a position, so record it first.
      this.api
        .sync(this.deviceId, this.engine.currentTime() * 1000)
        .pipe(switchMap(() => this.api.pause(this.deviceId)))
        .subscribe({ error: () => undefined });
    } else if (this.engine.hasSource()) {
      void this.engine.play();
      // Resuming also moves the session to this device if another one had it.
      this.api.resume(this.deviceId).subscribe({ error: () => undefined });
    } else {
      this.resumeRestored(track);
    }
  }

  next(): void {
    if (this.track()) {
      this.run(this.onThisDevice(() => this.api.next(this.deviceId)));
    }
  }

  previous(): void {
    if (!this.track()) {
      return;
    }
    if (this.positionSeconds() > RESTART_THRESHOLD_SECONDS || !this.engine.hasSource()) {
      this.seekTo(0);
      return;
    }
    this.run(this.onThisDevice(() => this.api.previous(this.deviceId)), { restartWhenEmpty: true });
  }

  /** While dragging the progress slider (0–100): moves the thumb and the time, not the audio. */
  scrub(percent: number): void {
    this.scrubbing.set(percent);
  }

  /** Drag released / key pressed on the progress slider (0–100). */
  seek(percent: number): void {
    this.scrubbing.set(null);
    this.seekTo((percent / 100) * this.durationSeconds());
  }

  toggleShuffle(): void {
    if (this.track()) {
      this.onThisDevice(() => this.api.toggleShuffle(this.deviceId)).subscribe({
        next: (on) => this.shuffle.set(on),
        error: () => this.toast.show("Couldn't change shuffle."),
      });
    }
  }

  cycleRepeat(): void {
    if (this.track()) {
      this.onThisDevice(() => this.api.toggleRepeat(this.deviceId)).subscribe({
        next: (mode) => this.repeat.set(toRepeatMode(mode)),
        error: () => this.toast.show("Couldn't change repeat."),
      });
    }
  }

  toggleSaved(): void {
    this.saved.update((saved) => !saved);
  }

  toggleMute(): void {
    this.muted.update((muted) => !muted);
  }

  setVolume(percent: number): void {
    this.volume.set(percent);
    this.muted.set(percent === 0);
  }

  // ---------------------------------------------------------------- internals

  private seekTo(seconds: number): void {
    if (!this.engine.hasSource()) {
      this.restoredPosition.set(seconds);
      return;
    }
    this.engine.seek(seconds);
    this.onThisDevice(() => this.api.seek(this.deviceId, seconds * 1000)).subscribe({ error: () => undefined });
  }

  /** Plays the stream a play/next/previous call returns; the latest call wins. */
  private run(request: Observable<PlaybackStreamDto>, options: { restartWhenEmpty?: boolean } = {}): void {
    this.loading?.unsubscribe();
    this.loading = request.subscribe({
      next: (stream) => {
        if (stream.trackId && stream.hlsUrl) {
          this.startStream(stream.trackId, stream.hlsUrl, (stream.startPositionMs ?? 0) / 1000);
        } else if (options.restartWhenEmpty) {
          this.seekTo(0);
        } else {
          // End of the queue: stop at the start of the last song.
          this.engine.pause();
          this.engine.seek(0);
        }
      },
      error: (error: unknown) => this.toast.show(playbackErrorMessage(error)),
    });
  }

  private startStream(trackId: string, url: string, startSeconds: number, autoplay = true): void {
    this.restoredPosition.set(0);
    if (this.track()?.id !== trackId) {
      this.saved.set(false);
      this.lookup.track(trackId).subscribe((track) => this.track.set(track));
    }
    void this.engine.load(url, startSeconds, autoplay);
  }

  /** A restored session has no stream yet: start its context again at the same song and position. */
  private resumeRestored(track: Track): void {
    const context = this.context() ?? (track.album.id ? { kind: 'album' as const, id: track.album.id } : null);
    if (!context) {
      return;
    }
    const position = this.restoredPosition();
    this.loading?.unsubscribe();
    this.loading = this.api
      .play({ deviceId: this.deviceId, contextType: context.kind, contextExternalId: context.id, startTrackId: track.id })
      .pipe(
        switchMap((stream) => {
          const seek: Observable<unknown> = position > 0 ? this.api.seek(this.deviceId, position * 1000) : of(null);
          return seek.pipe(map(() => stream));
        }),
      )
      .subscribe({
        next: (stream) => {
          if (stream.trackId && stream.hlsUrl) {
            this.context.set(context);
            this.startStream(stream.trackId, stream.hlsUrl, position);
          }
        },
        error: (error: unknown) => this.toast.show(playbackErrorMessage(error)),
      });
  }

  /**
   * Skip, seek, shuffle and repeat only work from the active device. If another device took over,
   * resume here first (that moves the session) and try once more.
   */
  private onThisDevice<T>(call: () => Observable<T>): Observable<T> {
    return call().pipe(
      catchError((error: unknown) => {
        const apiError = toApiError(error);
        return apiError.has('Playback.InvalidDevice')
          ? this.api.resume(this.deviceId).pipe(switchMap(() => call()))
          : throwError(() => apiError);
      }),
    );
  }

  /** Albums and playlists play as they are; an artist plays their newest track's album from that track. */
  private resolve(target: PlaybackTarget): Observable<{ kind: 'album' | 'playlist'; id: string; trackId?: string } | null> {
    if (target.kind !== 'artist') {
      return of(target);
    }
    return this.catalog.tracks({ mainArtistIds: [target.id] }, { pageSize: 1 }).pipe(
      map(({ items: [track] }) => (track?.albumId ? { kind: 'album' as const, id: track.albumId, trackId: track.id } : null)),
    );
  }

  private restore(): void {
    this.api.session().subscribe({
      next: (session) => (session?.trackId ? this.applySession(session, session.trackId) : undefined),
      error: () => undefined,
    });
  }

  private applySession(session: PlaybackSessionDto, trackId: string): void {
    const kind = session.contextType.toLowerCase();
    this.context.set(
      (kind === 'album' || kind === 'playlist') && session.contextExternalId ? { kind, id: session.contextExternalId } : null,
    );
    this.shuffle.set(session.isShuffled);
    this.repeat.set(toRepeatMode(session.repeatMode));
    this.restoredPosition.set(session.currentPositionMs / 1000);
    this.lookup.track(trackId).subscribe((track) => this.track.set(track));
  }

  private reset(): void {
    this.loading?.unsubscribe();
    this.engine.stop();
    this.track.set(null);
    this.context.set(null);
    this.source.set(null);
    this.shuffle.set(false);
    this.repeat.set('off');
    this.restoredPosition.set(0);
  }
}

function toRepeatMode(mode: string): RepeatMode {
  switch (mode.toLowerCase()) {
    case 'all':
    case '1':
      return 'all';
    case 'track':
    case '2':
      return 'one';
    default:
      return 'off';
  }
}

class NothingToPlayError extends Error {}

function playbackErrorMessage(error: unknown): string {
  if (error instanceof NothingToPlayError) {
    return 'There is nothing to play here yet.';
  }
  const apiError = toApiError(error);
  if (apiError.isNetworkError) {
    return "Couldn't reach Stopify. Check your connection.";
  }
  if (apiError.status === 404) {
    return "This song isn't available right now.";
  }
  if (apiError.has('Playback.EmptyQueue')) {
    return 'There is nothing to play here yet.';
  }
  return "Couldn't start playback.";
}

function readStoredVolume(): number {
  try {
    const value = Number(globalThis.localStorage?.getItem(VOLUME_KEY));
    return Number.isFinite(value) && value > 0 && value <= 100 ? value : 50;
  } catch {
    return 50;
  }
}

function storeVolume(volume: number): void {
  try {
    globalThis.localStorage?.setItem(VOLUME_KEY, String(volume));
  } catch {
    // Storage full or blocked: the volume just isn't remembered.
  }
}
