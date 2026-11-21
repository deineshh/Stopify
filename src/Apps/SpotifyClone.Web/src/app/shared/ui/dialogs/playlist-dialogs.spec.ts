import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { PlaylistDialogs } from './playlist-dialogs';
import { provideTestDefaults } from '../../../testing/test-providers';
import { mockAuthStore } from '../../../testing/auth-store.mock';

const roadTrip = { id: 'p1', name: 'Road trip', description: 'Long drives', isPublic: true, cover: '/images/c/image.webp' };

describe('PlaylistDialogs', () => {
  async function setup() {
    TestBed.configureTestingModule({ providers: [...provideTestDefaults(), mockAuthStore('authenticated').provider] });
    const dialogs = TestBed.inject(PlaylistDialogs);
    const http = TestBed.inject(HttpTestingController);
    vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    await settle();
    for (const req of http.match((r) => r.url === '/api/v1/me/playlists')) req.flush([]);
    return { dialogs, http };
  }

  async function settle() {
    TestBed.inject(ApplicationRef).tick();
    await new Promise((resolve) => setTimeout(resolve));
    TestBed.inject(ApplicationRef).tick();
  }

  const $ = <T extends Element = HTMLElement>(selector: string) => document.querySelector<T & HTMLElement>(selector);
  function type(selector: string, value: string) {
    const field = $<HTMLInputElement>(selector)!;
    field.value = value;
    field.dispatchEvent(new Event('input'));
  }

  afterEach(() => {
    document.querySelectorAll('.cdk-overlay-container').forEach((container) => (container.innerHTML = ''));
  });

  describe('Edit details', () => {
    // Initial focus (the name field) is checked in the browser e2e: jsdom has no layout, so the
    // CDK focus trap finds nothing it considers visible.
    it('opens filled in and labelled', async () => {
      const { dialogs } = await setup();
      dialogs.editDetails(roadTrip);
      await settle();

      expect($('[role="dialog"]')?.getAttribute('aria-labelledby')).toBe('edit-playlist-title');
      expect($<HTMLInputElement>('.edit-playlist__input[name="name"]')?.value).toBe('Road trip');
      expect($<HTMLTextAreaElement>('textarea[name="description"]')?.value).toBe('Long drives');
      expect($('.edit-playlist__cover-img')?.getAttribute('src')).toBe('/images/c/image.webp');
    });

    it('requires a name', async () => {
      const { dialogs } = await setup();
      dialogs.editDetails(roadTrip);
      await settle();

      type('.edit-playlist__input[name="name"]', '   ');
      await settle();
      expect($<HTMLButtonElement>('.dialog__btn--primary')?.disabled).toBe(true);
      expect($('#edit-playlist-name-hint')?.textContent).toContain('Playlist name is required.');
    });

    it('saves the trimmed values, keeps the visibility and closes', async () => {
      const { dialogs, http } = await setup();
      dialogs.editDetails(roadTrip);
      await settle();

      type('.edit-playlist__input[name="name"]', '  Night drive ');
      type('textarea[name="description"]', '   ');
      await settle();
      $('form.edit-playlist')!.dispatchEvent(new Event('submit', { cancelable: true }));
      const req = http.expectOne('/api/v1/playlists/p1');
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual({ name: 'Night drive', description: null, isPublic: true });
      req.flush(null);
      await settle();
      expect($('.edit-playlist')).toBeNull();
      http.expectOne((r) => r.url === '/api/v1/me/playlists').flush([]);
    });

    it('closes without a request when nothing changed', async () => {
      const { dialogs, http } = await setup();
      dialogs.editDetails(roadTrip);
      await settle();
      $('form.edit-playlist')!.dispatchEvent(new Event('submit', { cancelable: true }));
      await settle();
      http.expectNone('/api/v1/playlists/p1');
      expect($('.edit-playlist')).toBeNull();
    });

    it('shows API errors inside the dialog and stays open', async () => {
      const { dialogs, http } = await setup();
      dialogs.editDetails(roadTrip);
      await settle();
      type('.edit-playlist__input[name="name"]', 'Mine now');
      await settle();
      $('form.edit-playlist')!.dispatchEvent(new Event('submit', { cancelable: true }));
      http.expectOne('/api/v1/playlists/p1').flush({ title: 'Playlist.NotOwned' }, { status: 403, statusText: 'Forbidden' });
      await settle();
      expect($('.dialog__error')?.textContent).toContain('Only the owner can edit this playlist.');
      expect($<HTMLButtonElement>('.dialog__btn--primary')?.disabled).toBe(false);
    });
  });

  describe('Delete', () => {
    it('does nothing when cancelled', async () => {
      const { dialogs, http } = await setup();
      dialogs.confirmDelete(roadTrip);
      await settle();
      expect($('[role="alertdialog"]')).toBeTruthy();
      expect($('.dialog__text')?.textContent).toContain('This will delete Road trip from Your Library.');
      $<HTMLButtonElement>('.dialog__btn--secondary')!.click();
      await settle();
      http.expectNone('/api/v1/playlists/p1');
    });

    it('deletes once confirmed', async () => {
      const { dialogs, http } = await setup();
      dialogs.confirmDelete(roadTrip);
      await settle();
      $<HTMLButtonElement>('.dialog__btn--danger')!.click();
      await settle();
      const req = http.expectOne('/api/v1/playlists/p1');
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
      await settle();
      http.expectOne((r) => r.url === '/api/v1/me/playlists').flush([]);
    });
  });
});
