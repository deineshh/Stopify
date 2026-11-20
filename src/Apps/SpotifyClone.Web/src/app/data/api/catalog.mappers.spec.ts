import {
  albumToCard,
  albumTracks,
  albumTypeLabel,
  artistToCard,
  genreToTile,
  playlistToCard,
  searchToViewModels,
  TILE_COLORS,
  tileColor,
  trackToViewModel,
} from './catalog.mappers';
import { AlbumDetailsDto, ArtistSummaryDto, SearchResultsDto, TrackDetailsDto } from './catalog.dtos';

const desh: ArtistSummaryDto = { id: 'desh', name: 'DESH', status: 'active', ownerId: null, avatar: null };
const image = (imageId: string) => ({ imageId, width: 640, height: 640, fileType: 'webp', sizeInBytes: 1 });

describe('catalog mappers', () => {
  it('maps albums to cards with year and artists', () => {
    const card = albumToCard({
      id: 'al1',
      title: 'BAKPAKK',
      releaseDateUtc: '2024-05-01T00:00:00Z',
      status: 'published',
      type: 'EP',
      cover: image('c1'),
      mainArtists: [desh],
    });
    expect(card).toEqual({
      id: 'al1',
      kind: 'ep',
      title: 'BAKPAKK',
      cover: '/images/c1/image.webp',
      shape: 'rounded',
      meta: ['2024'],
      authors: [{ id: 'desh', name: 'DESH' }],
    });
    expect(albumTypeLabel('ep')).toBe('EP');
    expect(albumTypeLabel('single')).toBe('Single');
    expect(albumTypeLabel(undefined)).toBe('Album');
  });

  it('maps artists and playlists', () => {
    expect(artistToCard(desh)).toEqual(expect.objectContaining({ kind: 'artist', shape: 'circle', text: ['Artist'] }));
    const playlist = playlistToCard({
      id: 'p1',
      name: 'cali',
      description: '  ',
      isPublic: true,
      ownerId: 'u1',
      customCover: null,
      generatedCoverImageIds: ['g1'],
    });
    expect(playlist.cover).toBe('/images/g1/image.webp');
    expect(playlist.text).toEqual(['Playlist']);
  });

  it('gives genres a stable tile colour', () => {
    expect(tileColor('rock')).toBe(tileColor('rock'));
    expect(genreToTile({ id: 'rock', name: 'Rock', coverImageId: null }, '/x.jpg')).toEqual({
      id: 'rock',
      name: 'Rock',
      image: '/x.jpg',
      color: tileColor('rock'),
    });
  });

  it('only uses tile colours with AA contrast for the white label', () => {
    const luminance = (hex: string) => {
      const [r, g, b] = [1, 3, 5].map((i) => {
        const c = parseInt(hex.slice(i, i + 2), 16) / 255;
        return c <= 0.03928 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4;
      });
      return 0.2126 * r + 0.7152 * g + 0.0722 * b;
    };
    for (const color of TILE_COLORS) {
      expect(1.05 / (luminance(color) + 0.05), color).toBeGreaterThanOrEqual(4.5);
    }
  });

  it('maps tracks with album context and featured artists', () => {
    const track: TrackDetailsDto = {
      id: 't1',
      title: 'PANNONIA',
      duration: '00:02:27',
      releaseDate: null,
      containsExplicitContent: true,
      status: 'published',
      audioFileId: null,
      albumId: 'al1',
      mainArtists: [desh],
      featuredArtists: [{ ...desh, id: 'yf', name: 'Young Fly' }],
      genres: [],
      moods: [],
    };
    expect(trackToViewModel(track, { id: 'al1', title: 'BAKPAKK', coverImageId: 'c1' })).toEqual({
      id: 't1',
      title: 'PANNONIA',
      artists: [
        { id: 'desh', name: 'DESH' },
        { id: 'yf', name: 'Young Fly' },
      ],
      album: { id: 'al1', name: 'BAKPAKK' },
      cover: '/images/c1/image.webp',
      durationSeconds: 147,
      explicit: true,
    });
  });

  it('orders album tracks by position and gives them the album artists', () => {
    const album: AlbumDetailsDto = {
      id: 'al1',
      title: 'BAKPAKK',
      releaseDate: null,
      status: 'published',
      type: 'album',
      cover: null,
      mainArtists: [desh],
      tracks: [
        { id: 'b', title: 'Two', containsExplicitContent: false, status: 'p', duration: '00:03:00', position: 2 },
        { id: 'a', title: 'One', containsExplicitContent: false, status: 'p', duration: '00:02:00', position: 1 },
      ],
    };
    const tracks = albumTracks(album);
    expect(tracks.map((t) => t.title)).toEqual(['One', 'Two']);
    expect(tracks[0].artists).toEqual([{ id: 'desh', name: 'DESH' }]);
  });

  it('maps search documents', () => {
    const results: SearchResultsDto = {
      users: [{ id: 'u1', name: 'a', avatarImageId: null }],
      genres: [{ id: 'g1', name: 'Afro', coverImageId: 'gc' }],
      moods: [{ id: 'm1', name: 'Chill', coverImageId: null }],
      artists: [{ id: 'ar1', name: 'Azahriah', isVerified: true, avatarImageId: 'av' }],
      albums: [{ id: 'al1', title: 'AL NACER!', releaseYear: 2025, type: 'album', coverImageId: null, artists: [{ id: 's', name: 'Sayfalse' }] }],
      tracks: [{ id: 't1', title: 'ACELERADA', isExplicit: false, releaseYear: 2025, coverImageId: 'tc', album: { id: 'al1', title: 'AL NACER!' }, artists: [{ id: 's', name: 'sma$her' }], genres: [], moods: [] }],
      playlists: [{ id: 'p1', name: '+ 999999 AURA', owner: { id: 'u2', name: 'Lunar Media' }, customCoverImageId: null, generatedCoverImageIds: [] }],
    };
    const view = searchToViewModels(results);
    expect(view.tracks[0]).toEqual(expect.objectContaining({ title: 'ACELERADA', cover: '/images/tc/image.webp', album: { id: 'al1', name: 'AL NACER!' } }));
    expect(view.artists[0].cover).toBe('/images/av/image.webp');
    expect(view.albums[0].meta).toEqual(['2025']);
    expect(view.playlists[0].text).toEqual(['By', 'Lunar Media']);
    expect(view.profiles[0].kind).toBe('profile');
    expect(view.genres.map((g) => g.name)).toEqual(['Afro', 'Chill']);
  });
});
