# SpotifyClone API

SpotifyClone is a backend-focused music platform API built as a modular monolith on .NET 10.  
The project implements core Spotify-like capabilities around identity, catalog management, playlists, playback, media assets, search, and subscriptions.  
It is structured for long-term maintainability with clear bounded contexts, CQRS workflows, explicit domain models, and production-grade infrastructure integrations.

## Overview

This project solves the backend complexity of a modern music streaming platform by separating business capabilities into independent modules while keeping deployment simple as a single API application.

It is primarily for:
- Recruiters evaluating backend system design quality.
- Senior developers reviewing architecture, domain modeling, and implementation discipline.
- Teams that need a strong modular monolith baseline before moving to distributed services.

Main goals:
- Implement a realistic music-platform backend with clear domain boundaries.
- Enforce consistency through layered architecture and shared building blocks.
- Keep operational concerns production-oriented (authentication, observability, background jobs, storage, and search).

## Architecture

Architecture style:
- **Modular Monolith** with **DDD-inspired bounded contexts**.
- **Layered structure** per module: `Domain` -> `Application` -> `Infrastructure`.
- **CQRS + MediatR** for command/query separation and pipeline behaviors.

High-level structure:
- `src/Apps/SpotifyClone.Api`  
  HTTP API host, authentication/authorization setup, middleware, OpenAPI, module wiring.
- `src/Modules/*`  
  Business modules (`Accounts`, `Catalog`, `Playlists`, `Streaming`, `Billing`, `Search`) with dedicated layers.
- `src/Shared/BuildingBlocks/*`  
  Reusable primitives (aggregate root, value object, domain event), application abstractions, infrastructure utilities.
- `src/Shared/Kernel/*`  
  Shared cross-context value objects/contracts.
- `tests/*`  
  Architecture, domain, application, and infrastructure tests.
- `docs/*`  
  Strategic/tactical/domain documentation.

Layer responsibilities:
- **Domain:** aggregates, entities, value objects, invariants, domain events.
- **Application:** use cases, command/query handlers, orchestration, interfaces.
- **Infrastructure:** EF Core persistence, external integrations, repository implementations, background processing.
- **API:** controllers/contracts, transport mappings, auth policies, hosting/runtime composition.

## Domain Model and Aggregates

The domain model is organized by bounded context. Aggregate roots own invariants and emit domain events, while Infrastructure persists them via EF Core mappings.

### Accounts Bounded Context
- **Aggregate Root:** `UserProfile`
- **Purpose:** user profile lifecycle and profile metadata
- **Key concepts:** display name, avatar metadata, birth date, gender
- **Related auth model:** ASP.NET Identity user + refresh token persistence (in infrastructure)

### Catalog Bounded Context
- **Aggregate Roots:** `Track`, `Album`, `Artist`, `Genre`, `Mood`
- **Purpose:** authoritative music metadata and publishing lifecycle
- **Key concepts:**
  - Track ownership of main artists, featured artists, genres, moods
  - Album ownership of tracks and main artists
  - Artist media model (avatar, banner, gallery)
  - Genre/Mood as classification entities for discovery and filtering

### Playlists Bounded Context
- **Aggregate Root:** `Playlist`
- **Entities inside aggregate:** `PlaylistTrack`
- **Purpose:** user-curated collections and collaboration
- **Key concepts:** owner, collaborators, visibility/type, ordered tracks, cover metadata

### Streaming Bounded Context
- **Aggregate Roots:** `AudioAsset`, `ImageAsset`, `PlaybackSession`, `PlaybackHistoryEntry`
- **Purpose:** playback state, media asset handling, listening history
- **Key concepts:** current playback context, queue/sync mechanics, asset readiness lifecycle

### Billing Bounded Context
- **Aggregate Root:** `Subscription`
- **Purpose:** subscription status and billing period lifecycle
- **Key concepts:** user-to-subscription mapping and external provider identity binding

### Search Bounded Context
- **Purpose:** unified search and reindexing workflows
- **Pattern:** integration-event-driven index updates + query endpoint

## Technologies Used

### Backend
- .NET 10 (`net10.0`)
- ASP.NET Core Web API
- MediatR
- SignalR
- Hangfire
- Serilog
- Scalar + OpenAPI

### Database & Persistence
- PostgreSQL
- Entity Framework Core
- Code-first migrations (module-specific DbContexts)

### Caching / Queues / Search
- Redis (also used by Hangfire storage)
- Meilisearch

### Object and Media Storage
- MinIO (S3-compatible object storage)
- FFmpeg (media processing pipeline support)

### Authentication / Security
- JWT Bearer authentication
- ASP.NET Identity
- Google OAuth login
- Refresh token rotation via secure cookies
- ASP.NET rate limiting policies

### Billing / Notifications / Integrations
- Stripe integration (subscription checkout/webhook/cancel flows)
- SMTP email sender

### Dev Tools / Quality
- Docker + Docker Compose
- Analyzer-enforced build quality (`TreatWarningsAsErrors`, SonarAnalyzer)
- `.editorconfig` code style enforcement

### Design Patterns Used
- Modular Monolith
- DDD tactical patterns (aggregates, value objects, domain events, repositories)
- CQRS (commands/queries)
- Unit of Work
- Outbox processing (module outbox configurations + jobs)
- Pipeline behaviors (validation, logging, transaction, exception handling)

## Core Features

