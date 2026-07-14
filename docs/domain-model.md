# SafeFlow-AI — Domain Model

> **Versiyon:** 1.1  
> **Tarih:** 2026-07-07  
> **Durum:** ✅ Onaylandı
> **Yaklaşım:** Domain-Driven Design (DDD)

> [!IMPORTANT]
> **SafeFlow-AI**, yalnızca bir İSG yazılımı değildir. İş Sağlığı ve Güvenliği süreçlerini
> dijitalleştiren, otomatikleştiren ve yapay zekâ ile karar desteği sağlayan **modüler bir
> operasyon platformudur**. Eğitim, risk değerlendirme, saha denetimi, KKD yönetimi, olay
> yönetimi ve mevzuat uyumluluğunu tek çatı altında birleştirir.
>
> Domain modeli bu vizyon etrafında şekillendirilmiştir: her modül bağımsız bir Bounded
> Context olarak tasarlanmış, modüller arası iletişim Domain Event'ler aracılığıyla
> sağlanmıştır. Bu yapı, platformun gelecekte yeni İSG modülleriyle genişletilmesini
> ve her modülün bağımsız ölçeklenmesini mümkün kılar.

---

## 1. Bounded Context Map

SafeFlow-AI, İSG (İş Sağlığı ve Güvenliği) alanını kapsayan çoklu bounded context'ten oluşur. Her bounded context kendi Ubiquitous Language'ına, Aggregate'lerine ve iç tutarlılığına sahiptir.

```mermaid
graph TB
    subgraph "Core Domain"
        TRAINING["🎓 Training Context<br/>Eğitim Yönetimi"]
        CERT["📜 Certification Context<br/>Sertifika Yönetimi"]
        RISK["⚠️ Risk Assessment Context<br/>Risk Değerlendirme"]
    end

    subgraph "Supporting Domain"
        COMPANY["🏢 Company Context<br/>Şirket / Organizasyon"]
        EMPLOYEE["👤 Employee Context<br/>Çalışan Yönetimi"]
        INCIDENT["🚨 Incident Context<br/>İş Kazası / Ramak Kala"]
        AUDIT["🔍 Audit Context<br/>Denetim Yönetimi"]
        CAPA["🔧 CAPA Context<br/>DÖF Yönetimi"]
    end

    subgraph "Generic Domain"
        IDENTITY["🔐 Identity & Access Context<br/>Kimlik ve Yetkilendirme"]
        NOTIFICATION["🔔 Notification Context<br/>Bildirim Yönetimi"]
        REPORTING["📊 Reporting Context<br/>Raporlama"]
        DOCUMENT["📁 Document Context<br/>Doküman Yönetimi"]
    end

    subgraph "AI Domain"
        AI_RISK["🤖 AI Risk Context<br/>Risk Önerileri"]
        AI_PPE["👁️ AI PPE Context<br/>YOLO KKD Tespiti"]
    end

    TRAINING -->|"TrainingCompletedDomainEvent"| CERT
    TRAINING -->|"ParticipantEnrolledDomainEvent"| NOTIFICATION
    CERT -->|"CertificateExpiringDomainEvent"| NOTIFICATION
    RISK -->|"HighRiskIdentifiedDomainEvent"| NOTIFICATION
    RISK -->|"RiskAssessmentCompletedDomainEvent"| CAPA
    INCIDENT -->|"IncidentReportedDomainEvent"| CAPA
    INCIDENT -->|"IncidentReportedDomainEvent"| NOTIFICATION
    AUDIT -->|"FindingCreatedDomainEvent"| CAPA
    CAPA -->|"CorrectiveActionRequiredDomainEvent"| NOTIFICATION
    AI_RISK -->|"RiskRecommendationDomainEvent"| RISK
    AI_PPE -->|"PPEViolationDetectedDomainEvent"| INCIDENT

    IDENTITY -.->|"Shared Kernel"| TRAINING
    IDENTITY -.->|"Shared Kernel"| CERT
    IDENTITY -.->|"Shared Kernel"| RISK
    COMPANY -.->|"Shared Kernel"| EMPLOYEE
    EMPLOYEE -.->|"Shared Kernel"| TRAINING
```

### Context İlişki Türleri

| Kaynak Context | Hedef Context | İlişki Türü | Açıklama |
|---------------|---------------|-------------|----------|
| Training → Certification | Downstream | Event-Driven | Eğitim tamamlanınca sertifika oluşturulur |
| Training → Notification | Downstream | Event-Driven | Eğitim planlandığında bildirim gönderilir |
| Risk → CAPA | Downstream | Event-Driven | Yüksek risk tespit edilince DÖF açılır |
| Incident → CAPA | Downstream | Event-Driven | İş kazası sonrası DÖF açılır |
| Identity → All Contexts | Shared Kernel | Direct | Kullanıcı kimlik bilgileri tüm context'lerde paylaşılır |

### MVP Kapsamı

| Context | MVP | Phase 1 | Phase 2 | Phase 3 |
|---------|-----|---------|---------|---------|
| **Identity & Access** | ✅ | | | |
| **Company** | ✅ | | | |
| **Employee** | ✅ | | | |
| **Training** | ✅ | | | |
| **Certification** | ✅ | | | |
| **Notification** | ✅ (temel) | ✅ (gelişmiş) | | |
| **Reporting** | | ✅ | | |
| **Risk Assessment** | | ✅ | | |
| **Incident** | | | ✅ | |
| **CAPA** | | | ✅ | |
| **Audit** | | | ✅ | |
| **Document** | | | ✅ | |
| **AI Risk** | | | | ✅ |
| **AI PPE** | | | | ✅ |

