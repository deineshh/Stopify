import { Routes } from '@angular/router';
import { MainLayout } from '@layout/main-layout/main-layout';
import { authGuard, guestGuard } from '@core/auth/auth.guards';

const TITLE = 'Stopify - Web Player: Music for everyone';

export const routes: Routes = [
  {
    path: '',
    component: MainLayout,
    children: [
      {
        path: '',
        title: TITLE,
        loadComponent: () => import('@features/home/home-page').then((m) => m.HomePage),
      },
      {
        path: 'search',
        title: 'Search - Stopify',
        loadComponent: () =>
          import('@features/search/search-browse/search-browse-page').then((m) => m.SearchBrowsePage),
      },
      {
        path: 'search/:query',
        title: 'Search - Stopify',
        loadComponent: () =>
          import('@features/search/search-results/search-results-page').then((m) => m.SearchResultsPage),
      },
      {
        path: 'playlist/:id',
        title: TITLE,
        loadComponent: () => import('@features/playlist/playlist-page').then((m) => m.PlaylistPage),
      },
      {
        path: 'collection',
        title: 'Your Library - Stopify',
        canActivate: [authGuard],
        loadComponent: () => import('@features/library/library-page').then((m) => m.LibraryPage),
      },
    ],
  },
  {
    path: 'signup',
    title: 'Sign up - Stopify',
    data: { flow: 'signup' },
    canActivate: [guestGuard],
    loadComponent: () => import('@features/auth/auth-page').then((m) => m.AuthPage),
  },
  {
    path: 'login',
    title: 'Log in - Stopify',
    data: { flow: 'login' },
    canActivate: [guestGuard],
    loadComponent: () => import('@features/auth/auth-page').then((m) => m.AuthPage),
  },
  {
    // Google sign-in lands here (AuthController.LoginGoogleCallback).
    path: 'auth-success',
    title: 'Logging in - Stopify',
    loadComponent: () => import('@features/auth/auth-callback-page').then((m) => m.AuthCallbackPage),
  },
  { path: '**', redirectTo: '' },
];
