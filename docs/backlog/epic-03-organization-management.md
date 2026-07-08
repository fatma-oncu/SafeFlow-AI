# EPIC 3 — Organization Management

> **Epic Sahibi:** Tech Lead  
> **Öncelik:** P0 — Critical  
> **Tahmini Toplam Süre:** 5 iş günü  
> **Bağımlılık:** EPIC 2 (Multi-Tenant)  
> **Hedef Sprint:** Sprint 3

---

## TASK 3.1 — Company Entity ve Domain Modeli

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 3 — Organization Management |
| **Feature** | Company Domain Model |
| **User Story** | Şirket yöneticisi olarak, şirket bilgilerimi (unvan, adres, vergi bilgisi) sisteme kaydetmek istiyorum ki İSG dokümanlarında doğru şirket bilgileri kullanılsın. |

### Neden Yapılıyor?
İSG süreçlerinde şirket bilgileri yasal zorunluluktur. Sertifikalarda, risk raporlarında, resmi yazışmalarda şirket bilgisi yer almalıdır.

### Ürüne Hangi Değeri Katıyor?
- Yasal uyumluluk (şirket bilgileri İSG dokümanlarında)
- Organizasyon hiyerarşisi (departman/lokasyon yönetimi)
- Çoklu lokasyon desteği

### Teknik Amaç
Company aggregate root, Department entity, Location entity ve ilgili value object'leri implemente etmek.

### Yapılacak Teknik İşler
1. `CompanyInfo` value object (Name, ShortName, Logo, EmployeeCount, Sector, DangerClass)
2. `Address` value object (Street, City, District, PostalCode, Country)
3. `TaxInfo` value object (TaxNumber, TaxOffice, MersisNo)
4. `Company` aggregate root (ITenantEntity, Departments, Locations)
5. `Department` entity (Name, ManagerId, ParentDepartmentId, IsActive)
6. `Location` entity (Name, Address, LocationType, IsActive)
7. `LocationType` enumeration (HeadOffice, Factory, Warehouse, FieldOffice)
8. Domain metotlar: AddDepartment, RemoveDepartment, AddLocation, UpdateInfo
9. Domain events: DepartmentCreated, LocationCreated
10. Unit test'ler

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Domain.Companies.Entities.Company
SafeFlow.Domain.Companies.Entities.Department
SafeFlow.Domain.Companies.Entities.Location
SafeFlow.Domain.Companies.ValueObjects.CompanyInfo
SafeFlow.Domain.Companies.ValueObjects.Address
SafeFlow.Domain.Companies.ValueObjects.TaxInfo
SafeFlow.Domain.Companies.Enums.LocationType
SafeFlow.Domain.Companies.Enums.CompanyStatus
SafeFlow.Domain.Companies.Events.DepartmentCreated
SafeFlow.Domain.Companies.Events.LocationCreated
```

### Oluşturulacak Dosyalar
```
src/SafeFlow.Domain/Companies/Entities/Company.cs
src/SafeFlow.Domain/Companies/Entities/Department.cs
src/SafeFlow.Domain/Companies/Entities/Location.cs
src/SafeFlow.Domain/Companies/ValueObjects/CompanyInfo.cs
src/SafeFlow.Domain/Companies/ValueObjects/Address.cs
src/SafeFlow.Domain/Companies/ValueObjects/TaxInfo.cs
src/SafeFlow.Domain/Companies/Enums/LocationType.cs
src/SafeFlow.Domain/Companies/Enums/CompanyStatus.cs
src/SafeFlow.Domain/Companies/Events/DepartmentCreated.cs
src/SafeFlow.Domain/Companies/Events/LocationCreated.cs
tests/SafeFlow.Domain.Tests/Companies/CompanyTests.cs
tests/SafeFlow.Domain.Tests/Companies/DepartmentTests.cs
```

### Bağımlı Olduğu Görevler
- TASK 0.2 (Domain base classes)

### Acceptance Criteria
- [ ] Company'ye departman eklenip çıkarılabiliyor
- [ ] Hiyerarşik departman desteği (parent-child)
- [ ] Lokasyon eklenip güncellenebiliyor
- [ ] CompanyInfo, Address, TaxInfo value object'leri doğru çalışıyor
- [ ] Domain event'ler uygun işlemlerde dispatch ediliyor

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(domain): add Company aggregate with Department and Location entities
```

