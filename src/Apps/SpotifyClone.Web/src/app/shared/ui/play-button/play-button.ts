import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { PlaybackTarget } from '@data/models/player.models';
import { PlayerService } from '@data/services/player.service';

/**
 * The round green play button. Applied to a native `<button>` so callers keep full
 * control of classes, positioning and events:
 *
 *   <button appPlayButton revealOnHover [target]="{ kind: 'album', id }"></button>
 *
 * With a `target` the button plays/pauses it and shows the pause icon while it's playing;
 * without one it only reflects `playing` and the caller handles the click.
 * With `revealOnHover` the button stays hidden until the nearest `.play-btn-parent` is hovered.
 */
@Component({
  selector: 'button[appPlayButton]',
  imports: [FaIconComponent],
  template: `
    <span class="play-icon">
      <fa-icon [icon]="['fas', active() ? 'pause' : 'play']" />
    </span>
  `,
  styleUrl: './play-button.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'play-btn',
    type: 'button',
    '[class.hover-transition]': 'revealOnHover()',
    '[class.active]': 'active()',
    '[attr.aria-pressed]': 'active()',
    '(click)': 'onClick()',
  },
})
export class PlayButton {
  private readonly player = inject(PlayerService);

  readonly revealOnHover = input(false, { transform: (value: boolean | '') => value !== false });
  readonly playing = input(false);
  readonly target = input<PlaybackTarget | null>(null);

  protected readonly active = computed(() => {
    const target = this.target();
    return this.playing() || (target !== null && this.player.isPlayingTarget(target));
  });

  protected onClick(): void {
    const target = this.target();
    if (target) {
      this.player.toggle(target);
    }
  }
}
