import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { LibraryService } from './library.service';
import { mockAuthStore } from '../../testing/auth-store.mock';
import {
  flushLibrary,
  isCollaborativePlaylistsRequest,
  isOwnPlaylistsRequest,
} from '../../testing/library.fixtures';

const playlist = (id: string, name: string, extra: object = {}) => ({
  id,
  name,
  description: null,
  isPublic: false,
  ownerId: 'u1',
  customCover: null,
  generatedCoverImageIds: [],
  ...extra,
});

describe('LibraryService', () => {
  function setup(status: 'anonymous' | 'authenticated' = 'authenticated') {
    const auth = mockAuthStore(status);
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting(), auth.provider] });
    const service = TestBed.inject(LibraryService);
    const http = TestBed.inject(HttpTestingController);
    TestBed.tick();
    return { service, http, auth };
  }

  /** Lets the resource apply the response. */
  async function settle() {
    await TestBed.inject(ApplicationRef).whenStable();
  }

  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('loads nothing while signed out', () => {
    const { service, http } = setup('anonymous');
    http.expectNone(isOwnPlaylistsRequest);
    http.expectNone(isCollaborativePlaylistsRequest);
    expect(service.items()).toEqual([]);
    expect(service.isLoading()).toBe(false);
  });

  it("maps the user's playlists, then the ones they collaborate on", async () => {
    const { service, http } = setup();
    expect(service.isLoading()).toBe(true);
    const collaborative = http.expectOne(isCollaborativePlaylistsRequest);
    expect(collaborative.request.params.getAll('collaboratorIds')).toEqual(['u1']);
    collaborative.flush([playlist('p3', 'Band practice', { ownerId: 'u2' }), playlist('p1', 'Road trip')]);
    http.expectOne(isOwnPlaylistsRequest).flush([
      playlist('p1', 'Road trip', { generatedCoverImageIds: ['g1'] }),
      playlist('p2', 'Shared mix', { ownerId: 'someone-else' }),
    ]);
    await settle();

    expect(service.items()).toEqual([
      {
        id: 'p1',
        title: 'Road trip',
        kind: 'playlist',
        cover: '/images/g1/image.webp',
        subtitle: 'Aqua Studios',
        link: ['/playlist', 'p1'],
        owned: true,
        collaborative: false,
        isPublic: false,
        description: null,
      },
      {
        id: 'p2',
        title: 'Shared mix',
        kind: 'playlist',
        cover: undefined,
        subtitle: undefined,
        link: ['/playlist', 'p2'],
        owned: false,
        collaborative: false,
        isPublic: false,
        description: null,
      },
      {
        id: 'p3',
        title: 'Band practice',
        kind: 'playlist',
        cover: undefined,
        subtitle: 'Collaborative',
        link: ['/playlist', 'p3'],
        owned: false,
        collaborative: true,
        isPublic: false,
        description: null,
      },
    ]);
    // "Add to playlist" offers your own playlists and the ones you collaborate on.
    expect(service.editablePlaylists().map((item) => item.id)).toEqual(['p1', 'p3']);
  });

  it('keeps your own playlists when the collaborative ones fail to load', async () => {
    const { service, http } = setup();
    http.expectOne(isCollaborativePlaylistsRequest).flush(null, { status: 500, statusText: 'Server Error' });
    http.expectOne(isOwnPlaylistsRequest).flush([playlist('p1', 'Road trip')]);
    await settle();
    expect(service.items().map((item) => item.id)).toEqual(['p1']);
    expect(service.error()).toBeFalsy();
  });

  it('filters by title and kind', async () => {
    const { service, http } = setup();
    flushLibrary(http, [playlist('p1', 'Road trip'), playlist('p2', 'Night drive')]);
    await settle();

    service.query.set('ROAD');
    expect(service.visibleItems().map((i) => i.id)).toEqual(['p1']);
    service.query.set('');

    service.toggleFilter('artists');
    expect(service.filter()).toBe('artists');
    expect(service.visibleItems()).toEqual([]);
    service.toggleFilter('artists');
    expect(service.filter()).toBeNull();
    expect(service.visibleItems().length).toBe(2);
  });

  it('drops the library on sign-out', async () => {
    const { service, http, auth } = setup();
    flushLibrary(http, [playlist('p1', 'Road trip')]);
    await settle();
    expect(service.items().length).toBe(1);

    auth.mock.logout();
    TestBed.tick();
    expect(service.items()).toEqual([]);
  });

  it('exposes errors and reloads on request', async () => {
    const { service, http } = setup();
    http.expectOne(isCollaborativePlaylistsRequest).flush([]);
    http.expectOne(isOwnPlaylistsRequest).flush(null, { status: 500, statusText: 'Server Error' });
    await settle();
    expect(service.error()).toBeTruthy();

    service.reload();
    TestBed.tick();
    flushLibrary(http, []);
    await settle();
    expect(service.error()).toBeFalsy();
  });
});
