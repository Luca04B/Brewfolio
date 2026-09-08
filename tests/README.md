# Tests

Backend tests are separated by purpose:

- `Brewfolio.UnitTests` exercises domain and application behavior without infrastructure.
- `Brewfolio.ArchitectureTests` protects project dependency rules.
- `Brewfolio.IntegrationTests` runs the SQL Server adapter and HTTP API against a disposable
  Testcontainers SQL Server and MinIO instances. Docker must be running for this project.

The backend test projects target .NET 11 Preview 7. Domain tests verify both cases of the native C# 15 result unions: successful creation or mutation and structured validation failure. They exercise the public Entity factories and mutations, including aggregation of independent errors and the distinction between an explicit `Unknown` enum value and an undefined numeric value.

Angular component tests stay close to their components under `src/frontend` and run through the Angular test command.
The focused Playwright suite covers the complete Coffee Bean happy path in desktop and mobile Chromium against the real Docker stack.

Run everything currently available:

```bash
dotnet test Brewfolio.sln
npm test --prefix src/frontend -- --watch=false
npm run test:e2e --prefix src/frontend
```

Prefer testing observable behavior over mirroring the implementation structure.
