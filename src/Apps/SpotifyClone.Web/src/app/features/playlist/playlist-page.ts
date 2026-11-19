import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { rxResource } from '@angular/core/rxjs-interop';
import { CatalogService } from '@data/services/catalog.service';
import { PlayerService } from '@data/services/player.service';
import { CollectionLayout, CollectionMetaItem } from '@shared/ui/collection-layout/collection-layout';
import { MediaShelf } from '@shared/ui/media-shelf/media-shelf';
import { TrackTable } from '@shared/ui/track-table/track-table';
import { DurationPipe } from '@shared/pipes/duration.pipe';

@Component({
  selector: 'app-playlist-page',
  imports: [DatePipe, DecimalPipe, CollectionLayout, MediaShelf, TrackTable],
  templateUrl: './playlist-page.html',
  styleUrl: './playlist-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'playlist-page' },
})
export class PlaylistPage {
  /** Bound from the `:id` route parameter. */
  readonly id = input.required<string>();

  private readonly catalog = inject(CatalogService);
  protected readonly player = inject(PlayerService);

  protected readonly playlist = rxResource({
    params: () => this.id(),
    stream: ({ params }) => this.catalog.getPlaylist(params),
  });

  protected readonly following = signal<boolean | null>(true);
  protected readonly saved = signal<boolean | null>(false);
  protected readonly shuffle = signal(true);

  private readonly datePipe = new DatePipe('en-US');
  private readonly durationPipe = new DurationPipe();

  protected readonly meta = computed<CollectionMetaItem[]>(() => {
    const data = this.playlist.value();
    if (!data) {
      return [];
    }
    return [
      { text: data.owner.name, link: ['/playlist', data.owner.id] },
      { text: this.datePipe.transform(data.releaseDate, 'y') ?? '', title: this.datePipe.transform(data.releaseDate, 'MMMM d, y') ?? '' },
      { text: `${data.trackCount} songs, ${this.durationPipe.transform(data.totalDurationSeconds, 'long')}` },
    ];
  });
}