---

## TASK 3.2 — Company EF Core Konfigürasyonu ve Migration

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 3 — Organization Management |
| **Feature** | Company Persistence |
| **User Story** | Geliştirici olarak, Company entity'sinin veritabanında doğru yapıda saklanmasını istiyorum. |

### Yapılacak Teknik İşler
1. `CompanyConfiguration` — owned types (CompanyInfo, Address, TaxInfo)
2. `DepartmentConfiguration` — self-referencing (parent department)
3. `LocationConfiguration` — owned type (Address)
4. Migration oluştur
5. Seed data — demo şirket

### Database Değişiklikleri
- `companies` tablosu (id, tenant_id, name, short_name, logo, employee_count, sector, danger_class, street, city, district, postal_code, country, tax_number, tax_office, mersis_no, status, created_at, ...)
- `departments` tablosu (id, company_id, name, manager_id, parent_department_id, is_active, created_at)
- `locations` tablosu (id, company_id, name, type, street, city, district, postal_code, country, is_active, created_at)

### Migration Gerekiyor mu?
- **Evet** — `AddCompanyOrganization` migration

### Bağımlı Olduğu Görevler
- TASK 2.4 (Tenant migration)
- TASK 3.1 (Company entity)

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(infrastructure): add Company, Department, Location EF Core configs and migration
```

---

## TASK 3.3 — Company Repository ve CRUD Endpoint'leri

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 3 — Organization Management |
| **Feature** | Company CRUD API |
| **User Story** | Şirket yöneticisi olarak, şirket bilgilerimi API üzerinden görüntüleyip güncellemek istiyorum. |

### Yapılacak Teknik İşler
1. `ICompanyRepository` interface
2. `CompanyRepository` implementasyonu
3. `GetCompanyQuery` + handler (tenant'ın şirket bilgisi)
4. `UpdateCompanyCommand` + handler + validator
5. `GET /v1/company` endpoint
6. `PUT /v1/company` endpoint
7. Unit test + integration test

### Oluşturulacak Endpointler
- `GET /v1/company` 🔒 `companies.view`
- `PUT /v1/company` 🔒 `companies.update`

### Bağımlı Olduğu Görevler
- TASK 3.2 (Migration)
- TASK 1.11 (Permission auth)

### Acceptance Criteria
- [ ] Şirket bilgileri görüntülenebiliyor
- [ ] Şirket bilgileri güncellenebiliyor
- [ ] Yetkisiz kullanıcı 403 alıyor
- [ ] Validation hataları 422 dönüyor

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(api): add Company CRUD endpoints with permission authorization
```

---

## TASK 3.4 — Department CRUD Endpoint'leri

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 3 — Organization Management |
| **Feature** | Department Management API |
| **User Story** | Şirket yöneticisi olarak, departman eklemek, güncellemek ve silmek istiyorum ki organizasyon yapısını yönetebilyrityim. |

### Yapılacak Teknik İşler
1. `CreateDepartmentCommand` + handler + validator
2. `UpdateDepartmentCommand` + handler
3. `DeleteDepartmentCommand` + handler (soft delete, çalışan kontrolü)
4. `GetDepartmentsQuery` + handler (hiyerarşik liste)
5. API endpoint'leri (POST, PUT, DELETE, GET)
6. Unit test + integration test

### Oluşturulacak Endpointler
- `GET /v1/departments` 🔒 `companies.view`
- `POST /v1/departments` 🔒 `companies.manage-departments`
- `PUT /v1/departments/{id}` 🔒 `companies.manage-departments`
- `DELETE /v1/departments/{id}` 🔒 `companies.manage-departments`

