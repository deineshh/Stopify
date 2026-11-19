import { Component, inject, OnInit, ChangeDetectorRef, ElementRef, viewChild, HostListener } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faShuffle,
  faHeart,
  faDownload,
  faEllipsisH,
  faArrowRight,
  faCheck,
  faMusic,
  faListUl,
} from '@fortawesome/free-solid-svg-icons';
import { TrackTableComponent } from '../../shared/components/track-table/track-table.component';
import { PlayBtnComponent } from '../../shared/components/play-btn/play-btn.component';
import { TrackGroupComponent } from '../../shared/components/track-group/track-group.component';
import { TrackItemComponent } from '../../shared/components/track-item/track-item.component';
import { TrackTableRow, TrackItemData } from '../../shared/models/track.models';
import { PlaylistService } from '../../core/services/playlist.service';
import { PlaybackService } from '../../core/services/playback.service';
import { PlaylistResponse } from '../../core/models/playlist.models';

@Component({
  selector: 'app-playlist',
  standalone: true,
  imports: [FontAwesomeModule, TrackTableComponent, PlayBtnComponent, TrackGroupComponent, TrackItemComponent],
  templateUrl: './playlist.component.html',
  styleUrl: './playlist.component.css',
})
export class PlaylistComponent implements OnInit {
  protected readonly faShuffle = faShuffle;
  protected readonly faHeart = faHeart;
  protected readonly faDownload = faDownload;
  protected readonly faEllipsisH = faEllipsisH;
  protected readonly faArrowRight = faArrowRight;
  protected readonly faCheck = faCheck;
  protected readonly faMusic = faMusic;
  protected readonly faListUl = faListUl;

  private readonly route = inject(ActivatedRoute);
  private readonly playlistService = inject(PlaylistService);
  protected readonly playbackService = inject(PlaybackService);
  private readonly cdr = inject(ChangeDetectorRef);

  private readonly headerRef = viewChild<ElementRef<HTMLElement>>('playlistHeader');

  @HostListener('window:scroll', [])
  onScroll(): void {
    const headerEl = this.headerRef()?.nativeElement;
    if (!headerEl) return;
    const scrollY = window.scrollY || document.documentElement.scrollTop || 0;
    const fadeDistance = 300;
    const opacity = Math.min(scrollY / fadeDistance, 1);
    headerEl.style.opacity = String(opacity);
  }

  protected playlist: PlaylistResponse | null = null;
  protected tracks: TrackTableRow[] = [];

  protected discographyFilters = ['Popular releases', 'Albums', 'Singles and EPs'];
  protected activeDiscFilter = 'Popular releases';

  protected moreByTracks: TrackItemData[] = [];
  protected discographyItems: TrackItemData[] = [];
  protected fansLikeTracks: TrackItemData[] = [];
  protected appearsOnItems: TrackItemData[] = [];
  protected publicPlaylists: TrackItemData[] = [];

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    this.playlistService.getPlaylistWithTracks(id).subscribe({
      next: (result) => {
        this.playlist = result.playlist;
        this.tracks = this.playlistService.mapToTrackTableRows(result.playlist, result.tracks);
        this.cdr.markForCheck();
      },
      error: (err) => console.error('Failed to load playlist', err),
    });
  }

  setDiscFilter(filter: string): void {
    this.activeDiscFilter = filter;
  }

  playPlaylist(): void {
    if (!this.playlist) return;

    if (this.playbackService.isPlaying()) {
      this.playbackService.pause().subscribe({
        next: () => this.playbackService.pauseStream(),
      });
    } else if (this.playbackService.currentTrack()) {
      this.playbackService.resume().subscribe({
        next: () => this.playbackService.resumeStream(),
      });
    } else {
      this.playbackService.playPlaylist(this.playlist.id).subscribe({
        next: (response) => this.playbackService.playStream(response),
      });
    }
  }

  playTrack(trackId: string): void {
    if (!this.playlist) return;
    this.playbackService.playPlaylist(this.playlist.id, trackId).subscribe({
      next: (response) => this.playbackService.playStream(response),
    });
  }
}
