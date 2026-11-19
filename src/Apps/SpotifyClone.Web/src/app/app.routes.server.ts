import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  // Parameterised pages are rendered on demand; everything else is static.
  { path: 'search/:query', renderMode: RenderMode.Server },
  { path: 'playlist/:id', renderMode: RenderMode.Server },
  { path: '**', renderMode: RenderMode.Prerender },
];
