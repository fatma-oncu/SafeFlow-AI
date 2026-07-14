# PROJECT_RULES.md — General Project & Contribution Rules

This document outlines workspace conventions, language guidelines, git workflow, commit guidelines, and pull request policies for the SafeFlow-AI project.

---

## 1. Language Conventions

To maintain a clean and globally readable codebase while catering to the Turkish localized Occupational Health & Safety (İSG) market, the following language divisions must be strictly followed:

| Code Element | Allowed Language | Example |
|---|---|---|
| **Code Symbols** (Classes, methods, variables) | 🇬🇧 English | `EmployeeStatus`, `RecordFailedLogin()` |
| **Database Schemas** (Tables, columns) | 🇬🇧 English | `Users`, `RefreshTokens`, `TenantId` |
| **API Endpoints & JSON payloads** | 🇬🇧 English | `/v1/auth/login`, `{"email": "..."}` |
| **Logs & System Exceptions** | 🇬🇧 English | `UserNotFoundException`, `Failed to send email` |
| **UI Texts & Validation Messages** | 🇹🇷 Turkish | `E-posta adresi zaten kullanımda.`, `Giriş başarılı.` |
| **API Error Envelopes (User-facing)** | 🇹🇷 Turkish | `"message": "Geçersiz şifre girdiniz."` |
| **Documentation & Commit Messages** | 🇬🇧 English or 🇹🇷 Turkish | `docs/`, `feat: add verify-email endpoint` |

---

## 2. Git Workflow & Branching

*   **Branch Naming Convention:**
    *   Features: `feat/feature-name` (e.g., `feat/verify-email`)
    *   Bugfixes: `fix/bug-name` (e.g., `fix/token-rotation`)
    *   Refactorings: `refactor/refactor-name` (e.g., `refactor/user-aggregate`)
    *   Documentation: `docs/doc-name`
*   **Trunk-Based Development:**
    *   Short-lived branch strategy.
    *   Merge into `main` only after all tests pass and a pull request review has been approved.

---

## 3. Commit Message Convention

SafeFlow-AI uses the **Conventional Commits** specification. Every commit message must match the following pattern:

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

### Allowed Types:
*   `feat`: A new feature (e.g., `feat(auth): add email verification`)
*   `fix`: A bug fix (e.g., `fix(identity): resolve token expiry offset`)
*   `docs`: Documentation changes only (e.g., `docs(arch): update database diagram`)
*   `style`: Code style changes (whitespace, formatting, semi-colons, etc. - no logic changes)
*   `refactor`: Code changes that neither fix a bug nor add a feature (e.g., `refactor(domain): separate employee aggregate`)
*   `test`: Adding missing tests or correcting existing tests
*   `chore`: Changes to the build process, auxiliary tools, or library dependencies

---

## 4. Pull Request (PR) Policy

Before any PR can be merged:
1.  **Build Check:** The solution must build without warnings or errors:
    ```powershell
    dotnet build --configuration Release
    ```
2.  **Test Coverage:** All unit and integration tests must pass successfully:
    ```powershell
    dotnet test
    ```
3.  **Static Analysis:** No IDE styling errors or compiler warnings.
4.  **Database Migration:** If a change introduces schema updates, ensure that the migration files are generated and target both creation and rollback (down methods).
