import { Component, HostListener, ElementRef, viewChild } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faPlay } from '@fortawesome/free-solid-svg-icons';
import { TrackGroupComponent } from '../../shared/components/track-group/track-group.component';
import { TrackItemComponent } from '../../shared/components/track-item/track-item.component';
import { PlayBtnComponent } from '../../shared/components/play-btn/play-btn.component';
import { TrackItemData } from '../../shared/models/track.models';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [FontAwesomeModule, TrackGroupComponent, TrackItemComponent, PlayBtnComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent {
  protected readonly faPlay = faPlay;

  private readonly headerRef = viewChild<ElementRef<HTMLElement>>('homeHeader');

  @HostListener('window:scroll', [])
  onScroll(): void {
    const headerEl = this.headerRef()?.nativeElement;
    if (!headerEl) return;
    const scrollY = window.scrollY || document.documentElement.scrollTop || 0;
    const fadeDistance = 100;
    const opacity = Math.min(scrollY / fadeDistance, 1);
    headerEl.style.backgroundColor = `rgba(0, 0, 0, ${opacity})`;
  }

  protected recentlyPlayed = Array.from({ length: 8 }, (_, i) => ({
    id: `recent-${i}`,
    title: `Playlist ${i + 1}`,
    coverUrl: 'assets/images/song.jpg',
  }));

  protected navFilters = ['All', 'Music', 'Podcasts'];

  protected madeForTracks: TrackItemData[] = Array.from({ length: 8 }, (_, i) => ({
    id: `made-${i}`,
    title: 'Repsaj, Kordhell, -Prey and more',
    coverUrl: 'assets/images/song.jpg',
    authors: [{ name: 'Podcast' }],
    description: 'Podcast',
  }));

  protected recommendedTracks: TrackItemData[] = Array.from({ length: 10 }, (_, i) => ({
    id: `rec-${i}`,
    title: 'LOS VOLTAJE ULTRAFUNK',
    coverUrl: 'assets/images/song.jpg',
    authors: [{ name: 'VaporVice' }, { name: 'REMEMBXRED' }],
  }));

  protected discoverTracks: TrackItemData[] = Array.from({ length: 10 }, (_, i) => ({
    id: `disc-${i}`,
    title: `Discover Pick ${i + 1}`,
    coverUrl: 'assets/images/song.jpg',
    authors: [{ name: 'Artist Name' }],
  }));

  protected jumpBackTracks: TrackItemData[] = Array.from({ length: 10 }, (_, i) => ({
    id: `jump-${i}`,
    title: `Continue Listening ${i + 1}`,
    coverUrl: 'assets/images/song.jpg',
    authors: [{ name: 'Podcast' }],
    description: 'Podcast',
  }));

  protected newReleasesTracks: TrackItemData[] = Array.from({ length: 10 }, (_, i) => ({
    id: `new-${i}`,
    title: `New Release ${i + 1}`,
    coverUrl: 'assets/images/song.jpg',
    authors: [{ name: 'Artist Name' }],
  }));

  protected trendingTracks: TrackItemData[] = Array.from({ length: 10 }, (_, i) => ({
    id: `trend-${i}`,
    title: `Trending Album ${i + 1}`,
    coverUrl: 'assets/images/song.jpg',
    authors: [{ name: 'Artist Name' }],
  }));

  protected fromLikesTracks: TrackItemData[] = Array.from({ length: 10 }, (_, i) => ({
    id: `likes-${i}`,
    title: `Album You Might Like ${i + 1}`,
    coverUrl: 'assets/images/song.jpg',
    authors: [{ name: 'Artist Name' }],
  }));
}