---

## 2. Shared Kernel

Tüm Bounded Context'ler arasında paylaşılan temel yapılar.

```csharp
// ── Base Entity ──────────────────────────────────────────────
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public string CreatedBy { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public string? UpdatedBy { get; protected set; }
    public bool IsDeleted { get; protected set; }   // Soft Delete
}

// ── Aggregate Root ───────────────────────────────────────────
public abstract class AggregateRoot : BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents()
        => _domainEvents.Clear();
}

// ── Value Object ─────────────────────────────────────────────
public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is not ValueObject other) return false;
        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
        => GetEqualityComponents()
            .Aggregate(1, (current, obj) =>
                HashCode.Combine(current, obj?.GetHashCode() ?? 0));
}

// ── Domain Event ─────────────────────────────────────────────
public interface IDomainEvent : MediatR.INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}

// ── Multi-Tenant ─────────────────────────────────────────────
public interface ITenantEntity
{
    Guid TenantId { get; }
}

// ── Audit ────────────────────────────────────────────────────
public interface IAuditableEntity
{
    DateTime CreatedAt { get; }
    string CreatedBy { get; }
    DateTime? UpdatedAt { get; }
    string? UpdatedBy { get; }
}
```

---

## 3. Identity & Access Context

### 3.1 Aggregate: User

```mermaid
classDiagram
    class User {
        <<Aggregate Root>>
        +Guid Id
        +Email Email
        +FullName FullName
        +PhoneNumber? PhoneNumber
        +UserStatus Status
        +Guid TenantId              // ★ TenantId is the Guid mapping to the Company.Id (1 Tenant = 1 Company)
        +bool EmailConfirmed
        +DateTime? LastLoginAt
        +int FailedLoginAttempts
        +DateTime? LockoutEnd
        +Activate()
        +Deactivate()
        +Lock()
        +Unlock()
        +ConfirmEmail()
        +AssignRole(roleId, assignedBy)
        +RemoveRole(roleId)
        +UpdateProfile(name, phone)
        +RecordSuccessfulLogin()
        +RecordFailedLogin(maxAttempts)
    }

    class Email {
        <<Value Object>>
        +string Value
        +Validate()
    }

    class FullName {
        <<Value Object>>
        +string FirstName
        +string LastName
        +string DisplayName
    }

    class PhoneNumber {
        <<Value Object>>
        +string CountryCode
        +string Number
        +string Formatted
    }

    class UserRole {
        <<Entity>>
        +Guid UserId
        +Guid RoleId
        +DateTime AssignedAt
        +string AssignedBy
    }

    class UserStatus {
        <<Enumeration>>
        Pending
        Active
        Inactive
        Locked
    }

    User *-- Email
    User *-- FullName
    User *-- PhoneNumber
    User *-- UserStatus
    User "1" *-- "0..*" UserRole
```

> [!NOTE]
> **Password hashing** is delegated to Microsoft Identity's `IPasswordHasher<ApplicationUser>` in Infrastructure.
> The Domain `User` entity does NOT hold password data (ADR-001: Pragmatic Isolation).

### 3.2 RefreshToken Entity (Separate Table)

RefreshToken is NOT part of the User aggregate — it lives in its own table with a FK to `AspNetUsers.Id`.

```mermaid
classDiagram
    class RefreshToken {
        <<Entity>>
        +Guid Id
        +Guid UserId
        +string TokenHash
        +DateTime ExpiresAt
        +DateTime CreatedAt
        +string CreatedByIp
        +string? UserAgent
        +DateTime? RevokedAt
        +string? RevokedByIp
        +string? RevokedReason
        +string? ReplacedByTokenHash
        +string FamilyId
        +bool IsExpired
        +bool IsRevoked
        +bool IsActive
        +Revoke(ipAddress, reason, replacedByTokenHash)
    }
```

> [!IMPORTANT]
> - Only **SHA256 hash** of the token is stored in DB — plain text is NEVER persisted.
> - **FamilyId** enables stolen token detection: if a revoked token is reused, all tokens in that family are revoked.
> - Supports **multi-session**: one user can have multiple active tokens (mobile + web + tablet).

### 3.3 User State Machine

```mermaid
stateDiagram-v2
    [*] --> Pending : Register (EmailConfirmed=false)
    Pending --> Active : ConfirmEmail (email verified)
    Active --> Inactive : Deactivate (admin)
    Active --> Locked : RecordFailedLogin (5th attempt)
    Inactive --> Active : Activate (admin)
    Locked --> Active : Unlock (admin / timeout)
    Active --> [*] : Delete (Soft)
    Inactive --> [*] : Delete (Soft)

    note right of Pending
        Login BLOCKED
        "Email not verified"
    end note
```

### 3.4 Aggregate: Role

```mermaid
classDiagram
    class Role {
        <<Aggregate Root>>
        +Guid Id
        +string Name
        +string Description
        +bool IsSystem
        +List~RolePermission~ Permissions
        +AddPermission(permission)
        +RemovePermission(permission)
    }

    class RolePermission {
        <<Entity>>
        +Guid RoleId
        +string PermissionKey
        +DateTime GrantedAt
    }

    class Permission {
        <<Value Object>>
        +string Module
        +string Action
        +string Key
    }

    Role "1" *-- "0..*" RolePermission
    RolePermission *-- Permission
```

