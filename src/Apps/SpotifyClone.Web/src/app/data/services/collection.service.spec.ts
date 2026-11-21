import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { firstValueFrom } from 'rxjs';
import { CollectionService } from './collection.service';

const artist = { id: 'desh', name: 'DESH', status: 'Verified', ownerId: null, avatar: null };
const track = (id: string, albumId: string | null, duration = '00:02:00') => ({
  id,
  title: `Track ${id}`,
  duration,
  releaseDate: null,
  containsExplicitContent: false,
  status: 'published',
  audioFileId: null,
  albumId,
  mainArtists: [artist],
  featuredArtists: [],
  genres: [],
  moods: [],
});
const album = (id: string, extra: object = {}) => ({
  id,
  title: `Album ${id}`,
  releaseDate: '2024-03-01T00:00:00Z',
  releaseDateUtc: '2024-03-01T00:00:00Z',
  status: 'published',
  type: 'album',
  cover: { imageId: `cover-${id}`, width: 1, height: 1, fileType: 'webp', sizeInBytes: 1 },
  mainArtists: [artist],
  tracks: [],
  ...extra,
});

describe('CollectionService', () => {
  let service: CollectionService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(CollectionService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('resolves playlist tracks in order, fetches each album once and skips missing tracks', async () => {
    const result = firstValueFrom(service.getPlaylist('p1'));
    http.expectOne('/api/v1/playlists/p1').flush({
      id: 'p1',
      name: 'cali',
      description: null,
      ownerId: 'u1',
      isPublic: true,
      customCoverImageId: null,
      generatedCoverImageIds: ['g1'],
      collaborators: [],
      tracks: [
        { id: 't2', position: 2 },
        { id: 't1', position: 1 },
        { id: 'gone', position: 3 },
      ],
    });
    http.expectOne('/api/v1/tracks/t1').flush(track('t1', 'a1', '00:03:00'));
    http.expectOne('/api/v1/tracks/t2').flush(track('t2', 'a1'));
    http.expectOne('/api/v1/tracks/gone').flush(null, { status: 404, statusText: 'Not Found' });
    http.expectOne('/api/v1/albums/a1').flush(album('a1'));

    const playlist = await result;
    expect(playlist.tracks.map((t) => t.id)).toEqual(['t1', 't2']);
    expect(playlist.tracks[0].album).toEqual({ id: 'a1', name: 'Album a1' });
    expect(playlist.tracks[0].cover).toBe('/images/cover-a1/image.webp');
    expect(playlist.cover).toBe('/images/g1/image.webp');
    expect(playlist.totalDurationSeconds).toBe(300);
  });

  it('loads an album with "More by" from the same artist, excluding itself', async () => {
    const result = firstValueFrom(service.getAlbum('a1'));
    http.expectOne('/api/v1/albums/a1').flush(
      album('a1', { type: 'EP', tracks: [{ id: 't1', title: 'One', containsExplicitContent: false, status: 'p', duration: '00:01:00', position: 1 }] }),
    );
    const more = http.expectOne((req) => req.url === '/api/v1/albums' && req.params.get('mainArtistIds') === 'desh');
    more.flush([album('a1'), album('a2')]);

    const view = await result;
    expect(view.typeLabel).toBe('EP');
    expect(view.tracks.length).toBe(1);
    expect(view.moreBy?.title).toBe('More by DESH');
    expect(view.moreBy?.items.map((i) => i.id)).toEqual(['a2']);
  });

  it('loads an artist with popular tracks (album artwork attached) and discography', async () => {
    const result = firstValueFrom(service.getArtist('desh'));
    http.expectOne('/api/v1/artists/desh').flush({ ...artist, bio: 'Bio', banner: null, gallery: [] });
    http.expectOne((req) => req.url === '/api/v1/tracks').flush([track('t1', 'a1')]);
    http.expectOne((req) => req.url === '/api/v1/albums').flush([album('a1')]);

    const view = await result;
    expect(view.verified).toBe(true);
    expect(view.popularTracks[0].album.name).toBe('Album a1');
    expect(view.popularTracks[0].cover).toBe('/images/cover-a1/image.webp');
    expect(view.discography?.items.length).toBe(1);
  });

  it('still renders an artist when the track or album lists fail', async () => {
    const result = firstValueFrom(service.getArtist('desh'));
    http.expectOne('/api/v1/artists/desh').flush({ ...artist, bio: null, banner: null, gallery: [] });
    http.expectOne((req) => req.url === '/api/v1/tracks').flush(null, { status: 500, statusText: 'Error' });
    http.expectOne((req) => req.url === '/api/v1/albums').flush(null, { status: 500, statusText: 'Error' });
    const view = await result;
    expect(view.popularTracks).toEqual([]);
    expect(view.discography).toBeNull();
  });
});
