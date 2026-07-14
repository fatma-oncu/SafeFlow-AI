# CODING_STANDARD.md — C# Coding Standards & Guidelines

This document outlines the coding standards, formatting guidelines, file organization, exception handling, and logging practices for the SafeFlow-AI C# codebase.

---

## 1. Naming Conventions

We adhere strictly to Microsoft's standard C# naming conventions:

*   **PascalCase** for:
    *   Classes, Records, Structs, Enums, Interfaces (`IUserRepository`, `UserRegisteredDomainEvent`)
    *   Methods (`ValidateCredentialsAsync`, `RecordFailedLogin`)
    *   Properties (`Id`, `Email`, `FailedLoginAttempts`)
    *   Public Fields and Constants (`public const int MaxFailedAccessAttempts = 5;`)
*   **camelCase** for:
    *   Method parameters (`email`, `password`, `tenantId`)
    *   Local variables (`currentUser`, `tokenHash`)
*   **camelCase with leading underscore** (`_`) for:
    *   Private or protected fields (`_userManager`, `_context`)
*   **Suffixes:**
    *   Interfaces must start with `I` (e.g., `IIdentityService`).
    *   Asynchronous methods must end with `Async` (e.g., `CreateUserAsync`).
    *   Exception classes must end with `Exception` (e.g., `DomainException`).
    *   FluentValidation classes must end with `Validator` (e.g., `RegisterCommandValidator`).
    *   Domain events must end with `DomainEvent` (e.g., `UserLockedDomainEvent`).

---

## 2. Code Style & Formatting

*   **Implicit vs. Explicit Typing:** Use `var` when the type is obvious from the right-hand side of the assignment (e.g., `var user = new User()`). Otherwise, use the explicit type (e.g., `int failedCount = GetFailedAttempts()`).
*   **Braces:** Braces must always start on a new line (Allman style). Single-line statements in `if` blocks must still use braces.
    ```csharp
    // Correct
    if (user.IsLocked)
    {
        throw new UserLockedException(user.Id);
    }

    // Incorrect
    if (user.IsLocked) throw new UserLockedException(user.Id);
    ```
*   **Nullable Reference Types:** Enable Nullable Reference Types globally (`<Nullable>enable</Nullable>` in `Directory.Build.props`). Avoid using null-forgiving operators (`!`) unless during JSON deserialization or EF Core configurations where initialization is guaranteed.
*   **Expressive Constructors:** Use C# secondary records and constructors to establish invariants. Properties should be `init` or read-only properties.

---

## 3. File & Namespace Organization

*   **One Type Per File:** Each C# file must contain only one class, interface, record, or enum.
*   **Namespace Matches Directory Structure:** The namespace of a C# file must strictly mirror its physical directory structure relative to the project root.
    *   Example: `src/SafeFlow.Domain/Identity/Entities/User.cs` has namespace `SafeFlow.Domain.Identity.Entities`.
*   **Organized Using Directives:** Group `using` statements at the top of the file:
    1.  System namespaces
    2.  Third-party libraries (e.g., MediatR, FluentValidation)
    3.  Internal namespaces (from inner layers to outer layers)
    *   Keep usings sorted alphabetically and remove unused ones.

---

## 4. Exception Handling Strategy

*   **Exceptions are for Exceptional Circumstances:** Do not use exceptions for expected validation errors or flow control. Use functional results (`Result` or `Result<T>`) for business logic failures.
*   **Domain Exceptions:** All exceptions thrown from the domain layer must inherit from a common `DomainException` base class.
*   **HTTP Mapping:** The API layer must catch exceptions via a global Exception Handling Middleware and convert them to standard RFC 7807 Problem Details response formats:
    *   `DomainException` → `400 Bad Request` or `422 Unprocessable Entity`
    *   `ValidationException` → `400 Bad Request` (with field-level validation errors)
    *   `KeyNotFoundException` → `404 Not Found`
    *   Uncaught system exceptions → `500 Internal Server Error` (with sanitized public message)

---

## 5. Logging Rules

*   **Structured Logging:** Always use structured logging with parameters rather than string interpolation:
    *   *Correct:* `_logger.LogWarning("Failed login attempt for user {Email}. Attempt {AttemptCount}", email, count);`
    *   *Incorrect:* `_logger.LogWarning($"Failed login attempt for user {email}. Attempt {count}");`
*   **No Sensitive Information in Logs:** Never log passwords, tokens, API secrets, or PII (Personally Identifiable Information) like full TC Kimlik Nos.
*   **Log Levels:**
    *   `Information`: High-level business milestones (e.g., User registered, Training completed).
    *   `Warning`: Unexpected events that do not interrupt flow (e.g., Validation failed, login credentials invalid).
    *   `Error`: Operations that failed but did not crash the system (e.g., Email failed to send, database query timed out).
    *   `Critical`: Disastrous failure requiring developer intervention (e.g., Out of memory, database connection lost).
