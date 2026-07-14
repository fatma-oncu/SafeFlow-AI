# EPIC 5 — Training Management

> **Epic Sahibi:** Tech Lead  
> **Öncelik:** P0 — Critical  
> **Tahmini Toplam Süre:** 7 iş günü  
> **Bağımlılık:** EPIC 4 (Employee)  
> **Hedef Sprint:** Sprint 4–5

---

## TASK 5.1 — Training Entity ve Domain Modeli

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 5 — Training Management |
| **Feature** | Training Domain Model |
| **User Story** | İSG Uzmanı olarak, eğitim planlarını oluşturmak, oturumlar eklemek ve katılımcıları yönetmek istiyorum ki yasal zorunlu İSG eğitimlerini organize edebileyim. |

### Neden Yapılıyor?
6331 sayılı İSG Kanunu Md. 17 gereği tüm çalışanlara İSG eğitimi verilmesi zorunludur. Eğitim takibi olmadan yasal uyumluluk sağlanamaz.

### Ürüne Hangi Değeri Katıyor?
- Yasal zorunluluk (İSG eğitim takibi)
- Eğitim planı otomasyonu (periyodik yenileme)
- Katılım ve başarı takibi
- Sertifika üretimine girdi

### Teknik Amaç
Training aggregate root, TrainingSession, TrainingParticipation, TrainingMaterial entity'leri ve ilgili value object'leri implemente etmek.

