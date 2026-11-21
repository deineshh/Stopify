import { TestBed } from '@angular/core/testing';
import { DEVICE_ID } from './device-id';
import { PlaybackSignals } from './playback-signals';
import { mockAuthStore } from '../../testing/auth-store.mock';
import { fakeHub } from '../../testing/hub.fake';

describe('PlaybackSignals', () => {
  function setup(status: 'anonymous' | 'authenticated') {
    const auth = mockAuthStore(status);
    const hub = fakeHub();
    TestBed.configureTestingModule({ providers: [auth.provider, hub.provider, { provide: DEVICE_ID, useValue: 'device 1' }] });
    const signals = TestBed.inject(PlaybackSignals);
    TestBed.tick();
    return { signals, hub, auth };
  }

  const flush = () => new Promise((resolve) => setTimeout(resolve));

  it('stays offline while signed out', async () => {
    const { hub } = setup('anonymous');
    await flush();
    expect(hub.connections).toEqual([]);
  });

  it('connects to the streaming hub as this device and relays stop signals', async () => {
    const { signals, hub } = setup('authenticated');
    await flush();
    const connection = hub.latest()!;
    expect(connection.url).toBe('/hubs/streaming?deviceId=device%201');
    expect(connection.started).toBe(true);

    const stops = vi.fn();
    signals.stop.subscribe(stops);
    connection.emit('ReceiveStopPlayback');
    expect(stops).toHaveBeenCalledTimes(1);
  });

  it('disconnects on sign-out and reconnects on the next sign-in', async () => {
    const { hub, auth } = setup('authenticated');
    await flush();
    const first = hub.latest()!;

    auth.mock.logout();
    TestBed.tick();
    expect(first.stopped).toBe(true);

    auth.mock.signIn();
    TestBed.tick();
    await flush();
    expect(hub.connections.length).toBe(2);
    expect(hub.latest()!.started).toBe(true);
  });
});
