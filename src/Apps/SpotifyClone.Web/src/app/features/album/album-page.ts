import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { rxResource } from '@angular/core/rxjs-interop';
import { CollectionService } from '@data/services/collection.service';
import { PlayerService } from '@data/services/player.service';
import { CollectionLayout, CollectionMetaItem } from '@shared/ui/collection-layout/collection-layout';
import { MediaShelf } from '@shared/ui/media-shelf/media-shelf';
import { PageState } from '@shared/ui/page-state/page-state';
import { TrackTable } from '@shared/ui/track-table/track-table';
import { DurationPipe } from '@shared/pipes/duration.pipe';

@Component({
  selector: 'app-album-page',
  imports: [DatePipe, CollectionLayout, MediaShelf, PageState, TrackTable],
  templateUrl: './album-page.html',
  styleUrl: './album-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'album-page' },
})
export class AlbumPage {
  readonly id = input.required<string>();

  private readonly collections = inject(CollectionService);
  protected readonly player = inject(PlayerService);

  protected readonly album = rxResource({
    params: () => this.id(),
    stream: ({ params }) => this.collections.getAlbum(params),
  });

  protected readonly shuffle = signal(false);
  protected readonly saved = signal<boolean | null>(false);

  private readonly datePipe = new DatePipe('en-US');

  protected readonly meta = computed<CollectionMetaItem[]>(() => {
    const album = this.album.hasValue() ? this.album.value() : undefined;
    if (!album) {
      return [];
    }
    const items: CollectionMetaItem[] = album.artists.map((artist) => ({ text: artist.name, link: ['/artist', artist.id] }));
    if (album.releaseDate) {
      items.push({
        text: this.datePipe.transform(album.releaseDate, 'y', 'UTC') ?? '',
        title: this.datePipe.transform(album.releaseDate, 'MMMM d, y', 'UTC') ?? '',
      });
    }
    const count = album.tracks.length;
    items.push({ text: `${count} ${count === 1 ? 'song' : 'songs'}, ${new DurationPipe().transform(album.totalDurationSeconds, 'long')}` });
    return items;
  });
}
