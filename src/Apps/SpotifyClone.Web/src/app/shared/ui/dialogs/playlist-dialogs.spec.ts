import { ApplicationRef, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { PlaylistDialogs } from './playlist-dialogs';
import { CoverUploadError, PlaylistEditor } from '@data/services/playlist-editor.service';
import { ApiError } from '@core/api/api-error';
import { of, throwError } from 'rxjs';
import { provideTestDefaults } from '../../../testing/test-providers';
import { mockAuthStore } from '../../../testing/auth-store.mock';
import { flushLibrary, flushAnyLibrary } from '../../../testing/library.fixtures';

const roadTrip = { id: 'p1', name: 'Road trip', description: 'Long drives', isPublic: true, cover: '/images/c/image.webp' };

describe('PlaylistDialogs', () => {
  async function setup() {
    TestBed.configureTestingModule({ providers: [...provideTestDefaults(), mockAuthStore('authenticated').provider] });
    const dialogs = TestBed.inject(PlaylistDialogs);
    const http = TestBed.inject(HttpTestingController);
    vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    await settle();
    flushAnyLibrary(http);
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
      flushLibrary(http);
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

  describe('Edit details: cover photo', () => {
    const photo = (type = 'image/png', size = 1000) => new File([new Uint8Array(size)], 'cover.png', { type });
    function choose(file: File) {
      const input = $<HTMLInputElement>('input[type="file"][name="photo"]')!;
      Object.defineProperty(input, 'files', { configurable: true, value: [file] });
      input.dispatchEvent(new Event('change'));
    }

    beforeEach(() => {
      URL.createObjectURL = vi.fn(() => 'blob:preview');
      URL.revokeObjectURL = vi.fn();
    });

    it('checks the photo and shows it at once', async () => {
      const { dialogs } = await setup();
      dialogs.editDetails(roadTrip);
      await settle();
      expect($('.edit-playlist__cover')?.textContent).toContain('Choose photo');

      choose(photo('application/pdf'));
      await settle();
      expect($('.dialog__error')?.textContent).toContain('Choose a JPEG, PNG or WebP image.');

      choose(photo('image/png', 11 * 1024 * 1024));
      await settle();
      expect($('.dialog__error')?.textContent).toContain('Choose one under 10 MB.');

      choose(photo());
      await settle();
      expect($('.dialog__error')).toBeNull();
      expect($('.edit-playlist__cover-img')?.getAttribute('src')).toBe('blob:preview');
    });

    it('uploads the photo on Save, then the details', async () => {
      const { dialogs, http } = await setup();
      const editor = TestBed.inject(PlaylistEditor);
      const changeCover = vi.spyOn(editor, 'changeCover').mockReturnValue(of(undefined));
      dialogs.editDetails(roadTrip);
      await settle();
      const file = photo();
      choose(file);
      type('.edit-playlist__input[name="name"]', 'Night drive');
      await settle();
      $('form.edit-playlist')!.dispatchEvent(new Event('submit', { cancelable: true }));

      expect(changeCover).toHaveBeenCalledWith(roadTrip, file);
      const details = http.expectOne('/api/v1/playlists/p1');
      expect(details.request.body.name).toBe('Night drive');
      details.flush(null);
      await settle();
      expect($('.edit-playlist')).toBeNull();
      expect(URL.revokeObjectURL).toHaveBeenCalledWith('blob:preview');
      flushLibrary(http);
    });

    it('explains a failed upload and stays open', async () => {
      const { dialogs, http } = await setup();
      const editor = TestBed.inject(PlaylistEditor);
      vi.spyOn(editor, 'changeCover').mockReturnValue(
        throwError(() => new CoverUploadError('processing', new ApiError(404, 'NotFound', 'Not found'))),
      );
      dialogs.editDetails(roadTrip);
      await settle();
      choose(photo());
      type('.edit-playlist__input[name="name"]', 'Night drive');
      await settle();
      $('form.edit-playlist')!.dispatchEvent(new Event('submit', { cancelable: true }));
      await settle();
      expect($('.dialog__error')?.textContent).toContain('Your photo is taking too long to process.');
      // The details weren't sent: the save stopped at the photo.
      http.expectNone('/api/v1/playlists/p1');
      expect($<HTMLButtonElement>('.dialog__btn--primary')?.textContent?.trim()).toBe('Save');
    });
  });

  describe('Edit details: removing the photo', () => {
    const withPhoto = { ...roadTrip, hasCustomCover: true };
    const removeButton = () => $<HTMLButtonElement>('.edit-playlist__remove-photo');
    const submit = () => $('form.edit-playlist')!.dispatchEvent(new Event('submit', { cancelable: true }));

    beforeEach(() => {
      URL.createObjectURL = vi.fn(() => 'blob:preview');
      URL.revokeObjectURL = vi.fn();
    });

    it("isn't offered for a cover made from the songs", async () => {
      const { dialogs } = await setup();
      dialogs.editDetails(roadTrip);
      await settle();
      expect(removeButton()).toBeNull();
    });

    it('removes an uploaded photo on Save', async () => {
      const { dialogs, http } = await setup();
      dialogs.editDetails(withPhoto);
      await settle();
      removeButton()!.click();
      await settle();
      expect($('.edit-playlist__cover-img')).toBeNull();
      expect(removeButton()).toBeNull();
      expect(document.activeElement).toBe($('.edit-playlist__cover'));

      submit();
      const req = http.expectOne('/api/v1/playlists/p1/cover');
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
      await settle();
      expect($('.edit-playlist')).toBeNull();
      flushLibrary(http);
    });

    it("explains that the API can't remove photos yet", async () => {
      const { dialogs, http } = await setup();
      dialogs.editDetails(withPhoto);
      await settle();
      removeButton()!.click();
      await settle();
      submit();
      http.expectOne('/api/v1/playlists/p1/cover').flush(null, { status: 405, statusText: 'Method Not Allowed' });
      await settle();
      expect($('.dialog__error')?.textContent).toContain("Removing photos isn't available yet.");
      expect($('.edit-playlist')).toBeTruthy();
    });

    it('drops a newly chosen photo without any request', async () => {
      const { dialogs, http } = await setup();
      dialogs.editDetails(roadTrip);
      await settle();
      const input = $<HTMLInputElement>('input[type="file"][name="photo"]')!;
      Object.defineProperty(input, 'files', { configurable: true, value: [new File(['x'], 'a.png', { type: 'image/png' })] });
      input.dispatchEvent(new Event('change'));
      await settle();
      removeButton()!.click();
      await settle();
      expect($('.edit-playlist__cover-img')?.getAttribute('src')).toBe('/images/c/image.webp');
      expect(URL.revokeObjectURL).toHaveBeenCalledWith('blob:preview');
      submit();
      await settle();
      http.expectNone('/api/v1/playlists/p1/cover');
      expect($('.edit-playlist')).toBeNull();
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
      flushLibrary(http);
    });
  });

  describe('Invite collaborators', () => {
    const anna = { id: 'u2', name: 'Anna', avatar: null };
    const searchUsers = (users: { id: string; name: string }[]) => ({
      users: users.map((user) => ({ ...user, avatarImageId: null })),
      genres: [],
      moods: [],
      artists: [],
      albums: [],
      tracks: [],
      playlists: [],
    });
    const names = (section: string) =>
      [...document.querySelectorAll(`[aria-labelledby="${section}"] .collaborators__name`)].map((n) => n.textContent?.trim());
    const button = (label: string) => $<HTMLButtonElement>(`.collaborators [aria-label="${label}"]`)!;

    async function open() {
      const { dialogs, http } = await setup();
      const collaborators = signal([anna]);
      await dialogs.collaborators({ playlist: roadTrip, ownerId: 'u1', collaborators });
      await settle();
      return { http, collaborators };
    }

    async function search(http: HttpTestingController, term: string, users: { id: string; name: string }[]) {
      type('.collaborators__search-input', term);
      await settle();
      // The search waits for typing to pause.
      await new Promise((resolve) => setTimeout(resolve, 300));
      const req = http.expectOne((r) => r.url === '/api/v1/search');
      expect(req.request.params.get('searchTerm')).toBe(term);
      req.flush(searchUsers(users));
      await settle();
    }

    it('lists the collaborators and finds people to add, without the owner or current ones', async () => {
      const { http } = await open();
      expect($('[role="dialog"]')?.getAttribute('aria-labelledby')).toBe('collaborators-title');
      expect(names('collaborators-members-title')).toEqual(['Anna']);

      await search(http, 'a', [
        { id: 'u1', name: 'Aqua Studios' },
        { id: 'u2', name: 'Anna' },
        { id: 'u3', name: 'Bence' },
      ]);
      expect(names('collaborators-results-title')).toEqual(['Bence']);
      expect($('.collaborators__status')?.textContent).toContain('1 person found.');
    });

    it('adds and removes collaborators', async () => {
      const { http } = await open();
      await search(http, 'ben', [{ id: 'u3', name: 'Bence' }]);

      button('Add Bence').click();
      const add = http.expectOne('/api/v1/playlists/p1/collaborators');
      expect(add.request.method).toBe('POST');
      expect(add.request.body).toEqual({ collaboratorId: 'u3' });
      await settle();
      expect(button('Add Bence').disabled).toBe(true);
      add.flush(null);
      await settle();
      expect(names('collaborators-members-title')).toEqual(['Anna', 'Bence']);
      expect(names('collaborators-results-title')).toEqual([]);
      flushLibrary(http);

      button('Remove Anna').click();
      const remove = http.expectOne('/api/v1/playlists/p1/collaborators/u2');
      expect(remove.request.method).toBe('DELETE');
      remove.flush(null);
      await settle();
      expect(names('collaborators-members-title')).toEqual(['Bence']);
      flushLibrary(http);
    });

    it('says when no one matches and keeps the list when adding fails', async () => {
      const { http } = await open();
      await search(http, 'zz', []);
      expect($('.collaborators__status')?.textContent).toContain('No one found for “zz”.');

      await search(http, 'ben', [{ id: 'u3', name: 'Bence' }]);
      button('Add Bence').click();
      http
        .expectOne('/api/v1/playlists/p1/collaborators')
        .flush({ title: 'Playlist.InvalidCollaborator', status: 400 }, { status: 400, statusText: 'Bad Request' });
      await settle();
      expect(names('collaborators-members-title')).toEqual(['Anna']);
      expect(button('Add Bence').disabled).toBe(false);
    });
  });
});
