# EPIC 4 — Employee Management

> **Epic Sahibi:** Tech Lead  
> **Öncelik:** P0 — Critical  
> **Tahmini Toplam Süre:** 4 iş günü  
> **Bağımlılık:** EPIC 3 (Organization)  
> **Hedef Sprint:** Sprint 3–4

---

## TASK 4.1 — Employee Entity ve Domain Modeli

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 4 — Employee Management |
| **Feature** | Employee Domain Model |
| **User Story** | İSG Uzmanı olarak, çalışan bilgilerini sisteme kaydetmek istiyorum ki eğitim, sertifika ve risk yönetimi çalışan bazlı yapılabilsin. |

### Neden Yapılıyor?
Çalışan, İSG platformunun merkez entity'sidir. Eğitim katılımı, sertifika, risk değerlendirme, KKD ataması — hepsi çalışana bağlıdır.

### Ürüne Hangi Değeri Katıyor?
- Çalışan bazlı İSG takibi
- Yasal zorunluluk (çalışan İSG dosyası)
- Departman ve lokasyon bazlı raporlama

### Teknik Amaç
Employee aggregate root, EmploymentInfo, EmergencyContact value object'leri.

### Yapılacak Teknik İşler
1. `EmploymentInfo` value object (Position, Title, StartDate, EndDate, Type, SgkNo)
2. `EmergencyContact` value object (Name, Relationship, Phone)
3. `EmploymentType` enumeration (FullTime, PartTime, Contract, Intern)
4. `EmployeeStatus` enumeration (Active, OnLeave, Suspended, Terminated)
5. `EmployeeDocument` entity (Type, FileName, FilePath, UploadedAt)
6. `DocumentType` enumeration (HealthReport, IdCopy, Contract, Other)
7. `Employee` aggregate root (ITenantEntity, DepartmentId, LocationId)
8. Domain metotlar: Hire, Terminate, Transfer, UpdatePosition, AddDocument
9. State machine: Active ↔ OnLeave ↔ Suspended → Terminated
10. Domain events: EmployeeHiredDomainEvent, EmployeeTerminatedDomainEvent, EmployeeTransferredDomainEvent
11. Unit test'ler

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Domain.Employees.Entities.Employee
SafeFlow.Domain.Employees.Entities.EmployeeDocument
SafeFlow.Domain.Employees.ValueObjects.EmploymentInfo
SafeFlow.Domain.Employees.ValueObjects.EmergencyContact
SafeFlow.Domain.Employees.Enums.EmploymentType
SafeFlow.Domain.Employees.Enums.EmployeeStatus
SafeFlow.Domain.Employees.Enums.DocumentType
SafeFlow.Domain.Employees.Events.EmployeeHiredDomainEvent
SafeFlow.Domain.Employees.Events.EmployeeTerminatedDomainEvent
SafeFlow.Domain.Employees.Events.EmployeeTransferredDomainEvent
```

### Oluşturulacak Dosyalar
```
src/SafeFlow.Domain/Employees/Entities/Employee.cs
src/SafeFlow.Domain/Employees/Entities/EmployeeDocument.cs
src/SafeFlow.Domain/Employees/ValueObjects/EmploymentInfo.cs
src/SafeFlow.Domain/Employees/ValueObjects/EmergencyContact.cs
src/SafeFlow.Domain/Employees/Enums/EmploymentType.cs
src/SafeFlow.Domain/Employees/Enums/EmployeeStatus.cs
src/SafeFlow.Domain/Employees/Enums/DocumentType.cs
src/SafeFlow.Domain/Employees/Events/EmployeeHiredDomainEvent.cs
src/SafeFlow.Domain/Employees/Events/EmployeeTerminatedDomainEvent.cs
src/SafeFlow.Domain/Employees/Events/EmployeeTransferredDomainEvent.cs
tests/SafeFlow.Domain.Tests/Employees/EmployeeTests.cs
tests/SafeFlow.Domain.Tests/Employees/EmployeeStateTests.cs
```

### Bağımlı Olduğu Görevler
- TASK 0.2 (Domain base classes)
- TASK 3.1 (Department, Location — referans)

### Acceptance Criteria
- [ ] Employee state machine doğru çalışıyor
- [ ] Transfer ile departman/lokasyon değişebiliyor
- [ ] Terminated employee'ye tekrar işlem yapılamıyor
- [ ] EmergencyContact ve EmploymentInfo immutable
- [ ] Domain event'ler dispatch ediliyor

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(domain): add Employee aggregate with state machine and value objects
```

---

## TASK 4.2 — Employee EF Core Konfigürasyonu ve Migration

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 4 — Employee Management |
| **Feature** | Employee Persistence |
| **User Story** | Geliştirici olarak, Employee entity'sinin veritabanına doğru map'lenmesini istiyorum. |

### Yapılacak Teknik İşler
1. `EmployeeConfiguration` — owned types, FK'ler
2. `EmployeeDocumentConfiguration`
3. Migration oluştur
4. Index'ler: tenant_id + department_id, tc_kimlik_no (unique per tenant)

### Database Değişiklikleri
- `employees` tablosu (id, tenant_id, user_id, department_id, location_id, first_name, last_name, tc_kimlik_no, position, title, start_date, end_date, employment_type, sgk_no, emergency_name, emergency_relationship, emergency_phone, status, is_deleted, created_at, ...)
- `employee_documents` tablosu (id, employee_id, type, file_name, file_path, uploaded_at)

