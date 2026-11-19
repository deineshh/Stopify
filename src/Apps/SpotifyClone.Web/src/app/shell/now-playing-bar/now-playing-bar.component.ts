import { Component, inject } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faPlay,
  faPause,
  faHeart,
  faUpRightAndDownLeftFromCenter,
  faArrowDown,
  faForward,
  faBackward,
  faVolumeHigh,
  faMicrochip,
  faRepeat,
  faShuffle,
} from '@fortawesome/free-solid-svg-icons';
import { SliderComponent } from '../../shared/components/slider/slider.component';
import { PlaybackService } from '../../core/services/playback.service';

@Component({
  selector: 'app-now-playing-bar',
  standalone: true,
  imports: [FontAwesomeModule, SliderComponent],
  templateUrl: './now-playing-bar.component.html',
  styleUrl: './now-playing-bar.component.css',
})
export class NowPlayingBarComponent {
  protected readonly faPlay = faPlay;
  protected readonly faPause = faPause;
  protected readonly faHeart = faHeart;
  protected readonly faUpRightAndDownLeftFromCenter = faUpRightAndDownLeftFromCenter;
  protected readonly faArrowDown = faArrowDown;
  protected readonly faForward = faForward;
  protected readonly faBackward = faBackward;
  protected readonly faVolumeHigh = faVolumeHigh;
  protected readonly faMicrochip = faMicrochip;
  protected readonly faRepeat = faRepeat;
  protected readonly faShuffle = faShuffle;

  protected readonly playbackService = inject(PlaybackService);

  protected volumeValue = 70;
  protected isDragging = false;
  protected dragPosition = 0;

  protected get displayPosition(): number {
    return this.isDragging ? this.dragPosition : this.playbackService.currentPosition();
  }

  protected togglePlayback(): void {
    if (!this.playbackService.currentTrack()) return;

    if (this.playbackService.isPlaying()) {
      this.playbackService.pause().subscribe({
        next: () => this.playbackService.pauseStream(),
      });
    } else {
      this.playbackService.resume().subscribe({
        next: () => this.playbackService.resumeStream(),
      });
    }
  }

  protected onDrag(position: number): void {
    this.isDragging = true;
    this.dragPosition = position;
  }

  protected onSeek(position: number): void {
    this.isDragging = false;
    this.playbackService.seek(position);
  }

  protected nextTrack(): void {
    if (!this.playbackService.currentTrack()) return;
    this.playbackService.next().subscribe({
      next: (response) => this.playbackService.playStream(response),
    });
  }

  protected previousTrack(): void {
    if (!this.playbackService.currentTrack()) return;
    this.playbackService.previous().subscribe({
      next: (response) => this.playbackService.playStream(response),
    });
  }

  protected toggleShuffle(): void {
    this.playbackService.toggleShuffle();
  }

  protected toggleRepeat(): void {
    this.playbackService.toggleRepeat();
  }
}
