import { Track } from './catalog.models';
import { PlaylistOrder, moveInPlaylist } from './playlist-order';

const track = (id: string) => ({ id }) as Track;
const order = (ids: string[], shown = ids): PlaylistOrder => ({ ids, tracks: shown.map(track) });
const shownIds = (o: PlaylistOrder) => o.tracks.map((t) => t.id);

describe('moveInPlaylist', () => {
  it('moves down and up when every song is shown', () => {
    const down = moveInPlaylist(order(['a', 'b', 'c', 'd']), 0, 2)!;
    expect(shownIds(down.order)).toEqual(['b', 'c', 'a', 'd']);
    expect(down.order.ids).toEqual(['b', 'c', 'a', 'd']);
    expect(down).toMatchObject({ trackId: 'a', targetIndex: 2 });

    const up = moveInPlaylist(order(['a', 'b', 'c', 'd']), 3, 0)!;
    expect(up.order.ids).toEqual(['d', 'a', 'b', 'c']);
    expect(up.targetIndex).toBe(0);

    const last = moveInPlaylist(order(['a', 'b', 'c']), 0, 2)!;
    expect(last.order.ids).toEqual(['b', 'c', 'a']);
    expect(last.targetIndex).toBe(2);
  });

  it('keeps hidden songs where they are and lands next to the songs on screen', () => {
    // x and y are in the playlist but don't load, so the page shows a, b, c.
    const ids = ['x', 'a', 'b', 'y', 'c'];
    const toTop = moveInPlaylist(order(ids, ['a', 'b', 'c']), 2, 0)!;
    expect(shownIds(toTop.order)).toEqual(['c', 'a', 'b']);
    // Right before "a", which comes after the hidden "x".
    expect(toTop.order.ids).toEqual(['x', 'c', 'a', 'b', 'y']);
    expect(toTop.targetIndex).toBe(1);

    const toEnd = moveInPlaylist(order(ids, ['a', 'b', 'c']), 0, 2)!;
    expect(toEnd.order.ids).toEqual(['x', 'b', 'y', 'c', 'a']);
    expect(toEnd.targetIndex).toBe(4);

    const between = moveInPlaylist(order(ids, ['a', 'b', 'c']), 2, 1)!;
    expect(between.order.ids).toEqual(['x', 'a', 'c', 'b', 'y']);
    expect(between.targetIndex).toBe(2);
  });

  it('lands right after the song it passed when moving down, right before it when moving up', () => {
    // y (hidden) sits between b and c.
    const ids = ['a', 'b', 'y', 'c'];
    const down = moveInPlaylist(order(ids, ['a', 'b', 'c']), 0, 1)!;
    expect(down.order.ids).toEqual(['b', 'a', 'y', 'c']);
    expect(down.targetIndex).toBe(1);

    const up = moveInPlaylist(order(ids, ['a', 'b', 'c']), 2, 1)!;
    expect(up.order.ids).toEqual(['a', 'c', 'b', 'y']);
    expect(up.targetIndex).toBe(1);
  });

  it('ignores moves that change nothing or point outside the list', () => {
    expect(moveInPlaylist(order(['a', 'b']), 1, 1)).toBeNull();
    expect(moveInPlaylist(order(['a', 'b']), 0, 2)).toBeNull();
    expect(moveInPlaylist(order(['a', 'b']), -1, 0)).toBeNull();
  });
});
