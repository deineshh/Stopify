import { ChangeDetectionStrategy, Component, computed, inject, input, linkedSignal, signal, untracked } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { AuthStore } from '@core/auth/auth.store';
import { PlaylistView } from '@data/models/collection.models';
import { PlaylistRef } from '@data/models/playlist.models';
import { PlaylistOrder, moveInPlaylist } from '@data/models/playlist-order';
import { CollectionService } from '@data/services/collection.service';
import { PlaylistEditor } from '@data/services/playlist-editor.service';
import { PlaylistDialogs } from '@shared/ui/dialogs/playlist-dialogs';
import { PlaylistMenu } from '@shared/ui/menus/playlist-menu';
import { TrackMenuPlaylist } from '@shared/ui/menus/track-menu';
import { CollectionLayout, CollectionMetaItem } from '@shared/ui/collection-layout/collection-layout';
import { PageState } from '@shared/ui/page-state/page-state';
import { TrackReorder, TrackTable } from '@shared/ui/track-table/track-table';
import { DurationPipe } from '@shared/pipes/duration.pipe';

@Component({
  selector: 'app-playlist-page',
  imports: [CollectionLayout, PageState, TrackTable, PlaylistMenu],
  templateUrl: './playlist-page.html',
  styleUrl: './playlist-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'playlist-page' },
})
export class PlaylistPage {
  /** Bound from the `:id` route parameter. */
  readonly id = input.required<string>();

  private readonly collections = inject(CollectionService);
  private readonly auth = inject(AuthStore);
  private readonly editor = inject(PlaylistEditor);
  protected readonly dialogs = inject(PlaylistDialogs);

  /** Keyed on the revision too, so a change made anywhere (add/remove a song, edit) reloads it. */
  protected readonly playlist = rxResource({
    params: () => ({ id: this.id(), revision: this.editor.revision(this.id()) }),
    stream: ({ params }) => this.collections.getPlaylist(params.id),
  });

  /** The loaded playlist; while a newer revision of the same one loads, the old one stays on screen. */
  protected readonly view = linkedSignal<{ id: string; value: PlaylistView | null }, PlaylistView | null>({
    source: () => ({ id: this.id(), value: this.playlist.hasValue() ? this.playlist.value() : null }),
    computation: (source, previous) => source.value ?? (previous?.value?.id === source.id ? previous.value : null),
  });

  /**
   * The songs in the order on screen. A move shows at once; the page keeps this order while
   * moves are on their way, then takes the reloaded playlist (which has them, or undoes a failed one).
   */
  protected readonly order = linkedSignal<PlaylistView | null, PlaylistOrder | null>({
    source: () => this.view(),
    computation: (playlist, previous) => {
      if (!playlist) {
        return null;
      }
      const keep = previous?.value && previous.source?.id === playlist.id && untracked(() => this.editor.isMoving(playlist.id));
      return keep ? previous.value : { ids: playlist.trackIds, tracks: playlist.tracks };
    },
  });

  /** What the playlist menu and the edit dialog act on. */
  protected readonly ref = computed<PlaylistRef | null>(() => {
    const playlist = this.view();
    return playlist
      ? {
          id: playlist.id,
          name: playlist.title,
          description: playlist.description,
          isPublic: playlist.isPublic,
          cover: playlist.cover,
          hasCustomCover: playlist.hasCustomCover,
        }
      : null;
  });

  /** Track rows: "Remove from this playlist" for the owner and collaborators. */
  protected readonly menuPlaylist = computed<TrackMenuPlaylist | null>(() => {
    const ref = this.ref();
    return ref ? { id: ref.id, name: ref.name, canRemove: this.canEditSongs() } : null;
  });

  protected readonly collaborators = computed(() => this.view()?.collaborators ?? []);

  protected readonly isCollaborator = computed(() => {
    const userId = this.auth.user()?.id;
    return !!userId && this.collaborators().some((collaborator) => collaborator.id === userId);
  });

  /**
   * The owner and collaborators add, remove and reorder songs. (A collaborator may only remove
   * songs they added; the API says so if they try another, as the playlist doesn't say who added what.)
   */
  protected readonly canEditSongs = computed(() => this.isOwner() || this.isCollaborator());

  protected openCollaborators(): void {
    const ref = this.ref();
    const playlist = this.view();
    if (ref && playlist) {
      void this.dialogs.collaborators({ playlist: ref, ownerId: playlist.ownerId, collaborators: this.collaborators });
    }
  }

  protected reorder({ from, to }: TrackReorder): void {
    const order = this.order();
    const ref = this.ref();
    const move = order && ref ? moveInPlaylist(order, from, to) : null;
    if (move && ref) {
      this.order.set(move.order);
      this.editor.moveTrack(ref, move.trackId, move.targetIndex);
    }
  }

  protected readonly shuffle = signal(false);
  protected readonly saved = signal<boolean | null>(false);

  protected readonly isOwner = computed(() => {
    const playlist = this.view();
    return !!playlist && playlist.ownerId === this.auth.user()?.id;
  });

  protected readonly meta = computed<CollectionMetaItem[]>(() => {
    const playlist = this.view();
    if (!playlist) {
      return [];
    }
    const items: CollectionMetaItem[] = [];
    const owner = this.auth.user();
    if (owner && owner.id === playlist.ownerId) {
      items.push({ text: owner.displayName, link: ['/collection'] });
    }
    const collaborators = this.collaborators();
    if (collaborators.length) {
      const userId = this.auth.user()?.id;
      const names = collaborators.map((collaborator) => (collaborator.id === userId ? 'You' : collaborator.name));
      items.push({ text: `with ${listNames(names)}`, title: `Collaborators: ${names.join(', ')}` });
    }
    const count = playlist.tracks.length;
    items.push({
      text:
        count === 0
          ? 'No songs yet'
          : `${count} ${count === 1 ? 'song' : 'songs'}, ${new DurationPipe().transform(playlist.totalDurationSeconds, 'long')}`,
    });
    return items;
  });
}

/** "Anna", "Anna and Bence", "Anna, Bence and 2 more". */
function listNames(names: readonly string[]): string {
  if (names.length > 2) {
    return `${names.slice(0, 2).join(', ')} and ${names.length - 2} more`;
  }
  return names.join(' and ');
}
