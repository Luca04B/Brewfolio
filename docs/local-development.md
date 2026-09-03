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
dotnet run --project src/backend/Brewfolio.Api
```

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
```

## Containers

Create a local environment file and choose a strong development-only SQL Server password:

```bash
cp .env.example .env
docker compose up --build
```

Services:

| Service | Local address |
| --- | --- |
| Angular frontend | <http://localhost:4200> |
| ASP.NET Core API | <http://localhost:8080/api> |
| Health check | <http://localhost:8080/health> |
| SQL Server | `localhost,1433` |

On ARM-based Macs, the SQL Server container uses the `linux/amd64` platform and therefore runs through Docker's architecture emulation.

Stop the stack with `docker compose down`. Add `--volumes` only when you intentionally want to delete the local database volume.

## Secrets

- Keep Docker values in the ignored `.env` file.
- Use .NET user secrets or environment variables for native backend development.
- Commit placeholders and configuration keys, never real credentials.
