import { TestBed } from '@angular/core/testing';
import { HttpTestingController } from '@angular/common/http/testing';
import { SearchBrowsePage } from './search-browse-page';
import { provideTestDefaults } from '../../../testing/test-providers';

describe('SearchBrowsePage', () => {
  function setup() {
    TestBed.configureTestingModule({ providers: provideTestDefaults() });
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(SearchBrowsePage);
    fixture.detectChanges();
    const render = async () => {
      await fixture.whenStable();
      fixture.detectChanges();
    };
    return { http, render, el: fixture.nativeElement as HTMLElement };
  }

  it('shows genre and mood tiles, with artwork only when there is a cover', async () => {
    const { http, el, render } = setup();
    http.expectOne((r) => r.url === '/api/v1/genres').flush([{ id: 'g', name: 'Rock', coverImageId: 'img' }]);
    http.expectOne((r) => r.url === '/api/v1/moods').flush([{ id: 'm', name: 'Chill', coverImageId: null }]);
    await render();

    const tiles = el.querySelectorAll('.search-browse__main-item');
    expect(tiles.length).toBe(2);
    expect(tiles[0].querySelector('img')?.getAttribute('src')).toBe('/images/img/image.webp');
    expect(tiles[1].querySelector('img')).toBeNull();
  });

  it('says so when there are no categories yet', async () => {
    const { http, el, render } = setup();
    http.expectOne((r) => r.url === '/api/v1/genres').flush([]);
    http.expectOne((r) => r.url === '/api/v1/moods').flush([]);
    await render();
    expect(el.querySelector('.search-browse__empty')).toBeTruthy();
  });
});
