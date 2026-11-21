export type LibraryFilter = 'playlists' | 'artists';

export interface LibraryItem {
  id: string;
  title: string;
  kind: 'playlist' | 'artist';
  /** Library items without artwork show the solid placeholder tile. */
  cover?: string;
  /** Second line after the kind, e.g. the playlist owner. */
  subtitle?: string;
  link: readonly string[];
  /** The signed-in user owns it, so they may edit, delete and add songs. */
  owned?: boolean;
  isPublic?: boolean;
  description?: string | null;
}
