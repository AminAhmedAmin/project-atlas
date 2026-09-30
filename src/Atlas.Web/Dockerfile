# Build from the repository root:
#   docker build -f src/Atlas.Web/Dockerfile -t atlas-web .

# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore needs the full source: the SDK only adds the Blazor framework assets
# (blazor.web.js) when it sees Razor components during restore.
COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/ src/
RUN dotnet restore src/Atlas.Web/Atlas.Web.csproj
RUN dotnet publish src/Atlas.Web/Atlas.Web.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Uploads and Data Protection keys live here; mount a volume to persist them.
RUN mkdir -p /app/App_Data/uploads /app/App_Data/keys && chown -R $APP_UID /app/App_Data
VOLUME /app/App_Data

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DataProtection__KeysPath=/app/App_Data/keys \
    FileStorage__RootPath=/app/App_Data/uploads \
    DOTNET_RUNNING_IN_CONTAINER=true

COPY --from=build /app/publish .

USER $APP_UID
EXPOSE 8080

# The runtime image has no curl; point your orchestrator's probe (e.g. Azure App Service
# Health check) at GET /health (readiness, includes the database) or /health/live.
ENTRYPOINT ["dotnet", "Atlas.Web.dll"]
