# Frontend

The Brewfolio frontend is an Angular application using TypeScript and SCSS. It is part of the repository monorepo but keeps its own npm toolchain and lockfile.

## Commands

Run commands from the repository root:

```bash
npm install --prefix src/frontend
npm start --prefix src/frontend
npm run build --prefix src/frontend
npm test --prefix src/frontend -- --watch=false
npm run test:e2e --prefix src/frontend
npm run format:check --prefix src/frontend
```

The development server runs at <http://localhost:4200>. `proxy.conf.json` forwards `/api` and `/health` to the ASP.NET Core API at <http://localhost:5199>, so application code can use relative URLs.

The Playwright happy path expects the complete Docker stack at <http://localhost:4200>. Install its Chromium browser once with `npx --prefix src/frontend playwright install chromium`.

## Structure

Keep feature code together under `src/app`. A feature should own its components, routes, state, and API access. Introduce shared UI or utilities only after multiple features need the same abstraction.

Global styling belongs in `src/styles.scss`; component-specific styling stays next to the component. Prefer standalone Angular components and keep domain rules in the backend Domain project rather than reproducing them in the browser.
