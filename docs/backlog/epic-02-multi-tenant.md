# EPIC 2 — Multi-Tenant

> **Epic Sahibi:** Tech Lead  
> **Öncelik:** P0 — Critical  
> **Tahmini Toplam Süre:** 4 iş günü  
> **Bağımlılık:** EPIC 1 (Identity)  
> **Hedef Sprint:** Sprint 2

---

## TASK 2.1 — Tenant Entity ve Domain Modeli

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 2 — Multi-Tenant |
| **Feature** | Tenant Domain Model |
| **User Story** | Platform yöneticisi olarak, her şirketin kendi izole veri alanında çalışmasını istiyorum ki müşteri verilerinin birbirine karışması engellensin. |

> [!IMPORTANT]
> **MVP Kararı: 1 Tenant = 1 Company.** MVP'de ayrı bir `Tenant` entity oluşturulmaz.
> `Company` aggregate root aynı zamanda tenant görevi görür. `Company.Id` = `TenantId`.
> `SubscriptionPlan` Company üzerine taşınmıştır. Gelecekte OSGB modeli (1 Tenant = N Company)
> gerekirse ayrı Tenant entity oluşturulacaktır.

### Neden Yapılıyor?
SaaS platformda veri izolasyonu kritiktir. Her müşteri (tenant) yalnızca kendi verilerini görmeli ve yönetmelidir.

### Ürüne Hangi Değeri Katıyor?
- Veri güvenliği (müşteri izolasyonu)
- Çoklu müşteri desteği (tek instance, çoklu şirket)
- Müşteri bazlı özelleştirme altyapısı

### Teknik Amaç
Multi-tenant veri izolasyonu için EF Core Global Query Filter mekanizması, tenant resolution ve Company aggregate üzerine SubscriptionPlan entegrasyonu.

