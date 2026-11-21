import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HttpTestingController, TestRequest } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { DEVICE_ID } from '@core/playback/device-id';
import { ToastService } from '@core/notifications/toast.service';
import { PlayerService } from './player.service';
import { provideTestDefaults } from '../../testing/test-providers';
import { mockAuthStore } from '../../testing/auth-store.mock';
import { fakeAudioEngine } from '../../testing/audio-engine.fake';
import { fakeHub } from '../../testing/hub.fake';

const DEVICE = 'device-1';
const track = (id: string, albumId = 'a1') => ({
  id,
  title: `Song ${id}`,
  duration: '00:03:00',
  releaseDate: null,
  containsExplicitContent: false,
  status: 'published',
  audioFileId: `audio-${id}`,
  albumId,
  mainArtists: [{ id: 'desh', name: 'DESH', status: 'Verified', ownerId: null, avatar: null }],
  featuredArtists: [],
  genres: [],
  moods: [],
});
const album = (id: string) => ({
  id,
  title: `Album ${id}`,
  releaseDate: null,
  status: 'published',
  type: 'album',
  cover: { imageId: `cover-${id}`, width: 1, height: 1, fileType: 'webp', sizeInBytes: 1 },
  mainArtists: [],
  tracks: [],
});
const stream = (trackId: string | null, startPositionMs = 0) => ({
  hlsUrl: trackId ? `/audio/${trackId}/media_0.m3u8` : null,
  dashUrl: null,
  startPositionMs,
  trackId,
});

