import { signal } from '@angular/core';
import { Subject } from 'rxjs';
import { AudioEngine } from '@core/playback/audio-engine';

/** `AudioEngine` without audio: records loads and flips the same signals the real one does. */
export class FakeAudioEngine {
  readonly currentTime = signal(0);
  readonly duration = signal(0);
  readonly playing = signal(false);
  readonly buffering = signal(false);
  readonly hasSource = signal(false);
  readonly ended = new Subject<void>();
  readonly failed = new Subject<string>();
  volume = 1;

  readonly load = vi.fn(async (_src: string, startSeconds = 0, autoplay = true) => {
    this.hasSource.set(true);
    this.currentTime.set(startSeconds);
    this.duration.set(180);
    this.playing.set(autoplay);
  });
  readonly play = vi.fn(async () => this.playing.set(this.hasSource()));
  readonly pause = vi.fn(() => this.playing.set(false));
  readonly seek = vi.fn((seconds: number) => this.currentTime.set(seconds));
  readonly setVolume = vi.fn((volume: number) => (this.volume = volume));
  readonly stop = vi.fn(() => {
    this.hasSource.set(false);
    this.playing.set(false);
    this.currentTime.set(0);
  });
}

export function fakeAudioEngine() {
  const engine = new FakeAudioEngine();
  return { engine, provider: { provide: AudioEngine, useValue: engine } };
}
