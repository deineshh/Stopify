# Stopify Web

Angular 21 web client for Stopify. It is a server-rendered (SSR + hydration), zoneless, standalone-component app built from the HTML/CSS mockups in [`mockups/`](./mockups).

The UI is design-complete and runs on mock data. Every button, toggle, slider, panel and form reacts like the real thing, but nothing is streamed or sent to the API yet.

## Getting started

```bash
npm install
npm start           # dev server on http://localhost:4200
npm run build       # production build (browser + SSR server)
npm test            # unit tests (Vitest)
npm run serve:ssr:SpotifyClone.Web   # serve the production build on :4000
```

## Architecture

```
src/
├── styles/               Design tokens, reset, fonts, scrollbar, breakpoint mixins
├── styles.scss           Global entry (document-level rules only)
└── app/
    ├── core/             App-wide singletons
    │   ├── icons/        Font Awesome registry (only the icons we use are bundled)
    │   └── layout/       LayoutStore (sidebars, panels, scroll) + ViewportService (breakpoints)
    ├── data/             Everything the UI reads
    │   ├── models/       View models, independent of backend DTOs
    │   ├── mock/         Static fixtures mirroring the mockups
    │   └── services/     CatalogService, LibraryService, PlayerService
    ├── layout/           Application shell (header, library, right panels, player bar, footer, mobile nav)
    ├── features/         Lazy-loaded pages: home, search, playlist, library, auth
    ├── shared/           Reusable UI: media card/shelf, track table, play button, filter chip, slider, pipes
    └── testing/          Test helpers
```

Path aliases: `@core/*`, `@data/*`, `@layout/*`, `@features/*`, `@shared/*`.

### Conventions

- **Standalone, OnPush, signals everywhere.** State lives in signal-based services (`LayoutStore`, `PlayerService`, `LibraryService`). Components read it directly.
- **API-shaped data layer.** `CatalogService` returns `Observable`s just like `HttpClient`, and pages consume them through `rxResource` / `toSignal`. Connecting the backend means changing the service bodies, not the components.
- **Styles ported 1:1 from the mockups.** Each component's `.scss` starts from its mockup stylesheet. ID selectors became BEM classes, and responsive and state rules come after the ported block.
- **Route parameters as inputs** (`withComponentInputBinding`). For example, `PlaylistPage.id` is the `:id` parameter.

## Routes

| Path | Page |
| --- | --- |
| `/` | Home |
| `/search` | Browse all categories |
| `/search/:query` | Search results |
| `/playlist/:id` | Playlist / artist page |
| `/collection` | Your Library (the Library tab on phones) |
| `/signup`, `/login` | Auth flows (sign-up wizard, code and password log-in) |

## Responsive behaviour

| Width | Layout |
| --- | --- |
| ≥ 1280px | Full desktop: library, main view, Now Playing / Queue panel, player bar |
| 1024–1279px | Right panel closed by default |
| 768–1023px | Library collapses to an icon rail, the header gets more compact |
| < 768px | Phone: bottom tab bar, floating mini player, shelves become swipeable |

## Testing

- **Unit tests** (`npm test`): pipes, stores and services, plus component behaviour (cards, shelves, header search, player bar, the full auth wizard).
- The UI was also checked against the mockups with pixel diffs (0.03–0.17% difference per page at 1920×1080; the remaining pixels are intentional state differences). End-to-end interaction checks and an axe-core accessibility audit (0 violations) were run as well.
