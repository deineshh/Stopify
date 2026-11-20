import { DOCUMENT, Injectable, PLATFORM_ID, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Subject } from 'rxjs';
import type Hls from 'hls.js';

type HlsClass = typeof Hls;

/**
 * Plays one HLS audio stream at a time through a detached `<audio>` element.
 *
 * hls.js (its light build: no subtitles, DRM or alternate audio) is loaded on first use and drives Media Source Extensions where they
 * exist; browsers without them but with native HLS (older iOS Safari) get the URL directly.
 * On the server every method is a no-op.
 */
@Injectable({ providedIn: 'root' })
export class AudioEngine {
  private readonly document = inject(DOCUMENT);
  private readonly browser = isPlatformBrowser(inject(PLATFORM_ID));
  private audio: HTMLAudioElement | null = null;
  private hls: Hls | null = null;
  private hlsClass?: Promise<HlsClass | null>;
  /** Bumped by every load so a slow, superseded load can't attach its stream. */
  private loadId = 0;
  private mediaErrorRecovered = false;

  /** Seconds. */
  readonly currentTime = signal(0);
  /** Seconds; 0 until the stream's metadata is known. */
  readonly duration = signal(0);
  readonly playing = signal(false);
  readonly buffering = signal(false);
  readonly hasSource = signal(false);

  /** The stream played to its end. */
  readonly ended = new Subject<void>();
  /** The stream can't be played (network, decoding or unsupported browser). */
  readonly failed = new Subject<string>();

  async load(src: string, startSeconds = 0, autoplay = true): Promise<void> {
    const audio = this.element();
    if (!audio) {
      return;
    }
    const id = ++this.loadId;
    this.detach();
    this.currentTime.set(startSeconds);
    this.duration.set(0);
    this.buffering.set(true);
    this.hasSource.set(true);

    const HlsClass = await this.loadHls();
    if (id !== this.loadId) {
      return;
    }
    if (HlsClass) {
      const hls = new HlsClass({ startPosition: startSeconds });
      this.hls = hls;
      this.mediaErrorRecovered = false;
      hls.on(HlsClass.Events.ERROR, (_event, data) => {
        if (!data.fatal || id !== this.loadId) {
          return;
        }
        if (data.type === HlsClass.ErrorTypes.MEDIA_ERROR && !this.mediaErrorRecovered) {
          this.mediaErrorRecovered = true;
          hls.recoverMediaError();
          return;
        }
        this.fail(data.details);
      });
      hls.loadSource(src);
      hls.attachMedia(audio);
    } else if (audio.canPlayType('application/vnd.apple.mpegurl')) {
      audio.src = src;
      if (startSeconds > 0) {
        audio.addEventListener('loadedmetadata', () => (audio.currentTime = startSeconds), { once: true });
      }
    } else {
      this.fail('unsupported');
      return;
    }

    if (autoplay) {
      await this.play();
    }
  }

  async play(): Promise<void> {
    const audio = this.audio;
    if (!audio || !this.hasSource()) {
      return;
    }
    try {
      await audio.play();
    } catch (error) {
      // AbortError: a newer load interrupted this one. NotAllowedError: autoplay was blocked,
      // the user simply presses play again.
      if (!(error instanceof DOMException) || (error.name !== 'AbortError' && error.name !== 'NotAllowedError')) {
        this.fail(String(error));
      }
      this.playing.set(!audio.paused);
    }
  }

  pause(): void {
    this.audio?.pause();
  }

  seek(seconds: number): void {
    if (this.audio && this.hasSource()) {
      this.audio.currentTime = seconds;
    }
    this.currentTime.set(seconds);
  }

  /** 0–1. */
  setVolume(volume: number): void {
    const audio = this.element();
    if (audio) {
      audio.volume = Math.min(1, Math.max(0, volume));
    }
  }

  /** Stops and forgets the current stream. */
  stop(): void {
    this.loadId++;
    this.detach();
    this.currentTime.set(0);
    this.duration.set(0);
  }

  private fail(reason: string): void {
    this.detach();
    this.failed.next(reason);
  }

  private detach(): void {
    this.hls?.destroy();
    this.hls = null;
    const audio = this.audio;
    if (audio) {
      audio.pause();
      audio.removeAttribute('src');
      audio.load();
    }
    this.playing.set(false);
    this.buffering.set(false);
    this.hasSource.set(false);
  }

  private loadHls(): Promise<HlsClass | null> {
    this.hlsClass ??= import('hls.js/light').then(
      ({ default: HlsClass }) => (HlsClass.isSupported() ? HlsClass : null),
      () => null,
    );
    return this.hlsClass;
  }

  private element(): HTMLAudioElement | null {
    if (!this.browser) {
      return null;
    }
    if (!this.audio) {
      const audio = this.document.createElement('audio');
      audio.preload = 'auto';
      audio.addEventListener('timeupdate', () => this.currentTime.set(audio.currentTime));
      audio.addEventListener('durationchange', () =>
        this.duration.set(Number.isFinite(audio.duration) ? audio.duration : 0),
      );
      audio.addEventListener('playing', () => {
        this.playing.set(true);
        this.buffering.set(false);
      });
      audio.addEventListener('pause', () => this.playing.set(false));
      audio.addEventListener('waiting', () => this.buffering.set(true));
      audio.addEventListener('canplay', () => this.buffering.set(false));
      audio.addEventListener('ended', () => {
        this.playing.set(false);
        this.ended.next();
      });
      audio.addEventListener('error', () => {
        // hls.js reports its own errors; this catches native playback failures.
        if (!this.hls && this.hasSource()) {
          this.fail(audio.error?.message || 'media error');
        }
      });
      this.audio = audio;
    }
    return this.audio;
  }
}