### 3.5 Permission Keys

```
Permissions:
  Users:
    - users.view
    - users.create
    - users.update
    - users.delete
    - users.assign-role
  Trainings:
    - trainings.view
    - trainings.create
    - trainings.update
    - trainings.delete
    - trainings.manage-participants
    - trainings.complete
  Certificates:
    - certificates.view
    - certificates.create
    - certificates.revoke
    - certificates.download
  Companies:
    - companies.view
    - companies.create
    - companies.update
    - companies.manage-departments
  Employees:
    - employees.view
    - employees.create
    - employees.update
    - employees.delete
    - employees.transfer
  Reports:
    - reports.view
    - reports.generate
    - reports.export
  Dashboard:
    - dashboard.view
    - dashboard.view-all-departments
  System:
    - system.manage-tenants
    - system.manage-settings
    - system.view-audit-log
```

### 3.6 Domain Events — Identity Context

```csharp
public sealed record UserRegisteredDomainEvent(
    Guid UserId, string Email, string FullName, Guid TenantId
) : DomainEvent;

public sealed record EmailVerificationSentDomainEvent(
    Guid UserId, string Email
) : DomainEvent;

public sealed record UserActivatedDomainEvent(
    Guid UserId
) : DomainEvent;

public sealed record UserLockedDomainEvent(
    Guid UserId, string Reason, int FailedAttempts
) : DomainEvent;

public sealed record UserDeactivatedDomainEvent(
    Guid UserId, string DeactivatedBy
) : DomainEvent;

public sealed record RoleAssignedDomainEvent(
    Guid UserId, Guid RoleId, string RoleName
) : DomainEvent;

public sealed record RoleRemovedDomainEvent(
    Guid UserId, Guid RoleId
) : DomainEvent;

public sealed record RefreshTokenRotatedDomainEvent(
    Guid UserId, string IpAddress
) : DomainEvent;

public sealed record RefreshTokenRevokedDomainEvent(
    Guid UserId, string Reason, bool IsStolenTokenDetection
) : DomainEvent;

public sealed record PasswordChangedDomainEvent(
    Guid UserId
) : DomainEvent;

public sealed record PasswordResetRequestedDomainEvent(
    Guid UserId, string Email
) : DomainEvent;
```

---

## 4. Company Context (Tenant)

> [!IMPORTANT]
> **MVP Kararı: 1 Tenant = 1 Company.** Kayıt sırasında oluşturulan her Tenant aynı zamanda
> bir Company kaydıdır. `Company.Id` aynı zamanda `TenantId` olarak kullanılır. Gelecekte
> OSGB modeli (1 Tenant = N Company) desteklenebilir ancak MVP'de bu ayrım yoktur.

### 4.1 Aggregate: Company (= Tenant for MVP)

```mermaid
classDiagram
    class Company {
        <<Aggregate Root>>
        +Guid Id
        +CompanyInfo Info
        +Address Address
        +TaxInfo TaxInfo
        +SubscriptionPlan Plan
        +CompanyStatus Status
        +List~Department~ Departments
        +List~Location~ Locations
        +AddDepartment(name, managerId)
        +RemoveDepartment(departmentId)
        +AddLocation(name, address)
        +UpdateInfo(info)
        +ChangePlan(plan)
    }

    class CompanyInfo {
        <<Value Object>>
        +string Name
        +string ShortName
        +string? Logo
        +int EmployeeCount
        +string Sector
        +string DangerClass
    }

    class Address {
        <<Value Object>>
        +string Street
        +string City
        +string District
        +string PostalCode
        +string Country
    }

    class TaxInfo {
        <<Value Object>>
        +string TaxNumber
        +string TaxOffice
        +string? MersisNo
    }

    class SubscriptionPlan {
        <<Value Object>>
        +PlanType Type
        +int MaxUsers
        +int MaxEmployees
        +DateTime StartDate
        +DateTime EndDate
        +bool IsActive
    }

    class Department {
        <<Entity>>
        +Guid Id
        +string Name
        +Guid? ManagerId
        +Guid? ParentDepartmentId
        +bool IsActive
    }

    class Location {
        <<Entity>>
        +Guid Id
        +string Name
        +Address Address
        +LocationType Type
        +bool IsActive
    }

    class CompanyStatus {
        <<Enumeration>>
        Pending
        Active
        Inactive
        Suspended
    }

    class LocationType {
        <<Enumeration>>
        HeadOffice
        Factory
        Warehouse
        FieldOffice
    }

    Company *-- CompanyInfo
    Company *-- Address
    Company *-- TaxInfo
    Company *-- SubscriptionPlan
    Company *-- CompanyStatus
    Company "1" *-- "0..*" Department
    Company "1" *-- "0..*" Location
    Department *-- Department : Parent
    Location *-- Address
    Location *-- LocationType
```

---

## 5. Employee Context

> [!NOTE]
> Employee is a **separate Bounded Context** (Supporting Domain). It references Company
> Context entities (DepartmentId, LocationId) via IDs but has its own aggregate root and
> independent lifecycle.
>
> **Gelecek Planı (Phase 3+):** Çalışan Sağlık Gözetimi (Health Surveillance) bounded context'i
> eklendiğinde, **İşyeri Hekimi** persona'sı bu Aggregate üzerinden çalışanların muayene (işe giriş,
> periyodik), aşı, sevk ve sağlık raporlarını takip edecektir.

