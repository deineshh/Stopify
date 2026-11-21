import { ChangeDetectionStrategy, Component, inject, input, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { CollectionService } from '@data/services/collection.service';
import { CollectionLayout } from '@shared/ui/collection-layout/collection-layout';
import { MediaShelf } from '@shared/ui/media-shelf/media-shelf';
import { PageState } from '@shared/ui/page-state/page-state';
import { TrackTable } from '@shared/ui/track-table/track-table';

@Component({
  selector: 'app-artist-page',
  imports: [CollectionLayout, MediaShelf, PageState, TrackTable],
  templateUrl: './artist-page.html',
  styleUrl: './artist-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'artist-page' },
})
export class ArtistPage {
  readonly id = input.required<string>();

  private readonly collections = inject(CollectionService);

  protected readonly artist = rxResource({
    params: () => this.id(),
    stream: ({ params }) => this.collections.getArtist(params),
  });

  protected readonly shuffle = signal(false);
  protected readonly following = signal<boolean | null>(false);
}
