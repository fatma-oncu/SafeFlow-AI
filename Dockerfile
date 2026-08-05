# ── Stage 1: Runtime Base ────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ── Stage 2: SDK Build & Restore ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy CPM properties and global.json first for Central Package Management
COPY ["Directory.Packages.props", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["global.json", "./"]

# Copy project files for caching dependency restore layer
COPY ["src/SafeFlow.SharedKernel/SafeFlow.SharedKernel.csproj", "src/SafeFlow.SharedKernel/"]
COPY ["src/SafeFlow.Domain/SafeFlow.Domain.csproj", "src/SafeFlow.Domain/"]
COPY ["src/SafeFlow.Application/SafeFlow.Application.csproj", "src/SafeFlow.Application/"]
COPY ["src/SafeFlow.Infrastructure/SafeFlow.Infrastructure.csproj", "src/SafeFlow.Infrastructure/"]
COPY ["src/SafeFlow.API/SafeFlow.API.csproj", "src/SafeFlow.API/"]

RUN dotnet restore "src/SafeFlow.API/SafeFlow.API.csproj"

# Copy full source tree and build
COPY . .
WORKDIR "/src/src/SafeFlow.API"
RUN dotnet build "SafeFlow.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ── Stage 3: Publish API ──────────────────────────────────────────────────────
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SafeFlow.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ── Stage 4: Build Health Check Tool ─────────────────────────────────────────
# Compiles a tiny HttpClient-based health check binary using the SDK.
# The resulting DLL is copied to the runtime image so the HEALTHCHECK CMD can
# invoke it via `dotnet`, which is already present in mcr.microsoft.com/dotnet/aspnet.
# No wget, curl, or any other external tool is installed.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS healthcheck-build
WORKDIR /healthcheck
COPY ["tools/HealthCheck/HealthCheck.csproj", "./"]
RUN dotnet restore "HealthCheck.csproj"
COPY ["tools/HealthCheck/Program.cs", "./"]
RUN dotnet publish "HealthCheck.csproj" -c Release -o /healthcheck/publish --self-contained false

# ── Stage 5: Production Runtime Image ────────────────────────────────────────
FROM base AS final
WORKDIR /app

# API binaries
COPY --from=publish /app/publish .

# Health check binary (no extra OS packages required)
COPY --from=healthcheck-build /healthcheck/publish /healthcheck/

ENTRYPOINT ["dotnet", "SafeFlow.API.dll"]
