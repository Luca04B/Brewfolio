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

- create a reusable Coffee Bean product with name and roaster
- list existing Coffee Beans in Angular
- introduce Entity Framework Core and persist Coffee Beans in SQL Server
- add the first migration and repeatable local database setup
- test domain behavior, the application use cases, and persistence
- add origin, roast profile, product URL, and image as product metadata
- add Coffee Bags for purchase price, purchase and roast dates, weight, and stock status
- search, filter, sort, edit, duplicate, and delete the Coffee Bean collection
- replace and remove private product images with resilient object cleanup

## Phase 2 — Recipes

- configure and activate or deactivate Brewing Methods
- create and list reusable Recipes
- reference a Brewing Method and capture quantities, temperature, grind size, and target brew time
- keep Recipes independent of a particular Coffee Bean so they can be reused with several

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
- associate Member-owned data with the authenticated Member

## Phase 6 — Community Recipes

- publish Recipes for other Members to discover and prepare
- browse Published Recipes in a feed
- derive Recipe Scores from eligible Coffee Brew ratings
- rank Published Recipes using rules designed and tested with this vertical slice

## Phase 7 — Continuous integration

- build backend and frontend on relevant pushes
- run unit, architecture, integration, and frontend tests
- build container images
- protect the main branch with passing checks

## Phase 8 — Optional deployment

- select hosting from explicit requirements such as cost, container support, persistence, security, and observability
- add a container registry and deployment pipeline
- keep provider-specific code at the infrastructure boundary

## Deferred ideas

Microservices, event-driven communication, managed identity, managed databases, and LLM/MCP integrations are possible learning goals. They become roadmap work only when a concrete user scenario justifies their complexity.

Documenting and hardening access from a phone on the same local network is also deferred. It requires explicit host binding, LAN-aware URLs, firewall rules, and matching CORS/proxy configuration.
