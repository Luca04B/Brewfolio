# Backend

The Brewfolio backend is an ASP.NET Core modular monolith organized around Clean Architecture dependency rules. It currently targets .NET 11 Preview 7 and enables C# 15 preview features so native union types can be evaluated in a real vertical slice.

## Projects

- `Brewfolio.Domain`: business concepts and rules; no dependency on another Brewfolio project
- `Brewfolio.Application`: use cases and ports; depends on Domain
- `Brewfolio.Infrastructure`: persistence and external adapters; depends on Application and Domain
- `Brewfolio.Api`: HTTP boundary and composition root; depends on Application and Infrastructure

Add code for a feature only to the projects it needs. The API should translate HTTP concerns, the Application project should coordinate the use case, and the Domain project should contain rules that remain true without HTTP or database technology.

## Commands

```bash
dotnet build Brewfolio.sln
dotnet run --project src/backend/Brewfolio.Api
dotnet test Brewfolio.sln
```

The development API listens on <http://localhost:5199>. Verify it at `/api` and `/health`.

Persistence, authentication, and observability packages will be added with the first feature that requires them rather than as unused framework setup.

## Native Result experiment

`CoffeeBean.Create` returns the native union `Result<CoffeeBean>`, whose cases are `CoffeeBean` and `Error`. Expected validation failures are returned as structured errors; exceptions remain reserved for unexpected failures and broken invariants. Callers inspect the union through pattern matching or its generated `Value` property.

The native `union` syntax is a C# 15 preview feature. The repository therefore requires the exact prerelease SDK pinned in `global.json`; both the language and generated union behavior may change before .NET 11 is released.
