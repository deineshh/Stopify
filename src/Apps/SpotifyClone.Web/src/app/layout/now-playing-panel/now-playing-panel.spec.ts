import { TestBed } from '@angular/core/testing';
import { NowPlayingPanel } from './now-playing-panel';
import { playbackTestBed } from '../../testing/playback.harness';
import { albumDto, artistDto, trackDto } from '../../testing/catalog.fixtures';

describe('NowPlayingPanel', () => {
  function setup() {
    const bed = playbackTestBed();
    const fixture = TestBed.createComponent(NowPlayingPanel);
    fixture.detectChanges();
    const render = async () => {
      await bed.settle();
      fixture.detectChanges();
    };
    return { ...bed, fixture, render, el: fixture.nativeElement as HTMLElement };
  }

  it('has an empty state before anything plays', () => {
    const { el } = setup();
    expect(el.querySelector('.now-playing-view__empty--title')?.textContent).toContain('Nothing playing yet');
  });

  it('shows the song, its context, artist, credits and the next song', async () => {
    const { el, http, startPlaying, render } = setup();
    await startPlaying({ kind: 'album', id: 'a1' }, 't1', { featuredArtists: [artistDto('yf', 'Young Fly')] });
    TestBed.tick();
    http.expectOne('/api/v1/albums/a1').flush(albumDto('a1', 'BAKPAKK'));
    http.expectOne('/api/v1/artists/desh').flush({
      ...artistDto('desh', 'DESH'),
      bio: 'Hungarian rapper.',
      banner: null,
      gallery: [],
    });
    http.expectOne('/api/v1/me/playback/queue').flush({ currentTrack: trackDto('t1'), tracksInQueue: [trackDto('t2')] });
    await render();

    expect(el.querySelector('.now-playing-view__header--title-text')?.textContent).toContain('BAKPAKK');
    expect(el.querySelector('.now-playing-view__header--title-anchor')?.getAttribute('href')).toBe('/album/a1');
    expect(el.querySelector('.now-playing-view__track--title-span')?.textContent).toContain('Song t1');
    expect(el.querySelector('.now-playing-view__artist-info--artist-span')?.textContent).toContain('DESH');
    expect(el.querySelector('.now-playing-view__artist-info--monthly-listeners')?.textContent).toContain('Verified Artist');
    expect(el.querySelector('.now-playing-view__artist-info--description')?.textContent).toContain('Hungarian rapper.');
    const roles = [...el.querySelectorAll('.now-playing-view__credits--artist-role')].map((r) => r.textContent?.trim());
    expect(roles).toEqual(['Main Artist', 'Featured Artist']);
    expect(el.querySelector('.now-playing-view__next app-sidebar-track-row')?.textContent).toContain('Song t2');
  });

  it('toggles following an artist locally', async () => {
    const { el, http, startPlaying, render, fixture } = setup();
    await startPlaying({ kind: 'album', id: 'a1' }, 't1');
    TestBed.tick();
    http.expectOne('/api/v1/albums/a1').flush(albumDto('a1'));
    http.expectOne('/api/v1/artists/desh').flush({ ...artistDto('desh', 'DESH'), bio: null, banner: null, gallery: [] });
    http.expectOne('/api/v1/me/playback/queue').flush({ currentTrack: null, tracksInQueue: [] });
    await render();

    const follow = el.querySelector<HTMLButtonElement>('.now-playing-view__artist-info--follow-btn')!;
    follow.click();
    fixture.detectChanges();
    expect(follow.textContent).toContain('Unfollow');
    expect(el.querySelector('.now-playing-view__artist-info--description')).toBeNull();
  });
});
