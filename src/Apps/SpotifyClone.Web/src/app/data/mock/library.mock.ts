import { LibraryItem } from '../models/library.models';
import { repeat } from './catalog.mock';

export const LIBRARY_ITEMS: readonly LibraryItem[] = repeat(20, (i) => ({
  id: `liked-songs-${i + 1}`,
  title: 'Liked Songs',
  kind: 'playlist',
  trackCount: 81,
  pinned: true,
}));
