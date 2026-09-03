# Tests

Backend tests are separated by purpose:

- `Brewfolio.UnitTests` exercises domain and application behavior without infrastructure.
- `Brewfolio.ArchitectureTests` protects project dependency rules.

The backend test projects target .NET 11 Preview 7. Coffee Bean tests also verify both cases of the native C# 15 `Result<T>` union: successful Entity creation and structured validation failure.

Angular component tests stay close to their components under `src/frontend` and run through the Angular test command.

Run everything currently available:

```bash
dotnet test Brewfolio.sln
npm test --prefix src/frontend -- --watch=false
```

Introduce a backend integration-test project when a real infrastructure adapter exists. Prefer testing observable behavior over mirroring the implementation structure.
