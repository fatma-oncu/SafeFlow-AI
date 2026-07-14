# EPIC 9 — Notification

> **Epic Sahibi:** Tech Lead  
> **Öncelik:** P1 — High  
> **Tahmini Toplam Süre:** 4 iş günü  
> **Bağımlılık:** EPIC 5, 7 (Training, Certificate)  
> **Hedef Sprint:** Sprint 6–7

---

## TASK 9.1 — Notification Entity ve Domain Modeli

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 9 — Notification |
| **Feature** | Notification Domain Model |
| **User Story** | Kullanıcı olarak, önemli olaylar hakkında bildirim almak istiyorum ki eğitim, sertifika ve görevlerimi takip edebilyrityim. |

### Yapılacak Teknik İşler
1. `Notification` aggregate root (RecipientId, Type, Channel, Title, Message, Priority, Status)
2. `NotificationType` enumeration (TrainingReminder, CertificateExpiring, TaskAssigned, SystemAlert)
3. `NotificationChannel` enumeration (InApp, Email, SMS, Push)
4. `NotificationPriority` enumeration (Low, Normal, High, Critical)
5. `NotificationStatus` enumeration (Pending, Sent, Read, Failed)
6. Domain metotlar: MarkAsRead, MarkAsSent, MarkAsFailed
7. Unit test'ler

### Oluşturulacak Endpointler
- Yok (domain katmanı)

### Bağımlı Olduğu Görevler
- TASK 0.2 (Domain base classes)

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(domain): add Notification aggregate with type, channel and priority
```

---

## TASK 9.2 — Notification Persistence ve Repository

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 9 — Notification |
| **Feature** | Notification Persistence |

### Yapılacak Teknik İşler
1. `NotificationConfiguration`
2. `INotificationRepository` interface
3. `NotificationRepository` implementasyonu
4. Migration
5. Integration test

### Database Değişiklikleri
- `notifications` tablosu (id, tenant_id, recipient_id, type, channel, title, message, action_url, priority, status, read_at, sent_at, failed_reason, created_at)
- Index: `IX_notifications_recipient_status`

### Migration Gerekiyor mu?
- **Evet**

### Bağımlı Olduğu Görevler
- TASK 9.1

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(infrastructure): add Notification EF Core configuration and migration
```

---

## TASK 9.3 — Notification Dispatch Service ve Domain Event Handler'ları

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 9 — Notification |
| **Feature** | Notification Dispatching |
| **User Story** | Sistem olarak, domain event'ler gerçekleştiğinde ilgili kullanıcılara otomatik bildirim göndermek istiyorum. |

### Yapılacak Teknik İşler
1. `INotificationDispatchService` interface (Application)
2. `NotificationDispatchService` implementasyonu
3. `IEmailService` interface + basit SMTP implementasyonu
4. Event handler'lar:
   - `TrainingPublishedNotificationHandler` — Eğitim yayınlandığında departman çalışanlarına bildirim
   - `CertificateExpiringNotificationHandler` — Sertifika süresi dolmak üzere bildirim
   - `CertificateIssuedNotificationHandler` — Sertifika üretildiğinde çalışana bildirim
   - `ParticipantEnrolledNotificationHandler` — Eğitime kaydedildiğinde çalışana bildirim
