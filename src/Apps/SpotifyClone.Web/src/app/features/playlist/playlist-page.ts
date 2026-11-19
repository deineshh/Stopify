import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { AuthStore } from '@core/auth/auth.store';
import { CollectionService } from '@data/services/collection.service';
import { PlayerService } from '@data/services/player.service';
import { CollectionLayout, CollectionMetaItem } from '@shared/ui/collection-layout/collection-layout';
import { PageState } from '@shared/ui/page-state/page-state';
import { TrackTable } from '@shared/ui/track-table/track-table';
import { DurationPipe } from '@shared/pipes/duration.pipe';

@Component({
  selector: 'app-playlist-page',
  imports: [CollectionLayout, PageState, TrackTable],
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
  protected readonly player = inject(PlayerService);

  protected readonly playlist = rxResource({
    params: () => this.id(),
    stream: ({ params }) => this.collections.getPlaylist(params),
  });

  protected readonly shuffle = signal(false);
  protected readonly saved = signal<boolean | null>(false);

  protected readonly isOwner = computed(() => {
    const playlist = this.playlist.hasValue() ? this.playlist.value() : undefined;
    return !!playlist && playlist.ownerId === this.auth.user()?.id;
  });

  protected readonly meta = computed<CollectionMetaItem[]>(() => {
    const playlist = this.playlist.hasValue() ? this.playlist.value() : undefined;
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
