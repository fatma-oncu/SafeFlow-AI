# EPIC 7 — Certificate Management

> **Epic Sahibi:** Tech Lead  
> **Öncelik:** P0 — Critical  
> **Tahmini Toplam Süre:** 5 iş günü  
> **Bağımlılık:** EPIC 5 (Training Management)  
> **Hedef Sprint:** Sprint 5–6

---

## TASK 7.1 — Certificate Entity ve Domain Modeli

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 7 — Certificate Management |
| **Feature** | Certificate Domain Model |
| **User Story** | İSG Uzmanı olarak, eğitimi tamamlayan çalışanlara sertifika üretmek istiyorum ki yasal İSG eğitim belgelendirmesi yapılabilsin. |

### Neden Yapılıyor?
İSG mevzuatı gereği eğitim tamamlama belgeleri üretilmeli ve saklanmalıdır. Sertifika süresi dolduğunda eğitim yenilenmelidir.

### Ürüne Hangi Değeri Katıyor?
- Yasal belgelendirme otomasyonu
- Sertifika geçerlilik takibi (süre, yenileme)
- PDF sertifika üretimi
- Otomatik yenileme hatırlatma

### Yapılacak Teknik İşler
1. `CertificateNumber` value object (Prefix, Year, Sequence, FullNumber, static Generate)
2. `ValidityPeriod` value object (IssuedAt, ExpiresAt, DurationMonths, IsExpired, DaysUntilExpiry, IsExpiringSoon)
3. `CertificateStatus` enumeration (Draft, Issued, Active, ExpiringSoon, Expired, Suspended, Revoked)
4. `Certificate` aggregate root (ITenantEntity, EmployeeId, TrainingId, TemplateId)
5. Domain metotlar: Issue, Suspend, Revoke, Renew, MarkExpiring, MarkExpired
6. State machine: Draft → Issued → Active → ExpiringSoon → Expired / Suspended / Revoked
7. `CertificateTemplate` aggregate root (Name, Description, Layout, IsDefault)
8. `TemplateLayout` value object (HeaderText, BodyTemplate, FooterText, LogoPath)
9. Domain events: CertificateIssuedDomainEvent, CertificateExpiringDomainEvent, CertificateExpiredDomainEvent, CertificateRevokedDomainEvent
10. Unit test'ler

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Domain.Certificates.Entities.Certificate
SafeFlow.Domain.Certificates.Entities.CertificateTemplate
SafeFlow.Domain.Certificates.ValueObjects.CertificateNumber
SafeFlow.Domain.Certificates.ValueObjects.ValidityPeriod
SafeFlow.Domain.Certificates.ValueObjects.TemplateLayout
SafeFlow.Domain.Certificates.Enums.CertificateStatus
SafeFlow.Domain.Certificates.Events.CertificateIssuedDomainEvent
SafeFlow.Domain.Certificates.Events.CertificateExpiringDomainEvent
SafeFlow.Domain.Certificates.Events.CertificateExpiredDomainEvent
SafeFlow.Domain.Certificates.Events.CertificateRevokedDomainEvent
```

### Bağımlı Olduğu Görevler
- TASK 0.2 (Domain base classes)

### Acceptance Criteria
- [ ] Certificate state machine tüm geçişleri doğru
- [ ] CertificateNumber format: SF-{YEAR}-{SEQ:5} (örn: SF-2026-00001)
- [ ] ValidityPeriod IsExpiringSoon(30) 30 gün öncesini doğru hesaplıyor
- [ ] Revoked sertifika tekrar aktif edilemiyor
- [ ] Domain event'ler dispatch ediliyor

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(domain): add Certificate aggregate with state machine and validity tracking
```

---

## TASK 7.2 — Certificate Domain Service

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 7 — Certificate Management |
| **Feature** | Certificate Issuance Service |
| **User Story** | Sistem olarak, eğitimi başarıyla tamamlayan çalışanlara otomatik sertifika üretmek istiyorum. |

### Yapılacak Teknik İşler
1. `ICertificateIssuanceService` interface
2. `CertificateIssuanceService` implementasyonu
3. `ICertificateNumberGenerator` interface (sequential, tenant-scoped)
4. `CertificateNumberGenerator` implementasyonu
5. `IssueCertificate` — tekil sertifika
6. `IssueBulkCertificates` — oturum tamamlandığında toplu
7. Eğitim tipine göre geçerlilik süresi belirleme
8. Unit test'ler

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Domain.Certificates.Services.ICertificateIssuanceService
SafeFlow.Application.Certificates.Services.CertificateIssuanceService
SafeFlow.Domain.Certificates.Services.ICertificateNumberGenerator
SafeFlow.Infrastructure.Certificates.CertificateNumberGenerator
```

### Bağımlı Olduğu Görevler
- TASK 7.1 (Certificate entity)

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(application): add certificate issuance service with bulk generation
```

---

## TASK 7.3 — Certificate EF Core Konfigürasyonu ve Migration

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 7 — Certificate Management |
| **Feature** | Certificate Persistence |

### Yapılacak Teknik İşler
1. `CertificateConfiguration` — owned types, FK, indexes
2. `CertificateTemplateConfiguration`
3. Migration oluştur
4. Seed data — varsayılan sertifika şablonu

### Database Değişiklikleri
- `certificates` tablosu (id, tenant_id, number_prefix, number_year, number_sequence, full_number, employee_id, training_id, template_id, issued_at, expires_at, duration_months, status, file_path, notes, ...)
- `certificate_templates` tablosu
- Index: `IX_certificates_full_number` (unique per tenant), `IX_certificates_employee_id`

### Migration Gerekiyor mu?
- **Evet**