### 5.1 Aggregate: Employee

```mermaid
classDiagram
    class Employee {
        <<Aggregate Root>>
        +Guid Id
        +Guid TenantId
        +Guid? UserId
        +Guid DepartmentId
        +Guid LocationId
        +FullName Name
        +string TcKimlikNo
        +EmploymentInfo Employment
        +EmergencyContact EmergencyContact
        +EmployeeStatus Status
        +List~EmployeeDocument~ Documents
        +Hire()
        +Terminate(reason, date)
        +Transfer(newDepartmentId, newLocationId)
        +UpdatePosition(position)
        +AddDocument(document)
    }

    class EmploymentInfo {
        <<Value Object>>
        +string Position
        +string Title
        +DateTime StartDate
        +DateTime? EndDate
        +EmploymentType Type
        +string? SgkNo
    }

    class EmergencyContact {
        <<Value Object>>
        +string Name
        +string Relationship
        +PhoneNumber Phone
    }

    class EmployeeDocument {
        <<Entity>>
        +Guid Id
        +DocumentType Type
        +string FileName
        +string FilePath
        +DateTime UploadedAt
    }

    class EmployeeStatus {
        <<Enumeration>>
        Active
        OnLeave
        Suspended
        Terminated
    }

    Employee *-- FullName
    Employee *-- EmploymentInfo
    Employee *-- EmergencyContact
    Employee *-- EmployeeStatus
    Employee "1" *-- "0..*" EmployeeDocument
```

### 5.2 Employee State Machine

```mermaid
stateDiagram-v2
    [*] --> Active : Hire
    Active --> OnLeave : StartLeave
    OnLeave --> Active : EndLeave
    Active --> Suspended : Suspend
    Suspended --> Active : Reinstate
    Active --> Terminated : Terminate
    OnLeave --> Terminated : Terminate
    Suspended --> Terminated : Terminate
    Terminated --> [*]
```

### 5.3 Domain Events — Employee Context

```csharp
public sealed record EmployeeHiredDomainEvent(
    Guid EmployeeId, Guid TenantId, string FullName, Guid DepartmentId
) : DomainEvent;

public sealed record EmployeeTransferredDomainEvent(
    Guid EmployeeId, Guid OldDepartmentId, Guid NewDepartmentId
) : DomainEvent;

public sealed record EmployeeTerminatedDomainEvent(
    Guid EmployeeId, string Reason, DateTime TerminationDate
) : DomainEvent;
```

---

## 6. Training Context

### 6.1 Aggregate: Training

```mermaid
classDiagram
    class Training {
        <<Aggregate Root>>
        +Guid Id
        +Guid TenantId
        +string Title
        +string Description
        +TrainingType Type
        +TrainingCategory Category
        +Duration Duration
        +TrainingPeriod? RecurrencePeriod
        +Guid InstructorId
        +int MaxParticipants
        +TrainingStatus Status
        +DeliveryMode DeliveryMode
        +List~TrainingSession~ Sessions
        +List~TrainingMaterial~ Materials
        +AddSession(date, location, capacity, meetingUrl, platform)
        +RemoveSession(sessionId)
        +AddMaterial(title, filePath, type)
        +Publish()
        +Cancel(reason)
        +Archive()
        +Clone() : Training
    }

    class TrainingSession {
        <<Entity>>
        +Guid Id
        +DateTime StartDate
        +DateTime EndDate
        +string Location
        +int Capacity
        +string? MeetingUrl
        +string? Platform
        +SessionStatus Status
        +List~TrainingParticipation~ Participations
        +Enroll(employeeId)
        +MarkAttendance(employeeId, status)
        +Complete(results)
        +Cancel(reason)
    }

    class TrainingParticipation {
        <<Entity>>
        +Guid Id
        +Guid EmployeeId
        +AttendanceStatus AttendanceStatus
        +int? Score
        +bool IsPassed
        +DateTime? CompletedAt
        +string? Notes
    }

    class TrainingMaterial {
        <<Entity>>
        +Guid Id
        +string Title
        +MaterialType Type
        +string FilePath
        +long FileSize
        +int OrderIndex
    }

    class TrainingType {
        <<Enumeration>>
        Mandatory
        Optional
        Orientation
        Refresher
        SpecialRisk
    }

    class DeliveryMode {
        <<Enumeration>>
        Online
        FaceToFace
        Hybrid
    }

    class TrainingCategory {
        <<Value Object>>
        +string Name
        +string Code
        +bool IsLegal
        +string? LegalReference
    }

    class Duration {
        <<Value Object>>
        +int Hours
        +int Minutes
        +int TotalMinutes
    }

    class TrainingPeriod {
        <<Value Object>>
        +int Months
        +string Description
    }

    class TrainingStatus {
        <<Enumeration>>
        Draft
        Published
        InProgress
        Completed
        Cancelled
        Archived
    }

    class SessionStatus {
        <<Enumeration>>
        Scheduled
        InProgress
        Completed
        Cancelled
    }

    class AttendanceStatus {
        <<Enumeration>>
        Enrolled
        Attended
        Absent
        Excused
    }

    Training *-- TrainingType
    Training *-- TrainingCategory
    Training *-- Duration
    Training *-- TrainingPeriod
    Training *-- TrainingStatus
    Training "1" *-- "1..*" TrainingSession
    Training "1" *-- "0..*" TrainingMaterial
    TrainingSession *-- SessionStatus
    TrainingSession "1" *-- "0..*" TrainingParticipation
    TrainingParticipation *-- AttendanceStatus
```

