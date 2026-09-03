# Roadmap

The roadmap orders learning and delivery so each phase leaves Brewfolio usable. Dates and cloud vendors are intentionally absent.

## Phase 0 — Repository foundation

- monorepo and Visual Studio solution
- ASP.NET Core project boundaries
- Angular application
- backend and frontend test runners
- documentation and agent guidance
- optional Docker Compose platform

## Phase 1 — Coffee Bean inventory

- create a Coffee Bean with name, roaster, origin, roast level, price, and stock status
- list existing Coffee Beans in Angular
- introduce Entity Framework Core and persist Coffee Beans in SQL Server
- add the first migration and repeatable local database setup
- test domain behavior, the application use cases, and persistence

## Phase 2 — Recipes

- configure and activate or deactivate Brewing Methods
- create and list reusable Recipes
- reference a Brewing Method and capture quantities, temperature, grind size, and target brew time
- decide whether a Recipe belongs to one Coffee Bean or can be used with several

## Phase 3 — Coffee Brews

- record a Coffee Brew using a Coffee Bean and Recipe
- capture the brew timestamp, rating, and notes
- show brewing history and Coffee Bean details

## Phase 4 — Local platform

- make API, frontend, and database work together through Docker Compose
- add health checks for real dependencies
- introduce structured logging and OpenTelemetry
- evaluate a local OpenID Connect provider when authenticated behavior exists

## Phase 5 — Authentication

- introduce Keycloak when Coffee Beans, Recipes, and Coffee Brews need ownership
- protect Angular routes and API endpoints
- associate user-owned data with the authenticated user

## Phase 6 — Continuous integration

- build backend and frontend on relevant pushes
- run unit, architecture, integration, and frontend tests
- build container images
- protect the main branch with passing checks

## Phase 7 — Optional deployment

- select hosting from explicit requirements such as cost, container support, persistence, security, and observability
- add a container registry and deployment pipeline
- keep provider-specific code at the infrastructure boundary

## Deferred ideas

Microservices, event-driven communication, managed identity, managed databases, and LLM/MCP integrations are possible learning goals. They become roadmap work only when a concrete user scenario justifies their complexity.
