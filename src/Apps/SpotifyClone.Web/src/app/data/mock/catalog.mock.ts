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

const keepYou = (i: number): Track => ({
  id: `keep-you-${i + 1}`,
  title: 'Keep You',
  artists: [artist('Spektrum'), artist('Sara Skinner')],
  album: { id: 'keep-you', name: 'Keep You' },
  cover: COVER,
  durationSeconds: 289,
  addedAt: '2025-03-18',
  saved: true,
});

export const PLAYLIST_TRACKS: readonly Track[] = repeat(10, keepYou);

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

// ---------------------------------------------------------------- playlist page

export const PLAYLIST_SHELVES: readonly MediaShelf[] = [
  {
    id: 'more-by',
    title: 'More by Azahriah',
    showAllLabel: 'See discography',
    items: repeat(10, card('more-by', { kind: 'album', title: 'skatulya II', shape: 'rounded', text: ['2025'] })),
  },
  {
    id: 'discography',
    title: 'Discography',
    showAllLabel: 'Show all',
    filters: ['Popular releases', 'Albums', 'Singles and EPs'],
    items: repeat(
      10,
      card('discography', { kind: 'album', title: 'Aziverzum', shape: 'rounded', text: ['2025', 'Album'] }),
    ),
  },
];

export const PLAYLIST_SHELVES_AFTER_ABOUT: readonly MediaShelf[] = [
  {
    id: 'fans-also-like',
    title: 'Fans also like',
    showAllLabel: 'Show all',
    items: repeat(10, card('fans-like', { kind: 'artist', title: 'DESH', shape: 'circle', text: ['Artist'] })),
  },
  {
    id: 'appears-on',
    title: 'Appears on',
    showAllLabel: 'Show all',
    items: repeat(10, card('appears-on', { kind: 'ep', title: 'BAKPAKK', shape: 'rounded', text: ['2024', 'EP'] })),
  },
  {
    id: 'public-playlists',
    title: 'Public Playlists',
    showAllLabel: 'Show all',
    items: repeat(
      10,
      card('public-playlist', {
        kind: 'playlist',
        title: 'Aziverzum',
        shape: 'rounded',
        text: ['2 Followers'],
      }),
    ),
  },
];