### Bağımlı Olduğu Görevler
- TASK 5.3 (Training migration — FK)
- TASK 7.1 (Certificate entity)

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(infrastructure): add Certificate EF Core configuration and migration
```

---

## TASK 7.4 — Certificate Repository ve CRUD Endpoint'leri

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 7 — Certificate Management |
| **Feature** | Certificate CRUD API |
| **User Story** | İSG Uzmanı olarak, sertifikaları listelemek, detay görüntülemek ve iptal etmek istiyorum. |

### Yapılacak Teknik İşler
1. `ICertificateRepository` interface (Domain)
2. `CertificateRepository` implementasyonu
3. `GetCertificatesQuery` + handler (paginated, filtered by status/employee/training)
4. `GetCertificateByIdQuery` + handler
5. `RevokeCertificateCommand` + handler
6. `GetExpiringCertificatesQuery` + handler
7. API endpoint'leri
8. Unit test + integration test

### Oluşturulacak Endpointler
- `GET /v1/certificates` 🔒 `certificates.view`
- `GET /v1/certificates/{id}` 🔒 `certificates.view`
- `PATCH /v1/certificates/{id}/revoke` 🔒 `certificates.revoke`
- `GET /v1/certificates/expiring` 🔒 `certificates.view`

### Bağımlı Olduğu Görevler
- TASK 7.3 (Migration)
- TASK 1.11 (Permission auth)

### Cache
- Evet — Expiring certificates listesi 5 dakika cache

### Tahmini Süre
**1 gün**

### Önerilen Git Commit Mesajı
```
feat(api): add Certificate CRUD and expiring certificates endpoints
```

---

## TASK 7.5 — PDF Sertifika Üretimi

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 7 — Certificate Management |
| **Feature** | PDF Certificate Generation |
| **User Story** | İSG Uzmanı olarak, sertifikaları PDF formatında üretip indirmek istiyorum ki resmi belge olarak kullanılabilsin. |

### Yapılacak Teknik İşler
1. `ICertificatePdfService` interface
2. `QuestPdfCertificateService` implementasyonu (QuestPDF library)
3. PDF şablonu: şirket logosu, çalışan adı, eğitim adı, tarih, sertifika numarası, QR kod (opsiyonel)
4. `GenerateCertificatePdfCommand` + handler
5. `GET /v1/certificates/{id}/download` endpoint
6. Dosya depolama entegrasyonu (IFileStorageService)
7. Unit test + integration test

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Application.Common.Interfaces.ICertificatePdfService
SafeFlow.Infrastructure.Certificates.QuestPdfCertificateService
```

### Oluşturulacak Endpointler
- `GET /v1/certificates/{id}/download` 🔒 `certificates.download`

### Gerekli NuGet Paketleri
- `QuestPDF` (Infrastructure)

### Bağımlı Olduğu Görevler
- TASK 7.4 (Certificate CRUD)
- TASK 5.8 (File storage service)

### Acceptance Criteria
- [ ] PDF sertifika doğru bilgileri içeriyor
- [ ] Şirket logosu PDF'te görünüyor
- [ ] Download endpoint PDF dosyasını döndürüyor
- [ ] Content-Type: application/pdf
- [ ] Content-Disposition: attachment

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(infrastructure): add PDF certificate generation with QuestPDF
```

---

## TASK 7.6 — TrainingCompleted → Auto Certificate Event Handler

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 7 — Certificate Management |
| **Feature** | Automatic Certificate on Training Completion |
| **User Story** | Sistem olarak, eğitim tamamlandığında başarılı çalışanlara otomatik sertifika üretmek istiyorum ki manuel sertifika işlemi gerekmesiz. |

### Neden Yapılıyor?
TrainingCompleted domain event'i, Certificate Bounded Context'in otomatik sertifika üretmesini tetikler. Bu, domain event'lerin gerçek iş değeri ürettiği ilk senaryo.

### Yapılacak Teknik İşler
1. `TrainingCompletedEventHandler` — INotificationHandler<TrainingCompleted>
2. Handler: başarılı katılımcıları al, ICertificateIssuanceService ile toplu sertifika üret
3. PDF üretimi trigger
4. CertificateIssued event → Notification handler'a sinyal
5. Unit test + integration test

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Application.Certificates.EventHandlers.TrainingCompletedEventHandler
```

### Bağımlı Olduğu Görevler
- TASK 5.6 (TrainingSessionCompleted event)
- TASK 7.2 (Certificate issuance service)

### Acceptance Criteria
- [ ] TrainingSession complete → başarılı katılımcılara otomatik sertifika
- [ ] Sertifika numarası otomatik sequential
- [ ] CertificateIssuedDomainEvent event dispatch ediliyor
- [ ] Başarısız (score < pass) katılımcıya sertifika üretilmiyor

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(application): add auto certificate generation on training completion

- Handle TrainingCompletedDomainEvent
- Generate bulk certificates for passed participants
- Dispatch CertificateIssuedDomainEvent events
```

---

## EPIC 7 — Tamamlanma Özeti

| Metrik | Değer |
|--------|-------|
| **Toplam Görev** | 6 |
| **Toplam Tahmini Süre** | 28 saat (~3.5 iş günü) |
| **Teknik Borç** | QR code doğrulama henüz yok |
| **Refactoring Önerisi** | Certificate template engine daha esnek hale getirilebilir |

### Code Review Checklist — EPIC 7
- [ ] Certificate number uniqueness garantisi (concurrent generation)
- [ ] PDF üretimi async ve non-blocking
- [ ] TrainingCompleted event handler idempotent (aynı event iki kez gelirse duplicate sertifika oluşmaz)
- [ ] Revoked sertifika PDF'i "İPTAL" damgası içermeli
- [ ] ValidityPeriod hesaplamaları timezone-safe
