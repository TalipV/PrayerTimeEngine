## graphify

This project has a knowledge graph at graphify-out/ with center nodes, community structure, and cross-file relationships.

Rules:
- For codebase questions, first run `graphify query "<question>"` when graphify-out/graph.json exists. Use `graphify path "<A>" "<B>"` for relationships and `graphify explain "<concept>"` for focused concepts. These return a scoped subgraph, usually much smaller than GRAPH_REPORT.md or raw grep output.
- If graphify-out/wiki/index.md exists, use it for broad navigation instead of raw source browsing.
- Read graphify-out/GRAPH_REPORT.md only for broad architecture review or when query/path/explain do not surface enough context.
- After modifying code, run `graphify update .` to keep the graph current (AST-only, no API cost).

## Building and testing in cloud sessions

Claude Code cloud sessions get a .NET SDK from the environment's setup script, but
**no MAUI workload** (it would need the Android SDK and several GB of downloads).

- `dotnet build PrayerTimeEngine.slnx` fails with `error NETSDK1147` because of the
  MAUI project. This is expected and says nothing about the code — do not try to
  install `maui-android` to work around it.
- Build and test the non-MAUI projects instead. The former `PrayerTimeEngine.Core`
  was split into three ONION rings (`PrayerTimeEngine.Domain`,
  `PrayerTimeEngine.Application`, `PrayerTimeEngine.Infrastructure`); building the
  test projects pulls all three in transitively:
  ```
  dotnet build PrayerTimeEngine.Domain/PrayerTimeEngine.Domain.csproj
  dotnet build PrayerTimeEngine.Application/PrayerTimeEngine.Application.csproj
  dotnet build PrayerTimeEngine.Infrastructure/PrayerTimeEngine.Infrastructure.csproj
  dotnet build PrayerTimeEngine.Core.Tests.Unit/PrayerTimeEngine.Core.Tests.Unit.csproj
  dotnet build PrayerTimeEngine.Core.Tests.Integration/PrayerTimeEngine.Core.Tests.Integration.csproj
  dotnet test PrayerTimeEngine.Core.Tests.Unit/PrayerTimeEngine.Core.Tests.Unit.csproj
  dotnet test PrayerTimeEngine.Core.Tests.Integration/PrayerTimeEngine.Core.Tests.Integration.csproj
  ```
- `PrayerTimeEngine.MAUI` is built by GitHub Actions (`.github/workflows`), not here.
- The Domain/Application/Infrastructure builds emit ~160 nullable warnings today; they
  are pre-existing, so only worry about warnings your own change introduces.

## ONION architecture

The solution follows the ONION model, enforced by project references (dependencies
point inward only):

- **`PrayerTimeEngine.Domain`** (ring 1) — entities, value objects, enums, domain
  services and the technology-free PORT interfaces (repositories, `I*ApiService`
  gateways, `IPlaceService`, ...). References only NodaTime + logging abstractions;
  no EF Core / Refit / HTTP.
- **`PrayerTimeEngine.Application`** (ring 2) — use-case orchestration (provider
  managers, factories, the concrete prayer-time providers, `ProfileService`,
  configuration import/export). References Domain only. `AddApplication()` is its
  composition root.
- **`PrayerTimeEngine.Infrastructure`** (ring 3) — adapters implementing the Domain
  ports: EF Core (`AppDbContext` + repositories), Refit HTTP clients (the
  `I*ApiService` ports are implemented by adapters that delegate to internal Refit
  twin interfaces `I*ApiClient`), WebSocket, HTML scraping, LocationIQ. `dotnet ef`
  migrations target this project. `AddInfrastructure(databasePath, locationIQApiKey)`
  is its composition root.
- **`PrayerTimeEngine.MAUI`** (ring 4, presentation) — UI + the real composition root,
  which calls `AddApplication()` + `AddInfrastructure(...)` and registers the
  platform-specific port implementations.

Namespaces are still rooted at `PrayerTimeEngine.Core.*` (a deliberate no-churn choice);
the rings are enforced by the csproj references, not by namespace names.

The ring rule is additionally guarded by architecture tests
(`tests/.../Architecture/OnionArchitectureTests.cs`, NetArchTest): Domain and Application
must not depend on EF Core / Refit / HtmlAgilityPack / Http / SQLite.

### Persistence mapping & the compiled model

Domain entities are attribute-free POCOs — EF mapping lives entirely in
`AppDbContext.OnModelCreating` (Fluent API) in Infrastructure. Keys are discovered by
convention (the `ID` property); the four API-ID entities (Fazilet/Semerkand City+Country)
use `.ValueGeneratedNever()` because their IDs come from the external API.

The runtime uses a **compiled model** (`AppDbContextModel.Instance`). After any change to the
model configuration or entities, regenerate it (needs the `dotnet-ef` global tool):

```
dotnet tool install --global dotnet-ef --version 10.0.11   # once
dotnet ef dbcontext optimize \
  --output-dir Data/EntityFramework/Generated_CompiledModels \
  --namespace PrayerTimeEngine.Core.Data.EntityFramework.Generated_CompiledModels \
  --project Infrastructure/PrayerTimeEngine.Infrastructure/PrayerTimeEngine.Infrastructure.csproj
```

## Pull requests created by Claude

PRs opened by a Claude session must be recognizable as such at a glance:

- **Branch name** starts with `claude/`.
- **Label** `claude` is applied to the PR.
- **First line of the PR body** is this banner, before any heading:

  ```
  > 🤖 **Dieser Pull Request wurde von Claude Code erstellt.**
  ```

- **End of the PR body** keeps the generated-with footer and the session link.

Same for the PR title: describe the change normally, no marker there — the banner,
the label and the branch prefix carry that.
