import { Injectable, computed, signal } from '@angular/core';
import { NOW_PLAYING, QUEUE } from '../mock/player.mock';
import { RepeatMode } from '../models/player.models';

const REPEAT_ORDER: readonly RepeatMode[] = ['off', 'all', 'one'];

/**
 * Player UI state. Nothing is actually streamed yet: the controls only update
 * this state so every button, slider and toggle in the design reacts like the real thing.
 */
@Injectable({ providedIn: 'root' })
export class PlayerService {
  readonly nowPlaying = signal(NOW_PLAYING).asReadonly();
  readonly queue = signal(QUEUE).asReadonly();

  readonly isPlaying = signal(false);
  readonly shuffle = signal(false);
  readonly repeat = signal<RepeatMode>('off');
  readonly saved = signal(NOW_PLAYING.track.saved ?? false);

  /** Playback progress, 0–100. */
  readonly progress = signal(32);
  /** Volume, 0–100. */
  readonly volume = signal(32);
  readonly muted = signal(false);

  readonly positionSeconds = computed(() =>
    Math.round((this.progress() / 100) * this.nowPlaying().track.durationSeconds),
  );
  readonly effectiveVolume = computed(() => (this.muted() ? 0 : this.volume()));

  togglePlay(): void {
    this.isPlaying.update((playing) => !playing);
  }

  toggleShuffle(): void {
    this.shuffle.update((on) => !on);
  }

  cycleRepeat(): void {
    this.repeat.update((mode) => REPEAT_ORDER[(REPEAT_ORDER.indexOf(mode) + 1) % REPEAT_ORDER.length]);
  }

  toggleSaved(): void {
    this.saved.update((saved) => !saved);
  }

  toggleMute(): void {
    this.muted.update((muted) => !muted);
  }

  seek(percent: number): void {
    this.progress.set(percent);
  }

  setVolume(percent: number): void {
    this.volume.set(percent);
    this.muted.set(percent === 0);
  }
}
