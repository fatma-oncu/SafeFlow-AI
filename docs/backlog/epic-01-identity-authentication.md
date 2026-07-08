# EPIC 1 — Identity & Authentication

> **Epic Sahibi:** Tech Lead  
> **Öncelik:** P0 — Critical  
> **Tahmini Toplam Süre:** 8 iş günü  
> **Bağımlılık:** EPIC 0 (Foundation)  
> **Hedef Sprint:** Sprint 1–2

---

## TASK 1.1 — User Entity ve Identity Domain Modeli

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 1 — Identity & Authentication |
| **Feature** | User Domain Model |
| **User Story** | Geliştirici olarak, User aggregate root ve ilgili value object'leri oluşturmak istiyorum ki kimlik doğrulama ve yetkilendirme sistemi domain modeline dayansın. |

### Neden Yapılıyor?
Tüm kimlik ve yetkilendirme işlemleri User entity'si üzerine inşa edilir. User olmadan login, register, rol atama mümkün değil.

### Ürüne Hangi Değeri Katıyor?
- DDD uyumlu kimlik modeli
- İş kurallarının domain içinde kapsüllenmesi (lockout, failed attempts)
- Tip-güvenli value object'ler (Email, PhoneNumber yerine string kullanılmaz)

### Teknik Amaç
Mimari dokümanda tanımlanan User Aggregate Root, RefreshToken Entity ve ilgili Value Object'leri implemente etmek.

### Teknik Açıklama
User, AggregateRoot'tan türeyecek. Email, FullName, PhoneNumber, PasswordHash value object olarak modellenecek. UserStatus enumeration kullanılacak. RefreshToken entity olarak User aggregate içinde yer alacak. Tüm domain business rule'ları (lockout, max failed attempts) User entity içinde kapsüllenecek.

### Yapılacak Teknik İşler
1. `Email` value object oluştur (regex validation, normalization)
2. `FullName` value object oluştur (FirstName, LastName, DisplayName)
3. `PhoneNumber` value object oluştur (country code, format)
4. `PasswordHash` value object oluştur (hash + salt)
5. `UserStatus` enumeration oluştur (Pending, Active, Inactive, Locked)
6. `User` aggregate root oluştur (ITenantEntity, IAuditableEntity)
7. `RefreshToken` entity oluştur (expiry, revoke, rotate)
8. `UserRole` entity oluştur (userId, roleId, assignedAt)
9. User domain metotları: Activate, Deactivate, Lock, AssignRole, RemoveRole, RecordLogin, RecordFailedLogin, AddRefreshToken, RevokeRefreshToken, RotateRefreshToken
10. Domain event'ler: UserCreated, UserActivated, UserLocked, RoleAssigned
11. Unit test'ler yaz (tüm domain metotları ve state geçişleri)

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Domain.Identity.Entities.User
SafeFlow.Domain.Identity.Entities.RefreshToken
SafeFlow.Domain.Identity.Entities.UserRole
SafeFlow.Domain.Identity.ValueObjects.Email
SafeFlow.Domain.Identity.ValueObjects.FullName
SafeFlow.Domain.Identity.ValueObjects.PhoneNumber
SafeFlow.Domain.Identity.ValueObjects.PasswordHash
SafeFlow.Domain.Identity.Enums.UserStatus
SafeFlow.Domain.Identity.Events.UserCreated
SafeFlow.Domain.Identity.Events.UserActivated
SafeFlow.Domain.Identity.Events.UserLocked
SafeFlow.Domain.Identity.Events.RoleAssigned
SafeFlow.Domain.Identity.Events.RefreshTokenRotated
```

### Oluşturulacak Dosyalar
```
src/SafeFlow.Domain/Identity/Entities/User.cs
src/SafeFlow.Domain/Identity/Entities/RefreshToken.cs
src/SafeFlow.Domain/Identity/Entities/UserRole.cs
src/SafeFlow.Domain/Identity/ValueObjects/Email.cs
src/SafeFlow.Domain/Identity/ValueObjects/FullName.cs
src/SafeFlow.Domain/Identity/ValueObjects/PhoneNumber.cs
src/SafeFlow.Domain/Identity/ValueObjects/PasswordHash.cs
src/SafeFlow.Domain/Identity/Enums/UserStatus.cs
src/SafeFlow.Domain/Identity/Events/UserCreated.cs
src/SafeFlow.Domain/Identity/Events/UserActivated.cs
src/SafeFlow.Domain/Identity/Events/UserLocked.cs
src/SafeFlow.Domain/Identity/Events/RoleAssigned.cs
src/SafeFlow.Domain/Identity/Events/RefreshTokenRotated.cs
tests/SafeFlow.Domain.Tests/Identity/UserTests.cs
tests/SafeFlow.Domain.Tests/Identity/ValueObjects/EmailTests.cs
tests/SafeFlow.Domain.Tests/Identity/ValueObjects/FullNameTests.cs
tests/SafeFlow.Domain.Tests/Identity/ValueObjects/PhoneNumberTests.cs
tests/SafeFlow.Domain.Tests/Identity/RefreshTokenTests.cs
```

### Oluşturulacak Endpointler
- Yok (domain layer)

### Gerekli NuGet Paketleri
- Yok (yalnızca MediatR.Contracts, TASK 0.2'de eklendi)

### Database Değişiklikleri
- Yok (migration TASK 1.3'te)

### Migration Gerekiyor mu?
- Hayır (TASK 1.3'te)

### Bağımlı Olduğu Görevler
- TASK 0.2 (Domain base classes)

### Acceptance Criteria
- [ ] User entity Pending durumda oluşturulabiliyor
- [ ] Activate ile Pending → Active geçişi yapılabiliyor
- [ ] 5 başarısız login denemesinde otomatik Lock
- [ ] RefreshToken rotation çalışıyor (eski token revoke, yeni oluştur)
- [ ] Email value object geçersiz email'leri reddediyor
- [ ] FullName DisplayName property'si doğru çalışıyor
- [ ] Domain event'ler uygun işlemlerde ekleniyor
- [ ] Tüm unit test'ler geçiyor

### Definition of Done (DoD)
- [ ] Tüm entity, VO ve event sınıfları oluşturuldu
- [ ] Domain metotları iş kurallarını kapsüllüyor
- [ ] Unit test'ler %90+ coverage
- [ ] Mimari dokümana tam uygunluk
- [ ] Code review tamamlandı

### Test Gereksinimleri

| Test Tipi | Gereksinim |
|-----------|-----------|
| **Unit Test** | User state geçişleri (Pending→Active, Active→Locked), failed login counter, RefreshToken rotate/revoke, Email validation (valid/invalid), FullName display |
| **Integration Test** | Yok |
| **Manual Test** | Yok |

### SOLID & Design Pattern
- **SRP:** Her value object kendi validation'ını içerir
- **OCP:** UserStatus enumeration genişletilebilir
- **Design Pattern:** Aggregate Root, Value Object, State Pattern (UserStatus), Domain Event

### Clean Architecture Katmanları
- **Domain** (tek etkilenen katman)

### Domain Event
- `UserCreated` — User oluşturulduğunda
- `UserActivated` — Activate() çağrıldığında
- `UserLocked` — 5. başarısız login'de
- `RoleAssigned` — AssignRole() çağrıldığında
- `RefreshTokenRotated` — RotateRefreshToken() çağrıldığında

### Validation Kuralları
- Email formatı geçerli olmalı (regex)
- FirstName, LastName boş olamaz (max 100 karakter)
- PhoneNumber formatı doğru olmalı
- PasswordHash boş olamaz

### Authorization Policy
- Yok (domain katmanı)

### Log Kayıtları
- Yok (domain katmanı — handler'larda loglanacak)

### Audit Log
- Hayır (domain katmanı)

### Cache
- Hayır

### Transaction
- Hayır (domain katmanı)

### Olası Exception'lar
- `DomainException` — Geçersiz state geçişi (örn: Locked → Active yalnızca admin unlock ile)
- `ArgumentException` — Geçersiz Email, PhoneNumber formatı

### Performans Notları
- Value object oluşturma maliyetsiz (immutable, stack allocated olabilir)
- DomainEvents listesi lazy initialization ile

### Beklenen Çıktılar
- Tam fonksiyonel User aggregate, tüm value object'ler, tüm domain event'ler

### Önerilen Git Commit Mesajı
```
feat(domain): add User aggregate root with value objects and domain events

