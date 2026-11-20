import { TestBed } from '@angular/core/testing';
import { LayoutStore } from '@core/layout/layout.store';
import { NowPlayingService } from './now-playing.service';
import { playbackTestBed } from '../../testing/playback.harness';
import { albumDto, trackDto } from '../../testing/catalog.fixtures';

describe('NowPlayingService', () => {
  function setup() {
    const bed = playbackTestBed();
    const service = TestBed.inject(NowPlayingService);
    return { ...bed, service, layout: TestBed.inject(LayoutStore) };
  }

  it('loads nothing until something plays', () => {
    const { service, http } = setup();
    TestBed.tick();
    http.expectNone((r) => r.url.includes('/queue') || r.url.includes('/artists/'));
    expect(service.context()).toBeNull();
    expect(service.queue()).toEqual([]);
  });

  it('names the context, spotlights the first artist and loads the queue', async () => {
    const { service, http, startPlaying, settle } = setup();
    await startPlaying({ kind: 'playlist', id: 'p1' }, 't1');
    TestBed.tick();

    http.expectOne('/api/v1/playlists/p1').flush({
      id: 'p1',
      name: 'Road trip',
      description: null,
      ownerId: 'u1',
      isPublic: true,
      customCoverImageId: null,
      generatedCoverImageIds: [],
      collaborators: [],
      tracks: [],
    });
    http.expectOne('/api/v1/artists/desh').flush({
      id: 'desh',
      name: 'DESH',
      bio: '  Hungarian rapper.  ',
      ownerId: null,
      status: 'Verified',
      avatar: { imageId: 'avatar', width: 1, height: 1, fileType: 'webp', sizeInBytes: 1 },
      banner: null,
      gallery: [],
    });
    http.expectOne('/api/v1/me/playback/queue').flush({
      currentTrack: trackDto('t1'),
      tracksInQueue: [trackDto('t2', 'a2'), trackDto('t3', 'a2')],
    });
    http.expectOne('/api/v1/albums/a2').flush(albumDto('a2', 'Aziverzum'));
    await settle();

    expect(service.context()).toEqual({ name: 'Road trip', link: ['/playlist', 'p1'] });
    expect(service.artist()).toEqual({
      artist: { id: 'desh', name: 'DESH' },
      avatar: '/images/avatar/image.webp',
      bio: 'Hungarian rapper.',
      verified: true,
    });
    expect(service.queue().map((t) => [t.id, t.album.name])).toEqual([
      ['t2', 'Aziverzum'],
      ['t3', 'Aziverzum'],
    ]);
    expect(service.next()?.id).toBe('t2');
  });

  it('skips the queue while both panels are closed', async () => {
    const { http, startPlaying, layout } = setup();
    TestBed.tick(); // the layout opens the panel on desktop first
    layout.closeRightPanel();
    await startPlaying({ kind: 'album', id: 'a1' }, 't1');
    TestBed.tick();
    http.expectOne('/api/v1/albums/a1');
    http.expectOne('/api/v1/artists/desh');
    http.expectNone('/api/v1/me/playback/queue');
  });
});
