# CLAUDE.md — SafeFlow-AI Development Guide

Welcome to the SafeFlow-AI project! This document outlines development commands, architectural rules, and quick-start instructions for developers and AI assistants.

---

## Build & Test Commands

Use these commands to build, test, and run the project from the root directory:

### Backend (.NET 9)
*   **Build the solution:**
    ```powershell
    dotnet build SafeFlow.sln
    ```
*   **Run all tests:**
    ```powershell
    dotnet test SafeFlow.sln
    ```
*   **Run a specific test project:**
    ```powershell
    dotnet test tests/SafeFlow.Domain.Tests/SafeFlow.Domain.Tests.csproj
    ```
*   **Run the Web API:**
    ```powershell
    dotnet run --project src/SafeFlow.API/SafeFlow.API.csproj
    ```
*   **Add an EF Core Migration (from API directory, targeting Infrastructure):**
    ```powershell
    dotnet ef migrations add <MigrationName> --project src/SafeFlow.Infrastructure/SafeFlow.Infrastructure.csproj --startup-project src/SafeFlow.API/SafeFlow.API.csproj --context SafeFlowDbContext
    ```
*   **Update the Database:**
    ```powershell
    dotnet ef database update --project src/SafeFlow.Infrastructure/SafeFlow.Infrastructure.csproj --startup-project src/SafeFlow.API/SafeFlow.API.csproj --context SafeFlowDbContext
    ```

### Docker (Development Environment)
*   **Start SQL Server 2022:**
    ```powershell
    docker-compose up -d sqlserver
    ```

---

## Project Structure Overview

SafeFlow-AI follows **Clean Architecture** with **Domain-Driven Design (DDD)** principles:

```
SafeFlow-AI/
├── src/
│   ├── SafeFlow.Domain/         # Core business logic (entities, VOs, events, interface contracts)
│   ├── SafeFlow.Application/    # Use cases, CQRS commands/queries, MediatR, validation, DTOs
│   ├── SafeFlow.Infrastructure/ # Database persistence (EF Core), Auth (Identity), Email, JWT, RSA keys
│   └── SafeFlow.API/            # Web API endpoints, Middleware, Controllers, Swagger, Configuration
├── tests/
│   ├── SafeFlow.Domain.Tests/
│   ├── SafeFlow.Application.Tests/
│   ├── SafeFlow.Infrastructure.Tests/
│   └── SafeFlow.API.Tests/
└── docs/                        # Project documentation and architectural specs
```

---

## Coding Rules & Context Reference

1.  **Language:**
    *   Code (classes, variables, methods, schemas) is entirely in **English**.
    *   Validation messages and UI text target **Turkish** (İSG local market context).
    *   Audit logs and exceptions are in **English**.
2.  **Architecture:**
    *   **Domain** layer must have **zero external dependencies** (except lightweight MediatR.Contracts). No references to EF Core or Microsoft Identity.
    *   **Generic Repositories** are strictly forbidden. Create feature-specific repositories (e.g., `IUserRepository`, `ITrainingRepository`).
    *   Use **Value Objects** for domain primitives (e.g., `Email`, `FullName`, `PhoneNumber`) rather than plain strings.
    *   Domain events must follow the `*DomainEvent` naming convention (e.g., `UserRegisteredDomainEvent`).
3.  **Command Execution:**
    *   Always verify solution integrity by running `dotnet build` and `dotnet test` after making changes.
4.  **Assumptions & Preferences:**
    *   Platform targets **.NET 9** and **SQL Server 2022**.
    *   Identity uses **Microsoft Identity** bridged to the Domain layer without leaking Identity packages into the Domain.
    *   Security: RS256 for JWT access tokens. SHA256 hashed refresh tokens stored in a separate table using Token Family Rotation.
    *   Multi-tenant model: 1 Tenant = 1 Company for MVP. Separated via EF Core Global Query Filter on `ITenantEntity`.
