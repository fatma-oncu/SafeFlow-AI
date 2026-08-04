# SafeFlow-AI — AI-Powered Occupational Health & Safety Operations Platform

SafeFlow-AI is an enterprise-grade, modular Occupational Health & Safety (İSG) platform designed for multi-tenant organizations and Joint Health and Safety Units (OSGB). It automates employee tracking, hybrid training programs, certification validity, and site risk inspections utilizing AI-driven decision support models.

---

## 1. Technology Stack

*   **Runtime & Framework:** .NET 9 / ASP.NET Core 9
*   **Database Engine:** SQL Server 2022
*   **ORM:** Entity Framework Core 9 (EF Core)
*   **Authentication & Identity:** Microsoft ASP.NET Core Identity
*   **Architecture Pattern:** Clean Architecture, Domain-Driven Design (DDD), CQRS (MediatR)
*   **Input Validation:** FluentValidation
*   **Background Jobs:** Hangfire (using SQL Server storage)
*   **Mobile App:** Flutter (utilizing SQLite/Drift for offline synchronization)

---

## 2. Solution Structure

The project conforms to Clean Architecture design patterns:

```
SafeFlow.sln
├── Directory.Build.props        # Centralized build and compiler configurations
├── Directory.Packages.props     # Central Package Management (CPM) versions
├── global.json                  # Global .NET SDK target settings
├── src/
│   ├── SafeFlow.SharedKernel/   # Low-level core helpers (Entity, Result, ValueObject, PagedResult)
│   ├── SafeFlow.Domain/         # Domain aggregates, entities, value objects, events, and repo interfaces
│   ├── SafeFlow.Application/    # Use cases, MediatR command/query handlers, validation, DTOs, interfaces
│   ├── SafeFlow.Infrastructure/ # Database persistence (EF Core), authentication services, JWT management
│   └── SafeFlow.API/            # HTTP Controllers, API routing, middlewares, configurations, entry point
└── tests/
    ├── SafeFlow.Domain.Tests/   # Unit tests for Domain aggregates and business invariants
    ├── SafeFlow.Application.Tests/ # Unit tests for Application CQRS command/query logic
    └── SafeFlow.IntegrationTests/# Integration tests for API endpoints and database logic
```

---

## 3. Development Prerequisites & Environment

To develop, build, and run the backend solution, ensure you have:

1.  **SDK:** [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (or higher) installed on your system.
2.  **Container Runtime:** [Docker Desktop](https://www.docker.com/products/docker-desktop/) or Docker Engine supporting Docker Compose v2.
3.  **IDE:** Visual Studio 2022 (v17.12+), JetBrains Rider, or VS Code with C# Dev Kit.

---

## 4. Environment Configuration & Docker Setup

Before starting the containers or running the Web API:

1.  **Environment File (.env):**
    Copy `.env.example` to `.env` in the repository root and fill in your real values:
    ```powershell
    Copy-Item .env.example .env
    ```
    > ⚠️ **Never commit `.env` to Git.** It contains secrets and is excluded via `.gitignore`.
    > Open `.env.example` for a description of every required variable.

2.  **Start Docker Infrastructure:**
    Start SQL Server 2022 and SafeFlow API in detached mode:
    ```powershell
    docker compose up -d
    ```
    *This starts `safeflow-sqlserver` on port `1433` with health checks, and `safeflow-api` on port `5000`.*

3.  **View Container Logs:**
    ```powershell
    docker compose logs -f
    ```

4.  **Stop Docker Infrastructure:**
    ```powershell
    docker compose down
    ```

5.  **Swagger UI (OpenAPI):**
    Once the API is running, access the interactive documentation at:
    ```
    http://localhost:5000/swagger
    ```

6.  **Health Endpoint:**
    ```
    http://localhost:5000/health
    ```

### Local Development (`dotnet run`) — User Secrets

When running outside Docker, use [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) instead of `.env`:

```powershell
# From the repository root
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=SafeFlowDb;User Id=sa;Password=<your-sa-password>;TrustServerCertificate=True" --project src/SafeFlow.API
dotnet user-secrets set "SeedSettings:AdminEmail" "admin@example.com" --project src/SafeFlow.API
dotnet user-secrets set "SeedSettings:AdminPassword" "<YourStrongPassword>" --project src/SafeFlow.API
dotnet user-secrets set "JwtSettings:RsaPrivateKeyPem" "$(Get-Content private.pem -Raw)" --project src/SafeFlow.API
dotnet user-secrets set "JwtSettings:RsaPublicKeyPem" "$(Get-Content public.pem -Raw)" --project src/SafeFlow.API
```

> User Secrets are stored in your user profile (`%APPDATA%\Microsoft\UserSecrets`) and are **never committed to Git**.

---

## 5. Build, Database Migration & Run Instructions

Execute the following commands from the repository root:

*   **Build the Solution:**
    ```powershell
    dotnet build SafeFlow.sln
    ```
*   **Execute Test Suites:**
    ```powershell
    dotnet test SafeFlow.sln
    ```
*   **Apply EF Core Migrations (Host Execution):**
    ```powershell
    dotnet ef database update --project src/SafeFlow.Infrastructure/SafeFlow.Infrastructure.csproj --startup-project src/SafeFlow.API/SafeFlow.API.csproj
    ```
*   **Run the Web API Application (Host Execution):**
    ```powershell
    dotnet run --project src/SafeFlow.API/SafeFlow.API.csproj
    ```

---

## 6. Documentation

For detailed specifications, refer to the documentation in the `/docs` folder:
*   [Product Vision](docs/product-vision.md)
*   [C4 Architecture Diagrams](docs/c4-architecture.md)
*   [Domain Bounded Contexts Map](docs/domain-model.md)
*   [API Resource Contracts Specification](docs/api-specification.md)
*   [Error Handling & Validation Guidelines](docs/error-handling-strategy.md)
