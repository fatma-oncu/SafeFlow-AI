# ARCHITECTURE_RULES.md — Clean Architecture & DDD Guidelines

This document details the architectural boundaries, layer dependency rules, CQRS + MediatR conventions, and Domain-Driven Design (DDD) patterns for the SafeFlow-AI project.

---

## 1. Clean Architecture Layers

The solution is divided into four distinct project layers. Code must flow strictly from the outside in: **API → Infrastructure → Application → Domain**. Reverse references are strictly forbidden.

```
+-------------------------------------------------------------+
|                          API Layer                          |
|             (Controllers, Middlewares, Program.cs)          |
+------------------------------+------------------------------+
                               |
                               v
+-------------------------------------------------------------+
|                     Infrastructure Layer                    |
|       (EF Core, Identity, JWT Token, SMTP Mail, Keys)       |
+------------------------------+------------------------------+
                               |
                               v
+-------------------------------------------------------------+
|                      Application Layer                      |
|       (CQRS Handlers, Behaviors, Validation, Interfaces)    |
+------------------------------+------------------------------+
                               |
                               v
+-------------------------------------------------------------+
|                         Domain Layer                        |
|   (Aggregates, Entities, Value Objects, Domain Events, VOs) |
+-------------------------------------------------------------+
```

### 1.1 Domain Layer
*   Contains the core business model.
*   **Dependencies:** ZERO external dependencies (except MediatR.Contracts for event interfaces). No EF Core, no Microsoft Identity, no HTTP context.
*   Houses: Aggregates, Entities, Value Objects, Domain Events, Domain Exceptions, and repository interface definitions.

### 1.2 Application Layer
*   Contains the application logic and use cases.
*   **Dependencies:** Relies ONLY on the Domain layer.
*   Houses: CQRS commands/queries, MediatR handlers, FluentValidation rules, Application interface contracts (e.g., `IJwtTokenService`, `IEmailService`), and DTOs.
*   Contains the **Unit of Work** boundary: the handler is the transaction boundary.

### 1.3 Infrastructure Layer
*   Contains concrete implementations of the application interface contracts.
*   **Dependencies:** Relies on Application and Domain layers.
*   Houses: DbContext, EF Core mappings, Microsoft Identity integration (`ApplicationUser`, `UserManager`), JWT creation services, email clients, and hardware/key stores.

### 1.4 API Layer (Presentation)
*   The entry point of the application.
*   **Dependencies:** References all three underlying projects (API relies on Infrastructure for Dependency Injection setup in `Program.cs`).
*   Houses: HTTP Controllers, API routing, middlewares (rate limiting, logging, exception parsing), Swagger configuration, and appsettings files.

---

## 2. Domain-Driven Design (DDD) Patterns

*   **Aggregates & Root Entities:**
    *   Aggregates are the transactional consistency boundaries.
    *   Only Aggregate Roots can have repositories. Internal entities (e.g., `Department` inside `Company`) must be retrieved through their parent Aggregate Root.
    *   Aggregates must protect their internal state. All state modifications must go through public methods on the Aggregate Root that validate business rules. Public setters are forbidden.
*   **Value Objects:**
    *   Must be immutable records (`public sealed record Email(...)`).
    *   Used to validate and wrap business rules on primitives (e.g., checking email format, normalizing telephone prefixes).
*   **Pragmatic Isolation (ADR-001):**
    *   Do not mix domain entities with infrastructure details.
    *   Microsoft Identity uses a separate database table (`AspNetUsers`) mapped to `ApplicationUser` in Infrastructure.
    *   Domain layer has a pure `User` aggregate. The bridge is created in Infrastructure (`IdentityService.cs`).

---

## 3. CQRS & MediatR Rules

*   **Command vs. Query Separation:**
    *   **Commands:** Change state, return a result containing identifiers or a success indicator (`ICommand<TResponse>` or `ICommand`).
    *   **Queries:** Read state, return DTOs/Responses (`IQuery<TResponse>`). Queries must be side-effect free and should use EF Core `AsNoTracking()` for performance.
*   **Validation Pipeline:**
    *   Use MediatR Pipeline Behaviors to automatically validate incoming commands with FluentValidation before reaching the handler. Do not perform manual validation inside handlers.
*   **Transactional Boundaries:**
    *   Every command handler must save changes through the Unit of Work (`IUnitOfWork`) as the final step. Do not call `SaveChanges` directly inside the handler; delegate it to the wrapper pipeline or save cleanly at the handler's end.

---

## 4. Multi-Tenant Data Isolation

*   **MVP Model:** 1 Tenant = 1 Company.
*   **Entity Marking:** Every tenant-specific database entity must implement the `ITenantEntity` interface:
    ```csharp
    public interface ITenantEntity
    {
        Guid TenantId { get; }
    }
    ```
*   **Automatic Filtering:** The `SafeFlowDbContext` must register a Global Query Filter for all entities implementing `ITenantEntity`, binding them to the current Tenant ID resolved from the `ITenantService`.
*   **Safe Insertion:** Register an EF Core Interceptor or override `SaveChangesAsync` to automatically populate the `TenantId` field from the current context upon saving new entries.
