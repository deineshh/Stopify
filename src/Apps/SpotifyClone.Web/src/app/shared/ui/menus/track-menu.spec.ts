import { ApplicationRef, Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { Track } from '@data/models/catalog.models';
import { PlayerService } from '@data/services/player.service';
import { TrackReorder, TrackTable } from '../track-table/track-table';
import { TrackMenuPlaylist } from './track-menu';
import { provideTestDefaults } from '../../../testing/test-providers';
import { mockAuthStore } from '../../../testing/auth-store.mock';
import { flushLibrary } from '../../../testing/library.fixtures';

const pannonia: Track = {
  id: 't1',
  title: 'PANNONIA',
  artists: [
    { id: 'desh', name: 'DESH' },
    { id: 'yf', name: 'Young Fly' },
  ],
  album: { id: 'a1', name: 'BAKPAKK' },
  cover: '/c.webp',
  durationSeconds: 147,
};

const pnf: Track = { ...pannonia, id: 't2', title: 'PNF', artists: [{ id: 'desh', name: 'DESH' }] };

@Component({
  imports: [TrackTable],
  template: `<app-track-table
    [tracks]="tracks()"
    [menuPlaylist]="playlist()"
    [reorderable]="reorderable()"
    (reorder)="moves.push($event)"
  />`,
})
class Host {
  readonly tracks = signal<readonly Track[]>([pannonia]);
  readonly playlist = signal<TrackMenuPlaylist | null>(null);
  readonly reorderable = signal(false);
  readonly moves: TrackReorder[] = [];
}

const ownPlaylist = (id: string, name: string) => ({
  id,
  name,
  description: null,
  isPublic: false,
  ownerId: 'u1',
  customCover: null,
  generatedCoverImageIds: [],
});

describe('TrackMenu (through TrackTable)', () => {
  async function setup(status: 'anonymous' | 'authenticated' = 'authenticated') {
    TestBed.configureTestingModule({ providers: [...provideTestDefaults(), mockAuthStore(status).provider] });
    const http = TestBed.inject(HttpTestingController);
    const router = TestBed.inject(Router);
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    await settle();
    if (status === 'authenticated') {
      flushLibrary(
        http,
        [ownPlaylist('p1', 'Road trip'), { ...ownPlaylist('p2', 'Old mix'), ownerId: 'someone-else' }],
        [{ ...ownPlaylist('p3', 'Shared mix'), ownerId: 'u2' }],
      );
    }
    await settle();
    const row = (fixture.nativeElement as HTMLElement).querySelector<HTMLElement>('.track-table__track')!;
    return { fixture, http, navigate, row, player: TestBed.inject(PlayerService) };
  }

  async function settle() {
    TestBed.inject(ApplicationRef).tick();
    await new Promise((resolve) => setTimeout(resolve));
    TestBed.inject(ApplicationRef).tick();
  }

  async function rightClick(element: HTMLElement) {
    element.dispatchEvent(new MouseEvent('contextmenu', { bubbles: true, cancelable: true, clientX: 20, clientY: 20 }));
    await settle();
  }

  const menus = () => [...document.querySelectorAll<HTMLElement>('.cdk-overlay-container .menu')];
  const labels = (menu: HTMLElement) =>
    [...menu.querySelectorAll('.menu__item .menu__label')].map((label) => label.textContent?.trim());
  const item = (menu: HTMLElement, label: string) =>
    [...menu.querySelectorAll<HTMLButtonElement>('.menu__item')].find(
      (button) => button.querySelector('.menu__label')?.textContent?.trim() === label,
    )!;

  afterEach(() => {
    document.querySelectorAll('.cdk-overlay-container').forEach((container) => (container.innerHTML = ''));
  });

  it('opens on right-click with the song options', async () => {
    const { row } = await setup();
    await rightClick(row);
    expect(menus().length).toBe(1);
    expect(menus()[0].getAttribute('role')).toBe('menu');
    expect(labels(menus()[0])).toEqual(['Add to playlist', 'Add to queue', 'Go to artist', 'Go to album']);
  });

  it('opens from the "…" button too', async () => {
    const { fixture } = await setup();
    (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('.track-table__track--more-btn')!.click();
    await settle();
    expect(menus().length).toBe(1);
  });

  it('adds the song to one of your playlists', async () => {
    const { row, http } = await setup();
    await rightClick(row);
    item(menus()[0], 'Add to playlist').click();
    await settle();

    const submenu = menus()[1];
    // Your playlists and the ones you collaborate on.
    expect(labels(submenu)).toEqual(['New playlist', 'Road trip', 'Shared mix']);
    item(submenu, 'Road trip').click();
    const req = http.expectOne('/api/v1/playlists/p1/tracks');
    expect(req.request.body).toEqual({ trackId: 't1' });
    req.flush(null);
    await settle();
    expect(menus().length).toBe(0);
    flushLibrary(http);
  });

  it('offers "Remove from this playlist" only where allowed', async () => {
    const { row, fixture, http } = await setup();
    fixture.componentInstance.playlist.set({ id: 'p1', name: 'Road trip', canRemove: true });
    fixture.detectChanges();
    await rightClick(row);
    expect(labels(menus()[0])).toContain('Remove from this playlist');

    item(menus()[0], 'Remove from this playlist').click();
    const req = http.expectOne('/api/v1/playlists/p1/tracks/t1');
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
    await settle();
    flushLibrary(http);

    fixture.componentInstance.playlist.set({ id: 'p2', name: 'Shared mix', canRemove: false });
    fixture.detectChanges();
    await rightClick(row);
    expect(labels(menus()[0])).not.toContain('Remove from this playlist');
  });

  it('queues the song and lists several artists in a submenu', async () => {
    const { row, player, navigate } = await setup();
    const addToQueue = vi.spyOn(player, 'addToQueue').mockImplementation(() => undefined);
    await rightClick(row);
    item(menus()[0], 'Add to queue').click();
    expect(addToQueue).toHaveBeenCalledWith(pannonia);
    await settle();

    await rightClick(row);
    item(menus()[0], 'Go to artist').click();
    await settle();
    expect(labels(menus()[1])).toEqual(['DESH', 'Young Fly']);
    item(menus()[1], 'Young Fly').click();
    expect(navigate).toHaveBeenCalledWith(['/artist', 'yf']);
  });

  it('sends guests to log in from "Add to playlist"', async () => {
    const { row, navigate } = await setup('anonymous');
    await rightClick(row);
    item(menus()[0], 'Add to playlist').click();
    expect(navigate).toHaveBeenCalledWith(['/login'], { queryParams: { redirect: '/' } });
  });

  it('offers Move up / Move down in lists you can reorder and keeps focus on the row', async () => {
    const { fixture } = await setup();
    const host = fixture.componentInstance;
    const el = fixture.nativeElement as HTMLElement;
    host.tracks.set([pannonia, pnf]);
    fixture.detectChanges();
    await rightClick(el.querySelectorAll<HTMLElement>('.track-table__track')[0]);
    expect(labels(menus()[0])).not.toContain('Move down');
    menus()[0].dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', keyCode: 27, bubbles: true }));
    await settle();
    expect(menus().length).toBe(0);

    host.reorderable.set(true);
    fixture.detectChanges();
    await rightClick(el.querySelectorAll<HTMLElement>('.track-table__track')[0]);
    expect(labels(menus()[0])).toEqual(['Add to playlist', 'Move down', 'Add to queue', 'Go to artist', 'Go to album']);
    item(menus()[0], 'Move down').click();
    expect(host.moves).toEqual([{ trackId: 't1', from: 0, to: 1 }]);

    // The list applies the move; the moved row's "…" button gets focus back.
    host.tracks.set([pnf, pannonia]);
    await settle();
    expect(document.activeElement).toBe(el.querySelector('[data-track-id="t1"] .track-table__track--more-btn'));

    await settle();
    await rightClick(el.querySelector<HTMLElement>('[data-track-id="t1"]')!);
    expect(menus().length).toBe(1);
    expect(labels(menus()[0])).toContain('Move up');
    expect(labels(menus()[0])).not.toContain('Move down');
    item(menus()[0], 'Move up').click();
    expect(host.moves.at(-1)).toEqual({ trackId: 't1', from: 1, to: 0 });
  });
});
