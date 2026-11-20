import { imageUrl, timeSpanToSeconds } from '@core/api/media';
import { EntityRef, Genre, MediaCard, MediaKind, Track } from '../models/catalog.models';
import {
  AlbumDetailsDto,
  AlbumSummaryDto,
  ArtistSummaryDto,
  CompactDocument,
  GenreSummaryDto,
  PlaylistSummaryDto,
  SearchResultsDto,
  TrackDetailsDto,
  TrackSummaryDto,
} from './catalog.dtos';

/**
 * Tile colours from the browse mockup, keeping only those where the white label reaches
 * WCAG AA (4.5:1); each genre gets a stable one based on its id.
 */
export const TILE_COLORS: readonly string[] = [
  '#dc148c',
  '#006450',
  '#8400e7',
  '#608108',
  '#477d95',
  '#e1118c',
  '#8d67ab',
  '#af2896',
  '#b06239',
  '#e91429',
  '#bc5900',
  '#ba5d07',
  '#e61e32',
  '#1e3264',
  '#5179a1',
  '#b02897',
  '#7d4b32',
  '#d84000',
  '#a56752',
  '#e8115b',
  '#537aa1',
  '#0d72ed',
  '#8c1932',
  '#27856a',
];

export function tileColor(id: string): string {
  let hash = 0;
  for (const char of id) {
    hash = (hash * 31 + char.charCodeAt(0)) | 0;
  }
  return TILE_COLORS[Math.abs(hash) % TILE_COLORS.length];
}

export const toRef = (entity: { id: string; name: string }): EntityRef => ({ id: entity.id, name: entity.name });

export function releaseYear(date: string | null | undefined): string | undefined {
  const year = date ? new Date(date).getUTCFullYear() : NaN;
  return Number.isFinite(year) ? String(year) : undefined;
}

export function albumKind(type: string | null | undefined): MediaKind {
  switch (type?.toLowerCase()) {
    case 'single':
      return 'single';
    case 'ep':
      return 'ep';
    default:
      return 'album';
  }
}

/** "single" → "Single", "ep" → "EP". */
export function albumTypeLabel(type: string | null | undefined): string {
  const kind = albumKind(type);
  return kind === 'ep' ? 'EP' : kind.charAt(0).toUpperCase() + kind.slice(1);
}

// ---------------------------------------------------------------- cards

export function albumToCard(album: AlbumSummaryDto | AlbumDetailsDto): MediaCard {
  const year = releaseYear('releaseDateUtc' in album ? album.releaseDateUtc : album.releaseDate);
  return {
    id: album.id,
    kind: albumKind(album.type),
    title: album.title,
    cover: imageUrl(album.cover?.imageId),
    shape: 'rounded',
    meta: year ? [year] : undefined,
    authors: album.mainArtists.map(toRef),
  };
}

export function artistToCard(artist: ArtistSummaryDto): MediaCard {
  return {
    id: artist.id,
    kind: 'artist',
    title: artist.name,
    cover: imageUrl(artist.avatar?.imageId),
    shape: 'circle',
    text: ['Artist'],
  };
}

export function playlistToCard(playlist: PlaylistSummaryDto): MediaCard {
  return {
    id: playlist.id,
    kind: 'playlist',
    title: playlist.name,
    cover: imageUrl(playlist.customCover?.imageId ?? playlist.generatedCoverImageIds[0]),
    shape: 'rounded',
    text: [playlist.description?.trim() || 'Playlist'],
  };
}

export function genreToTile(genre: GenreSummaryDto, fallbackImage?: string): Genre {
  return {
    id: genre.id,
    name: genre.name,
    image: imageUrl(genre.coverImageId, fallbackImage),
    color: tileColor(genre.id),
  };
}

// ---------------------------------------------------------------- tracks

interface AlbumContext {
  id: string;
  title: string;
  coverImageId?: string | null;
}

/** Track from `/tracks` or `/tracks/{id}`; album title and cover come from the album, if known. */
export function trackToViewModel(track: TrackDetailsDto | TrackSummaryDto, album?: AlbumContext): Track {
  return {
    id: track.id,
    title: track.title,
    artists: [...track.mainArtists, ...track.featuredArtists].map(toRef),
    album: { id: album?.id ?? track.albumId ?? '', name: album?.title ?? '' },
    cover: imageUrl(album?.coverImageId),
    durationSeconds: timeSpanToSeconds(track.duration),
    explicit: track.containsExplicitContent,
  };
}

/** Tracks listed inside an album (they carry no artists of their own). */
export function albumTracks(album: AlbumDetailsDto): Track[] {
  const artists = album.mainArtists.map(toRef);
  return [...album.tracks]
    .sort((a, b) => a.position - b.position)
    .map((track) => ({
      id: track.id,
      title: track.title,
      artists,
      album: { id: album.id, name: album.title },
      cover: imageUrl(album.cover?.imageId),
      durationSeconds: timeSpanToSeconds(track.duration),
      explicit: track.containsExplicitContent,
    }));
}

// ---------------------------------------------------------------- search

const refs = (documents: readonly CompactDocument[]): EntityRef[] => documents.map(toRef);

export function searchToViewModels(results: SearchResultsDto) {
  return {
    tracks: results.tracks.map<Track>((track) => ({
      id: track.id,
      title: track.title,
      artists: refs(track.artists),
      album: { id: track.album?.id ?? '', name: track.album?.title ?? '' },
      cover: imageUrl(track.coverImageId),
      durationSeconds: 0,
      explicit: track.isExplicit,
    })),
    artists: results.artists.map<MediaCard>((artist) => ({
      id: artist.id,
      kind: 'artist',
      title: artist.name,
      cover: imageUrl(artist.avatarImageId),
      shape: 'circle',
      text: ['Artist'],
    })),
    albums: results.albums.map<MediaCard>((album) => ({
      id: album.id,
      kind: albumKind(album.type),
      title: album.title,
      cover: imageUrl(album.coverImageId),
      shape: 'rounded',
      meta: album.releaseYear ? [String(album.releaseYear)] : undefined,
      authors: refs(album.artists),
    })),
    playlists: results.playlists.map<MediaCard>((playlist) => ({
      id: playlist.id,
      kind: 'playlist',
      title: playlist.name,
      cover: imageUrl(playlist.customCoverImageId ?? playlist.generatedCoverImageIds[0]),
      shape: 'rounded',
      text: ['By', playlist.owner.name],
    })),
    profiles: results.users.map<MediaCard>((user) => ({
      id: user.id,
      kind: 'profile',
      title: user.name,
      cover: imageUrl(user.avatarImageId),
      shape: 'circle',
      text: ['Profile'],
    })),
    genres: [...results.genres, ...results.moods].map((genre) => ({
      id: genre.id,
      name: genre.name,
      cover: imageUrl(genre.coverImageId),
    })),
  };
}
