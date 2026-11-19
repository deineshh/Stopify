// Dev-server proxy: the browser calls same-origin /api, /audio and /images; the dev server forwards them.
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
