import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';

/**
 * The round green play button. Applied to a native `<button>` so callers keep full
 * control of classes, positioning and events:
 *
 *   <button appPlayButton revealOnHover title="Play"></button>
 *
 * With `revealOnHover` the button stays hidden until the nearest `.play-btn-parent` is hovered.
 */
@Component({
  selector: 'button[appPlayButton]',
  imports: [FaIconComponent],
  template: `
    <span class="play-icon">
      <fa-icon [icon]="['fas', playing() ? 'pause' : 'play']" />
    </span>
  `,
  styleUrl: './play-button.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'play-btn',
    type: 'button',
    '[class.hover-transition]': 'revealOnHover()',
    '[attr.aria-pressed]': 'playing()',
  },
})
export class PlayButton {
  readonly revealOnHover = input(false, { transform: (value: boolean | '') => value !== false });
  readonly playing = input(false);
}
