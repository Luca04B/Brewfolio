# Keep backend and frontend in one monorepo

Brewfolio keeps its ASP.NET Core backend, Angular frontend, tests, infrastructure, and documentation in one repository. A feature can therefore change its API contract and user interface atomically, and project-wide tooling has one source of truth; the trade-off is that boundaries must be maintained deliberately as the repository grows.

