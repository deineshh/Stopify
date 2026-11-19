import { ChangeDetectionStrategy, Component, computed, input, linkedSignal } from '@angular/core';
import { MediaShelf as MediaShelfModel } from '@data/models/catalog.models';
import { MediaCard } from '../media-card/media-card';
import { FilterChip } from '../filter-chip/filter-chip';
import { SingleRowDirective } from '../../directives/single-row.directive';

/**
 * A titled row of media cards (mockup `.track-group`). On desktop it shows a single
 * row and hides overflow; on phones the row scrolls horizontally.
 */
@Component({
  selector: 'app-media-shelf',
  imports: [MediaCard, FilterChip, SingleRowDirective],
  templateUrl: './media-shelf.html',
  styleUrl: './media-shelf.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'track-group',
    '[attr.aria-labelledby]': 'labelledBy()',
    role: 'region',
  },
})
export class MediaShelf {
  readonly shelf = input.required<MediaShelfModel>();

  protected readonly titleId = computed(() => `shelf-${this.shelf().id}-title`);
  protected readonly eyebrowId = computed(() => `shelf-${this.shelf().id}-eyebrow`);
  /** Eyebrow + title, so shelves that share a title ("Aqua Studios") stay distinguishable. */
  protected readonly labelledBy = computed(() =>
    this.shelf().eyebrow ? `${this.eyebrowId()} ${this.titleId()}` : this.titleId(),
  );

  /** Selected filter; resets to the first one whenever the shelf changes. */
  protected readonly activeFilter = linkedSignal(() => this.shelf().filters?.[0] ?? null);
}
