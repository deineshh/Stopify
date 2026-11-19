import { Component } from '@angular/core';

interface CategoryItem {
  id: string;
  label: string;
  className: string;
  img: string;
}

@Component({
  selector: 'app-search-browse',
  standalone: true,
  imports: [],
  templateUrl: './search-browse.component.html',
  styleUrl: './search-browse.component.css',
})
export class SearchBrowseComponent {
  protected categories: CategoryItem[] = [
    { id: 'music', label: 'Music', className: 'music', img: 'assets/images/search-categories/music.jpg' },
    { id: 'podcasts', label: 'Podcasts', className: 'podcasts', img: 'assets/images/search-categories/podcasts.jpg' },
    { id: 'live-events', label: 'Live Events', className: 'live-events', img: 'assets/images/search-categories/live-events.jpg' },
    { id: 'new-releases', label: 'New Releases', className: 'new-releases', img: 'assets/images/search-categories/new-releases.jpg' },
    { id: 'pop', label: 'Pop', className: 'pop', img: 'assets/images/search-categories/pop.jpg' },
    { id: 'hip-hop', label: 'Hip-Hop', className: 'hip-hop', img: 'assets/images/search-categories/hip-hop.jpg' },
    { id: 'rock', label: 'Rock', className: 'rock', img: 'assets/images/search-categories/rock.jpg' },
    { id: 'mood', label: 'Mood', className: 'mood', img: 'assets/images/search-categories/mood.jpg' },
    { id: 'charts', label: 'Charts', className: 'charts', img: 'assets/images/search-categories/charts.jpg' },
    { id: 'comedy', label: 'Comedy', className: 'comedy', img: 'assets/images/search-categories/comedy.jpg' },
    { id: 'educational', label: 'Educational', className: 'educational', img: 'assets/images/search-categories/educational.jpg' },
    { id: 'true-crime', label: 'True Crime', className: 'true-crime', img: 'assets/images/search-categories/true-crime.jpg' },
    { id: 'sports', label: 'Sports', className: 'sports', img: 'assets/images/search-categories/sports.jpg' },
    { id: 'dance-electronic', label: 'Dance/Electronic', className: 'dance-electronic', img: 'assets/images/search-categories/dance-electronic.jpg' },
    { id: 'chill', label: 'Chill', className: 'chill', img: 'assets/images/search-categories/chill.jpg' },
    { id: 'indie', label: 'Indie', className: 'indie', img: 'assets/images/search-categories/indie.jpg' },
    { id: 'workout', label: 'Workout', className: 'workout', img: 'assets/images/search-categories/workout.jpg' },
    { id: 'discover', label: 'Discover', className: 'discover', img: 'assets/images/search-categories/discover.jpeg' },
    { id: 'folk-acoustic', label: 'Folk & Acoustic', className: 'folk-acoustic', img: 'assets/images/search-categories/folk-acoustic.jpg' },
    { id: 'r-b', label: 'R&B', className: 'r-b', img: 'assets/images/search-categories/r-b.jpg' },
    { id: 'k-pop', label: 'K-pop', className: 'k-pop', img: 'assets/images/search-categories/k-pop.jpg' },
    { id: 'latin', label: 'Latin', className: 'latin', img: 'assets/images/search-categories/latin.jpg' },
    { id: 'sleep', label: 'Sleep', className: 'sleep', img: 'assets/images/search-categories/sleep.jpg' },
    { id: 'party', label: 'Party', className: 'party', img: 'assets/images/search-categories/party.jpg' },
    { id: 'at-home', label: 'At Home', className: 'at-home', img: 'assets/images/search-categories/at-home.jpg' },
    { id: 'decades', label: 'Decades', className: 'decades', img: 'assets/images/search-categories/decades.jpg' },
    { id: 'love', label: 'Love', className: 'love', img: 'assets/images/search-categories/love.jpg' },
    { id: 'metal', label: 'Metal', className: 'metal', img: 'assets/images/search-categories/metal.jpg' },
    { id: 'jazz', label: 'Jazz', className: 'jazz', img: 'assets/images/search-categories/jazz.jpg' },
    { id: 'trending', label: 'Trending', className: 'trending', img: 'assets/images/search-categories/trending.jpg' },
    { id: 'classical', label: 'Classical', className: 'classical', img: 'assets/images/search-categories/classical.jpg' },
    { id: 'country', label: 'Country', className: 'country', img: 'assets/images/search-categories/country.jpg' },
    { id: 'focus', label: 'Focus', className: 'focus', img: 'assets/images/search-categories/focus.jpg' },
    { id: 'soul', label: 'Soul', className: 'soul', img: 'assets/images/search-categories/soul.jpg' },
    { id: 'kids-family', label: 'Kids & Family', className: 'kids-family', img: 'assets/images/search-categories/kids-family.jpg' },
    { id: 'gaming', label: 'Gaming', className: 'gaming', img: 'assets/images/search-categories/gaming.jpg' },
    { id: 'anime', label: 'Anime', className: 'anime', img: 'assets/images/search-categories/anime.jpg' },
    { id: 'tv-movies', label: 'TV & Movies', className: 'tv-movies', img: 'assets/images/search-categories/tv-movies.jpg' },
    { id: 'instrumental', label: 'Instrumental', className: 'instrumental', img: 'assets/images/search-categories/instrumental.jpg' },
    { id: 'wellness', label: 'Wellness', className: 'wellness', img: 'assets/images/search-categories/wellness.jpg' },
    { id: 'punk', label: 'Punk', className: 'punk', img: 'assets/images/search-categories/punk.jpg' },
    { id: 'ambient', label: 'Ambient', className: 'ambient', img: 'assets/images/search-categories/ambient.jpg' },
    { id: 'blues', label: 'Blues', className: 'blues', img: 'assets/images/search-categories/blues.jpg' },
    { id: 'cooking-dining', label: 'Cooking & Dining', className: 'cooking-dining', img: 'assets/images/search-categories/cooking-dining.jpg' },
    { id: 'alternative', label: 'Alternative', className: 'alternative', img: 'assets/images/search-categories/alternative.jpg' },
    { id: 'travel', label: 'Travel', className: 'travel', img: 'assets/images/search-categories/travel.jpg' },
    { id: 'caribbean', label: 'Caribbean', className: 'caribbean', img: 'assets/images/search-categories/caribbean.jpg' },
    { id: 'afro', label: 'Afro', className: 'afro', img: 'assets/images/search-categories/afro.jpg' },
    { id: 'songwriters', label: 'Songwriters', className: 'songwriters', img: 'assets/images/search-categories/songwriters.jpg' },
    { id: 'nature-noise', label: 'Nature & Noise', className: 'nature-noise', img: 'assets/images/search-categories/nature-noise.jpg' },
    { id: 'funk-disco', label: 'Funk & Disco', className: 'funk-disco', img: 'assets/images/search-categories/funk-disco.jpg' },
    { id: 'glow', label: 'GLOW', className: 'glow', img: 'assets/images/search-categories/glow.jpg' },
    { id: 'stopify-singles', label: 'Stopify Singles', className: 'stopify-singles', img: 'assets/images/search-categories/stopify-singles.jpg' },
    { id: 'netflix', label: 'Netflix', className: 'netflix', img: 'assets/images/search-categories/netflix.jpg' },
    { id: 'summer', label: 'Summer', className: 'summer', img: 'assets/images/search-categories/summer.jpg' },
    { id: 'radar', label: 'RADAR', className: 'radar', img: 'assets/images/search-categories/radar.jpg' },
    { id: 'equal', label: 'EQUAL', className: 'equal', img: 'assets/images/search-categories/equal.jpg' },
    { id: 'fresh-finds', label: 'Fresh Finds', className: 'fresh-finds', img: 'assets/images/search-categories/fresh-finds.jpg' },
  ];
}
