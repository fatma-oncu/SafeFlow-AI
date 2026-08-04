# ── Stage 1: Runtime Base ───────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ── Stage 2: SDK Build & Publish ────────────────────────────────────────────
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

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SafeFlow.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ── Stage 3: Production Runtime Image ───────────────────────────────────────
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SafeFlow.API.dll"]