Implemented core features:
- Account registration, password login, refresh token login, logout.
- Email verification and password reset workflows.
- Google OAuth login callback flow.
- OTP login request/verify flows.
- User profile retrieval/update and avatar management.
- Catalog management for tracks, albums, artists, genres, and moods.
- Artist moderation/admin actions (ban/unban/verify/unverify).
- Playlist CRUD, cover updates, collaborators, and track reorder/add/remove.
- Current user playlists and current user tracks endpoints.
- Playback controls (play, pause, resume, seek, next/previous, shuffle/repeat, queue operations).
- Media upload/access endpoints for audio and images.
- Unified search endpoint and reindex endpoint.
- Shared cross-module lightweight query endpoints.
- Subscription checkout, webhook handling, cancellation, and current subscription retrieval.

## Technical Highlights

- Modular composition in `Program.cs` with explicit per-module bootstrapping.
- Per-module persistence boundaries (separate DbContexts and migrations).
- Strongly typed identifiers and value objects across contexts to reduce primitive misuse.
- Ownership collections persisted via `OwnsMany` and `OwnsOne`, preserving aggregate intent.
- Rate-limited authentication/verification endpoints reduce abuse surface.
- Real-time playback communication via SignalR hub (`/hubs/streaming`).
- Background processing via Hangfire with Redis backend.
- Search indexing integration through a dedicated Search module and reindex job.
- Production-friendly integrations: PostgreSQL, Redis, MinIO, Meilisearch, Stripe.

## Database Design

The data model is split by module while sharing a single PostgreSQL runtime connection configuration.

Main entities/aggregates:
- Accounts: `user_profiles` (+ identity tables and refresh tokens in Accounts infrastructure).
- Catalog: `tracks`, `albums`, `artists`, `genres`, `moods`, and ownership link tables.
- Playlists: `playlists`, `playlist_tracks`, `playlist_collaborators`, reference tables.
- Streaming: `audio_assets`, `image_assets`, `playback_history_entries`, playback entities.
- Billing: `subscriptions`.
- Cross-cutting module outbox tables with processed timestamp indexing.

Notable relationships and constraints visible in EF mappings:
- `Playlist` -> many `PlaylistTrack` (`playlist_tracks.playlist_id` FK).
- Unique playlist ordering constraint: `(playlist_id, position)` on `playlist_tracks`.
- Unique track presence per playlist: `(track_id, playlist_id)` on `playlist_tracks`.
- Track unique audio asset binding: unique index on `tracks.audio_file_id`.
- Track ownership collections enforce uniqueness (main/featured artists, genres, moods per track).
- Album main artists uniqueness per `(album_id, artist_id)`.
- Playlist collaborators uniqueness per `(playlist_id, collaborator_id)`.
- Artist gallery uses composite key `(artist_id, image_id)`.
- Owned image metadata columns are embedded into owner tables (cover/avatar/banner metadata fields).

## How to Run the Project

### Requirements
- .NET SDK 10
- Docker Desktop (recommended for local infrastructure)
- Optional local tools if not using Docker: PostgreSQL, Redis, MinIO, Meilisearch

### Environment Variables / Configuration

`src/Apps/SpotifyClone.Api/appsettings.json` defines required config sections:
- `ConnectionStrings__MainDb`
- `ConnectionStrings__Redis`
- `Jwt__Issuer`, `Jwt__Audience`, `Jwt__SecretKey`
- `Authentication__Google__ClientId`, `Authentication__Google__ClientSecret`
- `Smtp__*`
- `Application__FrontendUrl`, `Application__ApiUrl`
- `Minio__*`
- `Stripe__*`
- `MeiliSearch__Endpoint`, `MeiliSearch__MasterKey`

You can supply values via `appsettings.Development.json`, user secrets, or environment variables.

### Option A: Run with Docker Compose (recommended)

```bash
docker compose -f docker-compose.yml up --build
```

This starts:
- API (`http://localhost:5000`)
- PostgreSQL
- Redis
- MinIO
- Meilisearch

### Option B: Run API locally

1. Start infrastructure services (`postgres`, `redis`, `minio`, `meilisearch`) using:
```bash
docker compose -f docker-compose.infra.yml up -d
```

2. Run API:
```bash
dotnet run --project src/Apps/SpotifyClone.Api/SpotifyClone.Api.csproj
```

### Database Setup and Migrations

In Development, the API applies migrations automatically at startup for module DbContexts:
- Accounts
- Identity
- Streaming
- Catalog
- Playlists
- Billing

No manual migration command is required for the default development flow.

## API Endpoints

Base path: `api/v1`

Main route groups implemented:
- **Auth:** `/auth/*` (register, login, refresh, logout, email verification, password reset, Google login, OTP login)
- **Users:** `/users/*` and `/users/me`
- **Catalog:** `/tracks/*`, `/albums/*`, `/artists/*`, `/genres/*`, `/moods/*`
- **Admin catalog:** `/admin/artists/*`, `/admin/genres/*`, `/admin/moods/*`
- **Playlists:** `/playlists/*`, `/me/playlists`
- **Playback:** `/me/playback/*`
- **Media:** `/media/audio`, `/media/images`
- **Search:** `/search`, `/search/reindex`
- **Shared read endpoints:** `/shared/*`
- **Billing:** `/billing/me/*`

Authentication and authorization:
- JWT Bearer authentication is enabled globally.
- Refresh tokens are stored in HTTP-only cookies and rotated via `/auth/refresh`.
- Role-based authorization is used (`Admin`, `Creator`, `Listener`) on protected endpoints.
- Public endpoints are explicitly marked with `AllowAnonymous`.
- Rate limiting is configured for login/send/verification scenarios.

## Future Improvements

Based on current structure and existing module placeholders:
- Implement currently scaffolded but inactive modules (`Analytics`, `Recommendations`, `Social`) end-to-end.
- Add CI pipelines for build/test/lint/package workflows.
- Develop a Frontend web client application potentially using ReactJS.

## Author

- Author: Deinesh Trombola
- Role: .NET Developer
