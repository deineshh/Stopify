/** Someone who may add, remove and reorder songs in another user's playlist. */
export interface Collaborator {
  id: string;
  name: string;
  /** `null` shows the initial instead. */
  avatar: string | null;
}

/** What the playlist actions need to know about a playlist (from the library or its page). */
export interface PlaylistRef {
  id: string;
  name: string;
  description: string | null;
  isPublic: boolean;
  cover?: string;
  /** The cover is an uploaded photo (not the one made from the songs), so it can be removed. */
  hasCustomCover?: boolean;
}
