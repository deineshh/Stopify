import { TestBed } from '@angular/core/testing';
import { LayoutStore } from '@core/layout/layout.store';
import { QueuePanel } from './queue-panel';
import { playbackTestBed } from '../../testing/playback.harness';
import { albumDto, artistDto, streamDto, trackDto } from '../../testing/catalog.fixtures';

describe('QueuePanel', () => {
  function setup() {
    const bed = playbackTestBed();
    TestBed.inject(LayoutStore).openRightPanel('queue');
    const fixture = TestBed.createComponent(QueuePanel);
    fixture.detectChanges();
    const render = async () => {
      await bed.settle();
      fixture.detectChanges();
    };
    return { ...bed, fixture, render, el: fixture.nativeElement as HTMLElement };
  }

  it('says the queue is empty before anything plays', () => {
    const { el } = setup();
    expect(el.querySelector('.queue-view__empty-state--title')?.textContent).toContain('Your queue is empty');
  });

  it('shows the playing song and what comes next, and plays a queued song from there', async () => {
    const { el, http, startPlaying, render } = setup();
    await startPlaying({ kind: 'album', id: 'a1' }, 't1');
    TestBed.tick();
    http.expectOne('/api/v1/albums/a1').flush(albumDto('a1', 'BAKPAKK'));
    http.expectOne('/api/v1/artists/desh').flush({ ...artistDto('desh', 'DESH'), bio: null, banner: null, gallery: [] });
    http.expectOne('/api/v1/me/playback/queue').flush({ currentTrack: trackDto('t1'), tracksInQueue: [trackDto('t2'), trackDto('t3')] });
    await render();

    expect(el.querySelector('.queue-view__now-playing--group .right-sidebar__track-item--title-span')?.textContent).toContain('Song t1');
    expect(el.querySelector('app-sidebar-track-row.current')).toBeTruthy();
    expect(el.querySelector('.queue-view__next-from--title-artist--span')?.textContent).toContain('BAKPAKK');
    const queued = el.querySelectorAll('.queue-view__next-from--items app-sidebar-track-row');
    expect(queued.length).toBe(2);
    expect(queued[0].querySelector('img')?.getAttribute('src')).toBe('/images/cover-a1/image.webp');

    queued[1].querySelector<HTMLButtonElement>('.right-sidebar__track-item--play-btn')!.click();
    const play = http.expectOne('/api/v1/me/playback/play');
    expect(play.request.body).toMatchObject({ contextType: 'album', contextExternalId: 'a1', startTrackId: 't3' });
    play.flush(streamDto('t3'));
    http.expectOne('/api/v1/tracks/t3').flush(trackDto('t3'));
  });

  it('says when nothing else is queued', async () => {
    const { el, http, startPlaying, render } = setup();
    await startPlaying({ kind: 'album', id: 'a1' }, 't1');
    TestBed.tick();
    http.expectOne('/api/v1/albums/a1').flush(albumDto('a1'));
    http.expectOne('/api/v1/artists/desh').flush({ ...artistDto('desh'), bio: null, banner: null, gallery: [] });
    http.expectOne('/api/v1/me/playback/queue').flush({ currentTrack: trackDto('t1'), tracksInQueue: [] });
    await render();
    expect(el.querySelector('.queue-view__empty')?.textContent).toContain('Nothing else in the queue');
  });
});
