import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { firstValueFrom } from 'rxjs';
import { CatalogService } from './catalog.service';

const artistDto = (id: string) => ({ id, name: `Artist ${id}`, status: 'Verified', ownerId: null, avatar: null });
const albumDto = (id: string) => ({
  id,
  title: `Album ${id}`,
  releaseDateUtc: '2024-03-01T00:00:00Z',
  status: 'published',
  type: 'album',
  cover: null,
  mainArtists: [artistDto('a')],
});
const playlistDto = (id: string) => ({
  id,
  name: `Playlist ${id}`,
  description: null,
  isPublic: true,
  ownerId: 'u1',
  customCover: null,
  generatedCoverImageIds: [],
});
const emptySearch = { users: [], genres: [], moods: [], artists: [], albums: [], tracks: [], playlists: [] };

describe('CatalogService', () => {
  let service: CatalogService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(CatalogService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('composes the home shelves and drops the ones that are empty or failed', async () => {
    const result = firstValueFrom(service.getHomeShelves());
    http.expectOne((req) => req.url === '/api/v1/albums').flush([albumDto('x')]);
    http.expectOne((req) => req.url === '/api/v1/artists').flush(null, { status: 500, statusText: 'Server Error' });
    const playlists = http.expectOne((req) => req.url === '/api/v1/playlists');
    expect(playlists.request.params.get('isPublic')).toBe('true');
    playlists.flush([]);

    const shelves = await result;
    expect(shelves.map((s) => s.id)).toEqual(['new-releases']);
    expect(shelves[0].items[0]).toMatchObject({ id: 'x', title: 'Album x', kind: 'album' });
    expect(shelves[0].category).toBe('music');
  });

  it('uses the newest albums as shortcuts for guests and own playlists once signed in', async () => {
    const guest = firstValueFrom(service.getShortcuts(false));
    http.expectOne((req) => req.url === '/api/v1/albums').flush([albumDto('x')]);
    expect((await guest)[0]).toMatchObject({ id: 'x', link: ['/album', 'x'] });

    const member = firstValueFrom(service.getShortcuts(true));
    http.expectOne((req) => req.url === '/api/v1/me/playlists').flush([playlistDto('p')]);
    expect((await member)[0]).toMatchObject({ id: 'p', title: 'Playlist p', link: ['/playlist', 'p'] });
  });

  it('hides the shortcuts when they fail to load', async () => {
    const result = firstValueFrom(service.getShortcuts(true));
    http.expectOne((req) => req.url === '/api/v1/me/playlists').flush(null, { status: 401, statusText: 'Unauthorized' });
    expect(await result).toEqual([]);
  });

  it('lists genres before moods, without a placeholder image when there is no cover', async () => {
    const result = firstValueFrom(service.getGenres());
    http.expectOne((req) => req.url === '/api/v1/genres').flush([{ id: 'g', name: 'Rock', coverImageId: 'img' }]);
    http.expectOne((req) => req.url === '/api/v1/moods').flush([{ id: 'm', name: 'Chill', coverImageId: null }]);

    const tiles = await result;
    expect(tiles.map((t) => t.name)).toEqual(['Rock', 'Chill']);
    expect(tiles[0].image).toBe('/images/img/image.webp');
    expect(tiles[1].image).toBe('');
  });

  it('picks an artist as the top result over albums and songs', async () => {
    const result = firstValueFrom(service.search('desh'));
    const req = http.expectOne((r) => r.url === '/api/v1/search');
    expect(req.request.params.get('searchTerm')).toBe('desh');
    req.flush({
      ...emptySearch,
      artists: [{ id: 'desh', name: 'DESH', isVerified: true, avatarImageId: null }],
      albums: [{ id: 'al', title: 'BAKPAKK', releaseYear: 2024, type: 'ep', coverImageId: null, artists: [] }],
    });

    const results = await result;
    expect(results.topResult).toMatchObject({ name: 'DESH', type: 'Artist', shape: 'circle', link: ['/artist', 'desh'] });
    expect(results.albums.length).toBe(1);
  });

  it('falls back to a song as the top result and keeps only four songs', async () => {
    const result = firstValueFrom(service.search('pan'));
    const track = (id: string) => ({
      id,
      title: `Song ${id}`,
      isExplicit: false,
      releaseYear: 2024,
      coverImageId: null,
      album: { id: 'al', title: 'BAKPAKK' },
      artists: [{ id: 'desh', name: 'DESH' }],
      genres: [],
      moods: [],
    });
    http.expectOne((r) => r.url === '/api/v1/search').flush({ ...emptySearch, tracks: ['1', '2', '3', '4', '5'].map(track) });

    const results = await result;
    expect(results.topResult).toMatchObject({ type: 'Song', shape: 'rounded', link: ['/album', 'al'] });
    expect(results.songs.length).toBe(4);
  });

  it('has no top result when nothing matched', async () => {
    const result = firstValueFrom(service.search('zzz'));
    http.expectOne((r) => r.url === '/api/v1/search').flush(emptySearch);
    expect((await result).topResult).toBeNull();
  });
});
