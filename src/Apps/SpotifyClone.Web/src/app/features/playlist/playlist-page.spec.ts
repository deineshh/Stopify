import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HttpTestingController } from '@angular/common/http/testing';
import { PlaylistPage } from './playlist-page';
import { PlaylistEditor } from '@data/services/playlist-editor.service';
import { provideTestDefaults } from '../../testing/test-providers';
import { mockAuthStore } from '../../testing/auth-store.mock';
import { albumDto, trackDto } from '../../testing/catalog.fixtures';

const details = (ownerId: string, tracks: { id: string; position: number }[] = []) => ({
  id: 'p1',
  name: 'Road trip',
  description: 'Long drives',
  ownerId,
  isPublic: false,
  customCoverImageId: null,
  generatedCoverImageIds: [],
  collaborators: [],
  tracks,
});

describe('PlaylistPage', () => {
  async function setup(ownerId = 'u1', tracks: { id: string; position: number }[] = []) {
    TestBed.configureTestingModule({ providers: [...provideTestDefaults(), mockAuthStore('authenticated').provider] });
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(PlaylistPage);
    fixture.componentRef.setInput('id', 'p1');
    fixture.detectChanges();
    await settle();
    for (const req of http.match((r) => r.url === '/api/v1/me/playlists')) req.flush([]);
    http.expectOne('/api/v1/playlists/p1').flush(details(ownerId, tracks));
    await settle();
    flushTracks(http, tracks.map((track) => track.id));
    await settle();
    fixture.detectChanges();
    return { fixture, http, el: fixture.nativeElement as HTMLElement };
  }

  /** Answers the per-song requests; ids starting with "gone" fail like deleted songs. */
  function flushTracks(http: HttpTestingController, ids: string[]) {
    for (const id of ids) {
      const req = http.expectOne(`/api/v1/tracks/${id}`);
      if (id.startsWith('gone')) {
        req.flush(null, { status: 404, statusText: 'Not Found' });
      } else {
        req.flush(trackDto(id, 'a1', { title: `Song ${id}` }));
      }
    }
    if (ids.some((id) => !id.startsWith('gone'))) {
      return Promise.resolve().then(() => undefined);
    }
    return undefined;
  }

  async function flushAlbum(http: HttpTestingController) {
    await settle();
    for (const req of http.match('/api/v1/albums/a1')) req.flush(albumDto('a1', 'BAKPAKK'));
    await settle();
  }

  const rowTitles = (el: HTMLElement) =>
    [...el.querySelectorAll('.track-table__track--title-span')].map((title) => title.textContent?.trim());

  async function settle() {
    TestBed.inject(ApplicationRef).tick();
    await new Promise((resolve) => setTimeout(resolve));
    TestBed.inject(ApplicationRef).tick();
  }

  afterEach(() => {
    document.querySelectorAll('.cdk-overlay-container').forEach((container) => (container.innerHTML = ''));
  });

  it('lets the owner edit the details from the title', async () => {
    const { el } = await setup();
    const title = el.querySelector<HTMLButtonElement>('.playlist-page__info-section--title-btn');
    expect(title?.textContent?.trim()).toBe('Road trip');
    title!.click();
    await settle();
    expect(document.querySelector<HTMLInputElement>('.edit-playlist__input[name="name"]')?.value).toBe('Road trip');
  });

  it('gives the owner a playlist menu on "…"', async () => {
    const { el } = await setup();
    el.querySelector<HTMLButtonElement>('.playlist-page__main-section--more-options-btn')!.click();
    await settle();
    const labels = [...document.querySelectorAll('.cdk-overlay-container .menu .menu__label')].map((l) => l.textContent?.trim());
    expect(labels).toEqual(['Edit details', 'Make public', 'Delete']);
  });

  it("doesn't offer editing on someone else's playlist", async () => {
    const { el } = await setup('someone-else');
    expect(el.querySelector('.playlist-page__info-section--title-btn')).toBeNull();
    expect(el.querySelector('.playlist-page__info-section--title')?.textContent?.trim()).toBe('Road trip');
    el.querySelector<HTMLButtonElement>('.playlist-page__main-section--more-options-btn')!.click();
    await settle();
    expect(document.querySelector('.cdk-overlay-container .menu')).toBeNull();
  });

  it('reloads in place when a song is added from anywhere', async () => {
    const { el, http, fixture } = await setup();
    expect(el.querySelector('.playlist-page__empty-title')?.textContent).toContain("doesn't have any songs yet");

    TestBed.inject(PlaylistEditor).addTrack({ id: 'p1', name: 'Road trip' }, 't1').subscribe();
    http.expectOne('/api/v1/playlists/p1/tracks').flush(null);
    await settle();
    fixture.detectChanges();
    // Still showing the playlist (no loading state) while it reloads.
    expect(el.querySelector('app-page-state')).toBeNull();
    http.expectOne((r) => r.url === '/api/v1/me/playlists').flush([]);
    http.expectOne('/api/v1/playlists/p1').flush(details('u1', [{ id: 't1', position: 1 }]));
    await settle();
    http.expectOne('/api/v1/tracks/t1').flush(trackDto('t1'));
    http.expectOne('/api/v1/albums/a1').flush(albumDto('a1', 'BAKPAKK'));
    await settle();
    fixture.detectChanges();
    expect(el.querySelectorAll('.track-table__track').length).toBe(1);
  });

  it('shows a move at once, sends it with the index in the full list and reloads afterwards', async () => {
    const tracks = [
      { id: 'gone', position: 1 },
      { id: 't1', position: 2 },
      { id: 't2', position: 3 },
      { id: 't3', position: 4 },
    ];
    const { el, http, fixture } = await setup('u1', tracks);
    await flushAlbum(http);
    fixture.detectChanges();
    expect(rowTitles(el)).toEqual(['Song t1', 'Song t2', 'Song t3']);

    // Move up on the last row.
    el.querySelectorAll<HTMLButtonElement>('.track-table__track--more-btn')[2].click();
    await settle();
    const moveUp = [...document.querySelectorAll<HTMLButtonElement>('.cdk-overlay-container .menu__item')].find(
      (button) => button.textContent?.includes('Move up'),
    )!;
    moveUp.click();
    await settle();
    fixture.detectChanges();
    expect(rowTitles(el)).toEqual(['Song t1', 'Song t3', 'Song t2']);

    // The hidden song keeps its place, so t3 lands at index 2 of [gone, t1, t2]: before t2.
    const move = http.expectOne('/api/v1/playlists/p1/tracks/t3/move');
    expect(move.request.body).toEqual({ targetPositionIndex: 2 });
    move.flush(null);
    await settle();

    // Then it reloads, keeping the new order on screen meanwhile.
    http.expectOne((r) => r.url === '/api/v1/me/playlists').flush([]);
    const reordered = [tracks[0], tracks[1], tracks[3], tracks[2]].map((track, index) => ({ id: track.id, position: index }));
    http.expectOne('/api/v1/playlists/p1').flush(details('u1', reordered));
    fixture.detectChanges();
    expect(rowTitles(el)).toEqual(['Song t1', 'Song t3', 'Song t2']);
    await settle();
    flushTracks(http, reordered.map((track) => track.id));
    await flushAlbum(http);
    fixture.detectChanges();
    expect(rowTitles(el)).toEqual(['Song t1', 'Song t3', 'Song t2']);
  });

  it("doesn't let others reorder", async () => {
    const { el, http, fixture } = await setup('someone-else', [
      { id: 't1', position: 1 },
      { id: 't2', position: 2 },
    ]);
    await flushAlbum(http);
    fixture.detectChanges();
    el.querySelector<HTMLButtonElement>('.track-table__track--more-btn')!.click();
    await settle();
    const labels = [...document.querySelectorAll('.cdk-overlay-container .menu__label')].map((l) => l.textContent?.trim());
    expect(labels).not.toContain('Move down');
  });
});
