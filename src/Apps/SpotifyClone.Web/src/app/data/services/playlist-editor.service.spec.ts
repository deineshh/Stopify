import { TestBed } from '@angular/core/testing';
import { HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { ToastService } from '@core/notifications/toast.service';
import { PlaylistEditor } from './playlist-editor.service';
import { provideTestDefaults } from '../../testing/test-providers';
import { mockAuthStore } from '../../testing/auth-store.mock';
import { flushLibrary, flushAnyLibrary } from '../../testing/library.fixtures';

const roadTrip = { id: 'p1', name: 'Road trip', description: 'Long drives', isPublic: false };

describe('PlaylistEditor', () => {
  async function setup(status: 'anonymous' | 'authenticated' = 'authenticated') {
    TestBed.configureTestingModule({ providers: [...provideTestDefaults(), mockAuthStore(status).provider] });
    const editor = TestBed.inject(PlaylistEditor);
    const http = TestBed.inject(HttpTestingController);
    const router = TestBed.inject(Router);
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    const navigateByUrl = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    const toast = TestBed.inject(ToastService);
    await settleEffects();
    // The library loads the user's playlists as soon as it exists.
    flushAnyLibrary(http);
    return { editor, http, navigate, navigateByUrl, toast, router };
  }

  /** Resources send their request a moment after the effect that starts them. */
  async function settleEffects() {
    TestBed.tick();
    await new Promise((resolve) => setTimeout(resolve));
  }

  /** A change reloads Your Library (names, covers and the list itself can change). */
  async function expectLibraryReload(http: HttpTestingController) {
    await settleEffects();
    flushLibrary(http);
  }

  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('sends guests to log in instead of calling the API', async () => {
    const { editor, http, navigate } = await setup('anonymous');
    editor.create().subscribe();
    expect(navigate).toHaveBeenCalledWith(['/login'], { queryParams: { redirect: '/' } });
    http.expectNone('/api/v1/playlists');
  });

  it('creates a playlist, opens it and refreshes the library', async () => {
    const { editor, http, navigate } = await setup();
    const created = vi.fn();
    editor.create().subscribe(created);

    const req = http.expectOne((r) => r.method === 'POST' && r.url === '/api/v1/playlists');
    req.flush({ playlistId: 'new' }, { status: 201, statusText: 'Created' });
    expect(created).toHaveBeenCalledWith('new');
    expect(navigate).toHaveBeenCalledWith(['/playlist', 'new']);
    expect(editor.revision('new')).toBe(1);
    await expectLibraryReload(http);
  });

  it('creates a playlist with a song in it', async () => {
    const { editor, http, toast } = await setup();
    editor.createWith('t1').subscribe();
    http.expectOne((r) => r.method === 'POST' && r.url === '/api/v1/playlists').flush({ playlistId: 'new' });
    const add = http.expectOne('/api/v1/playlists/new/tracks');
    expect(add.request.body).toEqual({ trackId: 't1' });
    add.flush(null, { status: 204, statusText: 'No Content' });
    expect(toast.current()?.text).toBe('Added to a new playlist.');
    await expectLibraryReload(http);
  });

  it('adds a song and says so, or explains that it is already there', async () => {
    const { editor, http, toast } = await setup();
    editor.addTrack(roadTrip, 't1').subscribe();
    http.expectOne((r) => r.method === 'POST' && r.url === '/api/v1/playlists/p1/tracks').flush(null);
    expect(toast.current()?.text).toBe('Added to Road trip.');
    expect(editor.revision('p1')).toBe(1);
    await expectLibraryReload(http);

    editor.addTrack(roadTrip, 't1').subscribe();
    http
      .expectOne('/api/v1/playlists/p1/tracks')
      .flush({ title: 'Playlist.InvalidTrack', status: 400 }, { status: 400, statusText: 'Bad Request' });
    expect(toast.current()?.text).toBe('This song is already in Road trip.');
    expect(editor.revision('p1')).toBe(1);
  });

  it('removes a song', async () => {
    const { editor, http, toast } = await setup();
    editor.removeTrack(roadTrip, 't1').subscribe();
    const req = http.expectOne('/api/v1/playlists/p1/tracks/t1');
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
    expect(toast.current()?.text).toBe('Removed from Road trip.');
    await expectLibraryReload(http);
  });

  it('keeps the name and description when changing who can see it', async () => {
    const { editor, http, toast } = await setup();
    editor.setPublic(roadTrip, true).subscribe();
    const req = http.expectOne('/api/v1/playlists/p1');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ name: 'Road trip', description: 'Long drives', isPublic: true });
    req.flush(null);
    expect(toast.current()?.text).toBe('Road trip is now public.');
    await expectLibraryReload(http);
  });

  it('passes edit errors on for the dialog to show', async () => {
    const { editor, http } = await setup();
    const failed = vi.fn();
    editor.editDetails(roadTrip, { name: 'x', description: null, isPublic: false }).subscribe({ error: failed });
    http.expectOne('/api/v1/playlists/p1').flush({ title: 'Playlist.NotOwned', status: 403 }, { status: 403, statusText: 'Forbidden' });
    expect(failed.mock.calls[0][0].has('Playlist.NotOwned')).toBe(true);
  });

  it('deletes a playlist and leaves its page', async () => {
    const { editor, http, navigateByUrl, router, toast } = await setup();
    vi.spyOn(router, 'url', 'get').mockReturnValue('/playlist/p1');
    editor.delete(roadTrip).subscribe();
    const req = http.expectOne('/api/v1/playlists/p1');
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
    expect(toast.current()?.text).toBe('Removed Road trip from Your Library.');
    expect(navigateByUrl).toHaveBeenCalledWith('/');
    // No revision bump: the page must not reload a playlist that no longer exists.
    expect(editor.revision('p1')).toBe(0);
    await expectLibraryReload(http);
  });

  it("explains when the playlist isn't yours", async () => {
    const { editor, http, toast } = await setup();
    editor.delete(roadTrip).subscribe();
    http.expectOne('/api/v1/playlists/p1').flush({ title: 'Playlist.NotOwned' }, { status: 403, statusText: 'Forbidden' });
    expect(toast.current()?.text).toBe("You can't change this playlist.");
  });

  it('sends moves one at a time and reloads once they are all answered', async () => {
    const { editor, http } = await setup();
    editor.moveTrack(roadTrip, 't3', 0);
    editor.moveTrack(roadTrip, 't1', 2);
    expect(editor.isMoving('p1')).toBe(true);

    // The second move waits for the first: it was worked out from the order the first leaves.
    const first = http.expectOne('/api/v1/playlists/p1/tracks/t3/move');
    expect(first.request.method).toBe('POST');
    expect(first.request.body).toEqual({ targetPositionIndex: 0 });
    http.expectNone('/api/v1/playlists/p1/tracks/t1/move');
    first.flush(null);
    expect(editor.revision('p1')).toBe(0);

    const second = http.expectOne('/api/v1/playlists/p1/tracks/t1/move');
    expect(second.request.body).toEqual({ targetPositionIndex: 2 });
    second.flush(null);
    expect(editor.isMoving('p1')).toBe(false);
    expect(editor.revision('p1')).toBe(1);
    await expectLibraryReload(http);
  });

  it('drops the queued moves after a failed one and reloads the real order', async () => {
    const { editor, http, toast } = await setup();
    editor.moveTrack(roadTrip, 't3', 0);
    editor.moveTrack(roadTrip, 't1', 2);
    http
      .expectOne('/api/v1/playlists/p1/tracks/t3/move')
      .flush({ title: 'Playlist.NotOwned' }, { status: 403, statusText: 'Forbidden' });
    http.expectNone('/api/v1/playlists/p1/tracks/t1/move');
    expect(toast.current()?.text).toBe("You can't change this playlist.");
    expect(editor.isMoving('p1')).toBe(false);
    expect(editor.revision('p1')).toBe(1);
    await expectLibraryReload(http);

    // The next move goes out again.
    editor.moveTrack(roadTrip, 't1', 1);
    http.expectOne('/api/v1/playlists/p1/tracks/t1/move').flush(null);
    await expectLibraryReload(http);
  });

  it('sends guests to log in instead of moving', async () => {
    const { editor, http, navigate } = await setup('anonymous');
    editor.moveTrack(roadTrip, 't1', 1);
    expect(navigate).toHaveBeenCalledWith(['/login'], { queryParams: { redirect: '/' } });
    expect(editor.isMoving('p1')).toBe(false);
    http.expectNone((r) => r.url.endsWith('/move'));
  });

  it('adds and removes collaborators', async () => {
    const { editor, http, toast } = await setup();
    editor.addCollaborator(roadTrip, { id: 'u2', name: 'Anna' }).subscribe();
    const add = http.expectOne('/api/v1/playlists/p1/collaborators');
    expect(add.request.body).toEqual({ collaboratorId: 'u2' });
    add.flush(null);
    expect(toast.current()?.text).toBe('Anna can now edit Road trip.');
    expect(editor.revision('p1')).toBe(1);
    await expectLibraryReload(http);

    editor.removeCollaborator(roadTrip, { id: 'u2', name: 'Anna' }).subscribe();
    const remove = http.expectOne('/api/v1/playlists/p1/collaborators/u2');
    expect(remove.request.method).toBe('DELETE');
    remove.flush(null);
    expect(toast.current()?.text).toBe('Anna can no longer edit Road trip.');
    await expectLibraryReload(http);
  });

  it('explains failed collaborator changes', async () => {
    const { editor, http, toast } = await setup();
    editor.addCollaborator(roadTrip, { id: 'u2', name: 'Anna' }).subscribe();
    http
      .expectOne('/api/v1/playlists/p1/collaborators')
      .flush({ title: 'Playlist.InvalidCollaborator' }, { status: 400, statusText: 'Bad Request' });
    expect(toast.current()?.text).toBe("Couldn't add Anna. They may already be a collaborator.");

    editor.addCollaborator(roadTrip, { id: 'u2', name: 'Anna' }).subscribe();
    http.expectOne('/api/v1/playlists/p1/collaborators').flush({ title: 'Playlist.NotOwned' }, { status: 403, statusText: 'Forbidden' });
    expect(toast.current()?.text).toBe("You can't change this playlist.");
  });

  it("tells a collaborator they can only remove songs they added", async () => {
    const { editor, http, toast } = await setup();
    editor.removeTrack(roadTrip, 't1').subscribe();
    http
      .expectOne('/api/v1/playlists/p1/tracks/t1')
      .flush({ title: 'Playlist.InvalidCollaborator' }, { status: 400, statusText: 'Bad Request' });
    expect(toast.current()?.text).toBe('You can only remove songs you added.');
  });
});
