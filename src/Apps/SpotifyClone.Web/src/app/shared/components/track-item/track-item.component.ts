import { Component, Input } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faPlay } from '@fortawesome/free-solid-svg-icons';
import { PlayBtnComponent } from '../play-btn/play-btn.component';
import { TrackItemData } from '../../models/track.models';

@Component({
  selector: 'app-track-item',
  standalone: true,
  imports: [FontAwesomeModule, PlayBtnComponent],
  templateUrl: './track-item.component.html',
  styleUrl: './track-item.component.css',
})
export class TrackItemComponent {
  protected readonly faPlay = faPlay;

  @Input({ required: true }) track!: TrackItemData;
}
