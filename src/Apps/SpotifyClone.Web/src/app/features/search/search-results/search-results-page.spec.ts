import { TestBed } from '@angular/core/testing';
import { HttpTestingController } from '@angular/common/http/testing';
import { SearchResultsPage } from './search-results-page';
import { provideTestDefaults } from '../../../testing/test-providers';

const emptySearch = { users: [], genres: [], moods: [], artists: [], albums: [], tracks: [], playlists: [] };

describe('SearchResultsPage', () => {
  beforeEach(() => vi.useFakeTimers());
  afterEach(() => vi.useRealTimers());

  function setup(query: string) {
    TestBed.configureTestingModule({ providers: provideTestDefaults() });
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(SearchResultsPage);
    fixture.componentRef.setInput('query', query);
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    const render = async () => {
      // Past the search debounce (not runAllTimers: the player keeps an interval running).
      await vi.advanceTimersByTimeAsync(300);
      fixture.detectChanges();
    };
    return { fixture, http, el, render };
  }

  it('waits for a pause in typing before searching', async () => {
    const { fixture, http, render } = setup('d');
    vi.advanceTimersByTime(100);
    fixture.componentRef.setInput('query', 'de');
    fixture.detectChanges();
    vi.advanceTimersByTime(100);
    http.expectNone((r) => r.url === '/api/v1/search');

    await render();
    const req = http.expectOne((r) => r.url === '/api/v1/search');
    expect(req.request.params.get('searchTerm')).toBe('de');
    req.flush(emptySearch);
  });

  it('renders the top result, songs and only the sections that have items', async () => {
    const { http, el, render } = setup('desh');
    await render();
    http.expectOne((r) => r.url === '/api/v1/search').flush({
      ...emptySearch,
      artists: [{ id: 'desh', name: 'DESH', isVerified: true, avatarImageId: null }],
      tracks: [
        {
          id: 't1',
          title: 'PANNONIA',
          isExplicit: true,
          releaseYear: 2024,
          coverImageId: null,
          album: { id: 'al', title: 'BAKPAKK' },
          artists: [{ id: 'desh', name: 'DESH' }],
          genres: [],
          moods: [],
        },
      ],
    });
    await render();

    expect(el.querySelector('.search-results__top-result-item--title')?.getAttribute('href')).toBe('/artist/desh');
    expect(el.querySelector('.search-results__top-result-item--type')?.textContent).toContain('Artist');
    expect(el.textContent).toContain('PANNONIA');
    // The search API has no durations, so none are shown.
    expect(el.querySelector('.search-results__songs-item--duration')).toBeNull();
    const headings = [...el.querySelectorAll('h2')].map((h) => h.textContent?.trim());
    expect(headings).toContain('Artists');
    expect(headings).not.toContain('Albums');
  });

  it('says so when nothing matched', async () => {
    const { http, el, render } = setup('zzz');
    await render();
    http.expectOne((r) => r.url === '/api/v1/search').flush(emptySearch);
    await render();
    expect(el.querySelector('.search-results__empty-title')?.textContent).toContain('No results found for "zzz"');
  });

  it('offers a retry when the search fails', async () => {
    const { http, el, render } = setup('desh');
    await render();
    http.expectOne((r) => r.url === '/api/v1/search').flush(null, { status: 500, statusText: 'Server Error' });
    await render();
    el.querySelector<HTMLButtonElement>('.page-state__btn')!.click();
    await render();
    http.expectOne((r) => r.url === '/api/v1/search').flush(emptySearch);
  });
});
