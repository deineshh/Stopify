/**
 * Response shapes of the Catalog, Playlists and Search modules, as the API serialises them
 * (camelCase JSON). Mapped to the UI's view models in `catalog.mappers.ts`.
 */

export interface ImageMetadataDto {
  imageId: string;
  width: number;
  height: number;
  fileType: string;
  sizeInBytes: number;
}

export interface ArtistSummaryDto {
  id: string;
  name: string;
  status: string;
  ownerId: string | null;
  avatar: ImageMetadataDto | null;
}

export interface ArtistDetailsDto {
  id: string;
  name: string;
  bio: string | null;
  ownerId: string | null;
  status: string;
  avatar: ImageMetadataDto | null;
  banner: ImageMetadataDto | null;
  gallery: ImageMetadataDto[];
}

export interface AlbumSummaryDto {
  id: string;
  title: string;
  releaseDateUtc: string | null;
  status: string;
  /** e.g. "album", "single", "ep". */
  type: string;
  cover: ImageMetadataDto | null;
  mainArtists: ArtistSummaryDto[];
}

export interface AlbumTrackSummaryDto {
  id: string;
  title: string;
  containsExplicitContent: boolean;
  status: string;
  /** .NET TimeSpan, e.g. "00:04:49". */
  duration: string | null;
  position: number;
}

export interface AlbumDetailsDto {
  id: string;
  title: string;
  releaseDate: string | null;
  status: string;
  type: string;
  cover: ImageMetadataDto | null;
  mainArtists: ArtistSummaryDto[];
  tracks: AlbumTrackSummaryDto[];
}

export interface GenreRefDto {
  id: string;
  name: string;
  coverImageId: string | null;
}

export interface TrackDetailsDto {
  id: string;
  title: string;
  duration: string | null;
  releaseDate: string | null;
  containsExplicitContent: boolean;
  status: string;
  audioFileId: string | null;
  albumId: string | null;
  mainArtists: ArtistSummaryDto[];
  featuredArtists: ArtistSummaryDto[];
  genres: GenreRefDto[];
  moods: GenreRefDto[];
}

/** List variant: genres and moods are ids only. */
export interface TrackSummaryDto extends Omit<TrackDetailsDto, 'releaseDate' | 'genres' | 'moods'> {
  releaseDateUtc: string | null;
  genres: string[];
  moods: string[];
}

export type GenreSummaryDto = GenreRefDto;
export type MoodSummaryDto = GenreRefDto;

export interface PlaylistSummaryDto {
  id: string;
  name: string;
  description: string | null;
  isPublic: boolean;
  ownerId: string;
  customCover: ImageMetadataDto | null;
  generatedCoverImageIds: string[];
}

export interface CollaboratorDto {
  id: string;
  name: string;
  avatarImageId: string | null;
}

export interface PlaylistDetailsDto {
  id: string;
  name: string;
  description: string | null;
  ownerId: string;
  isPublic: boolean;
  /** Named `customCoverImageId` by the API, but it is the full image metadata. */
  customCoverImageId: ImageMetadataDto | null;
  generatedCoverImageIds: string[];
  collaborators: CollaboratorDto[];
  tracks: { id: string; position: number }[];
}

// ---------------------------------------------------------------- search (Meilisearch documents)

export interface CompactDocument {
  id: string;
  name: string;
}

export interface SearchResultsDto {
  users: { id: string; name: string; avatarImageId: string | null }[];
  genres: { id: string; name: string; coverImageId: string | null }[];
  moods: { id: string; name: string; coverImageId: string | null }[];
  artists: { id: string; name: string; isVerified: boolean; avatarImageId: string | null }[];
  albums: {
    id: string;
    title: string;
    releaseYear: number;
    type: string;
    coverImageId: string | null;
    artists: CompactDocument[];
  }[];
  tracks: {
    id: string;
    title: string;
    isExplicit: boolean;
    releaseYear: number;
    coverImageId: string | null;
    album: { id: string; title: string } | null;
    artists: CompactDocument[];
    genres: CompactDocument[];
    moods: CompactDocument[];
  }[];
  playlists: {
    id: string;
    name: string;
    owner: CompactDocument;
    customCoverImageId: string | null;
    generatedCoverImageIds: string[];
  }[];
}