### Migration Gerekiyor mu?
- **Evet** — `AddEmployee` migration

### Bağımlı Olduğu Görevler
- TASK 3.2 (Company migration)
- TASK 4.1 (Employee entity)

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(infrastructure): add Employee EF Core configuration and migration
```

---

## TASK 4.3 — Employee Repository

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 4 — Employee Management |
| **Feature** | Employee Data Access |
| **User Story** | Geliştirici olarak, Employee'ye domain-specific veri erişimi istiyorum. |

### Yapılacak Teknik İşler
1. `IEmployeeRepository` interface
2. `EmployeeRepository` implementasyonu
3. Metotlar: GetByIdAsync, GetByUserIdAsync, GetByDepartmentAsync, SearchAsync, CountByTenantAsync, AddAsync, Update
4. DI registration
5. Integration test

### Bağımlı Olduğu Görevler
- TASK 4.2 (Migration)

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(infrastructure): add IEmployeeRepository with domain-specific queries
```

---

## TASK 4.4 — Employee CRUD Endpoint'leri

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 4 — Employee Management |
| **Feature** | Employee CRUD API |
| **User Story** | İSG Uzmanı olarak, çalışanları listelemek, eklemek, güncellemek ve transfer etmek istiyorum ki organizasyondaki tüm çalışanları yönetebilyrityim. |

### Yapılacak Teknik İşler
1. `CreateEmployeeCommand` + handler + validator
2. `UpdateEmployeeCommand` + handler + validator
3. `TransferEmployeeCommand` + handler (departman/lokasyon değişimi)
4. `GetEmployeesQuery` + handler (paginated, search, filter by dept/location/status)
5. `GetEmployeeByIdQuery` + handler (detaylı — eğitim/sertifika özet dahil)
6. API endpoint'leri
7. Unit test + integration test

### Oluşturulacak Endpointler
- `GET /v1/employees` 🔒 Authenticated
- `GET /v1/employees/{id}` 🔒 Authenticated
- `POST /v1/employees` 🔒 `users.create`
- `PUT /v1/employees/{id}` 🔒 `users.update`
- `PATCH /v1/employees/{id}/transfer` 🔒 `users.update`

### Validation Kuralları
- FirstName, LastName: zorunlu, max 100
- TcKimlikNo: 11 hane, Türk TC Kimlik algoritması doğrulama
- DepartmentId: geçerli departman olmalı
- LocationId: geçerli lokasyon olmalı
- StartDate: gelecek veya bugün olmalı (yeni kayıt)
- EmergencyContact: Name ve Phone zorunlu

### Domain Event
- `EmployeeHiredDomainEvent` — Yeni çalışan oluşturulduğunda
- `EmployeeTransferredDomainEvent` — Transfer işleminde

### Log Kayıtları
- `[INFO] Employee created: {EmployeeId} in department {DeptId}`
- `[INFO] Employee transferred: {EmployeeId} from {OldDept} to {NewDept}`

### Audit Log
- Evet — Tüm CRUD işlemleri

### Cache
- Evet — Çalışan listesi kısa süreli cache

### Transaction
- Evet — Transfer işlemi tek transaction

### Bağımlı Olduğu Görevler
- TASK 4.3 (Repository)
- TASK 3.4 (Department — validation)
- TASK 1.11 (Permission auth)

### Tahmini Süre
**1 gün**

### Önerilen Git Commit Mesajı
```
feat(api): add Employee CRUD endpoints with TC validation and transfer support
```

---

## TASK 4.5 — Employee Search ve Filtering

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 4 — Employee Management |
| **Feature** | Advanced Employee Search |
| **User Story** | İSG Uzmanı olarak, çalışanları isim, TC, departman, lokasyon ve duruma göre aramak istiyorum ki büyük organizasyonlarda hızlıca doğru çalışanı bulabilyrityim. |

### Yapılacak Teknik İşler
1. `EmployeeSearchCriteria` sınıfı (search, departmentId, locationId, status, employmentType)
2. Repository search metodu güncelleme (dynamic LINQ)
3. Full-text search hazırlığı (SQL Server Full-Text Search — opsiyonel)
4. Sonuçlarda eğitim ve sertifika özet bilgisi

### Bağımlı Olduğu Görevler
- TASK 4.4 (CRUD)

### Performans Notları
- Search query'de index kullanımı critical
- LIKE '%term%' yerine full-text search tercih edilmeli
- Büyük dataset'lerde cursor-based pagination değerlendirilmeli

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(api): add advanced employee search and filtering
```

---

## EPIC 4 — Tamamlanma Özeti

| Metrik | Değer |
|--------|-------|
| **Toplam Görev** | 5 |
| **Toplam Tahmini Süre** | 24 saat (~3 iş günü) |
| **Teknik Borç** | Full-text search henüz implementasyon değil |
| **Refactoring Önerisi** | TC Kimlik validation ayrı service'e çıkarılabilir |

### Code Review Checklist — EPIC 4
- [ ] TC Kimlik No algoritma doğrulaması çalışıyor
- [ ] Employee state machine geçişleri korunuyor
- [ ] Terminated employee'ye yeni işlem yapılamıyor
- [ ] Transfer işlemi audit log'a yazılıyor
- [ ] Search query N+1 sorunu yok (AsSplitQuery veya projection)
