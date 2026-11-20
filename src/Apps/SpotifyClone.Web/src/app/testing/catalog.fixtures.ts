/** API DTOs for specs that need a playing song. */
export const artistDto = (id: string, name = id.toUpperCase()) => ({ id, name, status: 'Verified', ownerId: null, avatar: null });

export const trackDto = (id: string, albumId = 'a1', extra: object = {}) => ({
  id,
  title: `Song ${id}`,
  duration: '00:03:00',
  releaseDate: null,
  releaseDateUtc: null,
  containsExplicitContent: false,
  status: 'published',
  audioFileId: `audio-${id}`,
  albumId,
  mainArtists: [artistDto('desh', 'DESH')],
  featuredArtists: [] as ReturnType<typeof artistDto>[],
  genres: [],
  moods: [],
  ...extra,
});

export const albumDto = (id: string, title = `Album ${id}`) => ({
  id,
  title,
  releaseDate: null,
  status: 'published',
  type: 'album',
  cover: { imageId: `cover-${id}`, width: 1, height: 1, fileType: 'webp', sizeInBytes: 1 },
  mainArtists: [],
  tracks: [],
});

export const streamDto = (trackId: string | null) => ({
  hlsUrl: trackId ? `/audio/${trackId}/media_0.m3u8` : null,
  dashUrl: null,
  startPositionMs: 0,
  trackId,
});
