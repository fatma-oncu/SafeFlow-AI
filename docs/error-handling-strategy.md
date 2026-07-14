# Error Handling & Validation Strategy

This document outlines the standard patterns for error representation, validation, domain exception hierarchies, and HTTP mapping for the SafeFlow-AI application.

---

## 1. Core Principles

*   **Exceptions are for Exceptional Circumstances:** Do not use exceptions for expected validation errors, duplicate checks, or normal business flow control. Instead, return functional `Result` or `Result<T>` envelopes.
*   **Domain Exceptions:** Throw exceptions in the Domain layer ONLY when aggregate invariants are violated (e.g., trying to modify a closed corrective action or registering a participant over session capacity). All domain exceptions must inherit from a common `DomainException` base class.
*   **Validation:** Input validation must occur before use-case handlers are executed, handled automatically by FluentValidation and MediatR pipelines.
*   **API Mapping:** The API layer must catch all exceptions via a global exception handler middleware and serialize them using the **RFC 7807 Problem Details** standard format.

---

## 2. Validation & Flow Control

### 2.1 Use of the `Result` Pattern
Expected business failures (e.g., "invalid credentials", "email already in use") are handled using a generic `Result` pattern:

```csharp
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
}

public record Error(string Code, string Message);
```

Handlers return a `Result<TResponse>` instead of throwing:

```csharp
public async Task<Result<Guid>> Handle(RegisterCommand command, CancellationToken ct)
{
    if (await _identityService.IsEmailUniqueAsync(command.Email))
    {
        return Result.Failure(IdentityErrors.DuplicateEmail);
    }
    // ...
}
```

### 2.2 Automatic Request Validation
Every command/query input is validated via `FluentValidation` before MediatR handler execution using a pipeline behavior:

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, ct)));
        var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures); // Handled by Global Exception Handler Middleware
        }

        return await next();
    }
}
```

---

## 3. Domain Exception Hierarchy

When a domain rule is violated inside an Aggregate, throw a domain-specific exception deriving from `DomainException`:

```
DomainException (Base)
├── BusinessRuleException (Generic rule violation)
├── EntityNotFoundException (Aggregate root not found in repository)
├── UserLockedException (Account temporarily locked)
└── InvalidStateTransitionException (Aggregate state machine error)
```

Example domain check:
```csharp
public void Complete(string findings)
{
    if (Status != CapaStatus.InProgress)
    {
        throw new InvalidStateTransitionException(
            $"Cannot complete corrective action in status '{Status}'. Only 'InProgress' actions can be completed.");
    }
    Status = CapaStatus.UnderReview;
    // ...
}
```

---

## 4. HTTP API Error Mapping (RFC 7807)

The API layer uses a global middleware to intercept exceptions and map them to HTTP status codes following the RFC 7807 (Problem Details) specification:

```json
{
  "type": "https://api.safeflow.ai/errors/validation-error",
  "title": "Validation Error",
  "status": 422,
  "detail": "One or more validation errors occurred.",
  "instance": "/v1/auth/register",
  "errors": {
    "email": ["Invalid email address format."],
    "password": ["Password must contain at least one uppercase letter."]
  },
  "correlationId": "550e8400-e29b-41d4-a716-446655440000"
}
```

### Exception to Status Code Mapping:

| Exception Type | HTTP Status Code | Description |
|---|---|---|
| `ValidationException` | `422 Unprocessable Entity` | FluentValidation input errors (field-specific warnings) |
| `EntityNotFoundException` | `404 Not Found` | Requesting an entity that does not exist in the DB |
| `InvalidStateTransitionException` | `409 Conflict` | Conflict in aggregate state machine rules |
| `BusinessRuleException` | `400 Bad Request` | General domain logic violation |
| `UnauthorizedAccessException` | `401 Unauthorized` | Missing or invalid auth credentials |
| *All Other Unhandled Exceptions* | `500 Internal Server Error` | Uncaught system/runtime failures (sanitized for production) |