- Implement User entity with state management (Pending, Active, Locked)
- Add Email, FullName, PhoneNumber, PasswordHash value objects
- Add RefreshToken entity with rotation support
- Add UserCreated, UserActivated, UserLocked domain events
- Add comprehensive unit tests
```

### Tahmini Süre
**1 gün**

---

## TASK 1.2 — Role ve Permission Domain Modeli

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 1 — Identity & Authentication |
| **Feature** | Role & Permission Domain Model |
| **User Story** | Geliştirici olarak, permission-based yetkilendirme için Role aggregate ve Permission value object'ini oluşturmak istiyorum ki kullanıcılara granüler yetki atanabilsin. |

### Neden Yapılıyor?
İSG platformunda farklı roller (İSG Uzmanı, İşveren, Çalışan) farklı izinlere sahip olmalı. Permission-based authorization olmadan bu ayrım yapılamaz.

### Ürüne Hangi Değeri Katıyor?
- Granüler erişim kontrolü
- Rol bazlı menü ve özellik yönetimi
- Güvenlik (en az yetki prensibi)

### Teknik Amaç
Role Aggregate Root ve Permission value object implementasyonu.

### Teknik Açıklama
Role, AggregateRoot'tan türeyecek. Sistem rolleri (SystemAdmin, ISGUzmani vb.) seed data olarak tanımlanacak. Permission ise module.action formatında key-value olarak tutulacak. RolePermission entity olarak Role aggregate içinde yer alacak.

### Yapılacak Teknik İşler
1. `Permission` value object oluştur (Module, Action, Key)
2. `RolePermission` entity oluştur (RoleId, PermissionKey, GrantedAt)
3. `Role` aggregate root oluştur (Name, Description, IsSystem, Permissions)
4. Role domain metotları: AddPermission, RemovePermission
5. `Permissions` static class — tüm permission key'lerini tanımla
6. `DefaultRoles` static class — varsayılan rolleri ve izinlerini tanımla
7. Unit test'ler yaz

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Domain.Identity.Entities.Role
SafeFlow.Domain.Identity.Entities.RolePermission
SafeFlow.Domain.Identity.ValueObjects.Permission
SafeFlow.Domain.Identity.Constants.Permissions
SafeFlow.Domain.Identity.Constants.DefaultRoles
```

### Oluşturulacak Dosyalar
```
src/SafeFlow.Domain/Identity/Entities/Role.cs
src/SafeFlow.Domain/Identity/Entities/RolePermission.cs
src/SafeFlow.Domain/Identity/ValueObjects/Permission.cs
src/SafeFlow.Domain/Identity/Constants/Permissions.cs
src/SafeFlow.Domain/Identity/Constants/DefaultRoles.cs
tests/SafeFlow.Domain.Tests/Identity/RoleTests.cs
tests/SafeFlow.Domain.Tests/Identity/ValueObjects/PermissionTests.cs
```

### Oluşturulacak Endpointler
- Yok

### Gerekli NuGet Paketleri
- Yok

