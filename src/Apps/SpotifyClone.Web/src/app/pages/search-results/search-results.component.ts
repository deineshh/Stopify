import { Component } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faPlay, faPlus, faEllipsisH, faHeart } from '@fortawesome/free-solid-svg-icons';
import { PlayBtnComponent } from '../../shared/components/play-btn/play-btn.component';

@Component({
  selector: 'app-search-results',
  standalone: true,
  imports: [FontAwesomeModule, PlayBtnComponent],
  templateUrl: './search-results.component.html',
  styleUrl: './search-results.component.css',
})
export class SearchResultsComponent {
  protected readonly faPlay = faPlay;
  protected readonly faPlus = faPlus;
  protected readonly faEllipsisH = faEllipsisH;
  protected readonly faHeart = faHeart;

  protected filters = ['All', 'Albums', 'Playlists', 'Songs', 'Genres & Moods', 'Artists', 'Profiles'];
  protected activeFilter = 'All';

  protected songs = Array.from({ length: 4 }, (_, i) => ({
    id: `song-${i}`,
    title: `Song Title ${i + 1}`,
    coverUrl: 'assets/images/song.jpg',
    authors: [{ name: `Artist ${i + 1}` }],
    duration: `${Math.floor(Math.random() * 4) + 2}:${String(Math.floor(Math.random() * 60)).padStart(2, '0')}`,
  }));

  protected artists = Array.from({ length: 10 }, (_, i) => ({
    id: `artist-${i}`,
    name: `Artist ${i + 1}`,
    coverUrl: 'assets/images/song.jpg',
  }));

  protected albums = Array.from({ length: 10 }, (_, i) => ({
    id: `album-${i}`,
    title: `Album ${i + 1}`,
    coverUrl: 'assets/images/song.jpg',
    year: '2025',
    authors: [{ name: `Artist ${i + 1}` }],
  }));

  protected playlists = Array.from({ length: 5 }, (_, i) => ({
    id: `playlist-${i}`,
    title: `Playlist ${i + 1}`,
    coverUrl: 'assets/images/song.jpg',
    author: `Author ${i + 1}`,
  }));

  protected profiles = Array.from({ length: 6 }, (_, i) => ({
    id: `profile-${i}`,
    name: `User ${i + 1}`,
    coverUrl: 'assets/images/song.jpg',
  }));

  protected genres = Array.from({ length: 8 }, (_, i) => ({
    id: `genre-${i}`,
    name: `Genre ${i + 1}`,
    coverUrl: 'assets/images/song.jpg',
    color: '#477d95',
  }));

  setFilter(filter: string): void {
    this.activeFilter = filter;
  }
}
