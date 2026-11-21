import { Injectable, computed, inject, linkedSignal, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { catchError, forkJoin, map, of } from 'rxjs';
import { AuthStore } from '@core/auth/auth.store';
import { imageUrl } from '@core/api/media';
import { CatalogApi } from '../api/catalog-api';
import { PlaylistSummaryDto } from '../api/catalog.dtos';
import { LibraryFilter, LibraryItem } from '../models/library.models';

/** The API returns at most this many playlists per page; more than enough for a sidebar. */
const LIBRARY_PAGE_SIZE = 50;

/**
 * The signed-in user's library ("Your Library" sidebar): their playlists, then the ones they
 * collaborate on. Loads when a session starts and is dropped on sign-out. The API has no
 * followed artists yet.
 */
@Injectable({ providedIn: 'root' })
export class LibraryService {
  private readonly api = inject(CatalogApi);
  private readonly auth = inject(AuthStore);

  /**
   * Bumped by `reload()`. It's part of the request on purpose: `resource.reload()` is dropped when
   * it lands between a response arriving and being applied, so two quick changes could leave the
   * library stale. A new request always runs, and cancels an older one still in flight.
   */
  private readonly version = signal(0);

  /** Keyed on the user id: idle (no request) while signed out, reloads when the account changes. */
  private readonly playlists = rxResource({
    params: () => {
      const userId = this.auth.user()?.id;
      return userId ? { userId, version: this.version() } : undefined;
    },
    stream: ({ params }) =>
      forkJoin([
        this.api.myPlaylists({ pageSize: LIBRARY_PAGE_SIZE }).pipe(map((page) => page.items)),
        // A failure here shouldn't hide the user's own playlists.
        this.api
          .playlists({ collaboratorIds: [params.userId] }, { pageSize: LIBRARY_PAGE_SIZE })
          .pipe(
            map((page) => page.items),
            catchError(() => of<PlaylistSummaryDto[]>([])),
          ),
      ]).pipe(
        map(([own, shared]) => {
          const ownIds = new Set(own.map((playlist) => playlist.id));
          return [
            ...own.map((playlist) => ({ playlist, collaborative: false })),
            ...shared.filter((playlist) => !ownIds.has(playlist.id)).map((playlist) => ({ playlist, collaborative: true })),
          ];
        }),
      ),
  });

  /** The last loaded list, kept on screen while a newer one loads; empty once signed out. */
  private readonly loaded = linkedSignal<readonly LoadedPlaylist[] | undefined, readonly LoadedPlaylist[]>({
    source: () => {
      if (this.playlists.hasValue()) {
        return this.playlists.value();
      }
      return this.auth.user() ? undefined : [];
    },
    computation: (playlists, previous) => playlists ?? previous?.value ?? [],
  });

  readonly filter = signal<LibraryFilter | null>(null);
  readonly query = signal('');

  readonly isLoading = computed(() => this.playlists.isLoading());
  readonly error = computed(() => this.playlists.error());

  readonly items = computed<readonly LibraryItem[]>(() => {
    const user = this.auth.user();
    return this.loaded().map(({ playlist, collaborative }) =>
      toLibraryItem(playlist, playlist.ownerId === user?.id ? user.displayName : undefined, collaborative),
    );
  });

  /** Playlists the user can add songs to ("Add to playlist"): their own and collaborative ones. */
  readonly editablePlaylists = computed(() =>
    this.items().filter((item) => item.kind === 'playlist' && (item.owned || item.collaborative)),
  );

  readonly visibleItems = computed(() => {
    const filter = this.filter();
    const query = this.query().trim().toLowerCase();
    return this.items().filter(
      (item) =>
        (filter === null || item.kind === filterToKind[filter]) &&
        (query === '' || item.title.toLowerCase().includes(query)),
    );
  });

  toggleFilter(filter: LibraryFilter): void {
    this.filter.update((current) => (current === filter ? null : filter));
  }

  reload(): void {
    this.version.update((version) => version + 1);
  }
}

interface LoadedPlaylist {
  playlist: PlaylistSummaryDto;
  /** Someone else's playlist the user collaborates on. */
  collaborative: boolean;
}

const filterToKind: Record<LibraryFilter, LibraryItem['kind']> = {
  playlists: 'playlist',
  artists: 'artist',
};

function toLibraryItem(playlist: PlaylistSummaryDto, ownerName: string | undefined, collaborative: boolean): LibraryItem {
  return {
    id: playlist.id,
    title: playlist.name,
    kind: 'playlist',
    cover: imageUrl(playlist.customCover?.imageId ?? playlist.generatedCoverImageIds[0], '') || undefined,
    // The API doesn't return the owner's name, so other people's playlists say how they're yours.
    subtitle: ownerName ?? (collaborative ? 'Collaborative' : undefined),
    link: ['/playlist', playlist.id],
    owned: ownerName !== undefined,
    collaborative,
    isPublic: playlist.isPublic,
    description: playlist.description,
  };
}
