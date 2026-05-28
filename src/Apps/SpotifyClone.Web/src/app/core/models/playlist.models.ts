export interface PlaylistTrackRef {
  id: string;
  position: number;
}

export interface PlaylistResponse {
  id: string;
  name: string;
  description: string | null;
  ownerId: string;
  isPublic: boolean;
  customCoverImageId: string | null;
  generatedCoverImageIds: string[];
  collaborators: string[];
  tracks: PlaylistTrackRef[];
}

export interface ArtistInfo {
  id: string;
  name: string;
  status: string;
  ownerId: string;
  avatar: string | null;
}

export interface GenreInfo {
  id: string;
  name: string;
  coverImageId: string | null;
}

export interface MoodInfo {
  id: string;
  name: string;
  coverImageId: string | null;
}

export interface TrackResponse {
  id: string;
  title: string;
  duration: string;
  releaseDate: string;
  containsExplicitContent: boolean;
  status: string;
  audioFileId: string;
  albumId: string;
  mainArtists: ArtistInfo[];
  featuredArtists: ArtistInfo[];
  genres: GenreInfo[];
  moods: MoodInfo[];
}
