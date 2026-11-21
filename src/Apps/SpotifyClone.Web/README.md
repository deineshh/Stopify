# Stopify Web

Angular 21 web client for Stopify. It is a server-rendered (SSR + hydration), zoneless, standalone-component app built from the HTML/CSS mockups in [`mockups/`](./mockups).

Authentication, the catalog (home, search, albums, artists and playlists), Your Library and playlists (create, edit, delete, add and remove songs), and playback (HLS streaming through the API's playback session, with the Now Playing and Queue panels) are connected to the API. Following artists and Liked Songs are UI-only because the API has no endpoints for them yet. The API has no recommendations endpoint, so the home feed is composed from the album, artist and public playlist lists.

## Getting started

```bash
npm install
npm start           # dev server on http://localhost:4200
npm run build       # production build (browser + SSR server)
npm test            # unit tests (Vitest)
npm run serve:ssr:SpotifyClone.Web   # serve the production build on :4000
```

The dev server proxies `/api` and `/hubs` (SignalR, WebSockets included) to `API_PROXY_TARGET` (default `http://localhost:5000`), and `/audio` and `/images` to `STORAGE_PROXY_TARGET` (default `http://localhost:9000`). Docker Compose points them at the `api` and `minio` containers. Server-side rendering calls the API directly at `API_URL`.

In production the SSR server (`npm run serve:ssr:SpotifyClone.Web`) only serves the web app: put a reverse proxy in front that routes `/api` and `/hubs` (with WebSocket upgrades) to the API, and `/audio` and `/images` to the storage. If one of those paths reaches the SSR server anyway, it answers 404 instead of rendering a page.

## Architecture

```
src/
├── styles/               Design tokens, reset, fonts, scrollbar, breakpoint mixins, menus and dialogs (overlays)
├── styles.scss           Global entry (document-level rules only)
└── app/
    ├── core/             App-wide singletons
    │   ├── api/          API base URL, ProblemDetails → ApiError, paging, media URLs
    │   ├── auth/         AuthStore (session), interceptor, guards
    │   ├── icons/        Font Awesome registry (only the icons we use are bundled)
    │   ├── layout/       LayoutStore (sidebars, panels, scroll) + ViewportService (breakpoints)
    │   ├── notifications/ ToastService
    │   └── playback/     AudioEngine (HLS via hls.js), PlaybackSignals (SignalR), this browser's device id
    ├── data/             Everything the UI reads
    │   ├── api/          Typed API clients (CatalogApi, PlaylistsApi, PlaybackApi), DTOs and DTO → view model mappers
    │   ├── models/       View models, independent of backend DTOs
    │   └── services/     Catalog, Collection, Library, PlaylistEditor, Player, NowPlaying, TrackLookup
    ├── layout/           Application shell (header, library, right panels, player bar, footer, mobile nav)
    ├── features/         Lazy-loaded pages: home, search, playlist, library, auth
    ├── shared/           Reusable UI: media card/shelf, track table, play button, menus, dialogs, filter chip, slider, pipes
    └── testing/          Test helpers
```

Path aliases: `@core/*`, `@data/*`, `@layout/*`, `@features/*`, `@shared/*`.

### Conventions

- **Standalone, OnPush, signals everywhere.** State lives in signal-based services (`LayoutStore`, `PlayerService`, `LibraryService`). Components read it directly.
- **API-shaped data layer.** `CatalogService` returns `Observable`s just like `HttpClient`, and pages consume them through `rxResource` / `toSignal`. Connecting the backend means changing the service bodies, not the components.
- **Styles ported 1:1 from the mockups.** Each component's `.scss` starts from its mockup stylesheet. ID selectors became BEM classes, and responsive and state rules come after the ported block.
- **Route parameters as inputs** (`withComponentInputBinding`). For example, `PlaylistPage.id` is the `:id` parameter.

## Authentication

- **Session:** `AuthStore` keeps the access token in memory only. The refresh token is the API's HttpOnly `refreshToken` cookie, so a reload restores the session through `POST /auth/refresh` once the app has hydrated. The token is also refreshed a minute before it expires.
- **Requests:** `authInterceptor` adds `Authorization: Bearer …` to `/api/v1/*` calls. On a 401 it refreshes once, shared by every request waiting at that moment (the API rotates refresh tokens), then replays them.
- **Errors:** API failures are converted from ProblemDetails into `ApiError` (`code`, `message`, `errors[]`, plus helpers like `has('Auth.InvalidPassword')`).
- **Routes:** `authGuard` protects signed-in pages such as `/collection`. `guestGuard` sends signed-in users away from `/login` and `/signup`. Redirect targets are restricted to in-app paths.
- **Flows:** sign up with email verification, log in with a password or an SMS code (phone numbers), password reset, and Google sign-in (the API redirects to `/auth-success`).
- **Password policy:** the rules shown on screen mirror the API's ASP.NET Identity options (`features/auth/password-policy.ts`).

## Playback

- **The session lives on the server.** `PlayerService` sends every control to `/me/playback` (play, pause/resume, next/previous, seek, shuffle, repeat) and plays the HLS URL the API returns (`/audio/{asset}/media_0.m3u8`, or `master.m3u8` for Premium). Progress is synced every 10 seconds and before pausing, so a reload restores the last song, paused at the same second.
- **Audio:** `AudioEngine` plays the stream through a detached `<audio>` element. The light build of hls.js is loaded on the first play (it isn't in the main bundle); browsers without Media Source Extensions but with native HLS get the URL directly.
- **Play buttons:** `<button appPlayButton [target]="…">` plays an album, playlist or artist and shows pause while it is playing. Track rows play their album or playlist from that song. An artist plays their latest track's album. Guests are sent to log in.
- **Devices:** each browser has an id (`stopify.deviceId` in localStorage). While signed in, `PlaybackSignals` listens to the Streaming hub (`/hubs/streaming`, SignalR loaded on demand): when playback starts on another device, this one pauses and says so. Skip, seek, shuffle and repeat only work from the active device, so if another device took over, the player resumes here first and retries.
- **Panels:** `NowPlayingService` loads what the right-hand panels show: the album or playlist name, the first artist's bio and avatar, and the queue (`/me/playback/queue`, only while a panel is on screen). A queued song plays the same album or playlist from that song.
- **Not yet in the API:** Search has no playback context of its own (the API's `search` context can't be started), so a song from search plays from its album.

## Playlists

- **One place for changes.** `PlaylistEditor` creates, edits and deletes playlists and adds or removes songs through `PlaylistsApi`. It refreshes Your Library, bumps the playlist's revision (an open playlist page reloads in place) and reports the result as a toast. Guests are sent to log in.
- **Menus** use the Angular CDK (`@angular/cdk/menu`): right-click or the "…" button, keyboard navigation, submenus and focus return. Each list renders one menu and passes the row as trigger data:
  - `app-track-menu`: Add to playlist ▸ (New playlist or one of yours), Remove from this playlist (your own playlist), Add to queue, Go to artist, Go to album.
  - `app-playlist-menu`: Edit details, Make public/private, Delete. On your playlists in the library and on the playlist page.
- **Dialogs** use `@angular/cdk/dialog`: Edit details (also opened by clicking the title or the cover, like Spotify) and a delete confirmation. Menu and dialog styles live in `styles/_overlays.scss`, because the overlay renders outside every component.
- **Rules from the API:** new playlists are private and named "My Playlist #N"; names are required (100 characters max) and descriptions are limited to 300; a song can be in a playlist only once; only the owner edits or deletes. "Add to queue" needs something playing, because the queue belongs to the playback session.
- **Not yet:** uploading a cover (the API's cover endpoint needs image metadata that the upload endpoint doesn't return), collaborators and reordering songs.

## Routes

| Path | Page |
| --- | --- |
| `/` | Home |
| `/search` | Browse all categories |
| `/search/:query` | Search results |
| `/playlist/:id` | Playlist / artist page |
| `/collection` | Your Library (signed in only) |
| `/signup`, `/login` | Sign up, log in and password reset (signed out only) |
| `/auth-success` | Google sign-in return page |

## Responsive behaviour

| Width | Layout |
| --- | --- |
| ≥ 1280px | Full desktop: library, main view, Now Playing / Queue panel, player bar |
| 1024–1279px | Right panel closed by default |
| 768–1023px | Library collapses to an icon rail, the header gets more compact |
| < 768px | Phone: bottom tab bar, floating mini player, shelves become swipeable |

## Testing

- **Unit tests** (`npm test`): pipes, stores and services, the auth session and interceptor against a mocked HTTP backend, guards, and component behaviour (cards, shelves, header, account menu, library, home, search, collection pages, every auth flow).
- The UI was also checked against the mockups with pixel diffs (0.03–0.17% difference per page at 1920×1080; the remaining pixels are intentional state differences). End-to-end interaction checks and an axe-core accessibility audit (0 violations) were run as well.
