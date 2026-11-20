import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: number;
  text: string;
}

const DEFAULT_DURATION_MS = 4000;

/** One short message at a time at the bottom of the screen (Spotify's blue toast). */
@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 0;
  private timer?: ReturnType<typeof setTimeout>;
  readonly current = signal<Toast | null>(null);

  show(text: string, durationMs = DEFAULT_DURATION_MS): void {
    clearTimeout(this.timer);
    const toast = { id: ++this.nextId, text };
    this.current.set(toast);
    this.timer = setTimeout(() => {
      if (this.current()?.id === toast.id) {
        this.current.set(null);
      }
    }, durationMs);
  }

  dismiss(): void {
    clearTimeout(this.timer);
    this.current.set(null);
  }
}
