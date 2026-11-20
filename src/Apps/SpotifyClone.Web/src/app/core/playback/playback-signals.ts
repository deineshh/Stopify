import { DestroyRef, Injectable, InjectionToken, PLATFORM_ID, effect, inject, untracked } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Subject } from 'rxjs';
import { API_BASE_URL } from '@core/api/api.config';
import { AuthStore } from '@core/auth/auth.store';
import { DEVICE_ID } from './device-id';

/** The part of a SignalR `HubConnection` this app uses. */
export interface HubConnectionLike {
  on(method: string, handler: () => void): void;
  start(): Promise<void>;
  stop(): Promise<void>;
}

export type HubConnectionFactory = (url: string, accessToken: () => string) => Promise<HubConnectionLike>;

/** Builds the SignalR connection; `@microsoft/signalr` is only downloaded once someone signs in. */
export const HUB_CONNECTION_FACTORY = new InjectionToken<HubConnectionFactory>('HUB_CONNECTION_FACTORY', {
  providedIn: 'root',
  factory: () => async (url, accessToken) => {
    const signalR = await import('@microsoft/signalr');
    return new signalR.HubConnectionBuilder()
      .withUrl(url, { accessTokenFactory: accessToken })
      .withAutomaticReconnect()
      // The hub is optional (see PlaybackSignals): failures are retried quietly instead of logged.
      .configureLogging(signalR.LogLevel.None)
      .build();
  },
});

const HUB_PATH = '/hubs/streaming';
/** Waits before retrying a failed first connection (automatic reconnect only covers drops later on). */
const RETRY_DELAYS_MS = [2_000, 10_000, 30_000];

/**
 * Listens to the Streaming hub while signed in. When playback starts on another device the API
 * sends `ReceiveStopPlayback` to this one, which surfaces as `stop`. Playback still works without
 * the connection; it only means this tab doesn't hear about other devices.
 */
@Injectable({ providedIn: 'root' })
export class PlaybackSignals {
  private readonly auth = inject(AuthStore);
  private readonly createConnection = inject(HUB_CONNECTION_FACTORY);
  private readonly url = `${inject(API_BASE_URL).replace(/\/$/, '')}${HUB_PATH}?deviceId=${encodeURIComponent(inject(DEVICE_ID))}`;

  /** Another device took over playback. */
  readonly stop = new Subject<void>();

  private connection: HubConnectionLike | null = null;
  /** Bumped on every sign-in/out so a slow, outdated connect gives up. */
  private generation = 0;
  private retryTimer?: ReturnType<typeof setTimeout>;

  constructor() {
    if (!isPlatformBrowser(inject(PLATFORM_ID))) {
      return;
    }
    effect(() => {
      const userId = this.auth.user()?.id;
      untracked(() => {
        const generation = ++this.generation;
        this.close();
        if (userId) {
          void this.connect(generation, 0);
        }
      });
    });
    inject(DestroyRef).onDestroy(() => {
      this.generation++;
      this.close();
    });
  }

  private async connect(generation: number, attempt: number): Promise<void> {
    try {
      const connection = await this.createConnection(this.url, () => this.auth.accessToken() ?? '');
      if (generation !== this.generation) {
        return;
      }
      connection.on('ReceiveStopPlayback', () => this.stop.next());
      this.connection = connection;
      await connection.start();
    } catch {
      if (generation === this.generation && attempt < RETRY_DELAYS_MS.length) {
        this.connection = null;
        this.retryTimer = setTimeout(() => void this.connect(generation, attempt + 1), RETRY_DELAYS_MS[attempt]);
      }
    }
  }

  private close(): void {
    clearTimeout(this.retryTimer);
    const connection = this.connection;
    this.connection = null;
    void connection?.stop().catch(() => undefined);
  }
}
