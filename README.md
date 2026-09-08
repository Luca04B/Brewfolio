# Brewfolio

Brewfolio is a local-first full-stack portfolio and learning project. The repository keeps the ASP.NET Core backend, Angular frontend, tests, infrastructure, and documentation together in one monorepo.

The application starts locally. A future cloud deployment is deliberately provider-neutral and will only be introduced when the project needs it.

## Current status

The repository foundation and a working Coffee Bean inventory vertical slice are available:

- ASP.NET Core API on .NET 11 Preview 7 with C# 15 preview features
- Angular frontend with TypeScript and SCSS
- backend unit, architecture, and SQL/API integration test projects
- local container setup for the API, frontend, SQL Server, and private MinIO object storage
- architecture, roadmap, domain language, and decision records
- agent guidance for Codex and repository-local skills
- reusable Coffee Bean products with Coffee Bags, images, search, filters, and lifecycle actions

The current API exposes `/api`, `/health`, and the Coffee Bean resources below `/api/coffee-beans`.

## Repository map

```text
Brewfolio/
├── Brewfolio.sln
├── AGENTS.md
├── CONTEXT.md
├── compose.yaml
├── docs/
│   ├── adr/
│   ├── architecture.md
│   ├── local-development.md
│   └── roadmap.md
├── src/
│   ├── backend/
│   │   ├── Brewfolio.Api/
│   │   ├── Brewfolio.Application/
│   │   ├── Brewfolio.Domain/
│   │   └── Brewfolio.Infrastructure/
│   └── frontend/
└── tests/
    └── backend/
        ├── Brewfolio.ArchitectureTests/
        ├── Brewfolio.IntegrationTests/
        └── Brewfolio.UnitTests/
```

## Run locally

Requirements:

- .NET SDK `11.0.100-preview.7.26381.103`
- Node.js 24 and npm 11
- Docker Desktop only if you want to run the container stack

Start the backend:

```bash
dotnet restore Brewfolio.sln
dotnet user-secrets --project src/backend/Brewfolio.Api set \
  "ConnectionStrings:Brewfolio" \
  "Server=localhost,1433;Database=Brewfolio;User Id=sa;Password=<your-local-password>;Encrypt=True;TrustServerCertificate=True"
dotnet run --project src/backend/Brewfolio.Api
```

Start the frontend in a second terminal:

```bash
npm install --prefix src/frontend
npm start --prefix src/frontend
```

Open the frontend at <http://localhost:4200>. The API is available at <http://localhost:5199/api> and its health endpoint at <http://localhost:5199/health>.

For the fully containerized local setup:

```bash
cp .env.example .env
docker compose up --build
```

The containerized frontend is then available at <http://localhost:4200> and proxies `/api` to the API container. MinIO remains private to the backend; its local administration console is available at <http://localhost:9001>.

## Preview experiment

The backend intentionally targets .NET 11 Preview 7 and enables the C# preview language version. Domain operations use native C# 15 union-based results to represent successful values, structured validation failures, and categorized application errors. This exercises implicit union conversions and exhaustive pattern matching without a third-party result library.

.NET 11 and native C# unions are prerelease technology and are not supported for production use yet. The exact SDK is pinned in `global.json`; install that SDK before running native backend commands. The container workflow pins the corresponding Preview 7 SDK and runtime images.

## Documentation

- [Documentation index](docs/README.md)
- [Architecture](docs/architecture.md)
- [Domain model](docs/domain-model.md)
- [Design patterns and reuse](docs/design-patterns.md)
- [Local development](docs/local-development.md)
- [Roadmap](docs/roadmap.md)
- [Domain language](CONTEXT.md)
- [Architecture decisions](docs/adr/README.md)

## Working with coding agents

Repository-aware agents start with [AGENTS.md](AGENTS.md). It points them to the smallest relevant source of truth for each task. Repository-specific skills live under `.agents/skills` without copying architecture or domain knowledge into every skill.
