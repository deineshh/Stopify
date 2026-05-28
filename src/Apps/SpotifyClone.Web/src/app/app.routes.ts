import { Routes } from '@angular/router';
import { RenderMode } from '@angular/ssr';
import { ShellComponent } from './shell/shell.component';

export const routes: Routes = [
  {
    path: '',
    component: ShellComponent,
    children: [
      {
        path: '',
        // data: { renderMode: RenderMode.Prerender },
        loadComponent: () => import('./pages/playlist/playlist.component').then(m => m.PlaylistComponent),
      },
      {
        path: 'search',
        loadComponent: () => import('./pages/search-browse/search-browse.component').then(m => m.SearchBrowseComponent),
      },
      {
        path: 'search/:query',
        loadComponent: () => import('./pages/search-results/search-results.component').then(m => m.SearchResultsComponent),
      },
      {
        path: 'playlist/:id',
        loadComponent: () => import('./pages/playlist/playlist.component').then(m => m.PlaylistComponent),
      },
    ],
  },
  {
    path: '**',
    redirectTo: '',
  },
];
