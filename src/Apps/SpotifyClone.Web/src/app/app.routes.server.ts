import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  // Parameterised pages are rendered on demand; everything else is static.
  { path: 'search/:query', renderMode: RenderMode.Server },
  { path: 'playlist/:id', renderMode: RenderMode.Server },
  // Signed-in only: the session exists in the browser, so render there.
  { path: 'collection', renderMode: RenderMode.Client },
  // Reads browser cookies — nothing to render on the server.
  { path: 'auth-success', renderMode: RenderMode.Client },
  { path: '**', renderMode: RenderMode.Prerender },
];
