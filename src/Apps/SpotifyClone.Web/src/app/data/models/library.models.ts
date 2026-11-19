export type LibraryFilter = 'playlists' | 'artists';

export interface LibraryItem {
  id: string;
  title: string;
  kind: 'playlist' | 'artist';
  /** Library items without artwork show the solid placeholder tile. */
  cover?: string;
  trackCount: number;
  pinned: boolean;
}