### Yapılacak Teknik İşler
1. `ITenantEntity` interface'ine `TenantId` (Guid) property ekle (Shared Kernel'da)
2. `ITenantService` interface oluştur (current tenant resolution)
3. `CurrentTenantService` implementation (JWT claim'den TenantId okur)
4. EF Core Global Query Filter ile `TenantId` filtreleme (tüm ITenantEntity sorgularında)
5. SaveChanges interceptor ile otomatik `TenantId` atama
6. Company aggregate'e `SubscriptionPlan` value object ekle (MaxUsers, MaxEmployees, PlanType)
7. `PlanType` enumeration (Free, Starter, Professional, Enterprise)
8. Tenant resolution middleware (JWT → TenantId → HttpContext)
9. Domain event'ler: TenantSettingsUpdatedDomainEvent
10. Unit test'ler (izolasyon, filter, resolution)

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Domain.Common.Interfaces.ITenantEntity
SafeFlow.Domain.Common.Interfaces.ITenantService
SafeFlow.Domain.Companies.ValueObjects.SubscriptionPlan
SafeFlow.Domain.Companies.Enums.PlanType
SafeFlow.Domain.Companies.Events.TenantSettingsUpdatedDomainEvent
SafeFlow.Infrastructure.MultiTenant.CurrentTenantService
SafeFlow.Infrastructure.MultiTenant.TenantQueryFilter
```

### Oluşturulacak Dosyalar
```
src/SafeFlow.Domain/Common/Interfaces/ITenantEntity.cs
src/SafeFlow.Domain/Common/Interfaces/ITenantService.cs
src/SafeFlow.Domain/Companies/ValueObjects/SubscriptionPlan.cs
src/SafeFlow.Domain/Companies/Enums/PlanType.cs
src/SafeFlow.Domain/Companies/Events/TenantSettingsUpdatedDomainEvent.cs
src/SafeFlow.Infrastructure/MultiTenant/CurrentTenantService.cs
src/SafeFlow.Infrastructure/MultiTenant/TenantQueryFilter.cs
tests/SafeFlow.Infrastructure.Tests/MultiTenant/TenantQueryFilterTests.cs
```

### Bağımlı Olduğu Görevler
- TASK 0.2 (Domain base classes)

### Acceptance Criteria
- [ ] ITenantEntity interface tanımlı ve Company tarafından implement ediliyor
- [ ] EF Core Global Query Filter aktif — farklı tenantlar birbirinin verisini göremiyor
- [ ] JWT claim'den TenantId doğru çözülüyor
- [ ] SaveChanges interceptor yeni kayıtlara TenantId otomatik atanıyor
- [ ] SubscriptionPlan Company üzerine entegre ve MaxUsers limiti çalışıyor
- [ ] TenantSettingsUpdatedDomainEvent dispatch ediliyor
- [ ] Unit test'ler geçiyor

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(domain): add multi-tenant filter and subscription plan on Company

- Implement SubscriptionPlan value object
- Add PlanType enumeration
- Add TenantSettingsUpdatedDomainEvent
```

---

## TASK 2.2 — Tenant Resolution ve ITenantService

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 2 — Multi-Tenant |
| **Feature** | Tenant Resolution Middleware |
| **User Story** | Geliştirici olarak, her HTTP isteğinde mevcut tenant'ın otomatik olarak çözümlenmesini istiyorum ki servisler doğru tenant context'inde çalışsın. |

### Teknik Amaç
JWT claim'den TenantId çözümlemesi ve `ITenantService` ile tüm katmanlara dağıtımı.

### Yapılacak Teknik İşler
1. `ITenantService` interface (GetCurrentTenantId, GetCurrentTenant)
2. `TenantService` implementasyonu (JWT claim'den TenantId okuma)
3. `TenantResolutionMiddleware` (HttpContext'ten tenant çözümleme)
4. Tenant context'i ambient olarak tüm servislerde erişilebilir yapma
5. Unit test + integration test

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Application.Common.Interfaces.ITenantService
SafeFlow.Infrastructure.MultiTenancy.TenantService
SafeFlow.API.Middleware.TenantResolutionMiddleware
```

### Oluşturulacak Dosyalar
```
src/SafeFlow.Application/Common/Interfaces/ITenantService.cs
src/SafeFlow.Infrastructure/MultiTenancy/TenantService.cs
src/SafeFlow.API/Middleware/TenantResolutionMiddleware.cs
tests/SafeFlow.Infrastructure.Tests/MultiTenancy/TenantServiceTests.cs
```

### Bağımlı Olduğu Görevler
- TASK 1.5 (JWT — tenant_id claim)
- TASK 2.1 (Tenant entity)

### Acceptance Criteria
- [ ] JWT'den TenantId doğru çözümleniyor
- [ ] ITenantService.GetCurrentTenantId() doğru döndürüyor
- [ ] Tenant claim'i olmayan istekler reddediliyor (authenticated endpointlerde)

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(infrastructure): add tenant resolution from JWT claims

- Implement ITenantService for current tenant access
- Add TenantResolutionMiddleware
- Resolve tenant from JWT tenant_id claim
```

---

## TASK 2.3 — EF Core Global Query Filter ile Tenant Isolation

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 2 — Multi-Tenant |
| **Feature** | Automatic Tenant Data Filtering |
| **User Story** | Güvenlik uzmanı olarak, tüm veritabanı sorgularının otomatik olarak mevcut tenant'a filtrelenmesini istiyorum ki bir müşteri asla başka müşterinin verisini göremesin. |

### Neden Yapılıyor?
Manuel tenant filtreleme hata yapmaya açıktır. Bir geliştirici Where clause'u unutursa veri sızıntısı oluşur. Global query filter bunu imkansız kılar.

### Ürüne Hangi Değeri Katıyor?
- %100 veri izolasyonu garantisi
- Geliştirici hatası riski sıfır
- Compliance uyumluluğu (KVKK, GDPR)

### Teknik Amaç
EF Core `HasQueryFilter` ile ITenantEntity implementlerinde otomatik `TenantId == currentTenantId` filtresi.

### Yapılacak Teknik İşler
1. DbContext `OnModelCreating` — tüm ITenantEntity'ler için global filter
2. SaveChanges override — yeni entity'lere otomatik TenantId ataması
3. Cross-tenant query desteği (admin operasyonları için bypass)
4. Tenant isolation integration test'leri
5. DbContext'e ITenantService injection

### Oluşturulacak Dosyalar
```
src/SafeFlow.Infrastructure/Persistence/Extensions/TenantQueryFilterExtensions.cs
tests/SafeFlow.Infrastructure.Tests/Persistence/TenantIsolationTests.cs
```

### Bağımlı Olduğu Görevler
- TASK 0.5 (DbContext)
- TASK 2.2 (ITenantService)

### Acceptance Criteria
- [ ] Tenant A kullanıcısı Tenant B verisini göremiyor
- [ ] Yeni entity oluşturulurken TenantId otomatik set ediliyor
- [ ] Admin bypass mekanizması çalışıyor (gerektiğinde)
- [ ] Integration test ile tenant isolation doğrulandı

### Domain Event
- Yok

### Log Kayıtları
- `[WARN] Cross-tenant query attempted by {UserId}` (bypass durumunda)

### Transaction
- Evet — TenantId ataması SaveChanges içinde

### Olası Exception'lar
- `TenantMismatchException` — Yanlış tenant verisine erişim denemesi

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(infrastructure): add EF Core global query filter for tenant isolation

- Apply automatic TenantId filter on all ITenantEntity queries
- Auto-assign TenantId on entity creation
- Add admin bypass mechanism for cross-tenant operations
- Add integration tests verifying complete data isolation
```

---

## TASK 2.4 — Tenant EF Core Konfigürasyonu ve Migration

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 2 — Multi-Tenant |
| **Feature** | Tenant Persistence |
| **User Story** | Geliştirici olarak, Tenant entity'sinin veritabanında doğru şekilde saklanmasını istiyorum. |

### Yapılacak Teknik İşler
1. `TenantConfiguration` — EF Core entity configuration
2. Seed data — varsayılan demo tenant
3. Migration oluştur
4. Users tablosuna TenantId foreign key ekle (migration update)

### Oluşturulacak Dosyalar
```
src/SafeFlow.Infrastructure/Persistence/Configurations/TenantConfiguration.cs
src/SafeFlow.Infrastructure/Persistence/Seeds/TenantSeedData.cs
```

### Database Değişiklikleri
- `tenants` tablosu (id, name, subdomain, max_users, max_employees, plan_type, plan_start, plan_end, is_active, created_at)
- `users` tablosuna `tenant_id` FK ekleme

### Migration Gerekiyor mu?
- **Evet** — `AddTenant` migration

### Bağımlı Olduğu Görevler
- TASK 1.3 (Identity migration)
- TASK 2.1 (Tenant entity)

### Tahmini Süre
**2 saat**

### Önerilen Git Commit Mesajı
```
feat(infrastructure): add Tenant EF Core configuration and migration

- Add TenantConfiguration with owned types
- Add tenant seed data
- Add tenant_id FK to users table
```

---

## TASK 2.5 — Tenant Repository ve CRUD

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 2 — Multi-Tenant |
| **Feature** | Tenant Management |
| **User Story** | Sistem yöneticisi olarak, tenant oluşturma ve yönetme istiyorum ki yeni müşterileri platforma ekleyebileyim. |

### Yapılacak Teknik İşler
1. `ITenantRepository` interface
2. `TenantRepository` implementasyonu
3. Register flow'a tenant oluşturma entegrasyonu (TASK 1.7 güncelleme)
4. Integration test

### Bağımlı Olduğu Görevler
- TASK 2.4 (Tenant migration)
- TASK 1.7 (Register — tenant oluşturma)

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(infrastructure): add ITenantRepository and integrate with registration

- Implement tenant creation during user registration
- Add ITenantRepository with GetById, GetBySubdomain
```

---

## EPIC 2 — Tamamlanma Özeti

| Metrik | Değer |
|--------|-------|
| **Toplam Görev** | 5 |
| **Toplam Tahmini Süre** | 18 saat (~2.5 iş günü) |
| **Teknik Borç** | Tenant admin panel henüz yok |
| **Refactoring Önerisi** | Register flow güncellemesi TASK 1.7'ye dependency oluşturur |

### Code Review Checklist — EPIC 2
- [ ] Tüm ITenantEntity implementasyonları global query filter kapsamında
- [ ] Cross-tenant data leak testi yapıldı
- [ ] TenantId boş geçilemez (authenticated endpoint'lerde)
- [ ] SaveChanges'de TenantId otomatik atanıyor
- [ ] Admin bypass yalnızca SystemAdmin rolünde çalışıyor
