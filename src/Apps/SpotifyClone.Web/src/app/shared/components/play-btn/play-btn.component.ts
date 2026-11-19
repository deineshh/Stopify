import { Component, Input, HostBinding } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faPlay, faPause } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'button[app-play-btn]',
  standalone: true,
  imports: [FontAwesomeModule],
  templateUrl: './play-btn.component.html',
  styleUrl: './play-btn.component.css',
})
export class PlayBtnComponent {
  protected readonly faPlay = faPlay;
  protected readonly faPause = faPause;

  @Input() showOnHover = false;
  @Input() playing = false;

  @HostBinding('class.hover-transition') get hoverTransitionClass() {
    return this.showOnHover;
  }
}
