import { HUB_CONNECTION_FACTORY, HubConnectionLike } from '@core/playback/playback-signals';

/** A SignalR connection that never touches the network; `emit` plays the server's part. */
export class FakeHubConnection implements HubConnectionLike {
  readonly handlers = new Map<string, () => void>();
  started = false;
  stopped = false;

  constructor(
    readonly url: string,
    readonly accessToken: () => string,
  ) {}

  on(method: string, handler: () => void): void {
    this.handlers.set(method, handler);
  }

  async start(): Promise<void> {
    this.started = true;
  }

  async stop(): Promise<void> {
    this.stopped = true;
  }

  emit(method: string): void {
    this.handlers.get(method)?.();
  }
}

export function fakeHub() {
  const connections: FakeHubConnection[] = [];
  const factory = async (url: string, accessToken: () => string) => {
    const connection = new FakeHubConnection(url, accessToken);
    connections.push(connection);
    return connection;
  };
  return {
    connections,
    latest: () => connections.at(-1),
    provider: { provide: HUB_CONNECTION_FACTORY, useValue: factory },
  };
}
