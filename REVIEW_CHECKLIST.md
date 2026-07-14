# REVIEW_CHECKLIST.md — Pull Request & Code Review Checklist

This checklist must be reviewed and filled out for every pull request (PR) before it is merged into the `main` branch.

---

## 1. Developer Self-Review Checklist

Before submitting a PR for review, the developer must verify the following:

### Build & Run
- [ ] The solution compiles with zero errors and warnings:
  ```powershell
  dotnet build --configuration Release
  ```
- [ ] All unit and integration tests run and pass successfully:
  ```powershell
  dotnet test
  ```

### Architecture & Layer Boundaries
- [ ] The Domain project (`src/SafeFlow.Domain`) has **no external library dependencies** (except MediatR.Contracts).
- [ ] No infrastructure-specific technologies (EF Core, ASP.NET Core Identity, Microsoft Identity packages, or HTTP dependencies) are referenced in the Domain or Application layers.
- [ ] Use cases are implemented as CQRS Commands/Queries handled via MediatR.
- [ ] No generic repositories are used. Custom repositories are defined under Domain and implemented in Infrastructure.

### Domain-Driven Design (DDD) Compliance
- [ ] Aggregates protect their invariants (no public setters on properties; all updates are done through descriptive domain methods).
- [ ] Entities use **Value Objects** for complex properties (e.g., `Email`, `FullName`, `PhoneNumber`) to encapsulate validation logic.
- [ ] Domain events follow the `*DomainEvent` suffix pattern (e.g., `UserRegisteredDomainEvent`).
- [ ] 1 Tenant = 1 Company rule is respected (for MVP). Company entity acts as the tenant holder.

### Code Style & Standards
- [ ] All code conforms to [CODING_STANDARD.md](file:///c:/Users/drmus/Desktop/SafeFlow-AI/CODING_STANDARD.md).
- [ ] Proper C# naming conventions are followed (PascalCase, camelCase, leading underscores).
- [ ] All clean-up activities are performed (no commented-out code, no unused namespace usings).

---

## 2. Code Review Checklist (For Reviewers)

Reviewers must inspect the PR for the following aspects:

### Architectural Compliance
- [ ] Are use-case boundaries violated? (e.g., does a controller execute business logic directly?)
- [ ] Is there data leaking across tenant boundaries?
- [ ] Does every domain entity implement `ITenantEntity` if it is tenant-specific?

### Security Checkpoints
- [ ] **Data Validation:** Are all input models validated using FluentValidation before handling?
- [ ] **SQL Injection:** Are all database queries parameterized? (Ensure no raw string concatenation is used for SQL command assembly).
- [ ] **Token Security:** Does refresh token management hash tokens (SHA256) and use the family rotation pattern?
- [ ] **Sensitive Data:** Ensure no passwords, plain-text tokens, or PII are logged in the application logs.

### Database Migrations
- [ ] If database schemas were updated, was an EF Core migration generated?
- [ ] Does the migration have a matching, safe rollback implementation (`Down` method)?

### Testing Coverage
- [ ] Do new features have corresponding unit tests targeting aggregate boundaries and command/query handlers?
- [ ] Are edge cases tested (e.g., checking validation failures, boundary condition violations)?

---

## 3. Pull Request Template

When creating a PR, use the following description template:

```markdown
## Description
Provide a summary of the changes introduced by this PR. Mention any related task files or architectural designs.

## Type of Change
- [ ] Bug fix (non-breaking change fixing an issue)
- [ ] New feature (non-breaking change adding functionality)
- [ ] Breaking change (fix or feature requiring database schema updates or interface shifts)
- [ ] Refactoring (structure modification without behavior changes)
- [ ] Documentation update

## How Has This Been Tested?
Describe the testing process. Provide commands and test projects.

## Checklist
- [ ] My code follows the project style guidelines ([CODING_STANDARD.md](file:///c:/Users/drmus/Desktop/SafeFlow-AI/CODING_STANDARD.md))
- [ ] I have verified that layer boundaries are respected ([ARCHITECTURE_RULES.md](file:///c:/Users/drmus/Desktop/SafeFlow-AI/ARCHITECTURE_RULES.md))
- [ ] I have generated EF Core migrations if the database schema changed
- [ ] All new and existing tests pass successfully
```
