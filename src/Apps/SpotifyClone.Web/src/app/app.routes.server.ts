import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  // Static pages, rendered at build time.
  { path: 'signup', renderMode: RenderMode.Prerender },
  { path: 'login', renderMode: RenderMode.Prerender },
  // Signed-in only: the session exists in the browser, so render there.
  { path: 'collection', renderMode: RenderMode.Client },
  // Reads browser cookies — nothing to render on the server.
  { path: 'auth-success', renderMode: RenderMode.Client },
  // Everything else shows live catalog data: render per request.
  { path: '**', renderMode: RenderMode.Server },
];