### Yapılacak Teknik İşler
1. `TrainingType` enumeration (Mandatory, Optional, Orientation, Refresher, SpecialRisk)
2. `TrainingCategory` value object (Name, Code, IsLegal, LegalReference)
3. `Duration` value object (Hours, Minutes, TotalMinutes)
4. `TrainingPeriod` value object (Months, Description)
5. `TrainingStatus` enumeration (Draft, Published, InProgress, Completed, Cancelled, Archived)
6. `SessionStatus` enumeration (Scheduled, InProgress, Completed, Cancelled)
7. `AttendanceStatus` enumeration (Enrolled, Attended, Absent, Excused)
8. `MaterialType` enumeration (Presentation, Document, Video, Image)
9. `Training` aggregate root
10. `TrainingSession` entity (StartDate, EndDate, Location, Capacity, Participations)
11. `TrainingParticipation` entity (EmployeeId, AttendanceStatus, Score, IsPassed)
12. `TrainingMaterial` entity (Title, Type, FilePath, FileSize, OrderIndex)
13. Training domain metotları: AddSession, RemoveSession, AddMaterial, Publish, Cancel, Archive, Clone
14. TrainingSession metotları: Enroll, MarkAttendance, Complete, Cancel
15. Training state machine: Draft → Published → InProgress → Completed → Archived
16. Domain events: TrainingCreatedDomainEvent, TrainingPublishedDomainEvent, TrainingSessionScheduledDomainEvent, ParticipantEnrolledDomainEvent, TrainingSessionCompletedDomainEvent, TrainingCompletedDomainEvent, TrainingCancelledDomainEvent
17. Kapsamlı unit test'ler

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Domain.Trainings.Entities.Training
SafeFlow.Domain.Trainings.Entities.TrainingSession
SafeFlow.Domain.Trainings.Entities.TrainingParticipation
SafeFlow.Domain.Trainings.Entities.TrainingMaterial
SafeFlow.Domain.Trainings.ValueObjects.TrainingCategory
SafeFlow.Domain.Trainings.ValueObjects.Duration
SafeFlow.Domain.Trainings.ValueObjects.TrainingPeriod
SafeFlow.Domain.Trainings.Enums.TrainingType
SafeFlow.Domain.Trainings.Enums.TrainingStatus
SafeFlow.Domain.Trainings.Enums.SessionStatus
SafeFlow.Domain.Trainings.Enums.AttendanceStatus
SafeFlow.Domain.Trainings.Enums.MaterialType
SafeFlow.Domain.Trainings.Events.TrainingCreatedDomainEvent
SafeFlow.Domain.Trainings.Events.TrainingPublishedDomainEvent
SafeFlow.Domain.Trainings.Events.TrainingSessionScheduledDomainEvent
SafeFlow.Domain.Trainings.Events.ParticipantEnrolledDomainEvent
SafeFlow.Domain.Trainings.Events.TrainingSessionCompletedDomainEvent
SafeFlow.Domain.Trainings.Events.TrainingCompletedDomainEvent
SafeFlow.Domain.Trainings.Events.TrainingCancelledDomainEvent
```

### Bağımlı Olduğu Görevler
- TASK 0.2 (Domain base classes)

### Acceptance Criteria
- [ ] Training state machine tüm geçişleri doğru çalışıyor
- [ ] Published olmayan eğitime oturum eklenemiyor (Draft'ta eklenir, Published sonrası ek oturum eklenebilir)
- [ ] Oturum kapasitesi aşılamıyor (Enroll kontrolü)
- [ ] MarkAttendance yalnızca Enrolled katılımcılar için
- [ ] Session Complete olduğunda katılım sonuçları zorunlu
- [ ] TrainingCompleted event tüm oturumlar tamamlandığında
- [ ] Clone metodu yeni bir Draft eğitim oluşturuyor

### Tahmini Süre
**1 gün**

### Önerilen Git Commit Mesajı
```
feat(domain): add Training aggregate with session, participation and material management
```

---

## TASK 5.2 — Training Domain Service'leri

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 5 — Training Management |
| **Feature** | Training Domain Services |
| **User Story** | İSG Uzmanı olarak, çalışanların eğitim uygunluğunun otomatik kontrol edilmesini ve periyodik eğitim yenileme ihtiyaçlarının tespit edilmesini istiyorum. |

### Yapılacak Teknik İşler
1. `ITrainingEligibilityService` interface — çalışan eğitim uygunluk kontrolü
2. `TrainingEligibilityService` implementasyonu (aktif çalışan, departman uyumu, kontenjan, daha önce tamamlanma)
3. `ITrainingSchedulingService` interface — eğitim çizelgeleme, çakışma kontrolü
4. `TrainingSchedulingService` implementasyonu
5. `EligibilityResult` record (IsEligible, Reason, Warnings)
6. `TrainingRenewalInfo` record
7. Unit test'ler

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Domain.Trainings.Services.ITrainingEligibilityService
SafeFlow.Application.Trainings.Services.TrainingEligibilityService
SafeFlow.Domain.Trainings.Services.ITrainingSchedulingService
SafeFlow.Application.Trainings.Services.TrainingSchedulingService
SafeFlow.Domain.Trainings.Models.EligibilityResult
SafeFlow.Domain.Trainings.Models.TrainingRenewalInfo
```

### Bağımlı Olduğu Görevler
- TASK 5.1 (Training entity)
- TASK 4.3 (Employee repository)

### Acceptance Criteria
- [ ] Terminated çalışan eğitime katılamaz
- [ ] Kontenjan dolu ise uygunluk reddedilir
- [ ] Aynı eğitimi daha önce tamamlamış çalışan uyarı alır
- [ ] Periyodik yenileme tarihleri doğru hesaplanıyor

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(application): add training eligibility and scheduling domain services
```

---

## TASK 5.3 — Training EF Core Konfigürasyonu ve Migration

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 5 — Training Management |
| **Feature** | Training Persistence |
| **User Story** | Geliştirici olarak, Training entity'lerinin veritabanına doğru map'lenmesini istiyorum. |

### Yapılacak Teknik İşler
1. `TrainingConfiguration` — owned types, relationships
2. `TrainingSessionConfiguration`
3. `TrainingParticipationConfiguration`
4. `TrainingMaterialConfiguration`
5. Migration oluştur
6. Index'ler: tenant_id + status, session start_date

### Database Değişiklikleri
- `trainings` tablosu
- `training_sessions` tablosu
- `training_participations` tablosu
- `training_materials` tablosu

### Migration Gerekiyor mu?
- **Evet** — `AddTraining` migration

### Bağımlı Olduğu Görevler
- TASK 4.2 (Employee migration — FK)
- TASK 5.1 (Training entity)

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(infrastructure): add Training EF Core configuration and migration
```