### 6.2 Training State Machine

```mermaid
stateDiagram-v2
    [*] --> Draft : Create
    Draft --> Published : Publish
    Draft --> Cancelled : Cancel
    Published --> InProgress : First Session Starts
    Published --> Cancelled : Cancel
    InProgress --> Completed : All Sessions Completed
    InProgress --> Cancelled : Cancel
    Completed --> Archived : Archive
    Cancelled --> Draft : Reopen
    Archived --> [*]

    state InProgress {
        [*] --> SessionScheduled
        SessionScheduled --> SessionInProgress : Start
        SessionInProgress --> SessionCompleted : Complete
        SessionCompleted --> [*]
    }
```

### 6.3 Domain Events — Training Context

```csharp
public sealed record TrainingCreatedDomainEvent(
    Guid TrainingId, Guid TenantId, string Title, TrainingType Type
) : DomainEvent;

public sealed record TrainingPublishedDomainEvent(
    Guid TrainingId, string Title
) : DomainEvent;

public sealed record TrainingSessionScheduledDomainEvent(
    Guid TrainingId, Guid SessionId, DateTime StartDate,
    string Location, int Capacity
) : DomainEvent;

public sealed record ParticipantEnrolledDomainEvent(
    Guid TrainingId, Guid SessionId, Guid EmployeeId
) : DomainEvent;

public sealed record TrainingSessionCompletedDomainEvent(
    Guid TrainingId, Guid SessionId,
    int TotalParticipants, int PassedCount
) : DomainEvent;

public sealed record TrainingCompletedDomainEvent(
    Guid TrainingId, string Title,
    List<Guid> PassedEmployeeIds
) : DomainEvent;

public sealed record TrainingCancelledDomainEvent(
    Guid TrainingId, string Reason
) : DomainEvent;
```

### 6.4 Domain Service: TrainingEligibilityService

```csharp
public interface ITrainingEligibilityService
{
    /// <summary>
    /// Çalışanın eğitime katılma uygunluğunu kontrol eder.
    /// İş kuralları: Aktif çalışan, departman uyumu, 
    /// periyodik eğitim süresi, kontenjan.
    /// </summary>
    Task<EligibilityResult> CheckEligibility(Guid employeeId, Guid trainingSessionId);
    
    /// <summary>
    /// Periyodik eğitim yenileme süresine yaklaşan çalışanları bulur.
    /// </summary>
    Task<IReadOnlyList<TrainingRenewalInfo>> GetUpcomingRenewals(
        Guid tenantId, int daysAhead = 30);
}

public record EligibilityResult(
    bool IsEligible, 
    string? Reason, 
    List<string> Warnings);

public record TrainingRenewalInfo(
    Guid EmployeeId, 
    Guid TrainingId,
    string TrainingTitle,
    DateTime LastCompletedAt, 
    DateTime RenewalDueDate,
    int DaysRemaining);
```

---

## 7. Certification Context

### 7.1 Aggregate: Certificate

```mermaid
classDiagram
    class Certificate {
        <<Aggregate Root>>
        +Guid Id
        +Guid TenantId
        +CertificateNumber Number
        +Guid EmployeeId
        +Guid TrainingId
        +Guid? TemplateId
        +ValidityPeriod Validity
        +CertificateStatus Status
        +string? FilePath
        +string? Notes
        +Issue()
        +Suspend(reason)
        +Revoke(reason)
        +Renew(newValidityPeriod)
        +MarkExpiring()
        +MarkExpired()
    }

    class CertificateNumber {
        <<Value Object>>
        +string Prefix
        +int Year
        +int Sequence
        +string FullNumber
        +static Generate(tenantCode, year, seq)
    }

    class ValidityPeriod {
        <<Value Object>>
        +DateTime IssuedAt
        +DateTime ExpiresAt
        +int DurationMonths
        +bool IsExpired
        +int DaysUntilExpiry
        +bool IsExpiringSoon(int thresholdDays)
    }

    class CertificateStatus {
        <<Enumeration>>
        Draft
        Issued
        Active
        ExpiringSoon
        Expired
        Suspended
        Revoked
    }

    Certificate *-- CertificateNumber
    Certificate *-- ValidityPeriod
    Certificate *-- CertificateStatus
```

### 7.2 Certificate State Machine

```mermaid
stateDiagram-v2
    [*] --> Draft : Create
    Draft --> Issued : Issue
    Issued --> Active : Activate
    Active --> ExpiringSoon : 30 Days Before Expiry
    ExpiringSoon --> Expired : Expiry Date Reached
    Active --> Suspended : Suspend
    Active --> Revoked : Revoke
    ExpiringSoon --> Suspended : Suspend
    ExpiringSoon --> Revoked : Revoke
    Suspended --> Active : Reinstate
    Expired --> [*] : Renew (New Certificate)
    Revoked --> [*]

    note right of ExpiringSoon
        Background job ile otomatik
        geçiş ve bildirim gönderimi
    end note
```

### 7.3 Aggregate: CertificateTemplate

