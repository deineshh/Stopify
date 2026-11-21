import { Track } from './catalog.models';

/**
 * A playlist's song order. `ids` is the full list as the API stores it; `tracks` are the songs
 * on screen, in the same order. They can differ: songs that no longer load (deleted, unpublished)
 * stay in the playlist but aren't shown.
 */
export interface PlaylistOrder {
  readonly ids: readonly string[];
  readonly tracks: readonly Track[];
}

/** A move as the API needs it: the song and where it lands in the full list. */
export interface PlaylistMove {
  readonly order: PlaylistOrder;
  readonly trackId: string;
  readonly targetIndex: number;
}

/**
 * Moves the song shown at `from` to `to` (both indexes into `order.tracks`) and works out the
 * index in the full list that puts it in the same place. It lands next to the song it passed:
 * right after it when moving down, right before it when moving up, so hidden songs keep their
 * place among the others. Returns `null` when nothing moves.
 */
export function moveInPlaylist(order: PlaylistOrder, from: number, to: number): PlaylistMove | null {
  const { tracks } = order;
  if (from === to || from < 0 || to < 0 || from >= tracks.length || to >= tracks.length) {
    return null;
  }
  const moved = tracks[from];
  const nextTracks = [...tracks];
  nextTracks.splice(from, 1);
  nextTracks.splice(to, 0, moved);

  const rest = order.ids.filter((id) => id !== moved.id);
  const targetIndex = to > from ? rest.indexOf(nextTracks[to - 1].id) + 1 : rest.indexOf(nextTracks[to + 1].id);
  const ids = [...rest];
  ids.splice(targetIndex, 0, moved.id);

  return { order: { ids, tracks: nextTracks }, trackId: moved.id, targetIndex };
}