describe('PlayerService', () => {
  function setup(status: 'anonymous' | 'authenticated' = 'authenticated') {
    const auth = mockAuthStore(status);
    const audio = fakeAudioEngine();
    const hub = fakeHub();
    TestBed.configureTestingModule({
      providers: [...provideTestDefaults(), auth.provider, audio.provider, hub.provider, { provide: DEVICE_ID, useValue: DEVICE }],
    });
    const player = TestBed.inject(PlayerService);
    const http = TestBed.inject(HttpTestingController);
    TestBed.tick();
    return { player, http, engine: audio.engine, auth, hub };
  }

  /** Signed in with no previous session. */
  function signedIn() {
    const ctx = setup();
    ctx.http.expectOne('/api/v1/me/playback').flush(null, { status: 404, statusText: 'Not Found' });
    return ctx;
  }

  const settle = () => TestBed.inject(ApplicationRef).whenStable();

  function expectPost(http: HttpTestingController, path: string): TestRequest {
    const req = http.expectOne((r) => r.method === 'POST' && r.url === `/api/v1/me/playback/${path}`);
    expect(req.request.body.deviceId).toBe(DEVICE);
    return req;
  }

  /** Answers the track + album lookups that follow a new stream. */
  function flushTrack(http: HttpTestingController, id: string, albumId = 'a1') {
    http.expectOne(`/api/v1/tracks/${id}`).flush(track(id, albumId));
    for (const req of http.match(`/api/v1/albums/${albumId}`)) req.flush(album(albumId));
  }

  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('sends guests to log in instead of playing', () => {
    const { player, http } = setup('anonymous');
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    player.play({ kind: 'album', id: 'a1' });
    expect(navigate).toHaveBeenCalledWith(['/login'], { queryParams: { redirect: '/' } });
    http.expectNone((r) => r.url.includes('/me/playback'));
  });

  it('starts an album, streams what the server returns and shows its track', async () => {
    const { player, http, engine } = signedIn();
    player.play({ kind: 'album', id: 'a1', trackId: 't2' });

    const req = expectPost(http, 'play');
    expect(req.request.body).toEqual({ deviceId: DEVICE, contextType: 'album', contextExternalId: 'a1', startTrackId: 't2' });
    req.flush(stream('t2'));
    flushTrack(http, 't2');
    await settle();

    expect(engine.load).toHaveBeenCalledWith('/audio/t2/media_0.m3u8', 0, true);
    expect(player.track()).toMatchObject({ id: 't2', title: 'Song t2', cover: '/images/cover-a1/image.webp' });
    expect(player.isPlayingTarget({ kind: 'album', id: 'a1' })).toBe(true);
    expect(player.isPlayingTarget({ kind: 'album', id: 'a1', trackId: 't2' })).toBe(true);
    expect(player.isPlayingTarget({ kind: 'album', id: 'a1', trackId: 't3' })).toBe(false);
    expect(player.isPlayingTarget({ kind: 'playlist', id: 'a1' })).toBe(false);
  });

  it('pauses (recording the position first) and resumes through the API', async () => {
    const { player, http, engine } = signedIn();
    player.play({ kind: 'playlist', id: 'p1' });
    expectPost(http, 'play').flush(stream('t1'));
    flushTrack(http, 't1');
    await settle();
    engine.currentTime.set(42.5);

    player.toggle({ kind: 'playlist', id: 'p1' });
    expect(engine.pause).toHaveBeenCalled();
    const sync = http.expectOne('/api/v1/me/playback/sync');
    expect(sync.request.body).toEqual({ deviceId: DEVICE, positionMs: 42500 });
    sync.flush(null);
    expectPost(http, 'pause').flush(null);

    player.togglePlay();
    expect(engine.play).toHaveBeenCalled();
    expectPost(http, 'resume').flush(null);
  });

  it('skips to the next song and stops at the end of the queue', async () => {
    const { player, http, engine } = signedIn();
    player.play({ kind: 'album', id: 'a1' });
    expectPost(http, 'play').flush(stream('t1'));
    flushTrack(http, 't1');
    await settle();

    player.next();
    expectPost(http, 'next').flush(stream('t2'));
    flushTrack(http, 't2');
    await settle();
    expect(engine.load).toHaveBeenLastCalledWith('/audio/t2/media_0.m3u8', 0, true);
    expect(player.track()?.id).toBe('t2');

    // The song ending asks the server for the next one; nothing left → stop at its start.
    engine.ended.next();
    expectPost(http, 'next').flush(stream(null));
    expect(engine.pause).toHaveBeenCalled();
    expect(engine.seek).toHaveBeenLastCalledWith(0);
    expect(player.track()?.id).toBe('t2');
  });

  it('restarts the song on "previous" once it has played a few seconds', async () => {
    const { player, http, engine } = signedIn();
    player.play({ kind: 'album', id: 'a1' });
    expectPost(http, 'play').flush(stream('t1'));
    flushTrack(http, 't1');
    await settle();

    engine.currentTime.set(10);
    player.previous();
    expect(engine.seek).toHaveBeenCalledWith(0);
    expect(http.expectOne('/api/v1/me/playback/seek').request.body).toEqual({ deviceId: DEVICE, positionMs: 0 });

    engine.currentTime.set(1);
    player.previous();
    expectPost(http, 'previous').flush(stream('t0'));
    flushTrack(http, 't0');
  });

  it('takes the session back when another device is active, then retries', async () => {
    const { player, http } = signedIn();
    player.play({ kind: 'album', id: 'a1' });
    expectPost(http, 'play').flush(stream('t1'));
    flushTrack(http, 't1');
    await settle();

    player.next();
    expectPost(http, 'next').flush({ title: 'Playback.InvalidDevice' }, { status: 400, statusText: 'Bad Request' });
    expectPost(http, 'resume').flush(null);
    expectPost(http, 'next').flush(stream('t2'));
    flushTrack(http, 't2');
  });

  it('restores the last session on sign-in and continues it where it stopped', async () => {
    const { player, http, engine } = setup();
    http.expectOne('/api/v1/me/playback').flush({
      id: 's1',
      userId: 'u1',
      trackId: 't7',
      deviceId: 'other-device',
      contextType: 'Playlist',
      contextExternalId: 'p1',
      currentPositionMs: 30_000,
      isPlaying: false,
      isShuffled: true,
      repeatMode: 'Track',
      updatedAtUtc: '2026-11-20T00:00:00Z',
    });
    flushTrack(http, 't7');
    await settle();

    expect(player.track()?.id).toBe('t7');
    expect(player.shuffle()).toBe(true);
    expect(player.repeat()).toBe('one');
    expect(player.positionSeconds()).toBe(30);
    expect(engine.load).not.toHaveBeenCalled();

    player.togglePlay();
    const play = expectPost(http, 'play');
    expect(play.request.body).toMatchObject({ contextType: 'playlist', contextExternalId: 'p1', startTrackId: 't7' });
    play.flush(stream('t7'));
    const seek = http.expectOne('/api/v1/me/playback/seek');
    expect(seek.request.body.positionMs).toBe(30_000);
    seek.flush(null);
    await settle();
    expect(engine.load).toHaveBeenCalledWith('/audio/t7/media_0.m3u8', 30, true);
  });

  it('only seeks the audio once the slider is released', async () => {
    const { player, http, engine } = signedIn();
    player.play({ kind: 'album', id: 'a1' });
    expectPost(http, 'play').flush(stream('t1'));
    flushTrack(http, 't1');
    await settle();

    player.scrub(50);
    expect(player.positionSeconds()).toBe(90);
    expect(engine.seek).not.toHaveBeenCalled();

    player.seek(50);
    expect(engine.seek).toHaveBeenCalledWith(90);
    expect(http.expectOne('/api/v1/me/playback/seek').request.body.positionMs).toBe(90_000);
  });

  it('takes shuffle and repeat from the server', async () => {
    const { player, http } = signedIn();
    player.play({ kind: 'album', id: 'a1' });
    expectPost(http, 'play').flush(stream('t1'));
    flushTrack(http, 't1');
    await settle();

    player.toggleShuffle();
    http.expectOne('/api/v1/me/playback/shuffle').flush({ isShuffled: true });
    expect(player.shuffle()).toBe(true);

    player.cycleRepeat();
    http.expectOne('/api/v1/me/playback/repeat').flush({ repeatMode: 'All' });
    expect(player.repeat()).toBe('all');
  });

  it("plays an artist from their latest track's album", async () => {
    const { player, http } = signedIn();
    player.play({ kind: 'artist', id: 'desh' });
    const tracks = http.expectOne((r) => r.url === '/api/v1/tracks');
    expect(tracks.request.params.get('mainArtistIds')).toBe('desh');
    tracks.flush([track('t9', 'a9')]);
    const play = expectPost(http, 'play');
    expect(play.request.body).toMatchObject({ contextType: 'album', contextExternalId: 'a9', startTrackId: 't9' });
    play.flush(stream('t9'));
    flushTrack(http, 't9', 'a9');
    await settle();
    expect(player.isPlayingTarget({ kind: 'artist', id: 'desh' })).toBe(true);
  });

  it('says so when a song has no audio', () => {
    const { player, http } = signedIn();
    const toast = TestBed.inject(ToastService);
    player.play({ kind: 'album', id: 'a1' });
    expectPost(http, 'play').flush({ title: 'Media.NotFound' }, { status: 404, statusText: 'Not Found' });
    expect(toast.current()?.text).toBe("This song isn't available right now.");
  });

  it('stops when another device takes over, and takes it back on play', async () => {
    const { player, http, engine, hub } = signedIn();
    player.play({ kind: 'album', id: 'a1' });
    expectPost(http, 'play').flush(stream('t1'));
    flushTrack(http, 't1');
    await settle();
    await new Promise((resolve) => setTimeout(resolve));

    hub.latest()!.emit('ReceiveStopPlayback');
    expect(engine.pause).toHaveBeenCalled();
    expect(player.isPlaying()).toBe(false);
    expect(TestBed.inject(ToastService).current()?.text).toBe('Playing on another device.');
    http.expectNone((r) => r.url.endsWith('/pause'));

    player.togglePlay();
    expectPost(http, 'resume').flush(null);
  });

  it('forgets the session on sign-out', async () => {
    const { player, http, engine, auth } = signedIn();
    player.play({ kind: 'album', id: 'a1' });
    expectPost(http, 'play').flush(stream('t1'));
    flushTrack(http, 't1');
    await settle();

    auth.mock.logout();
    TestBed.tick();
    expect(engine.stop).toHaveBeenCalled();
    expect(player.track()).toBeNull();
  });

  it('mutes when the volume is dragged to zero and passes the volume to the audio', () => {
    const { player, engine } = signedIn();
    player.setVolume(0);
    expect(player.muted()).toBe(true);
    expect(player.effectiveVolume()).toBe(0);

    player.setVolume(60);
    TestBed.tick();
    expect(engine.volume).toBe(0.6);
    player.toggleMute();
    TestBed.tick();
    expect(engine.volume).toBe(0);
    expect(player.volume()).toBe(60);
  });
});