---

## TASK 5.4 — Training Repository

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 5 — Training Management |
| **Feature** | Training Data Access |
| **User Story** | Geliştirici olarak, Training'e domain-specific veri erişimi istiyorum. |

### Yapılacak Teknik İşler
1. `ITrainingRepository` interface (Domain)
2. `TrainingRepository` implementasyonu
3. Metotlar: GetByIdAsync, GetByIdWithSessionsAsync, GetByIdWithFullDetailsAsync, GetByTenantAsync, GetUpcomingByDepartmentAsync, SearchAsync, ExistsAsync, AddAsync, Update, Remove
4. `TrainingSearchCriteria` sınıfı
5. Integration test'ler

### Bağımlı Olduğu Görevler
- TASK 5.3 (Migration)

### Performans Notları
- `GetByIdWithFullDetailsAsync` — `AsSplitQuery()` kullanılmalı (N+1 prevention)
- `GetUpcomingByDepartmentAsync` — date index'i critical
- Search sorgusu dinamik WHERE oluşturacak

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(infrastructure): add ITrainingRepository with domain-specific queries
```

---

## TASK 5.5 — Training CRUD Endpoint'leri

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 5 — Training Management |
| **Feature** | Training CRUD API |
| **User Story** | İSG Uzmanı olarak, eğitimleri oluşturmak, listelemek, güncellemek ve yayınlamak istiyorum. |

### Yapılacak Teknik İşler
1. `CreateTrainingCommand` + handler + validator
2. `UpdateTrainingCommand` + handler + validator
3. `PublishTrainingCommand` + handler (Draft → Published)
4. `CancelTrainingCommand` + handler
5. `GetTrainingsQuery` + handler (paginated, filtered)
6. `GetTrainingByIdQuery` + handler (full details)
7. API endpoint'leri
8. Unit test + integration test

### Oluşturulacak Endpointler
- `GET /v1/trainings` 🔒 `trainings.view`
- `GET /v1/trainings/{id}` 🔒 `trainings.view`
- `POST /v1/trainings` 🔒 `trainings.create`
- `PUT /v1/trainings/{id}` 🔒 `trainings.update`
- `PATCH /v1/trainings/{id}/publish` 🔒 `trainings.update`
- `PATCH /v1/trainings/{id}/cancel` 🔒 `trainings.update`

### Validation Kuralları
- Title: zorunlu, max 200 karakter
- DurationHours: 0-24 arasında
- MaxParticipants: 1-500 arasında
- InstructorId: geçerli kullanıcı olmalı
- CategoryCode: geçerli kategori olmalı

### Domain Event
- `TrainingCreatedDomainEvent`, `TrainingPublishedDomainEvent`, `TrainingCancelledDomainEvent`

### Cache
- Evet — Training listesi kısa süreli cache

### Bağımlı Olduğu Görevler
- TASK 5.4 (Repository)
- TASK 1.11 (Permission auth)

### Tahmini Süre
**1 gün**

### Önerilen Git Commit Mesajı
```
feat(api): add Training CRUD endpoints with state management
```

---

## TASK 5.6 — Training Session Yönetimi

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 5 — Training Management |
| **Feature** | Training Session Management |
| **User Story** | İSG Uzmanı olarak, eğitime oturumlar eklemek, tarih/yer/kapasite belirlemek ve oturumu tamamlamak istiyorum. |

### Yapılacak Teknik İşler
1. `AddTrainingSessionCommand` + handler + validator
2. `RemoveTrainingSessionCommand` + handler
3. `CompleteTrainingSessionCommand` + handler (katılım sonuçları ile)
4. API endpoint'leri
5. Unit test + integration test

### Oluşturulacak Endpointler
- `POST /v1/trainings/{id}/sessions` 🔒 `trainings.update`
- `DELETE /v1/trainings/{id}/sessions/{sessionId}` 🔒 `trainings.update`
- `PATCH /v1/trainings/{id}/sessions/{sessionId}/complete` 🔒 `trainings.complete`

### Domain Event
- `TrainingSessionScheduled`, `TrainingSessionCompleted`

### Bağımlı Olduğu Görevler
- TASK 5.5 (Training CRUD)

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(api): add Training Session management endpoints
```

