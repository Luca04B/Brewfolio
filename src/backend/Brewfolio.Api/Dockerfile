FROM mcr.microsoft.com/dotnet/sdk:11.0.100-preview.7 AS build
WORKDIR /source

COPY ["src/backend/Brewfolio.Api/Brewfolio.Api.csproj", "src/backend/Brewfolio.Api/"]
COPY ["src/backend/Brewfolio.Application/Brewfolio.Application.csproj", "src/backend/Brewfolio.Application/"]
COPY ["src/backend/Brewfolio.Domain/Brewfolio.Domain.csproj", "src/backend/Brewfolio.Domain/"]
COPY ["src/backend/Brewfolio.Infrastructure/Brewfolio.Infrastructure.csproj", "src/backend/Brewfolio.Infrastructure/"]
RUN dotnet restore "src/backend/Brewfolio.Api/Brewfolio.Api.csproj"

COPY . .
RUN dotnet publish "src/backend/Brewfolio.Api/Brewfolio.Api.csproj" --configuration Release --output /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:11.0.0-preview.7 AS final
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Brewfolio.Api.dll"]