```mermaid
classDiagram
    class CertificateTemplate {
        <<Aggregate Root>>
        +Guid Id
        +Guid TenantId
        +string Name
        +string Description
        +TemplateLayout Layout
        +bool IsDefault
        +bool IsActive
        +Activate()
        +Deactivate()
        +UpdateLayout(layout)
    }

    class TemplateLayout {
        <<Value Object>>
        +string HeaderText
        +string BodyTemplate
        +string FooterText
        +string? LogoPath
        +string? BackgroundPath
        +PageSize Size
        +PageOrientation Orientation
    }

    CertificateTemplate *-- TemplateLayout
```

### 7.4 Domain Events — Certification Context

```csharp
public sealed record CertificateIssuedDomainEvent(
    Guid CertificateId, Guid EmployeeId, Guid TrainingId,
    string CertificateNumber, DateTime ExpiresAt
) : DomainEvent;

public sealed record CertificateExpiringDomainEvent(
    Guid CertificateId, Guid EmployeeId,
    string CertificateNumber, DateTime ExpiresAt,
    int DaysRemaining
) : DomainEvent;

public sealed record CertificateExpiredDomainEvent(
    Guid CertificateId, Guid EmployeeId,
    string CertificateNumber
) : DomainEvent;

public sealed record CertificateRevokedDomainEvent(
    Guid CertificateId, string Reason
) : DomainEvent;
```

### 7.5 Domain Service: CertificateIssuanceService

```csharp
public interface ICertificateIssuanceService
{
    /// <summary>
    /// Eğitimi başarıyla tamamlayan çalışanlara sertifika üretir.
    /// Eğitim tipine göre geçerlilik süresi belirlenir.
    /// Sertifika numarası otomatik generate edilir.
    /// </summary>
    Task<Certificate> IssueCertificate(
        Guid employeeId, Guid trainingId, Guid? templateId = null);

    /// <summary>
    /// Toplu sertifika üretimi (bir eğitim oturumu tamamlandığında).
    /// </summary>
    Task<IReadOnlyList<Certificate>> IssueBulkCertificates(
        Guid trainingSessionId, Guid? templateId = null);

    /// <summary>
    /// Sertifika PDF'i oluşturur ve dosya yolunu döner.
    /// </summary>
    Task<string> GenerateCertificatePdf(Guid certificateId);
}
```

---

## 8. Risk Assessment Context (Phase 1)

### 8.1 Aggregate: RiskAssessment

```mermaid
classDiagram
    class RiskAssessment {
        <<Aggregate Root>>
        +Guid Id
        +Guid TenantId
        +string Title
        +RiskMethodType Method
        +Guid DepartmentId
        +Guid LocationId
        +Guid AssessorId
        +DateTime AssessmentDate
        +RiskAssessmentStatus Status
        +List~Hazard~ Hazards
        +AddHazard(description, source)
        +RemoveHazard(hazardId)
        +Submit()
        +Approve(approverId)
        +Reject(reason)
        +Revise()
    }

    class Hazard {
        <<Entity>>
        +Guid Id
        +string Description
        +string Source
        +string AffectedPersons
        +RiskScore CurrentRisk
        +RiskScore? ResidualRisk
        +List~ControlMeasure~ ControlMeasures
        +Evaluate(riskScore)
        +AddControlMeasure(measure)
    }

    class RiskScore {
        <<Value Object>>
        +int Probability
        +int Severity
        +int Frequency
        +int Score
        +RiskLevel Level
        +static CalculateFineKinney(p, s, f)
        +static Calculate5x5(p, s)
        +static CalculateLMatrix(p, s)
    }

    class ControlMeasure {
        <<Entity>>
        +Guid Id
        +string Description
        +ControlType Type
        +Guid? ResponsibleId
        +DateTime? DueDate
        +ControlStatus Status
    }

    class RiskLevel {
        <<Enumeration>>
        Acceptable
        Tolerable
        Moderate
        Significant
        Intolerable
    }

    class RiskMethodType {
        <<Enumeration>>
        FineKinney
        Matrix5x5
        LTypeMatrix
    }

    RiskAssessment "1" *-- "1..*" Hazard
    Hazard *-- RiskScore : CurrentRisk
    Hazard *-- RiskScore : ResidualRisk
    Hazard "1" *-- "0..*" ControlMeasure
    RiskScore *-- RiskLevel
    RiskAssessment *-- RiskMethodType
```

### 8.2 Fine-Kinney Value Table

```
Fine-Kinney Risk Score = Probability × Severity × Frequency

Probability (P):        Severity (S):           Frequency (F):
0.1 - Hemen hemen       1 - Dikkate değer       0.5 - Çok seyrek
      imkansız           3 - Önemli              1   - Seyrek
0.2 - Pratik olarak      7 - Ciddi               2   - Ara sıra
      imkansız          15 - Çok ciddi            3   - Sık
0.5 - Düşük             40 - Çok kötü             6   - Sürekli
  1 - Oldukça düşük    100 - Felaket
  3 - Normal
  6 - Yüksek
 10 - Çok yüksek

Risk Level:
  Score ≤ 20     → Acceptable (Kabul Edilebilir)
  20 < Score ≤ 70    → Tolerable (Olası Risk - Gözetim Gerekir)
  70 < Score ≤ 200   → Moderate (Önemli Risk)
  200 < Score ≤ 400  → Significant (Yüksek Risk)
  Score > 400        → Intolerable (Kabul Edilemez)
```

