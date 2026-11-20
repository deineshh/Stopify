import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { LayoutStore } from '@core/layout/layout.store';
import { PlayerService } from '@data/services/player.service';
import { RangeSlider } from '@shared/ui/range-slider/range-slider';
import { DurationPipe } from '@shared/pipes/duration.pipe';
import { ArtistNamesPipe } from '@shared/pipes/artist-names.pipe';

/**
 * Bottom player bar. On phones the same component renders as the compact
 * mini player that floats above the tab bar.
 */
@Component({
  selector: 'app-now-playing-bar',
  imports: [RouterLink, FaIconComponent, RangeSlider, DurationPipe, ArtistNamesPipe],
  templateUrl: './now-playing-bar.html',
  styleUrl: './now-playing-bar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'now-playing-bar layout-section',
    '[class.empty]': '!player.track()',
    role: 'region',
    'aria-label': 'Now playing',
  },
})
export class NowPlayingBar {
  protected readonly layout = inject(LayoutStore);
  protected readonly player = inject(PlayerService);

  protected readonly volumeIcon = computed(() =>
    this.player.effectiveVolume() === 0 ? 'volume-xmark' : 'volume-high',
  );

  protected readonly repeatTitle = computed(() => {
    switch (this.player.repeat()) {
      case 'off':
        return 'Enable repeat';
      case 'all':
        return 'Enable repeat one';
      default:
        return 'Disable repeat';
    }
  });
}
