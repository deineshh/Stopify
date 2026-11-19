import { NowPlayingDetails } from '../models/player.models';
import { Track } from '../models/catalog.models';
import { COVER, artist, repeat } from './catalog.mock';

const pannonia: Track = {
  id: 'pannonia',
  title: 'PANNONIA',
  artists: [artist('DESH'), artist('Young Fly'), artist('Azahriah')],
  album: { id: 'bakpakk', name: 'BAKPAKK' },
  cover: COVER,
  durationSeconds: 147,
  saved: true,
};

export const NOW_PLAYING: NowPlayingDetails = {
  context: artist('Azahriah'),
  track: pannonia,
  spotlight: {
    artist: artist('DESH'),
    avatar: COVER,
    monthlyListeners: 653_000,
    // Placeholder copy — the real bio will come from the Streaming API.
    bio:
      'DESH is a Hungarian rapper and songwriter whose story went from working odd jobs to ' +
      'topping the national charts. He regularly collaborates with Azahriah and Young Fly, ' +
      'tours with a full live band and keeps releasing new EPs every year.',
    following: false,
  },
  credits: [
    { artist: artist('DESH'), role: 'Main Artist', following: false },
    { artist: artist('Young Fly'), role: 'Main Artist', following: false },
    { artist: artist('Azahriah'), role: 'Main Artist', following: true },
  ],
};

const bakpakk = (i: number): Track => ({
  id: `bakpakk-${i + 1}`,
  title: 'BAKPAKK',
  artists: [artist('DESH'), artist('Young Fly'), artist('Azahriah')],
  album: { id: 'bakpakk', name: 'BAKPAKK' },
  cover: COVER,
  durationSeconds: 162,
});

/** Queue panel content: the current track followed by what plays next. */
export const QUEUE = {
  current: bakpakk(-1),
  nextFrom: artist('Azahriah'),
  upNext: repeat(15, bakpakk),
};