---

## 9. Incident Context (Phase 2)

### 9.1 Aggregate: Incident

```mermaid
classDiagram
    class Incident {
        <<Aggregate Root>>
        +Guid Id
        +Guid TenantId
        +IncidentType Type
        +string Description
        +DateTime OccurredAt
        +Guid LocationId
        +Guid DepartmentId
        +Guid ReportedById
        +IncidentSeverity Severity
        +IncidentStatus Status
        +List~AffectedPerson~ AffectedPersons
        +List~IncidentEvidence~ Evidence
        +Investigation? Investigation
        +Report(reportedBy)
        +Classify(severity)
        +StartInvestigation(investigatorId)
        +CompleteInvestigation(findings)
        +Close()
    }

    class AffectedPerson {
        <<Entity>>
        +Guid Id
        +Guid EmployeeId
        +InjuryType? Injury
        +int LostWorkDays
        +string? HospitalInfo
    }

    class Investigation {
        <<Entity>>
        +Guid Id
        +Guid InvestigatorId
        +DateTime StartedAt
        +DateTime? CompletedAt
        +string RootCause
        +string Findings
        +List~string~ Recommendations
    }

    class IncidentType {
        <<Enumeration>>
        WorkAccident
        NearMiss
        OccupationalDisease
        EnvironmentalIncident
    }

    class IncidentEvidence {
        <<Entity>>
        +Guid Id
        +string Description
        +string FilePath
        +string FileType
        +DateTime UploadedAt
    }

    Incident "1" *-- "0..*" AffectedPerson
    Incident "1" *-- "0..1" Investigation
    Incident "1" *-- "0..*" IncidentEvidence
    Incident *-- IncidentType
```,StartLine:1225,TargetContent:
```

---

## 10. Notification Context

### 10.1 Aggregate: Notification

```mermaid
classDiagram
    class Notification {
        <<Aggregate Root>>
        +Guid Id
        +Guid TenantId
        +Guid RecipientId
        +NotificationType Type
        +NotificationChannel Channel
        +string Title
        +string Message
        +string? ActionUrl
        +NotificationPriority Priority
        +NotificationStatus Status
        +DateTime? ReadAt
        +MarkAsRead()
        +MarkAsSent()
        +MarkAsFailed(error)
    }

    class NotificationType {
        <<Enumeration>>
        TrainingReminder
        CertificateExpiring
        RiskAssessmentDue
        IncidentReported
        TaskAssigned
        SystemAlert
    }

    class NotificationChannel {
        <<Enumeration>>
        InApp
        Email
        SMS
        Push
    }

    Notification *-- NotificationType
    Notification *-- NotificationChannel
```

---

## 11. Audit Context (Supporting Domain)

### 11.1 Aggregate: AuditLog

```mermaid
classDiagram
    class AuditLog {
        <<Aggregate Root>>
        +Guid Id
        +Guid TenantId
        +Guid? UserId
        +string? Email
        +AuditAction Action
        +string IpAddress
        +string? UserAgent
        +string? AdditionalData
        +DateTime Timestamp
        +bool IsSuccess
        +string? FailureReason
    }

    class AuditAction {
        <<Enumeration>>
        Register
        EmailVerified
        Login
        LoginFailed
        Logout
        TokenRefreshed
        StolenTokenDetected
        PasswordChanged
        PasswordResetRequested
        UserLocked
    }

    AuditLog *-- AuditAction
```

---

## 12. CAPA Context (Supporting Domain)

### 12.1 Aggregate: CorrectiveAction

```mermaid
classDiagram
    class CorrectiveAction {
        <<Aggregate Root>>
        +Guid Id
        +Guid TenantId
        +string Title
        +string Description
        +CapaSource Source
        +Guid SourceId
        +Guid? AssignedToId
        +Guid CreatedById
        +CapaStatus Status
        +DateTime CreatedAt
        +DateTime TargetDate
        +DateTime? CompletionDate
        +string? ActionsTaken
        +AssignTo(employeeId)
        +StartExecution()
        +SubmitForReview()
        +Close(actionsTaken)
        +Cancel(reason)
    }

    class CapaSource {
        <<Enumeration>>
        RiskAssessment
        IncidentReport
        AuditFinding
        Manual
    }

    class CapaStatus {
        <<Enumeration>>
        Open
        InProgress
        UnderReview
        Closed
        Cancelled
    }

    CorrectiveAction *-- CapaSource
    CorrectiveAction *-- CapaStatus
```

### 12.2 Domain Events — CAPA Context

```csharp
public sealed record CorrectiveActionCreatedDomainEvent(
    Guid CorrectiveActionId, Guid TenantId, string Title
) : DomainEvent;

public sealed record CorrectiveActionAssignedDomainEvent(
    Guid CorrectiveActionId, Guid AssignedToId
) : DomainEvent;

public sealed record CorrectiveActionClosedDomainEvent(
    Guid CorrectiveActionId, DateTime ClosedAt
) : DomainEvent;
```

---

## 13. Document Context (Generic Domain)

### 13.1 Aggregate: Document

```mermaid
classDiagram
    class Document {
        <<Aggregate Root>>
        +Guid Id
        +Guid TenantId
        +string Title
        +string FileName
        +string FilePath
        +long FileSize
        +string ContentType
        +Guid UploadedById
        +DateTime UploadedAt
        +DocumentPurpose Purpose
    }

    class DocumentPurpose {
        <<Enumeration>>
        EmployeeDocument
        TrainingMaterial
        CertificateTemplate
        IncidentEvidence
        AuditReport
    }

    Document *-- DocumentPurpose
