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
- Build and test the four non-MAUI projects instead:
  ```
  dotnet build PrayerTimeEngine.Core/PrayerTimeEngine.Core.csproj
  dotnet build PrayerTimeEngine.Core.Tests.Common/PrayerTimeEngine.Core.Tests.Common.csproj
  dotnet build PrayerTimeEngine.Core.Tests.Unit/PrayerTimeEngine.Core.Tests.Unit.csproj
  dotnet build PrayerTimeEngine.Core.Tests.Integration/PrayerTimeEngine.Core.Tests.Integration.csproj
  dotnet test PrayerTimeEngine.Core.Tests.Unit/PrayerTimeEngine.Core.Tests.Unit.csproj
  ```
- `PrayerTimeEngine.MAUI` is built by GitHub Actions (`.github/workflows`), not here.
- The Core build emits ~160 nullable warnings today; they are pre-existing, so only
  worry about warnings your own change introduces.
