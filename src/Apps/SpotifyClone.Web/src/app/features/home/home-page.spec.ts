import { TestBed } from '@angular/core/testing';
import { HttpTestingController } from '@angular/common/http/testing';
import { HomePage } from './home-page';
import { provideTestDefaults } from '../../testing/test-providers';
import { mockAuthStore } from '../../testing/auth-store.mock';

const album = (id: string) => ({
  id,
  title: `Album ${id}`,
  releaseDateUtc: null,
  status: 'published',
  type: 'album',
  cover: null,
  mainArtists: [{ id: 'desh', name: 'DESH', status: 'Verified', ownerId: null, avatar: null }],
});
const playlist = (id: string) => ({
  id,
  name: `Mine ${id}`,
  description: null,
  isPublic: false,
  ownerId: 'u1',
  customCover: null,
  generatedCoverImageIds: [],
});

describe('HomePage', () => {
  function setup() {
    const auth = mockAuthStore();
    TestBed.configureTestingModule({ providers: [...provideTestDefaults(), auth.provider] });
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(HomePage);
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    const render = async () => {
      await fixture.whenStable();
      fixture.detectChanges();
    };
    return { fixture, http, el, auth, render };
  }

  /** The shelves and the guest shortcuts both read /albums. */
  function flushFeed(http: HttpTestingController, albums = [album('a1')]) {
    for (const req of http.match((r) => r.url === '/api/v1/albums')) req.flush(albums);
    http.expectOne((r) => r.url === '/api/v1/artists').flush([]);
    http.expectOne((r) => r.url === '/api/v1/playlists').flush([]);
  }

  it('shows a loading skeleton, then album shelves and album shortcuts for guests', async () => {
    const { http, el, render } = setup();
    expect(el.querySelector('.home-page__loading')).toBeTruthy();

    flushFeed(http);
    await render();

    expect(el.querySelector('.home-page__loading')).toBeNull();
    expect(el.textContent).toContain('New releases');
    expect(el.querySelector('.home-page__recently-played-item a')?.getAttribute('href')).toBe('/album/a1');
  });

  it('swaps the shortcuts for the user’s playlists after signing in', async () => {
    const { http, el, auth, render } = setup();
    flushFeed(http);
    await render();

    auth.mock.signIn();
    // Not whenStable(): an unanswered request keeps the app unstable.
    TestBed.tick();
    http.expectOne((r) => r.url === '/api/v1/me/playlists').flush([playlist('p1')]);
    await render();

    const shortcut = el.querySelector('.home-page__recently-played-item a');
    expect(shortcut?.getAttribute('href')).toBe('/playlist/p1');
    expect(el.textContent).toContain('Mine p1');
  });

  it('shows an empty message for podcasts and a retry when nothing loaded', async () => {
    const { http, el, render } = setup();
    flushFeed(http, []);
    await render();

    expect(el.querySelector('.home-page__empty-title')?.textContent).toContain('Nothing to show yet');
    el.querySelector<HTMLButtonElement>('.home-page__empty-btn')!.click();
    TestBed.tick();
    flushFeed(http, [album('a1')]);
    await render();
    expect(el.querySelector('.home-page__empty')).toBeNull();

    const podcasts = [...el.querySelectorAll<HTMLButtonElement>('button')].find((b) => b.textContent?.trim() === 'Podcasts')!;
    podcasts.click();
    await render();
    expect(el.querySelector('.home-page__empty-title')?.textContent).toContain('No podcasts yet');
    expect(el.querySelector('.home-page__recently-played-section')).toBeNull();
  });
});
