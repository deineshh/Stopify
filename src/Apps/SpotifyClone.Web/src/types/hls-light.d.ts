// hls.js ships no typings for its light build; it exposes the same API as the full one.
declare module 'hls.js/light' {
  export * from 'hls.js';
  export { default } from 'hls.js';
}
