# SECURITY_GUIDELINES.md — Security & Authentication Policies

This document lists the mandatory security implementations, cryptographic policies, threat mitigations, and data protection rules for the SafeFlow-AI application.

---

## 1. Authentication System

*   **Asymmetric Token Signing (RS256):**
    *   JWT Access Tokens are signed using asymmetric cryptography (RS256).
    *   **Private Key:** Kept strictly secure on the server side (Azure Key Vault or equivalent in production; PEM file outside repository path in development). Used to sign tokens.
    *   **Public Key:** Shared to verify signatures.
*   **Refresh Token Safety:**
    *   Refresh tokens must be cryptographically secure random values (64 bytes encoded as Base64).
    *   Only the **SHA256 hash** of the refresh token is stored in the database. Plain text tokens are NEVER persisted.
*   **Refresh Token Delivery:**
    *   Delivered to clients using `HttpOnly`, `Secure`, `SameSite=Strict` cookies.
    *   JavaScript must be blocked from accessing these cookies to prevent Cross-Site Scripting (XSS) extraction.
    *   Mobile apps can fall back to HTTP Header `X-Refresh-Token` if cookies are not supported.
*   **Token Family Rotation & Theft Detection:**
    *   Every login creates a new family of tokens (`FamilyId`).
    *   When a refresh token is rotated, the old token is marked as revoked, and a new token is issued under the same family.
    *   **Theft Detection:** If a revoked refresh token is presented, the system flags it as a breach, immediately revokes ALL active tokens in that `FamilyId`, and forces a complete logout and re-authentication.

---

## 2. Authorization (Permission-Based Access)

*   **Role-Based to Permission-Based:**
    *   Do not bind controller endpoints to roles (e.g., `[Authorize(Roles = "Admin")]`).
    *   Instead, bind them to explicit permission keys (e.g., `[HasPermission("users.create")]`).
*   **Permissions Mapping:**
    *   Roles are simply logical collections of permissions.
    *   The JWT token payload contains the list of permissions associated with the user's role.
    *   Authorization checks verify that the user's claims contain the required permission key.

---

## 3. Multi-Tenant Isolation

*   **1 Tenant = 1 Company (MVP):**
    *   Tenant separation is maintained at the database query level using EF Core Global Query Filters.
*   **No Cross-Tenant Leaks:**
    *   Under no circumstances should a user query return data belonging to another `TenantId`.
    *   Ensure that any raw SQL queries (which bypass EF Core query filters) explicitly check for the current user's `TenantId`.

---

## 4. Threat Mitigations

*   **SQL Injection:**
    *   Always use Entity Framework Core LINQ queries (which are parameterized by default).
    *   Never use raw string concatenation in `FromSqlRaw` or direct ADO.NET calls. Always use placeholder interpolation.
*   **Brute Force Protection (Lockout):**
    *   Track failed login attempts on the `User` aggregate.
    *   If a user records 5 consecutive failed attempts, their status changes to `Locked` for 15 minutes.
*   **Cross-Site Scripting (XSS):**
    *   Ensure all HTML inputs are validated and sanitized in the API controllers.
    *   Ensure JSON models returned by the API do not echo unescaped scripting commands.
*   **Input Validation:**
    *   Every API request model must go through strict validation constraints via FluentValidation before use.
    *   Verify string lengths, pattern rules, and range requirements on all properties.

---

## 5. Password Security

*   **PBKDF2 Hashing:**
    *   Passwords are hashed using ASP.NET Core Identity's default `IPasswordHasher` implementation (currently using PBKDF2 with SHA256, 10,000+ iterations).
*   **Complexity Rules:**
    *   Minimum length: 8 characters.
    *   Must contain at least: 1 uppercase letter, 1 lowercase letter, 1 digit, and 1 non-alphanumeric character.

---

## 6. Audit Logging

*   **Critical Events Audit:**
    *   All authorization and authentication lifecycle operations must be logged to the `AuditLogs` table:
        *   Registration, Email Verification, Login (Success and Failure), Token Refreshes, Password Changes, and Account Lockouts.
*   **Details Captured:**
    *   Timestamp, Action type (Enum), Success status, IP Address, User Agent, User ID (if authenticated), and failure reasons.
*   **Immutability:**
    *   Audit logs must be read-only once created. No update or delete operations are permitted.