5. Notification template sistemi (basit string interpolation)
6. Unit test + integration test

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Application.Common.Interfaces.INotificationDispatchService
SafeFlow.Application.Common.Interfaces.IEmailService
SafeFlow.Infrastructure.Notifications.NotificationDispatchService
SafeFlow.Infrastructure.Email.SmtpEmailService
SafeFlow.Application.Notifications.EventHandlers.TrainingPublishedNotificationHandler
SafeFlow.Application.Notifications.EventHandlers.CertificateExpiringNotificationHandler
SafeFlow.Application.Notifications.EventHandlers.CertificateIssuedNotificationHandler
SafeFlow.Application.Notifications.EventHandlers.ParticipantEnrolledNotificationHandler
```

### Bağımlı Olduğu Görevler
- TASK 9.2 (Repository)
- TASK 5.5 (TrainingPublishedDomainEvent event)
- TASK 7.6 (CertificateIssuedDomainEvent event)

### Acceptance Criteria
- [ ] TrainingPublishedDomainEvent → departman çalışanlarına in-app bildirim
- [ ] CertificateIssuedDomainEvent → çalışana in-app + email bildirim
- [ ] CertificateExpiringDomainEvent → çalışan + İSG uzmanına bildirim
- [ ] Bildirim templatelerden üretiliyor
- [ ] Bildirim veritabanına kaydediliyor (audit trail)

### Tahmini Süre
**1 gün**

### Önerilen Git Commit Mesajı
```
feat(application): add notification dispatch service and domain event handlers
```

---

## TASK 9.4 — Notification API Endpoint'leri

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 9 — Notification |
| **Feature** | Notification API |
| **User Story** | Kullanıcı olarak, bildirimlerimi listelemek, okundu olarak işaretlemek ve okunmamış sayısını görmek istiyorum. |

### Yapılacak Teknik İşler
1. `GetNotificationsQuery` + handler (paginated, filtered by isRead/type)
2. `GetUnreadCountQuery` + handler
3. `MarkAsReadCommand` + handler
4. `MarkAllAsReadCommand` + handler
5. API endpoint'leri
6. Unit test + integration test

### Oluşturulacak Endpointler
- `GET /v1/notifications` 🔒 Authenticated
- `GET /v1/notifications/unread-count` 🔒 Authenticated
- `PATCH /v1/notifications/{id}/read` 🔒 Authenticated
- `PATCH /v1/notifications/read-all` 🔒 Authenticated

### Cache
- Evet — Unread count 1 dakika cache

### Bağımlı Olduğu Görevler
- TASK 9.2 (Repository)

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(api): add Notification endpoints for listing and read management
```

---

## TASK 9.5 — Sertifika Hatırlatma Background Job

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 9 — Notification |
| **Feature** | Certificate Expiry Reminder Job |
| **User Story** | Sistem olarak, süresi dolmak üzere olan sertifikalar için günlük otomatik hatırlatma göndermek istiyorum. |

### Yapılacak Teknik İşler
1. `ICertificateExpiryChecker` interface
2. `CertificateExpiryCheckerJob` — Hangfire recurring job
3. Günlük çalışma: 30 gün, 7 gün, 1 gün öncesi bildirim
4. Certificate status güncelleme: Active → ExpiringSoon
5. Hangfire DI registration
6. Integration test

### Gerekli NuGet Paketleri
- `Hangfire.Core` (Infrastructure)
- `Hangfire.AspNetCore` (API)
- `Hangfire.PostgreSql` (Infrastructure)

### Bağımlı Olduğu Görevler
- TASK 7.4 (Certificate repository)
- TASK 9.3 (Notification dispatch)

### Acceptance Criteria
- [ ] Günlük çalışan job süresi dolacak sertifikaları buluyor
- [ ] 30/7/1 gün eşiklerinde bildirim gönderiyor
- [ ] Certificate status'u ExpiringSoon'a güncelliyor
- [ ] Duplicate bildirim göndermiyor (aynı gün)

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(infrastructure): add certificate expiry reminder background job with Hangfire
```

---

## EPIC 9 — Tamamlanma Özeti

| Metrik | Değer |
|--------|-------|
| **Toplam Görev** | 5 |
| **Toplam Tahmini Süre** | 24 saat (~3 iş günü) |
| **Teknik Borç** | Push notification (FCM) henüz yok |
| **Refactoring Önerisi** | Template engine Razor veya Scriban ile güçlendirilebilir |

### Code Review Checklist — EPIC 9
- [ ] Event handler'lar idempotent (aynı event iki kez gelse tekrar bildirim oluşturmaz)
- [ ] Email gönderimi async ve non-blocking
- [ ] Hangfire job retry policy tanımlı
- [ ] Notification recipient yalnızca kendi tenant'ı
- [ ] Unread count cache invalidation MarkAsRead'de yapılıyor
