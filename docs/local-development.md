# Local development

## Prerequisites

- .NET SDK `11.0.100-preview.7.26381.103`
- Node.js 24 with npm 11
- Docker Desktop for the optional container workflow

The repository pins its prerelease .NET SDK in `global.json`, allows prerelease SDK resolution, and enables the C# preview language version in `Directory.Build.props`. Install the exact .NET 11 Preview 7 SDK from the [.NET 11 download page](https://dotnet.microsoft.com/download/dotnet/11.0) before running native backend commands. The Angular toolchain remains pinned in `src/frontend/package-lock.json`.

The .NET 11/C# 15 setup is an explicit learning experiment. Preview SDKs and native union syntax can change before release and should not be treated as a production baseline.

## Visual Studio

Open `Brewfolio.sln` to work with the backend and backend tests. Keep the repository root open in Solution Explorer or a second editor window when working on the Angular application, which remains an independent npm project under `src/frontend`.

Choose `Brewfolio.Api` as the startup project. The HTTP launch profile uses <http://localhost:5199>.

## Native development

Restore, build, and run the backend:

```bash
dotnet restore Brewfolio.sln
dotnet build Brewfolio.sln
dotnet user-secrets --project src/backend/Brewfolio.Api set \
  "ConnectionStrings:Brewfolio" \
  "Server=localhost,1433;Database=Brewfolio;User Id=sa;Password=<your-local-password>;Encrypt=True;TrustServerCertificate=True"
dotnet run --project src/backend/Brewfolio.Api
```

The native API requires a reachable SQL Server. Image uploads additionally require MinIO. Start both dependencies with
`docker compose up -d sqlserver minio`, then provide the `ObjectStorage__AccessKey` and `ObjectStorage__SecretKey` values from your ignored `.env` file to the API process. In Development, the API applies pending EF Core migrations on
startup.

Install and run the frontend in a second terminal:

```bash
npm install --prefix src/frontend
npm start --prefix src/frontend
```

Angular runs at <http://localhost:4200>. Requests to `/api` and `/health` are proxied to the backend at <http://localhost:5199>.

## Tests

```bash
dotnet test Brewfolio.sln
npm test --prefix src/frontend -- --watch=false
npm run test:e2e --prefix src/frontend
```

The backend integration project starts a disposable SQL Server through Testcontainers, so Docker
must be running for the complete .NET test command.

## Containers

Create a local environment file and choose a strong development-only SQL Server password:

```bash
cp .env.example .env
docker compose up --build
```

Services:

| Service          | Local address                  |
| ---------------- | ------------------------------ |
| Angular frontend | <http://localhost:4200>        |
| ASP.NET Core API | <http://localhost:8080/api>    |
| Health check     | <http://localhost:8080/health> |
| SQL Server       | `localhost,1433`               |
| MinIO S3 API     | <http://localhost:9000>        |
| MinIO console    | <http://localhost:9001>        |

On ARM-based Macs, the SQL Server container uses the `linux/amd64` platform and therefore runs through Docker's architecture emulation.

Stop the stack with `docker compose down`. Add `--volumes` only when you intentionally want to delete the local database volume.

## Secrets

- Keep Docker values in the ignored `.env` file.
- Use .NET user secrets or environment variables for native backend development.
- Commit placeholders and configuration keys, never real credentials.

## Browser end-to-end test

Install the Chromium test browser once with `npx --prefix src/frontend playwright install chromium`. With the Docker stack running, `npm run test:e2e --prefix src/frontend` exercises the Coffee Bean create, bag, and collection search flow in desktop and mobile viewports.

Access from a phone on the same LAN is intentionally tracked as a separate follow-up. It needs explicit host binding, firewall and CORS/proxy configuration, and the computer's LAN address instead of `localhost`.
