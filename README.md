# AniTrack

![CI](https://github.com/caiosaraiv1/AniTrack/actions/workflows/ci.yml/badge.svg)

A REST API for keeping track of the anime you are watching. Search the AniList catalog, add a series to your watchlist and record the episode you are on. The API knows how many episodes have actually aired, warns you when your progress goes past them, and refreshes its data from AniList when it needs to.

I built AniTrack to learn C# and ASP.NET Core coming from Java and Spring Boot. The scope is deliberately small, so the interesting part is in the design decisions, which are documented below.

## Features

| Method | Route | Description | Responses |
|---|---|---|---|
| `GET` | `/anime/search?query={text}` | Search AniList for episodic anime (TV, TV short, OVA, ONA) | `200`, `400`, `503` |
| `POST` | `/watchlist` | Add an anime to the watchlist by its AniList ID | `201`, `400`, `404`, `409`, `503` |
| `GET` | `/watchlist` | List the watchlist, ordered by title | `200` |
| `PATCH` | `/watchlist/{aniListId}` | Update the current episode | `200`, `400`, `404` |
| `DELETE` | `/watchlist/{aniListId}` | Remove an anime from the watchlist | `204`, `404` |

Errors follow the [ProblemDetails](https://www.rfc-editor.org/rfc/rfc9457) format.

### Example: updating progress past the latest aired episode

```http
PATCH /watchlist/21
Content-Type: application/json

{ "episode": 1181 }
```

```json
{
  "anime": {
    "aniListId": 21,
    "title": "ONE PIECE",
    "status": "Releasing",
    "currentEpisode": 1181,
    "maxKnownEpisode": 1180
  },
  "warning": "Episode 1181 is beyond the latest known episode (1180)."
}
```

The progress is saved anyway. Before answering, the API asked AniList for fresh data, since a new episode may have aired. Because AniList still listed 1180 as the latest episode, the warning remains.

## Tech stack

- **.NET 10** and **ASP.NET Core** with controllers
- **Entity Framework Core 10** with **PostgreSQL 18** (Npgsql), running in Docker Compose
- **AniList GraphQL API** as the anime data source
- **xUnit** and **NSubstitute** for unit tests
- **GitHub Actions** for continuous integration
- **Bruno** for the API request collection in [`bruno/`](bruno)

## Architecture

The code is organized by feature rather than by layer, with ports and adapters isolating the external API.

```
src/AniTrack.Api/
├── Features/          What users can do: controllers, services, request and response DTOs
│   ├── AnimeSearch/
│   └── PersonalAnimes/
├── Domain/            Entities, rules and the IAnimeProvider port, with no HTTP, database or AniList code
├── Integrations/      The AniList adapter: GraphQL client, AniList DTOs and the mapper
└── Infrastructure/    EF Core DbContext, entity configurations, migrations and global error handling
```

The domain defines the interface it needs (`IAnimeProvider`), and the AniList client implements it. Features depend only on the interface, so swapping the data provider means writing a new adapter and changing one line in `Program.cs`.

## Design decisions

### The provider is replaceable because the first one went away

The original plan used Jikan, an unofficial MyAnimeList API. While I was still designing the project, its public instance was shut down. That shaped two decisions: the provider sits behind `IAnimeProvider`, and the database stores both the AniList ID (used for every call) and the MyAnimeList ID when one exists, since other anime sources also understand it and a future migration could map entries by it.

### A local snapshot instead of querying AniList on every read

When an anime is added, the API stores a snapshot of its data (title, status, episode counts, cover image) together with a `LastSyncedAt` timestamp, instead of only the external ID. This means that:

- the watchlist still works when AniList is down;
- listing does not spend AniList's rate limit, which is 30 requests per minute;
- reads come from the local database, which is faster.

The trade-off is that the data can go stale, so the snapshot is refreshed when needed (see below).

### Two entities: the anime and your progress

`Anime` holds facts about the show, shared by anyone watching it. `PersonalAnime` holds your progress: the current episode and when it was added and updated. Keeping them separate gives each table one responsibility and leaves room for multiple users.

Since the application currently has a single user, removing an anime from the watchlist also deletes its snapshot. The foreign key uses `Restrict`, so the deletion is explicit in the code rather than a hidden database cascade.

### Validation depends on the airing status

The total episode count alone cannot validate progress. One Piece has no total, and a season that is still airing has a total that has not aired yet. `MaxKnownEpisode` is computed from the status:

| Status | Latest known episode |
|---|---|
| Finished | the total episode count |
| Releasing, with a scheduled next episode | the next episode minus one |
| Not yet released | 0 |
| Hiatus, cancelled or no schedule | unknown |

It is a computed property rather than a stored column, so it is always consistent with the latest snapshot.

### Progress never fails because of AniList

Going past the latest known episode is allowed. Users often watch an episode before the catalog updates. The update flow is:

1. Save the new progress.
2. If it is beyond the latest known episode, ask AniList for fresh data and resync the snapshot.
3. If AniList is unavailable or returns unusable data, log a warning and continue.

The user's own data is saved first, and the external call is best-effort, with a short timeout.

### Only episodic formats

Movies and trailers do not fit an episode tracker. The GraphQL query filters them out (`format_in: [TV, TV_SHORT, OVA, ONA]`), and the mapper enforces the same rule, so adding a movie by ID directly returns `404`.

### Tolerant DTOs, strict domain

The AniList DTOs accept everything the API might send, with almost every field nullable and enums kept as strings. The domain only accepts valid data. The mapper sits between them. It discards items it cannot represent, such as an item with no title or with an unknown format. It normalizes values the domain would reject but that only mean "unknown", such as a zero episode count, to `null`.

### Distinguishing "not found" from "unavailable"

- AniList returning HTTP `404` for an ID means the anime does not exist, which becomes `404`.
- Network failures, timeouts, error statuses, invalid JSON, GraphQL errors returned with HTTP `200`, and a response for a different ID than requested mean the provider is unavailable, which becomes `503`.
- A request cancelled by the client is not reported as an AniList failure.

Domain exceptions are mapped to HTTP responses in a single global `IExceptionHandler`, so controllers only contain the success path.

### Persistence details

- Enums are stored as text, so reordering an enum never changes the meaning of existing rows.
- Tables and columns use snake_case, the PostgreSQL convention.
- Unique indexes on the AniList ID and on the watchlist entry guarantee no duplicates, even if two requests race past the application-level check.
- The schema is versioned with EF Core migrations.

## Testing

The project has **132 unit tests** covering:

- domain rules: construction, sync, the latest-known-episode logic and progress updates;
- the AniList mapper, including malformed and partial data;
- the AniList client, with a fake HTTP handler for every failure mode (status errors, timeouts, cancellation, invalid JSON and GraphQL errors);
- the watchlist service with mocked dependencies, including resync failures that must not affect saved progress;
- the global exception handler.

The suite uses no network and no database, so it runs in a few seconds, locally and in CI. To check that the tests actually catch regressions, defects were deliberately injected into the code one at a time, and the suite caught every one of them.

Reviewing the code while writing the tests surfaced five real bugs, all now fixed and covered by tests. Examples include a GraphQL error returned with HTTP `200` being reported as "not found", and invalid data from AniList turning a successful progress update into a `500`.

Controllers and the repository are not unit tested. They only pass calls through, and testing them meaningfully requires integration tests with a real database.

## Running locally

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/)
- The EF Core CLI: `dotnet tool install --global dotnet-ef`

### Steps

1. Clone the repository and enter it:

   ```bash
   git clone https://github.com/caiosaraiv1/AniTrack.git
   cd AniTrack
   ```

2. Create the environment file and set a password in it:

   ```bash
   cp .env.example .env
   ```

3. Start PostgreSQL:

   ```bash
   docker compose up -d
   ```

4. Store the connection string with User Secrets, using the values from your `.env`:

   ```bash
   dotnet user-secrets set "ConnectionStrings:AniTrack" \
     "Host=localhost;Port=5432;Database=anime_tracker;Username=postgres;Password=<your password>" \
     --project src/AniTrack.Api
   ```

5. Apply the migrations:

   ```bash
   dotnet ef database update --project src/AniTrack.Api
   ```

6. Run the API:

   ```bash
   dotnet run --project src/AniTrack.Api
   ```

   It listens on `http://localhost:5180`. To try the endpoints, open the [`bruno/`](bruno) folder in [Bruno](https://www.usebruno.com/) and select the `Local` environment.

### Running the tests

```bash
dotnet test
```

## Limitations and next steps

- Single user, with no authentication.
- Search returns only the first page of results.
- No integration tests for controllers and persistence yet. Testcontainers with PostgreSQL would be the natural next step.
- No retry or backoff when AniList rate-limits requests.
- Only the database runs in Docker. Containerizing the API itself is a next step.
