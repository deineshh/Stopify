import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/** Pill-shaped filter toggle ("All", "Music", "Podcasts", ...). */
@Component({
  selector: 'button[appFilterChip]',
  template: `<span class="filter-btn-span"><ng-content /></span>`,
  styleUrl: './filter-chip.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'filter-btn',
    type: 'button',
    '[class.active]': 'active()',
    '[attr.aria-pressed]': 'active()',
  },
})
export class FilterChip {
  readonly active = input(false);
}
