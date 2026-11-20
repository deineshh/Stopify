import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HttpTestingController } from '@angular/common/http/testing';
import { NowPlayingBar } from './now-playing-bar';
import { LayoutStore } from '@core/layout/layout.store';
import { DEVICE_ID } from '@core/playback/device-id';
import { PlayerService } from '@data/services/player.service';
import { provideTestDefaults } from '../../testing/test-providers';
import { mockAuthStore } from '../../testing/auth-store.mock';
import { fakeAudioEngine } from '../../testing/audio-engine.fake';

describe('NowPlayingBar', () => {
  function setup() {
    const audio = fakeAudioEngine();
    TestBed.configureTestingModule({
      providers: [...provideTestDefaults(), mockAuthStore('authenticated').provider, audio.provider, { provide: DEVICE_ID, useValue: 'd1' }],
    });
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(NowPlayingBar);
    fixture.detectChanges();
    http.expectOne('/api/v1/me/playback').flush(null, { status: 404, statusText: 'Not Found' });
    const el = fixture.nativeElement as HTMLElement;
    const render = async () => {
      await TestBed.inject(ApplicationRef).whenStable();
      fixture.detectChanges();
    };
    const click = (selector: string) => {
      el.querySelector<HTMLButtonElement>(selector)!.click();
      fixture.detectChanges();
    };
    return { el, http, render, click, engine: audio.engine, layout: TestBed.inject(LayoutStore), player: TestBed.inject(PlayerService) };
  }

  async function startPlaying(ctx: ReturnType<typeof setup>) {
    ctx.player.play({ kind: 'album', id: 'a1' });
    ctx.http.expectOne('/api/v1/me/playback/play').flush({ hlsUrl: '/audio/x/media_0.m3u8', dashUrl: null, startPositionMs: 0, trackId: 't1' });
    ctx.http.expectOne('/api/v1/tracks/t1').flush({
      id: 't1',
      title: 'PANNONIA',
      duration: '00:02:27',
      releaseDate: null,
      containsExplicitContent: false,
      status: 'published',
      audioFileId: 'x',
      albumId: 'a1',
      mainArtists: [
        { id: 'desh', name: 'DESH', status: 'Verified', ownerId: null, avatar: null },
        { id: 'yf', name: 'Young Fly', status: 'Verified', ownerId: null, avatar: null },
      ],
      featuredArtists: [],
      genres: [],
      moods: [],
    });
    ctx.http.expectOne('/api/v1/albums/a1').flush({ id: 'a1', title: 'BAKPAKK', releaseDate: null, status: 'p', type: 'ep', cover: null, mainArtists: [], tracks: [] });
    await ctx.render();
  }

  it('is empty, with disabled controls, until something plays', () => {
    const { el } = setup();
    expect(el.classList).toContain('empty');
    expect(el.querySelector('.now-playing-bar__track-info--title-span')).toBeNull();
    expect(el.querySelector<HTMLButtonElement>('.now-playing-bar__player--play-btn')?.disabled).toBe(true);
    expect(el.querySelector('.now-playing-bar__player--playback-position')?.textContent?.trim()).toBe('-:--');
  });

  it('shows the playing track and pauses it', async () => {
    const ctx = setup();
    await startPlaying(ctx);
    const { el, click, engine, http } = ctx;

    expect(el.querySelector('.now-playing-bar__track-info--title-span')?.textContent).toContain('PANNONIA');
    expect(el.querySelectorAll('.now-playing-bar__track-info--artist-anchor').length).toBe(2);
    expect(el.querySelector('.now-playing-bar__player--playback-duration')?.textContent?.trim()).toBe('3:00');
    expect(el.querySelector('.now-playing-bar__player--play-btn')?.getAttribute('title')).toBe('Pause');

    click('.now-playing-bar__player--play-btn');
    expect(engine.pause).toHaveBeenCalled();
    expect(el.querySelector('.now-playing-bar__player--play-btn')?.getAttribute('title')).toBe('Play');
    http.expectOne('/api/v1/me/playback/sync').flush(null);
    http.expectOne('/api/v1/me/playback/pause').flush(null);
  });

  it('sends shuffle, repeat and skips to the server', async () => {
    const ctx = setup();
    await startPlaying(ctx);
    const { el, click, http, render } = ctx;

    click('.now-playing-bar__player--shuffle-btn');
    http.expectOne('/api/v1/me/playback/shuffle').flush({ isShuffled: true });
    await render();
    expect(el.querySelector('.now-playing-bar__player--shuffle-btn')?.classList).toContain('active');

    click('.now-playing-bar__player--repeat-btn');
    http.expectOne('/api/v1/me/playback/repeat').flush({ repeatMode: 'Track' });
    await render();
    expect(el.querySelector('.now-playing-bar__player--repeat-btn')?.classList).toContain('repeat-one');

    click('.now-playing-bar__player--next-btn');
    http.expectOne('/api/v1/me/playback/next').flush({ hlsUrl: null, dashUrl: null, startPositionMs: null, trackId: null });
  });

  it('opens and closes the queue panel', () => {
    const { el, click, layout } = setup();
    click('.now-playing-bar__queue-btn');
    expect(layout.rightPanel()).toBe('queue');
    expect(el.querySelector('.now-playing-bar__queue-btn')?.getAttribute('aria-pressed')).toBe('true');
    click('.now-playing-bar__queue-btn');
    expect(layout.rightPanel()).toBeNull();
  });
});
