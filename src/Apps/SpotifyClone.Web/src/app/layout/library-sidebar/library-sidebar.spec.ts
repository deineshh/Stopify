import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { HttpTestingController } from '@angular/common/http/testing';
import { LibrarySidebar } from './library-sidebar';
import { provideTestDefaults } from '../../testing/test-providers';
import { mockAuthStore } from '../../testing/auth-store.mock';
import { flushLibrary, isCollaborativePlaylistsRequest, isOwnPlaylistsRequest } from '../../testing/library.fixtures';

const playlist = (id: string, name: string) => ({
  id,
  name,
  description: null,
  isPublic: false,
  ownerId: 'u1',
  customCover: null,
  generatedCoverImageIds: [],
});

describe('LibrarySidebar', () => {
  function setup(status: 'anonymous' | 'authenticated') {
    const auth = mockAuthStore(status);
    TestBed.configureTestingModule({ providers: [...provideTestDefaults(), auth.provider] });
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(LibrarySidebar);
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    const render = async () => {
      await fixture.whenStable();
      fixture.detectChanges();
    };
    return { el, http, render, fixture };
  }

  it('invites signed-out visitors to log in', () => {
    const { el, http } = setup('anonymous');
    expect(el.querySelector('.left-sidebar__guest-card')).toBeTruthy();
    expect(el.querySelector('.left-sidebar__guest-btn')?.getAttribute('href')).toBe('/login?redirect=%2Fcollection');
    expect(el.querySelector('.left-sidebar__main-item')).toBeNull();
    expect(el.querySelector('.left-sidebar__filter-btns')).toBeNull();
    http.expectNone('/api/v1/me/playlists');
  });

  it("shows a skeleton, then the user's playlists linking to their pages", async () => {
    const { el, http, render } = setup('authenticated');
    expect(el.querySelectorAll('.left-sidebar__skeleton').length).toBe(3);

    flushLibrary(http, [playlist('p1', 'Road trip'), playlist('p2', 'Night drive')]);
    await render();

    const items = el.querySelectorAll('.left-sidebar__main-item');
    expect(items.length).toBe(2);
    const link = items[0].querySelector('a');
    expect(link?.getAttribute('href')).toBe('/playlist/p1');
    expect(link?.getAttribute('aria-label')).toBe('Road trip, Playlist, Aqua Studios');
    expect(items[0].querySelector('.left-sidebar__main-item--description')?.textContent).toContain('Playlist • Aqua Studios');
    expect(el.querySelector('.left-sidebar__filter-btns')).toBeTruthy();
  });

  it('invites a new user to create a playlist', async () => {
    const { el, http, render } = setup('authenticated');
    flushLibrary(http);
    await render();
    expect(el.querySelector('#library-empty-title')?.textContent).toContain('Create your first playlist');
  });

  it('creates a playlist from the empty-library card and from Create', async () => {
    const { el, http, render } = setup('authenticated');
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    flushLibrary(http);
    await render();

    el.querySelector<HTMLButtonElement>('.left-sidebar__guest-card .left-sidebar__guest-btn')!.click();
    http.expectOne((r) => r.method === 'POST' && r.url === '/api/v1/playlists').flush({ playlistId: 'new' });
    expect(navigate).toHaveBeenCalledWith(['/playlist', 'new']);
    await settle();
    flushLibrary(http, [playlist('new', 'My Playlist #1')]);

    el.querySelector<HTMLButtonElement>('.left-sidebar__title-bar--create-btn')!.click();
    http.expectOne((r) => r.method === 'POST' && r.url === '/api/v1/playlists').flush({ playlistId: 'new2' });
    expect(navigate).toHaveBeenCalledWith(['/playlist', 'new2']);
    await settle();
    flushLibrary(http);
  });

  it('opens the playlist menu on right-click, only for your own playlists', async () => {
    const { el, http, render } = setup('authenticated');
    flushLibrary(http, [playlist('p1', 'Road trip'), { ...playlist('p2', 'Shared mix'), ownerId: 'someone-else' }]);
    await render();
    const [own, shared] = el.querySelectorAll<HTMLElement>('.left-sidebar__main-item');

    shared.dispatchEvent(new MouseEvent('contextmenu', { bubbles: true, cancelable: true }));
    await settle();
    expect(document.querySelector('.cdk-overlay-container .menu')).toBeNull();

    own.dispatchEvent(new MouseEvent('contextmenu', { bubbles: true, cancelable: true, clientX: 5, clientY: 5 }));
    await settle();
    const labels = [...document.querySelectorAll('.cdk-overlay-container .menu .menu__label')].map((l) => l.textContent?.trim());
    expect(labels).toEqual(['Edit details', 'Make public', 'Delete']);
    document.querySelectorAll('.cdk-overlay-container').forEach((container) => (container.innerHTML = ''));
  });

  async function settle() {
    TestBed.inject(ApplicationRef).tick();
    await new Promise((resolve) => setTimeout(resolve));
    TestBed.inject(ApplicationRef).tick();
  }

  it('offers a retry when the library fails to load', async () => {
    const { el, http, render, fixture } = setup('authenticated');
    http.expectOne(isCollaborativePlaylistsRequest).flush([]);
    http.expectOne(isOwnPlaylistsRequest).flush(null, { status: 500, statusText: 'Server Error' });
    await render();
    el.querySelector<HTMLButtonElement>('.left-sidebar__retry-btn')!.click();
    TestBed.tick();
    fixture.detectChanges();
    flushLibrary(http, [playlist('p1', 'Road trip')]);
    await render();
    expect(el.querySelectorAll('.left-sidebar__main-item').length).toBe(1);
  });

  it('shows the current visibility the next time the menu opens', async () => {
    const { el, http, render } = setup('authenticated');
    flushLibrary(http, [playlist('p1', 'Road trip')]);
    await render();
    const row = el.querySelector<HTMLElement>('.left-sidebar__main-item')!;
    const open = async () => {
      row.dispatchEvent(new MouseEvent('contextmenu', { bubbles: true, cancelable: true, clientX: 5, clientY: 5 }));
      await settle();
    };
    const item = (label: string) =>
      [...document.querySelectorAll<HTMLButtonElement>('.cdk-overlay-container .menu__item')].find(
        (button) => button.querySelector('.menu__label')?.textContent?.trim() === label,
      );

    await open();
    item('Make public')!.click();
    const req = http.expectOne((r) => r.method === 'PUT' && r.url === '/api/v1/playlists/p1');
    expect(req.request.body.isPublic).toBe(true);
    req.flush(null);
    await settle();
    flushLibrary(http, [{ ...playlist('p1', 'Road trip'), isPublic: true }]);
    await render();

    // Same row, same trigger: the menu must not reuse what it was first opened with.
    await open();
    expect(item('Make private')).toBeTruthy();
    expect(item('Make public')).toBeUndefined();
    document.querySelectorAll('.cdk-overlay-container').forEach((container) => (container.innerHTML = ''));
  });
});
