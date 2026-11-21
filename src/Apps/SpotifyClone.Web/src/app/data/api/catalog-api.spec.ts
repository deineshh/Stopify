import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { firstValueFrom } from 'rxjs';
import { CatalogApi } from './catalog-api';

describe('CatalogApi', () => {
  let api: CatalogApi;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    api = TestBed.inject(CatalogApi);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('sends filters and paging as query parameters, arrays as repeated keys', async () => {
    const page = firstValueFrom(api.albums({ mainArtistIds: ['a1', 'a2'], title: '' }, { page: 2, pageSize: 20 }));
    const request = http.expectOne((req) => req.url === '/api/v1/albums');
    expect(request.request.params.getAll('mainArtistIds')).toEqual(['a1', 'a2']);
    expect(request.request.params.get('page')).toBe('2');
    expect(request.request.params.get('pageSize')).toBe('20');
    expect(request.request.params.has('title')).toBe(false);
    request.flush([{ id: 'x' }], {
      headers: { 'X-Pagination': JSON.stringify({ page: 2, pageSize: 20, hasNextPage: false, hasPreviousPage: true, totalCount: 21 }) },
    });
    const result = await page;
    expect(result.items.length).toBe(1);
    expect(result.totalCount).toBe(21);
  });

  it('hits the detail, me and search endpoints', () => {
    api.playlist('p1').subscribe();
    api.myPlaylists().subscribe();
    api.search('desh', 5).subscribe();
    http.expectOne('/api/v1/playlists/p1').flush({});
    http.expectOne((req) => req.url === '/api/v1/me/playlists').flush([]);
    const search = http.expectOne((req) => req.url === '/api/v1/search');
    expect(search.request.params.get('searchTerm')).toBe('desh');
    expect(search.request.params.get('limit')).toBe('5');
    search.flush({});
  });

  it('escapes ids in paths', () => {
    api.artist('a/b').subscribe();
    http.expectOne('/api/v1/artists/a%2Fb').flush({});
  });
});
