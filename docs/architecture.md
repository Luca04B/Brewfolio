# Architecture

Brewfolio begins as a local-first modular monolith with a separate browser client. Backend and frontend live in one monorepo and can evolve together in a single change.

## System shape

```text
Angular frontend
       |
       | HTTP/JSON
       v
ASP.NET Core API
       |
       v
Application use cases
       |
       v
Domain model

Infrastructure adapters ---> Application and Domain
       |
       v
SQL Server
```

The frontend communicates with the backend through HTTP. During native development, Angular proxies `/api` and `/health` to the local API. In Docker Compose, nginx proxies the same paths to the API container.

## Backend projects

| Project | Responsibility | May depend on |
| --- | --- | --- |
| `Brewfolio.Domain` | Business language, entities, value objects, and domain rules | No other Brewfolio project |
| `Brewfolio.Application` | Use cases and ports required by those use cases | Domain |
| `Brewfolio.Infrastructure` | Database and external-system adapters | Application, Domain |
| `Brewfolio.Api` | HTTP endpoints, composition root, and runtime configuration | Application, Infrastructure |

Architecture tests protect this dependency direction. Features should be added as vertical slices; a layer receives a file only when the feature actually needs that layer.

Pattern and reuse decisions follow [Design patterns and reuse](design-patterns.md). Brewfolio favors deep modules and concrete feature code over speculative generic abstractions.

## Frontend

The Angular application owns browser presentation, navigation, and client-side interaction. Feature code should be grouped by user capability rather than by technical file type. Shared UI or utilities earn a shared location only after at least two features need them.

## Data and authentication

SQL Server is the initial local database choice. Persistence has not yet been wired into the starter API; it will enter with the first feature that needs durable data.

Authentication is intentionally deferred. OAuth 2.0, OpenID Connect, JWT-based API authorization, and a locally hosted identity provider such as Keycloak remain candidates rather than current dependencies.

## Deployment

The first operating environment is the developer machine. Docker Compose provides an optional local platform containing the frontend, API, and SQL Server.

Cloud hosting, managed databases, and CI/CD are later infrastructure decisions. Application code must not assume a specific cloud provider.

## Observability

ASP.NET Core health checks are available from the start. Structured logging and OpenTelemetry will be introduced when there is meaningful application behavior to observe.