### Database Değişiklikleri
- Yok (migration TASK 1.3'te)

### Migration Gerekiyor mu?
- Hayır

### Bağımlı Olduğu Görevler
- TASK 0.2 (Domain base classes)

### Acceptance Criteria
- [ ] Role'e permission eklenip çıkarılabiliyor
- [ ] Aynı permission tekrar eklenemez
- [ ] System role'leri değiştirilemiyor (IsSystem=true koruması)
- [ ] Permission key formatı "module.action" şeklinde
- [ ] Varsayılan roller tanımlı (SystemAdmin, CompanyAdmin, ISGUzmani, Employee)
- [ ] Unit test'ler geçiyor

### Definition of Done (DoD)
- [ ] Entity, VO ve constant sınıfları oluşturuldu
- [ ] Unit test'ler geçiyor
- [ ] Code review tamamlandı

### Test Gereksinimleri

| Test Tipi | Gereksinim |
|-----------|-----------|
| **Unit Test** | AddPermission, RemovePermission, duplicate prevention, system role protection, Permission key format |
| **Integration Test** | Yok |
| **Manual Test** | Yok |

### SOLID & Design Pattern
- **SRP:** Role yetki yönetimi, Permission key yönetimi
- **Design Pattern:** Aggregate Root, Value Object, Constants Pattern

### Clean Architecture Katmanları
- **Domain**

### Domain Event
- Yok (Role değişiklikleri MVP'de event gerekmez)

### Validation Kuralları
- Role Name boş olamaz (max 50 karakter)
- Permission Key formatı "module.action" olmalı
- Duplicate permission eklenemez

### Olası Exception'lar
- `DomainException` — System role'ü değiştirme denemesi
- `DomainException` — Duplicate permission ekleme

### Beklenen Çıktılar
- Role aggregate, Permission value object, varsayılan rol tanımları

### Önerilen Git Commit Mesajı
```
feat(domain): add Role aggregate with permission-based authorization model

- Implement Role aggregate root with permission management
- Add Permission value object with module.action format
- Define default roles and permissions constants
- Add unit tests
```

### Tahmini Süre
**4 saat**

---

## TASK 1.3 — Identity EF Core Konfigürasyonu ve Migration

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 1 — Identity & Authentication |
| **Feature** | Identity Persistence Configuration |
| **User Story** | Geliştirici olarak, User ve Role entity'lerinin veritabanına doğru şekilde map'lenmesini istiyorum ki domain modeli persistence layer'da sorunsuz çalışsın. |

### Neden Yapılıyor?
Domain entity'leri EF Core configuration olmadan veritabanına yazılamaz. Value object'ler owned type olarak, entity ilişkileri foreign key olarak doğru map'lenmelidir.

### Ürüne Hangi Değeri Katıyor?
- Domain modelin veritabanına kalıcı olarak yazılması
- Value object'lerin veritabanında doğru temsili
- Index'ler ile sorgu performansı

### Teknik Amaç
EF Core `IEntityTypeConfiguration<T>` ile User, Role, RefreshToken entity konfigürasyonları ve initial migration.

### Teknik Açıklama
Her entity için ayrı configuration class. Value object'ler `OwnsOne()` ile, enum'lar string conversion ile map'lenecek. Unique index'ler (Email, CertificateNumber), foreign key'ler ve cascade delete kuralları tanımlanacak.

### Yapılacak Teknik İşler
1. `UserConfiguration` — User entity map'leme, value object owned types
2. `RoleConfiguration` — Role entity map'leme
3. `RefreshTokenConfiguration` — RefreshToken entity map'leme
4. `RolePermissionConfiguration` — RolePermission entity map'leme
5. `UserRoleConfiguration` — UserRole join entity map'leme
6. DbContext'e DbSet'ler ekle (Users, Roles, RefreshTokens)
7. Seed data — varsayılan roller ve izinler
8. Initial migration oluştur (`dotnet ef migrations add InitialIdentity`)
9. Migration'ı test et (`dotnet ef database update`)

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Infrastructure.Persistence.Configurations.UserConfiguration
SafeFlow.Infrastructure.Persistence.Configurations.RoleConfiguration
SafeFlow.Infrastructure.Persistence.Configurations.RefreshTokenConfiguration
SafeFlow.Infrastructure.Persistence.Configurations.RolePermissionConfiguration
SafeFlow.Infrastructure.Persistence.Configurations.UserRoleConfiguration
SafeFlow.Infrastructure.Persistence.Seeds.IdentitySeedData
```

### Oluşturulacak Dosyalar
```
src/SafeFlow.Infrastructure/Persistence/Configurations/UserConfiguration.cs
src/SafeFlow.Infrastructure/Persistence/Configurations/RoleConfiguration.cs
src/SafeFlow.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs
src/SafeFlow.Infrastructure/Persistence/Configurations/RolePermissionConfiguration.cs
src/SafeFlow.Infrastructure/Persistence/Configurations/UserRoleConfiguration.cs
src/SafeFlow.Infrastructure/Persistence/Seeds/IdentitySeedData.cs
src/SafeFlow.Infrastructure/Persistence/Migrations/YYYYMMDD_InitialIdentity.cs (auto-generated)
```

### Oluşturulacak Endpointler
- Yok

### Gerekli NuGet Paketleri
- Yok (EF Core TASK 0.5'te eklendi)

### Database Değişiklikleri
- `users` tablosu (id, email, first_name, last_name, phone_country_code, phone_number, password_hash, password_salt, status, tenant_id, last_login_at, failed_login_attempts, lockout_end, is_deleted, created_at, created_by, updated_at, updated_by)
- `roles` tablosu (id, name, description, is_system, tenant_id, is_deleted, created_at)
- `role_permissions` tablosu (id, role_id, permission_key, granted_at)
- `user_roles` tablosu (user_id, role_id, assigned_at, assigned_by)
- `refresh_tokens` tablosu (id, user_id, token, expires_at, created_at, created_by_ip, revoked_at, revoked_by_ip, replaced_by_token)
- Index: `IX_users_email` (unique), `IX_users_tenant_id`, `IX_refresh_tokens_token`

### Migration Gerekiyor mu?
- **Evet** — `InitialIdentity` migration

### Bağımlı Olduğu Görevler
- TASK 0.5 (DbContext)
- TASK 1.1 (User entity)
- TASK 1.2 (Role entity)

### Acceptance Criteria
- [ ] Migration başarıyla çalışıyor
- [ ] Users tablosu tüm alanları içeriyor
- [ ] Email unique index mevcut
- [ ] Value object'ler owned type olarak map'lenmiş
- [ ] Seed data ile varsayılan roller oluşuyor
- [ ] Cascade delete kuralları doğru

### Definition of Done (DoD)
- [ ] Migration oluşturuldu ve çalışıyor
- [ ] Veritabanı şeması doğrulandı
- [ ] Seed data başarıyla yükleniyor
- [ ] Code review tamamlandı

### Test Gereksinimleri

| Test Tipi | Gereksinim |
|-----------|-----------|
| **Unit Test** | Yok |
| **Integration Test** | Migration çalıştırma, seed data doğrulama, entity CRUD |
| **Manual Test** | pgAdmin ile tablo yapısı kontrolü |

### SOLID & Design Pattern
- **SRP:** Her configuration class tek entity sorumlu
- **Design Pattern:** Configuration as Code, Seed Data Pattern

### Clean Architecture Katmanları
- **Infrastructure** (EF Core configurations)

### Audit Log
- Evet — Seed data audit alanlarını doldurur

### Transaction
- Evet — Migration tek transaction

### Olası Exception'lar
- `DbUpdateException` — Unique constraint violation (duplicate email)
- `PostgresException` — Migration hatası

### Performans Notları
- Email index unique olmalı (login sorgularında kritik)
- RefreshToken token alanı indexed olmalı (token refresh sorgularında)
- User query'lerinde gereksiz Include yapılmamalı

### Beklenen Çıktılar
- Çalışan migration, seed data ile varsayılan roller

### Önerilen Git Commit Mesajı
```
feat(infrastructure): add Identity EF Core configuration and initial migration

- Add entity configurations for User, Role, RefreshToken
- Map value objects as owned types
- Add seed data for default roles and permissions
- Create InitialIdentity migration
```

### Tahmini Süre
**4 saat**

---

## TASK 1.4 — User Repository Implementasyonu

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 1 — Identity & Authentication |
| **Feature** | User Repository |
| **User Story** | Geliştirici olarak, User entity'sine veri erişimi için domain-specific repository istiyorum ki EF Core detaylarından bağımsız, test edilebilir veri erişim katmanı olsun. |

### Neden Yapılıyor?
Mimari karar: Generic Repository yerine domain/feature bazlı repository kullanılacak. Her repository, ilgili aggregate'in veri erişim ihtiyaçlarına özel metotlar sunar.

### Ürüne Hangi Değeri Katıyor?
- Domain-specific query metotları (GetByEmail, GetByRefreshToken)
- Test edilebilirlik (mock repository)
- EF Core bağımlılığının Infrastructure'a izole edilmesi

### Teknik Amaç
`IUserRepository` interface (Application) ve `UserRepository` implementasyonu (Infrastructure) oluşturmak.

### Teknik Açıklama
Interface Application katmanında, implementasyon Infrastructure katmanında. EF Core `DbSet<User>`, LINQ, `Include()` doğrudan kullanılacak. Specification pattern opsiyonel.

### Yapılacak Teknik İşler
1. `IUserRepository` interface tanımla (Application katmanı)
2. `UserRepository` implementasyonu (Infrastructure katmanı)
3. Metotlar: GetByIdAsync, GetByEmailAsync, GetByRefreshTokenAsync, GetByTenantAsync, EmailExistsAsync, AddAsync, Update
4. DI registration
5. Integration test'ler yaz (TestContainers ile PostgreSQL)

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Domain.Identity.Repositories.IUserRepository
SafeFlow.Infrastructure.Persistence.Repositories.UserRepository
```

### Oluşturulacak Dosyalar
```
src/SafeFlow.Domain/Identity/Repositories/IUserRepository.cs
src/SafeFlow.Infrastructure/Persistence/Repositories/UserRepository.cs
tests/SafeFlow.Infrastructure.Tests/Persistence/Repositories/UserRepositoryTests.cs
```

### Oluşturulacak Endpointler
- Yok

### Gerekli NuGet Paketleri
- `Testcontainers.PostgreSql` (test projesi — opsiyonel)

### Database Değişiklikleri
- Yok

### Migration Gerekiyor mu?
- Hayır

### Bağımlı Olduğu Görevler
- TASK 1.1 (User entity)
- TASK 1.3 (EF Core configuration)

### Acceptance Criteria
- [ ] GetByEmailAsync email ile kullanıcı bulabiliyor
- [ ] GetByRefreshTokenAsync aktif refresh token ile kullanıcı bulabiliyor
- [ ] EmailExistsAsync duplicate email kontrolü yapıyor
- [ ] AddAsync yeni kullanıcı ekleyebiliyor
- [ ] Integration test'ler geçiyor

### Definition of Done (DoD)
- [ ] Interface ve implementasyon oluşturuldu
- [ ] DI registration yapıldı
- [ ] Integration test'ler geçiyor
- [ ] Code review tamamlandı

### Test Gereksinimleri

| Test Tipi | Gereksinim |
|-----------|-----------|
| **Unit Test** | Yok (repository test'i integration test) |
| **Integration Test** | CRUD operasyonları, query metotları, edge cases |
| **Manual Test** | Yok |

### SOLID & Design Pattern
- **DIP:** Interface Application'da, implementasyon Infrastructure'da
- **ISP:** Yalnızca gerekli metotlar interface'te
- **Design Pattern:** Repository Pattern (domain-specific), Dependency Injection

### Clean Architecture Katmanları
- **Domain** (IUserRepository interface — repository contracts domain'de)
- **Infrastructure** (UserRepository implementasyonu)

### Performans Notları
- GetByEmailAsync — indexed query, performans sorunu yok
- GetByRefreshTokenAsync — token indexed olmalı
- Include() kullanımı gereksiz eager loading'den kaçınılmalı

### Beklenen Çıktılar
- Çalışan User repository, integration test'ler

### Önerilen Git Commit Mesajı
```
feat(infrastructure): add IUserRepository and UserRepository implementation

- Define IUserRepository in Domain layer
- Implement UserRepository with EF Core
- Add GetByEmail, GetByRefreshToken, EmailExists methods
- Add integration tests with TestContainers
```

### Tahmini Süre
**4 saat**

---

## TASK 1.5 — JWT Token Servisi

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 1 — Identity & Authentication |
| **Feature** | JWT Token Generation & Validation |
| **User Story** | Geliştirici olarak, güvenli JWT access token ve refresh token üretimi istiyorum ki kullanıcılar stateless authentication ile API'ye erişebilsin. |

### Neden Yapılıyor?
SPA ve mobil uygulama için session-based auth uygun değil. JWT stateless, ölçeklenebilir ve cross-platform uyumlu.

### Ürüne Hangi Değeri Katıyor?
- Ölçeklenebilir kimlik doğrulama (sunucu state tutmaz)
- Mobil ve web için tek auth mekanizması
- Permission claim'leri ile granüler yetki kontrolü

### Teknik Amaç
`IJwtTokenService` interface ve `JwtTokenService` implementasyonu oluşturmak.

### Teknik Açıklama
Access token RSA256 ile imzalanacak, 15 dakika ömürlü. Refresh token random byte array, 7 gün ömürlü. Token Rotation: her refresh işleminde eski token revoke, yeni üretilir. JWT claim'ler: sub, email, name, tenant_id, roles, permissions.

### Yapılacak Teknik İşler
1. `IJwtTokenService` interface tanımla
2. `JwtTokenService` implementasyonu — access token üretimi
3. Refresh token üretimi (cryptographic random)
4. Token validation (signature, expiry, claims)
5. `JwtSettings` configuration class (appsettings)
6. DI registration
7. Unit test'ler yaz

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Application.Common.Interfaces.IJwtTokenService
SafeFlow.Infrastructure.Authentication.JwtTokenService
SafeFlow.Infrastructure.Authentication.JwtSettings
SafeFlow.Application.Identity.DTOs.TokenResponse
```

### Oluşturulacak Dosyalar
```
src/SafeFlow.Application/Common/Interfaces/IJwtTokenService.cs
src/SafeFlow.Application/Identity/DTOs/TokenResponse.cs
src/SafeFlow.Infrastructure/Authentication/JwtTokenService.cs
src/SafeFlow.Infrastructure/Authentication/JwtSettings.cs
src/SafeFlow.API/appsettings.json (JWT bölümü)
tests/SafeFlow.Infrastructure.Tests/Authentication/JwtTokenServiceTests.cs
```

### Oluşturulacak Endpointler
- Yok (servis)

### Gerekli NuGet Paketleri
- `System.IdentityModel.Tokens.Jwt` (Infrastructure)
- `Microsoft.IdentityModel.Tokens` (Infrastructure)

### Database Değişiklikleri
- Yok

### Migration Gerekiyor mu?
- Hayır

### Bağımlı Olduğu Görevler
- TASK 1.1 (User entity — claims source)
- TASK 0.6 (Program.cs — JWT auth middleware)

### Acceptance Criteria
- [ ] Access token JWT formatında, 15 dk ömürlü
- [ ] Token claim'leri doğru: sub, email, tenant_id, roles, permissions
- [ ] Refresh token kriptografik random, 7 gün ömürlü
- [ ] Invalid/expired token reddediliyor
- [ ] JwtSettings appsettings'ten okunuyor
- [ ] Unit test'ler geçiyor

### Definition of Done (DoD)
- [ ] Token üretimi ve doğrulaması çalışıyor
- [ ] Unit test'ler geçiyor
- [ ] JwtSettings konfigüre edildi
- [ ] Code review tamamlandı

### Test Gereksinimleri

| Test Tipi | Gereksinim |
|-----------|-----------|
| **Unit Test** | Token üretimi, claim doğrulama, expired token reddi, invalid signature reddi |
| **Integration Test** | Yok |
| **Manual Test** | jwt.io ile token decode |

### SOLID & Design Pattern
- **DIP:** IJwtTokenService interface
- **SRP:** Token üretimi ve doğrulaması ayrı metotlar
- **Design Pattern:** Strategy (token provider swap edilebilir)

### Clean Architecture Katmanları
- **Application** (interface, DTO)
- **Infrastructure** (implementasyon)

### Log Kayıtları
- `[INFO] JWT token generated for user {UserId}`
- `[WARN] Token validation failed: {Reason}`

### Güvenlik Notları
- Secret key minimum 256 bit olmalı
- Token'da password hash gibi hassas veri bulunmamalı
- Refresh token veritabanında hashed olarak saklanabilir (opsiyonel)

### Beklenen Çıktılar
- Çalışan JWT token servisi

### Önerilen Git Commit Mesajı
```
feat(infrastructure): add JWT token service with refresh token support

- Implement IJwtTokenService for access and refresh token generation
- Add token validation with signature and expiry checks
- Configure JwtSettings from appsettings
- Add unit tests for token operations
```

### Tahmini Süre
**4 saat**

---

## TASK 1.6 — Password Hashing Servisi

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 1 — Identity & Authentication |
| **Feature** | Secure Password Hashing |
| **User Story** | Güvenlik uzmanı olarak, kullanıcı parolalarının güvenli hash algoritması ile saklanmasını istiyorum ki veritabanı sızıntısında parolalar ele geçirilemesin. |

### Teknik Amaç
`IPasswordHasher` interface ve bcrypt/Argon2 tabanlı implementasyon.

### Yapılacak Teknik İşler
1. `IPasswordHasher` interface tanımla (HashPassword, VerifyPassword)
2. `BcryptPasswordHasher` implementasyonu
3. Unit test'ler yaz

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Application.Common.Interfaces.IPasswordHasher
SafeFlow.Infrastructure.Authentication.BcryptPasswordHasher
```

### Oluşturulacak Dosyalar
```
src/SafeFlow.Application/Common/Interfaces/IPasswordHasher.cs
src/SafeFlow.Infrastructure/Authentication/BcryptPasswordHasher.cs
tests/SafeFlow.Infrastructure.Tests/Authentication/BcryptPasswordHasherTests.cs
```

### Gerekli NuGet Paketleri
- `BCrypt.Net-Next` (Infrastructure)

### Bağımlı Olduğu Görevler
- TASK 0.1

### Acceptance Criteria
- [ ] Parola hash'lenebiliyor
- [ ] Hash karşılaştırma doğru çalışıyor
- [ ] Aynı parola farklı hash üretiyor (salt)

### Tahmini Süre
**2 saat**

---

## TASK 1.7 — Register Command Handler

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 1 — Identity & Authentication |
| **Feature** | User Registration |
| **User Story** | Yeni kullanıcı olarak, e-posta ve şifremle sisteme kayıt olmak istiyorum ki SafeFlow-AI platformunu kullanmaya başlayabileyim. |

### Neden Yapılıyor?
Kullanıcı kaydı sistemin giriş noktasıdır. Register olmadan hiçbir kullanıcı platforma erişemez.

### Ürüne Hangi Değeri Katıyor?
- Self-service kullanıcı kaydı
- Otomatik tenant oluşturma (ilk kayıtta)
- Güvenli kayıt süreci (email doğrulama hazırlığı)

### Teknik Amaç
CQRS RegisterCommand, handler, validator ve API endpoint oluşturmak.

### Teknik Açıklama
RegisterCommand email, password, firstName, lastName, phoneNumber, companyName alacak. Handler: email duplicate kontrolü, password hash, User entity oluşturma, Pending status, UserCreated event dispatch. Validator: email format, password strength, required alanlar.

### Yapılacak Teknik İşler
1. `RegisterCommand` — ICommand<RegisterResponse> 
2. `RegisterCommandValidator` — FluentValidation
3. `RegisterCommandHandler` — ICommandHandler
4. `RegisterResponse` DTO
5. `POST /v1/auth/register` endpoint
6. Unit test (handler)
7. Integration test (endpoint)

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Application.Identity.Commands.Register.RegisterCommand
SafeFlow.Application.Identity.Commands.Register.RegisterCommandValidator
SafeFlow.Application.Identity.Commands.Register.RegisterCommandHandler
SafeFlow.Application.Identity.Commands.Register.RegisterResponse
```

### Oluşturulacak Dosyalar
```
src/SafeFlow.Application/Identity/Commands/Register/RegisterCommand.cs
src/SafeFlow.Application/Identity/Commands/Register/RegisterCommandValidator.cs
src/SafeFlow.Application/Identity/Commands/Register/RegisterCommandHandler.cs
src/SafeFlow.Application/Identity/Commands/Register/RegisterResponse.cs
src/SafeFlow.API/Controllers/AuthController.cs
tests/SafeFlow.Application.Tests/Identity/Commands/RegisterCommandHandlerTests.cs
tests/SafeFlow.API.Tests/Controllers/AuthControllerTests.cs
```

### Oluşturulacak Endpointler
- `POST /v1/auth/register`

### Gerekli NuGet Paketleri
- Yok (önceki task'larda eklendi)

### Database Değişiklikleri
- Users tablosuna yeni kayıt eklenir

### Migration Gerekiyor mu?
- Hayır

### Bağımlı Olduğu Görevler
- TASK 1.3 (Migration)
- TASK 1.4 (UserRepository)
- TASK 1.5 (JwtTokenService)
- TASK 1.6 (PasswordHasher)

### Acceptance Criteria
- [ ] Yeni kullanıcı kaydı başarılı — 201 Created
- [ ] Duplicate email — 409 Conflict
- [ ] Geçersiz email formatı — 422 Validation Error
- [ ] Zayıf parola — 422 Validation Error
- [ ] UserCreated domain event dispatch ediliyor
- [ ] Kullanıcı Pending durumda oluşturuluyor

### Definition of Done (DoD)
- [ ] Command, handler, validator, endpoint oluşturuldu
- [ ] Unit test + integration test geçiyor
- [ ] Swagger'da endpoint görünüyor
- [ ] Code review tamamlandı

### Test Gereksinimleri

| Test Tipi | Gereksinim |
|-----------|-----------|
| **Unit Test** | Handler: başarılı kayıt, duplicate email, validation hataları |
| **Integration Test** | API endpoint: 201, 409, 422 response'ları |
| **Manual Test** | Swagger/Postman ile register isteği |

### SOLID & Design Pattern
- **SRP:** Command, Validator, Handler ayrı sınıflar
- **Design Pattern:** CQRS (Command), Mediator, Validation Pipeline

### Clean Architecture Katmanları
- **Application** (Command, Handler, Validator, DTO)
- **API** (Controller endpoint)

### Domain Event
- `UserCreated` — Kayıt başarılı olduğunda

### Validation Kuralları
- Email: zorunlu, valid format, max 256 karakter
- Password: zorunlu, min 8 karakter, en az 1 büyük harf, 1 rakam, 1 özel karakter
- FirstName: zorunlu, max 100 karakter
- LastName: zorunlu, max 100 karakter
- CompanyName: zorunlu, max 200 karakter

### Authorization Policy
- Yok (public endpoint)

### Log Kayıtları
- `[INFO] User registration attempt: {Email}`
- `[INFO] User registered successfully: {UserId}`
- `[WARN] Registration failed - duplicate email: {Email}`

### Audit Log
- Evet — Yeni kullanıcı kaydı audit log'a yazılır

### Cache
- Hayır

### Transaction
- Evet — User + Tenant oluşturma aynı transaction

### Olası Exception'lar
- `DuplicateEmailException` (veya Result.Failure ile)
- `DbUpdateException` (unique constraint)

### Performans Notları
- Password hashing CPU-intensive — async olmalı
- Email existence check indexed query

### Beklenen Çıktılar
- Çalışan register endpoint, validation, domain event

### Önerilen Git Commit Mesajı
```
feat(identity): add user registration command and endpoint

- Implement RegisterCommand with FluentValidation
- Add RegisterCommandHandler with duplicate email check
- Add POST /v1/auth/register endpoint
- Dispatch UserCreated domain event
- Add unit and integration tests
```

### Tahmini Süre
**4 saat**

---

## TASK 1.8 — Login Command Handler

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 1 — Identity & Authentication |
| **Feature** | User Login |
| **User Story** | Kayıtlı kullanıcı olarak, e-posta ve şifremle giriş yapmak istiyorum ki JWT token alıp API'ye güvenli erişim sağlayabileyim. |

### Neden Yapılıyor?
Login, kullanıcının platforma erişiminin kapısıdır. Token olmadan hiçbir korumalı endpoint'e erişilemez.

### Ürüne Hangi Değeri Katıyor?
- Güvenli kimlik doğrulama
- Access + Refresh token ile kesintisiz oturum
- Başarısız giriş takibi (brute force koruması)

### Teknik Amaç
LoginCommand, handler, validator ve API endpoint.

### Teknik Açıklama
Handler: email ile kullanıcı bul, status kontrol (Active?), parola doğrula, başarısız girişte counter artır (5'te lock), başarılıda access + refresh token üret. Token'lar response'ta dönecek.

### Yapılacak Teknik İşler
1. `LoginCommand` oluştur
2. `LoginCommandValidator` oluştur
3. `LoginCommandHandler` oluştur
4. `LoginResponse` DTO oluştur (accessToken, refreshToken, expiresIn, user bilgileri)
5. `POST /v1/auth/login` endpoint (AuthController'a ekle)
6. Failed login tracking (5 denemede lock)
7. Unit test + integration test

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Application.Identity.Commands.Login.LoginCommand
SafeFlow.Application.Identity.Commands.Login.LoginCommandValidator
SafeFlow.Application.Identity.Commands.Login.LoginCommandHandler
SafeFlow.Application.Identity.Commands.Login.LoginResponse
SafeFlow.Application.Identity.Commands.Login.UserInfoDto
```

### Oluşturulacak Dosyalar
```
src/SafeFlow.Application/Identity/Commands/Login/LoginCommand.cs
src/SafeFlow.Application/Identity/Commands/Login/LoginCommandValidator.cs
src/SafeFlow.Application/Identity/Commands/Login/LoginCommandHandler.cs
src/SafeFlow.Application/Identity/Commands/Login/LoginResponse.cs
tests/SafeFlow.Application.Tests/Identity/Commands/LoginCommandHandlerTests.cs
```

### Oluşturulacak Endpointler
- `POST /v1/auth/login`

### Bağımlı Olduğu Görevler
- TASK 1.4 (UserRepository)
- TASK 1.5 (JwtTokenService)
- TASK 1.6 (PasswordHasher)
- TASK 1.7 (Register — test verisi)

### Acceptance Criteria
- [ ] Doğru email/password ile login — 200 OK + token
- [ ] Yanlış password — 401 Unauthorized + failed attempt arttı
- [ ] Inactive user — 403 Forbidden
- [ ] Locked user — 403 Forbidden + lockout süresi bilgisi
- [ ] 5 başarısız deneme — otomatik lock + UserLocked event
- [ ] RefreshToken veritabanına kaydedildi

### Test Gereksinimleri

| Test Tipi | Gereksinim |
|-----------|-----------|
| **Unit Test** | Başarılı login, yanlış password, inactive user, locked user, 5. deneme lock |
| **Integration Test** | API endpoint response'ları |
| **Manual Test** | Swagger ile login, jwt.io ile token kontrol |

### Domain Event
- `UserLoggedIn` (opsiyonel — audit amaçlı)
- `UserLocked` — 5. başarısız denemede

### Log Kayıtları
- `[INFO] Login attempt: {Email}`
- `[INFO] Login successful: {UserId}`
- `[WARN] Login failed - invalid password: {Email} (attempt {Count})`
- `[WARN] User locked due to failed attempts: {UserId}`

### Audit Log
- Evet — Başarılı ve başarısız login denemeleri

### Transaction
- Evet — Failed attempt counter + lock status güncellemesi

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(identity): add user login command with brute force protection

- Implement LoginCommand with credential validation
- Add failed login tracking and automatic lockout
- Generate JWT access and refresh tokens on success
- Add POST /v1/auth/login endpoint
- Add unit and integration tests
```

---

## TASK 1.9 — Refresh Token ve Token Rotation

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 1 — Identity & Authentication |
| **Feature** | Token Refresh & Rotation |
| **User Story** | Aktif kullanıcı olarak, access token süresi dolduğunda refresh token ile yeni token almak istiyorum ki oturumum kesintisiz devam etsin. |

### Teknik Amaç
RefreshTokenCommand handler ve Token Rotation implementasyonu.

### Yapılacak Teknik İşler
1. `RefreshTokenCommand` ve handler oluştur
2. Token rotation: eski refresh token revoke, yeni üret
3. Stolen token detection: revoked token kullanılırsa tüm token'ları revoke et
4. `POST /v1/auth/refresh-token` endpoint
5. `POST /v1/auth/revoke-token` endpoint (logout)
6. Unit test + integration test

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Application.Identity.Commands.RefreshToken.RefreshTokenCommand
SafeFlow.Application.Identity.Commands.RefreshToken.RefreshTokenCommandHandler
SafeFlow.Application.Identity.Commands.RevokeToken.RevokeTokenCommand
SafeFlow.Application.Identity.Commands.RevokeToken.RevokeTokenCommandHandler
```

### Oluşturulacak Endpointler
- `POST /v1/auth/refresh-token`
- `POST /v1/auth/revoke-token`

### Bağımlı Olduğu Görevler
- TASK 1.5 (JwtTokenService)
- TASK 1.8 (Login — refresh token oluşturma)

### Acceptance Criteria
- [ ] Valid refresh token ile yeni access + refresh token alınabiliyor
- [ ] Eski refresh token otomatik revoke oluyor (rotation)
- [ ] Expired refresh token reddediliyor
- [ ] Revoked refresh token reddediliyor
- [ ] Stolen token detection: revoked token kullanılırsa tüm user token'ları revoke
- [ ] Revoke endpoint ile logout yapılabiliyor

### Domain Event
- `RefreshTokenRotated` — Başarılı rotation'da

### Log Kayıtları
- `[INFO] Token refreshed for user {UserId}`
- `[WARN] Attempted reuse of revoked refresh token for user {UserId} — possible token theft`
- `[INFO] All refresh tokens revoked for user {UserId}`

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(identity): add refresh token rotation and stolen token detection

- Implement RefreshTokenCommand with token rotation
- Add stolen token detection (revoked token reuse)
- Add POST /v1/auth/refresh-token endpoint
- Add POST /v1/auth/revoke-token endpoint for logout
```

---

## TASK 1.10 — GetCurrentUser Query ve Auth/Me Endpoint

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 1 — Identity & Authentication |
| **Feature** | Current User Profile |
| **User Story** | Giriş yapmış kullanıcı olarak, kendi profil bilgilerimi, rollerimi ve izinlerimi görüntülemek istiyorum ki hangi yetkilere sahip olduğumu bileyim. |

### Teknik Amaç
GetCurrentUserQuery handler ve GET /v1/auth/me endpoint.

### Yapılacak Teknik İşler
1. `GetCurrentUserQuery` ve handler
2. `CurrentUserResponse` DTO
3. `GET /v1/auth/me` endpoint
4. JWT claim'den userId okuma
5. Unit test + integration test

### Oluşturulacak Endpointler
- `GET /v1/auth/me`

### Bağımlı Olduğu Görevler
- TASK 1.4 (UserRepository)
- TASK 1.8 (Login — token üretimi)

### Authorization Policy
- `[Authorize]` — Authenticated user gerekli

### Acceptance Criteria
- [ ] Authenticated user kendi bilgilerini görebiliyor
- [ ] Response: id, email, fullName, roles, permissions, tenantId
- [ ] Unauthenticated — 401
- [ ] Token expired — 401

### Tahmini Süre
**2 saat**

### Önerilen Git Commit Mesajı
```
feat(identity): add GET /v1/auth/me endpoint for current user profile

- Implement GetCurrentUserQuery
- Return user profile, roles, and permissions
- Require authenticated user
```

---

## TASK 1.11 — Permission-Based Authorization Altyapısı

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 1 — Identity & Authentication |
| **Feature** | Permission-Based Authorization |
| **User Story** | Geliştirici olarak, endpoint'leri permission bazlı korumak istiyorum ki sadece yetkili kullanıcılar belirli işlemleri yapabilsin. |

### Neden Yapılıyor?
Role-based yetkilendirme yeterince granüler değil. Permission-based authorization ile her endpoint, kullanıcının spesifik bir izne sahip olup olmadığını kontrol eder.

### Teknik Amaç
Custom `[HasPermission("module.action")]` attribute ve authorization handler.

### Yapılacak Teknik İşler
1. `HasPermissionAttribute` — Custom authorize attribute
2. `PermissionAuthorizationHandler` — JWT claim'den permission kontrolü
3. `PermissionRequirement` — IAuthorizationRequirement
4. DI registration (Authorization policy)
5. Unit test'ler yaz

### Oluşturulacak Sınıflar
```csharp
SafeFlow.API.Authorization.HasPermissionAttribute
SafeFlow.API.Authorization.PermissionAuthorizationHandler
SafeFlow.API.Authorization.PermissionRequirement
```

### Oluşturulacak Dosyalar
```
src/SafeFlow.API/Authorization/HasPermissionAttribute.cs
src/SafeFlow.API/Authorization/PermissionAuthorizationHandler.cs
src/SafeFlow.API/Authorization/PermissionRequirement.cs
tests/SafeFlow.API.Tests/Authorization/PermissionAuthorizationHandlerTests.cs
```

### Bağımlı Olduğu Görevler
- TASK 1.2 (Permissions constant)
- TASK 1.5 (JWT — permission claims)

### Acceptance Criteria
- [ ] `[HasPermission("trainings.create")]` ile endpoint korunabiliyor
- [ ] Permission claim'i olan kullanıcı erişebiliyor
- [ ] Permission claim'i olmayan kullanıcı — 403 Forbidden
- [ ] Multiple permission kontrolü destekleniyor

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(api): add permission-based authorization infrastructure

- Implement HasPermissionAttribute for endpoint protection
- Add PermissionAuthorizationHandler checking JWT claims
- Add unit tests for permission evaluation
```

---

## EPIC 1 — Tamamlanma Özeti

| Metrik | Değer |
|--------|-------|
| **Toplam Görev** | 11 |
| **Toplam Tahmini Süre** | 40 saat (~5 iş günü) |
| **Teknik Borç** | Email verification henüz yok (backlog'a eklenmeli) |
| **Refactoring Önerisi** | Password policy kuralları konfigürasyona taşınabilir |

### Görev Bağımlılık Grafiği
```
TASK 1.1 (User Entity) ──┬──→ TASK 1.3 (EF Config) ──→ TASK 1.4 (Repository)
                         │                                      ↓
TASK 1.2 (Role Entity)  ─┘                              TASK 1.7 (Register) ──→ TASK 1.8 (Login) ──→ TASK 1.9 (Refresh Token)
                                                                                       ↓
TASK 1.5 (JWT Service) ─────────────────────────────────────────────────────→ TASK 1.10 (Me)
TASK 1.6 (Password Hash) ──────────────────────────────→ TASK 1.7                      ↓
                                                                              TASK 1.11 (Permissions)
```

### Code Review Checklist — EPIC 1
- [ ] Parolalar hiçbir yerde plain text loglanmıyor
- [ ] JWT secret appsettings'te, production'da Key Vault'ta olacak
- [ ] Refresh token veritabanında güvenli saklanıyor
- [ ] Failed login counter race condition'a karşı korunuyor
- [ ] Domain event'ler doğru yerlerde dispatch ediliyor
- [ ] Value object validation'ları constructor'da çalışıyor
- [ ] Permission key'leri constant'tan kullanılıyor (magic string yok)
- [ ] Tüm endpoint'ler rate limiting'e tabi (TASK 0.6'da yapılandırıldı)
