import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { LayoutStore } from '@core/layout/layout.store';
import { CatalogService } from '@data/services/catalog.service';
import { PlayerService } from '@data/services/player.service';
import { MediaShelf } from '@shared/ui/media-shelf/media-shelf';
import { PlayButton } from '@shared/ui/play-button/play-button';
import { TrackTable } from '@shared/ui/track-table/track-table';
import { DurationPipe } from '@shared/pipes/duration.pipe';

/** Scroll distance (px) over which the compact sticky header fades in (mockup playlistPageHeaderBgChangeOnScroll.js). */
const HEADER_FADE_DISTANCE = 300;

@Component({
  selector: 'app-playlist-page',
  imports: [
    DatePipe,
    DecimalPipe,
    RouterLink,
    FaIconComponent,
    MediaShelf,
    PlayButton,
    TrackTable,
    DurationPipe,
  ],
  templateUrl: './playlist-page.html',
  styleUrl: './playlist-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'playlist-page' },
})
export class PlaylistPage {
  /** Bound from the `:id` route parameter. */
  readonly id = input.required<string>();

  private readonly catalog = inject(CatalogService);
  private readonly layout = inject(LayoutStore);
  protected readonly player = inject(PlayerService);

  protected readonly playlist = rxResource({
    params: () => this.id(),
    stream: ({ params }) => this.catalog.getPlaylist(params),
  });

  protected readonly following = signal(true);
  protected readonly saved = signal(false);
  protected readonly shuffle = signal(true);

  protected readonly headerOpacity = computed(() =>
    Math.min(this.layout.mainScrollTop() / HEADER_FADE_DISTANCE, 1),
  );
}
