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

## 3. Development Prerequisites

To develop, build, and run the backend solution, ensure you have:

1.  **SDK:** [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (or higher) installed on your system.
2.  **IDE:** Visual Studio 2022 (v17.12+), JetBrains Rider, or VS Code with C# Dev Kit.
3.  **Database:** A local SQL Server instance or a Docker engine to run SQL Server 2022.

---

## 4. Build and Run Instructions

Execute the following commands from the repository root:

*   **Build the Solution:**
    ```powershell
    dotnet build SafeFlow.sln
    ```
*   **Execute Test Suites:**
    ```powershell
    dotnet test SafeFlow.sln
    ```
*   **Run the Web API Application:**
    ```powershell
    dotnet run --project src/SafeFlow.API/SafeFlow.API.csproj
    ```

---

## 5. Documentation

For detailed specifications, refer to the documentation in the `/docs` folder:
*   [Product Vision](file:///c:/Users/drmus/Desktop/SafeFlow-AI/docs/product-vision.md)
*   [C4 Architecture Diagrams](file:///c:/Users/drmus/Desktop/SafeFlow-AI/docs/c4-architecture.md)
*   [Domain Bounded Contexts Map](file:///c:/Users/drmus/Desktop/SafeFlow-AI/docs/domain-model.md)
*   [API Resource Contracts Specification](file:///c:/Users/drmus/Desktop/SafeFlow-AI/docs/api-specification.md)
*   [Error Handling & Validation Guidelines](file:///c:/Users/drmus/Desktop/SafeFlow-AI/docs/error-handling-strategy.md)
