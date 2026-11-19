import { TestBed } from '@angular/core/testing';
import { HttpTestingController } from '@angular/common/http/testing';
import { AlbumPage } from './album-page';
import { provideTestDefaults } from '../../testing/test-providers';

describe('AlbumPage', () => {
  function setup() {
    TestBed.configureTestingModule({ providers: provideTestDefaults() });
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(AlbumPage);
    fixture.componentRef.setInput('id', 'a1');
    fixture.detectChanges();
    return { fixture, http, el: fixture.nativeElement as HTMLElement };
  }

  it('shows a skeleton, then the album', async () => {
    const { fixture, http, el } = setup();
    expect(el.querySelector('.page-state__skeleton')).toBeTruthy();

    http.expectOne('/api/v1/albums/a1').flush({
      id: 'a1',
      title: 'BAKPAKK',
      releaseDate: '2024-03-01T00:00:00Z',
      status: 'published',
      type: 'ep',
      cover: null,
      mainArtists: [{ id: 'desh', name: 'DESH', status: 'active', ownerId: null, avatar: null }],
      tracks: [{ id: 't1', title: 'PANNONIA', containsExplicitContent: false, status: 'p', duration: '00:02:27', position: 1 }],
    });
    http.expectOne((req) => req.url === '/api/v1/albums').flush([]);
    await fixture.whenStable();
    fixture.detectChanges();

    expect(el.querySelector('h1')?.textContent).toContain('BAKPAKK');
    expect(el.querySelector('.playlist-page__info-section--header-text')?.textContent).toContain('EP');
    expect(el.querySelector('.playlist-page__info-section--author-anchor')?.getAttribute('href')).toBe('/artist/desh');
    expect(el.querySelectorAll('.track-table__track').length).toBe(1);
    expect(el.querySelector('.track-table__header--album-column')).toBeNull();
  });

  it('shows not found for unknown albums and retries other errors', async () => {
    const { fixture, http, el } = setup();
    http.expectOne('/api/v1/albums/a1').flush({ title: 'Album.NotFound' }, { status: 404, statusText: 'Not Found' });
    await fixture.whenStable();
    fixture.detectChanges();
    expect(el.querySelector('.page-state__title')?.textContent).toContain("Couldn't find that album");
  });

  it('offers a retry for server errors', async () => {
    const { fixture, http, el } = setup();
    http.expectOne('/api/v1/albums/a1').flush(null, { status: 500, statusText: 'Server Error' });
    await fixture.whenStable();
    fixture.detectChanges();
    el.querySelector<HTMLButtonElement>('.page-state__btn')!.click();
    fixture.detectChanges();
    http.expectOne('/api/v1/albums/a1').flush(null, { status: 500, statusText: 'Server Error' });
  });
});
