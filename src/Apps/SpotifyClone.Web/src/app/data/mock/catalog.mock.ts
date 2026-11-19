import { EntityRef, MediaCard, MediaShelf, ShortcutItem, Track } from '../models/catalog.models';

/**
 * Static content mirroring the HTML mockups (mockups/index.html).
 * Every list is generated so items get unique ids, exactly like API data would.
 */

export const COVER = '/assets/images/song.jpg';

export const artist = (name: string): EntityRef => ({
  id: name.toLowerCase().replace(/[^a-z0-9]+/g, '-'),
  name,
});

export function repeat<T>(count: number, make: (index: number) => T): T[] {
  return Array.from({ length: count }, (_, index) => make(index));
}

const card = (prefix: string, item: Omit<MediaCard, 'id' | 'cover'> & { cover?: string }) =>
  (index: number): MediaCard => ({ cover: COVER, ...item, id: `${prefix}-${index + 1}` });

// ---------------------------------------------------------------- home

export const RECENTLY_PLAYED: readonly ShortcutItem[] = repeat(8, (i) => ({
  id: `recent-${i + 1}`,
  title: 'AL NACER!',
  cover: COVER,
}));

const albumCard = (prefix: string) =>
  card(prefix, {
    kind: 'album',
    title: 'LOS VOLTAJE ULTRAFUNK',
    shape: 'rounded',
    authors: [artist('VaporVice'), artist('REMEMBXRED')],
  });

export const HOME_SHELVES: readonly MediaShelf[] = [
  {
    id: 'made-for',
    eyebrow: 'Made For',
    title: 'Aqua Studios',
    showAllLabel: 'Show all',
    items: repeat(
      8,
      card('made-for', { kind: 'mix', shape: 'rounded', text: ['Repsaj, Kordhell, -Prey and more'] }),
    ),
  },
  {
    id: 'recommended',
    eyebrow: 'Recommended For You',
    title: 'Aqua Studios',
    showAllLabel: 'Show all',
    items: repeat(10, albumCard('recommended')),
  },
  {
    id: 'discover',
    title: 'Discover picks for you',
    showAllLabel: 'Show all',
    items: repeat(10, albumCard('discover')),
  },
  {
    id: 'jump-back-in',
    title: 'Jump back in',
    showAllLabel: 'Show all',
    items: repeat(10, albumCard('jump-back-in')),
  },
  {
    id: 'new-releases',
    eyebrow: 'Brand new music from artists you love.',
    title: 'New releases for you',
    showAllLabel: 'Show all',
    items: repeat(10, albumCard('new-releases')),
  },
  {
    id: 'trending',
    title: 'Trending albums for you',
    showAllLabel: 'Show all',
    items: repeat(10, albumCard('trending')),
  },
  {
    id: 'from-your-likes',
    title: 'Albums featuring songs you like',
    showAllLabel: 'Show all',
    items: repeat(10, albumCard('from-your-likes')),
  },
];

// ---------------------------------------------------------------- tracks

export const SEARCH_SONGS: readonly Track[] = repeat(4, (i) => ({
  id: `acelerada-${i + 1}`,
  title: 'ACELERADA',
  artists: [artist('sma$her'), artist('MXZI')],
  album: { id: 'acelerada', name: 'ACELERADA' },
  cover: COVER,
  durationSeconds: 61,
}));

// ---------------------------------------------------------------- search

export const SEARCH_RESULTS = {
  topResult: { id: 'azahriah', name: 'Azahriah', type: 'Artist', cover: COVER },
  songs: SEARCH_SONGS,
  artists: repeat(10, card('artist', { kind: 'artist', title: 'Arctic Monkeys', shape: 'circle', text: ['Artist'] })),
  albums: repeat(
    10,
    card('album', {
      kind: 'album',
      title: 'AL NACER!',
      shape: 'rounded',
      meta: ['2025'],
      authors: [artist('Sayfalse'), artist('Nulteex')],
    }),
  ),
  playlists: repeat(
    10,
    card('playlist', { kind: 'playlist', title: '+ 999999 AURA', shape: 'rounded', text: ['By', 'Lunar Media'] }),
  ),
  profiles: repeat(10, card('profile', { kind: 'profile', title: 'a', shape: 'circle', text: ['Profile'] })),
  genres: repeat(10, (i) => ({ id: `genre-${i + 1}`, name: 'Afro', cover: COVER })),
} as const;
