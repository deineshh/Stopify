/** What the playlist actions need to know about a playlist (from the library or its page). */
export interface PlaylistRef {
  id: string;
  name: string;
  description: string | null;
  isPublic: boolean;
  cover?: string;
}