```

---

## 14. Domain Services Summary

| Context | Domain Service | Sorumluluk |
|---------|---------------|------------|
| **Training** | `ITrainingEligibilityService` | Çalışan eğitim uygunluk kontrolü |
| **Training** | `ITrainingSchedulingService` | Eğitim çizelgeleme, çakışma kontrolü |
| **Certification** | `ICertificateIssuanceService` | Sertifika üretimi, numara oluşturma |
| **Certification** | `ICertificateRenewalService` | Sertifika yenileme iş kuralları |
| **Risk** | `IRiskCalculationService` | Fine-Kinney, 5x5, L-Matrix hesaplama |
| **Risk** | `IRiskPrioritizationService` | Risk önceliklendirme, aksiyon önerme |
| **Incident** | `IIncidentClassificationService` | Olay sınıflandırma, SGK bildirim kuralları |
| **Notification** | `INotificationDispatchService` | Kanal seçimi, şablon render, gönderim |

---

## 15. Repository Contracts (Domain/Feature Bazlı)

Generic Repository **kullanılmayacaktır**. Her Aggregate Root için ayrı, domain-specific repository tanımlanır. EF Core yeteneklerinden (LINQ, Include, AsSplitQuery) doğrudan yararlanılır.

```csharp
// ── Training Repository ──────────────────────────────────────
public interface ITrainingRepository
{
    Task<Training?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Training?> GetByIdWithSessionsAsync(Guid id, CancellationToken ct = default);
    Task<Training?> GetByIdWithFullDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Training>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<Training>> GetUpcomingByDepartmentAsync(
        Guid tenantId, Guid departmentId, int daysAhead = 30, CancellationToken ct = default);
    Task<IReadOnlyList<Training>> SearchAsync(
        Guid tenantId, TrainingSearchCriteria criteria, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Training training, CancellationToken ct = default);
    void Update(Training training);
    void Remove(Training training);
}

// ── Certificate Repository ───────────────────────────────────
public interface ICertificateRepository
{
    Task<Certificate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Certificate?> GetByNumberAsync(string certificateNumber, CancellationToken ct = default);
    Task<IReadOnlyList<Certificate>> GetByEmployeeAsync(
        Guid employeeId, CancellationToken ct = default);
    Task<IReadOnlyList<Certificate>> GetExpiringAsync(
        Guid tenantId, int daysThreshold = 30, CancellationToken ct = default);
    Task<IReadOnlyList<Certificate>> GetExpiredAsync(
        Guid tenantId, CancellationToken ct = default);
    Task AddAsync(Certificate certificate, CancellationToken ct = default);
    void Update(Certificate certificate);
}

// ── Employee Repository ──────────────────────────────────────
public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Employee?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Employee>> GetByDepartmentAsync(
        Guid departmentId, CancellationToken ct = default);
    Task<IReadOnlyList<Employee>> SearchAsync(
        Guid tenantId, EmployeeSearchCriteria criteria, CancellationToken ct = default);
    Task<int> CountByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(Employee employee, CancellationToken ct = default);
    void Update(Employee employee);
}

// ── User Repository ──────────────────────────────────────────
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByRefreshTokenAsync(string token, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetByTenantAsync(
        Guid tenantId, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    void Update(User user);
}

// ── Unit of Work ─────────────────────────────────────────────
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

---

## 16. Domain Event Flow — End-to-End

```mermaid
graph LR
    subgraph "Training Context"
        A["Training.Complete()"] --> B["TrainingCompletedDomainEvent"]
    end

    subgraph "MediatR (In-Process)"
        B --> C["INotificationHandler<br/>TrainingCompletedDomainEvent"]
    end

    subgraph "Certification Context"
        C --> D["CertificateIssuanceService<br/>.IssueBulkCertificates()"]
        D --> E["Certificate.Issue()"]
        E --> F["CertificateIssuedDomainEvent"]
    end

    subgraph "Notification Context"
        C --> G["NotificationDispatchService"]
        F --> G
        G --> H["📧 Email: Sertifika Hazır"]
        G --> I["🔔 In-App Notification"]
    end

    subgraph "Dashboard"
        F --> J["Cache Invalidation"]
        J --> K["Dashboard Stats Updated"]
    end
```

---

## Açık Sorular

| # | Soru | Etki |
|---|------|------|
| 1 | Flutter state management olarak **Riverpod** mı yoksa **Bloc** mu tercih edilecek? | Repository Layer yapısı |
| 2 | ~~Multi-tenant yapı: Schema-per-tenant mi yoksa Row-Level Security mi?~~ **Çözüldü: EF Core Global Query Filter (1 Tenant = 1 Company for MVP)** | Company Aggregate, DB tasarımı |
| 3 | Eğitim tipleri ve kategorileri Türk İSG mevzuatına göre sabit mi olmalı, yoksa tenant bazlı özelleştirilebilir mi? | Training Aggregate, Rule Engine |
| 4 | Sertifika numarası formatı için tercih var mı? (Örn: `SF-2026-00001`) | CertificateNumber Value Object |