### Validation Kuralları
- Name zorunlu, max 100 karakter
- ParentDepartmentId (opsiyonel) geçerli departman olmalı
- Döngüsel hiyerarşi engellenmeli (A→B→A)
- Çalışanı olan departman silinemez

### Bağımlı Olduğu Görevler
- TASK 3.3 (Company repository)

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(api): add Department CRUD endpoints with hierarchical support
```

---

## TASK 3.5 — Location CRUD Endpoint'leri

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 3 — Organization Management |
| **Feature** | Location Management API |
| **User Story** | Şirket yöneticisi olarak, işyeri lokasyonlarını yönetmek istiyorum ki risk değerlendirme ve denetimler lokasyon bazlı yapılabilsin. |

### Yapılacak Teknik İşler
1. `CreateLocationCommand` + handler + validator
2. `UpdateLocationCommand` + handler
3. `GetLocationsQuery` + handler
4. API endpoint'leri
5. Unit test + integration test

### Oluşturulacak Endpointler
- `GET /v1/locations` 🔒 `companies.view`
- `POST /v1/locations` 🔒 `companies.manage-departments`
- `PUT /v1/locations/{id}` 🔒 `companies.manage-departments`

### Bağımlı Olduğu Görevler
- TASK 3.3 (Company repository)

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(api): add Location CRUD endpoints with address management
```

---

## TASK 3.6 — User Management CRUD (Admin)

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 3 — Organization Management |
| **Feature** | User Management by Admin |
| **User Story** | Şirket yöneticisi olarak, kullanıcıları listelemek, oluşturmak, durumunu değiştirmek ve rol atamak istiyorum ki organizasyondaki erişimleri yönetebilyrityim. |

### Yapılacak Teknik İşler
1. `GetUsersQuery` + handler (paginated, filterable)
2. `GetUserByIdQuery` + handler
3. `CreateUserCommand` + handler (admin tarafından kullanıcı oluşturma)
4. `UpdateUserCommand` + handler
5. `ChangeUserStatusCommand` + handler
6. `AssignRoleCommand` / `RemoveRoleCommand` + handler
7. API endpoint'leri
8. Unit test + integration test

### Oluşturulacak Endpointler
- `GET /v1/users` 🔒 `users.view`
- `GET /v1/users/{id}` 🔒 `users.view`
- `POST /v1/users` 🔒 `users.create`
- `PUT /v1/users/{id}` 🔒 `users.update`
- `PATCH /v1/users/{id}/status` 🔒 `users.update`
- `POST /v1/users/{id}/roles` 🔒 `users.assign-role`
- `DELETE /v1/users/{id}/roles/{roleId}` 🔒 `users.assign-role`

### Bağımlı Olduğu Görevler
- TASK 1.4 (UserRepository)
- TASK 1.11 (Permission auth)

### Cache
- Evet — Kullanıcı listesi kısa süreli cache (2 dakika)

### Tahmini Süre
**1 gün**

### Önerilen Git Commit Mesajı
```
feat(api): add User management CRUD endpoints for admin operations
```

---

## EPIC 3 — Tamamlanma Özeti

| Metrik | Değer |
|--------|-------|
| **Toplam Görev** | 6 |
| **Toplam Tahmini Süre** | 24 saat (~3 iş günü) |
| **Teknik Borç** | Departman hiyerarşi derinlik sınırı belirlenmeli |
| **Refactoring Önerisi** | Address value object Company ve Location'da paylaşılıyor — doğru |

### Code Review Checklist — EPIC 3
- [ ] Tüm entity'ler ITenantEntity implementliyor
- [ ] Departman döngüsel hiyerarşi koruması var
- [ ] Soft delete yapılan departmana çalışan atanamaz
- [ ] CompanyInfo value object immutable
- [ ] Pagination ve filtering doğru çalışıyor