---

## TASK 5.7 — Katılımcı Yönetimi (Enrollment)

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 5 — Training Management |
| **Feature** | Participant Enrollment |
| **User Story** | İSG Uzmanı olarak, eğitim oturumlarına çalışanları kaydetmek ve toplu kayıt yapmak istiyorum. |

### Yapılacak Teknik İşler
1. `EnrollParticipantsCommand` + handler (toplu kayıt)
2. `RemoveParticipantCommand` + handler
3. Eligibility check entegrasyonu (TASK 5.2)
4. API endpoint'leri
5. Unit test + integration test

### Oluşturulacak Endpointler
- `POST /v1/trainings/{id}/sessions/{sessionId}/participants` 🔒 `trainings.manage-participants`
- `DELETE /v1/trainings/{id}/sessions/{sessionId}/participants/{employeeId}` 🔒 `trainings.manage-participants`

### Domain Event
- `ParticipantEnrolled`

### Bağımlı Olduğu Görevler
- TASK 5.6 (Session yönetimi)
- TASK 5.2 (Eligibility service)

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(api): add participant enrollment with eligibility check
```

---

## TASK 5.8 — Eğitim Materyali Yükleme

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 5 — Training Management |
| **Feature** | Training Material Upload |
| **User Story** | İSG Uzmanı olarak, eğitim materyallerini (PDF, sunucu, video) sisteme yüklemek istiyorum ki katılımcılar materyallere erişebilsin. |

### Yapılacak Teknik İşler
1. `IFileStorageService` interface (Application)
2. `LocalFileStorageService` implementasyonu (MVP — local disk)
3. `UploadTrainingMaterialCommand` + handler
4. Dosya güvenlik kontrolleri (MIME, boyut, extension)
5. `POST /v1/trainings/{id}/materials` endpoint (multipart/form-data)
6. `GET /v1/trainings/{id}/materials/{materialId}/download` endpoint
7. Unit test + integration test

### Dosya Güvenlik Kuralları
- Max boyut: 50 MB
- İzin verilen MIME: pdf, pptx, docx, mp4, jpg, png
- Dosya adı sanitizasyonu (UUID prefix)
- Extension ve MIME type eşleşme kontrolü

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Application.Common.Interfaces.IFileStorageService
SafeFlow.Infrastructure.FileStorage.LocalFileStorageService
SafeFlow.Infrastructure.FileStorage.FileStorageSettings
```

### Bağımlı Olduğu Görevler
- TASK 5.5 (Training CRUD)

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(api): add training material upload with file security validation
```

---

## EPIC 5 — Tamamlanma Özeti

| Metrik | Değer |
|--------|-------|
| **Toplam Görev** | 8 |
| **Toplam Tahmini Süre** | 40 saat (~5 iş günü) |
| **Teknik Borç** | File storage MVP'de local disk — production'da blob storage gerekli |
| **Refactoring Önerisi** | TrainingSearchCriteria Specification Pattern'e çevrilebilir |

### Code Review Checklist — EPIC 5
- [ ] Training state machine tüm edge case'leri kapsıyor
- [ ] Enrollment kapasiste aşılamıyor (race condition koruması)
- [ ] Dosya yükleme güvenlik kontrolleri çalışıyor
- [ ] TrainingCompleted event tüm oturumlar tamamlandığında otomatik
- [ ] N+1 sorgu sorunu yok (Split Query kullanılıyor)
- [ ] Periyodik eğitim yenileme hesaplaması doğru
