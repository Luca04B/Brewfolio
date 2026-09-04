# Brewfolio agent guide

Brewfolio is a local-first monorepo containing an ASP.NET Core backend and an Angular frontend.

## Context routing

- Read `CONTEXT.md` before naming or changing domain concepts.
- Read `docs/architecture.md` before changing project boundaries, dependencies, persistence, authentication, deployment, or cross-cutting concerns.
- Read `docs/design-patterns.md` before adding a shared abstraction, reusable module, interface, handler pipeline, or design pattern.
- Read `src/backend/README.md` before backend work.
- Read `src/frontend/README.md` before frontend work.
- Read `tests/README.md` before changing test strategy or project boundaries.
- Read `docs/roadmap.md` when prioritizing or scoping features.
- Read the relevant file in `docs/adr/` before revisiting a recorded decision.

## Agent skills

### Issue tracker

Issues and specs are tracked in GitHub Issues. See `docs/agents/issue-tracker.md`.

### Triage labels

Triage uses the five canonical Matt Pocock skill labels. See `docs/agents/triage-labels.md`.

### Domain docs

Brewfolio uses a single-context domain layout. See `docs/agents/domain.md`.

## Working agreements

- Keep the system runnable locally and keep cloud-provider concerns outside the application core.
- Implement business behavior as small vertical slices through domain, application, infrastructure, API, and UI only where each layer is needed.
- Preserve the dependency direction documented in `docs/architecture.md`.
- Make patterns earn their place through reuse, a real adapter seam, or concentrated behavior behind a small interface.
- Record resolved domain terminology in `CONTEXT.md`; keep implementation details out of that glossary.
- Add an ADR only for a consequential, difficult-to-reverse decision with a real trade-off.
- Keep secrets out of Git. Use `.env`, .NET user secrets, or environment variables locally.

## Completion checks

Run every check affected by the change:

```bash
dotnet build Brewfolio.sln
dotnet test Brewfolio.sln
npm run build --prefix src/frontend
npm test --prefix src/frontend -- --watch=false
npm run format:check --prefix src/frontend
```

Update the nearest README when a developer-facing command, boundary, or workflow changes.
