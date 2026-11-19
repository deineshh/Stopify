import { Component, inject } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faCircleRight,
  faEllipsisH,
  faUpRightAndDownLeftFromCenter,
  faUpload,
  faCheck,
} from '@fortawesome/free-solid-svg-icons';
import { PlaybackService } from '../../../core/services/playback.service';

@Component({
  selector: 'app-now-playing-view',
  standalone: true,
  imports: [FontAwesomeModule],
  templateUrl: './now-playing-view.component.html',
  styleUrl: './now-playing-view.component.css',
})
export class NowPlayingViewComponent {
  protected readonly faCircleRight = faCircleRight;
  protected readonly faEllipsisH = faEllipsisH;
  protected readonly faUpRightAndDownLeftFromCenter = faUpRightAndDownLeftFromCenter;
  protected readonly faUpload = faUpload;
  protected readonly faCheck = faCheck;

  protected readonly playbackService = inject(PlaybackService);

  protected creditArtists = [
    { name: 'DESH', role: 'Main Artist', following: false },
    { name: 'Young Fly', role: 'Main Artist', following: false },
    { name: 'Azahriah', role: 'Main Artist', following: true },
  ];
}
