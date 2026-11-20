import { EntityRef } from '../models/catalog.models';

/**
 * Helpers for the remaining static content (the player until playback is connected).
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

