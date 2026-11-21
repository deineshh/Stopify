import { ChangeDetectionStrategy, Component, computed, inject, input, linkedSignal, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { AuthStore } from '@core/auth/auth.store';
import { PlaylistView } from '@data/models/collection.models';
import { PlaylistRef } from '@data/models/playlist.models';
import { CollectionService } from '@data/services/collection.service';
import { PlaylistEditor } from '@data/services/playlist-editor.service';
import { PlaylistDialogs } from '@shared/ui/dialogs/playlist-dialogs';
import { PlaylistMenu } from '@shared/ui/menus/playlist-menu';
import { TrackMenuPlaylist } from '@shared/ui/menus/track-menu';
import { CollectionLayout, CollectionMetaItem } from '@shared/ui/collection-layout/collection-layout';
import { PageState } from '@shared/ui/page-state/page-state';
import { TrackTable } from '@shared/ui/track-table/track-table';
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

  /** What the playlist menu and the edit dialog act on. */
  protected readonly ref = computed<PlaylistRef | null>(() => {
    const playlist = this.view();
    return playlist
      ? { id: playlist.id, name: playlist.title, description: playlist.description, isPublic: playlist.isPublic, cover: playlist.cover }
      : null;
  });

  /** Track rows: "Remove from this playlist" for the owner. */
  protected readonly menuPlaylist = computed<TrackMenuPlaylist | null>(() => {
    const ref = this.ref();
    return ref ? { id: ref.id, name: ref.name, canRemove: this.isOwner() } : null;
  });

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
