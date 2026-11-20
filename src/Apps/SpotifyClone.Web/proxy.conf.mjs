// Dev-server proxy: the browser calls same-origin /api, /hubs, /audio and /images; the dev server forwards them.
// Inside Docker Compose the API is `api:8080`; running `ng serve` locally it is `localhost:5000`.
const api = process.env.API_PROXY_TARGET ?? 'http://localhost:5000';
const storage = process.env.STORAGE_PROXY_TARGET ?? 'http://localhost:9000';

export default {
  '/api': {
    target: api,
    secure: false,
    changeOrigin: true,
    logLevel: 'warn',
  },
  // SignalR (playback moved to another device). WebSockets need `ws: true`.
  '/hubs': {
    target: api,
    secure: false,
    changeOrigin: true,
    ws: true,
  },
  '/audio': {
    target: storage,
    secure: false,
    changeOrigin: true,
  },
  '/images': {
    target: storage,
    secure: false,
    changeOrigin: true,
  },
};
