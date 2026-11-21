import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { Track } from '@data/models/catalog.models';
import { PlaybackTarget } from '@data/models/player.models';
import { PlayerService } from '@data/services/player.service';
import { ArtistNamesPipe } from '../../pipes/artist-names.pipe';

/** Compact track row used in the right sidebar ("Next in queue", queue list). */
@Component({
  selector: 'app-sidebar-track-row',
  imports: [RouterLink, FaIconComponent, ArtistNamesPipe],
  templateUrl: './sidebar-track-row.html',
  styleUrl: './sidebar-track-row.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'right-sidebar__track-item',
    '[class.current]': 'current()',
  },
})
export class SidebarTrackRow {
  private readonly player = inject(PlayerService);

  readonly track = input.required<Track>();
  /** What the row's play button starts; without one the button is hidden. */
  readonly target = input<PlaybackTarget | null>(null);
  /** Marks the playing song (green title, pause button). */
  readonly current = input(false);

  protected readonly playing = computed(() => this.current() && this.player.isPlaying());

  protected onPlay(): void {
    const target = this.target();
    if (this.current()) {
      this.player.togglePlay();
    } else if (target) {
      this.player.play(target);
    }
  }
}
