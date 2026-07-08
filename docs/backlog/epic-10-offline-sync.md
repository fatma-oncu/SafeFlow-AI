# EPIC 10 — Offline Synchronization

> **Epic Sahibi:** Tech Lead  
> **Öncelik:** P1 — High  
> **Tahmini Toplam Süre:** 5 iş günü  
> **Bağımlılık:** EPIC 5, 7 (Training, Certificate)  
> **Hedef Sprint:** Sprint 7

---

## TASK 10.1 — Sync API Altyapısı (Delta Sync)

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 10 — Offline Synchronization |
| **Feature** | Delta Sync API |
| **User Story** | Saha çalışanı olarak, internet bağlantısı kesildiğinde de çalışmaya devam etmek ve bağlantı geri geldiğinde verilerimin senkronize olmasını istiyorum. |

### Neden Yapılıyor?
İSG saha çalışanları (fabrika, şantiye, maden) sıklıkla internet erişimi olmayan ortamlarda çalışır. Offline-first yaklaşım bu kullanıcılar için zorunludur.

### Ürüne Hangi Değeri Katıyor?
- Saha çalışanları için kesintisiz iş süreci
- Veri kaybı önleme
- Rekabetçi avantaj (çoğu İSG yazılımı offline desteklemez)

### Teknik Amaç
Server-side delta sync API: son sync'ten bu yana değişen verileri döndürme ve offline yapılan değişiklikleri alma.

### Yapılacak Teknik İşler
1. `SyncPullQuery` + handler (lastSyncTimestamp, entity listesi → delta changes)
2. `SyncPushCommand` + handler (offline operations → server apply)
3. `SyncStatusQuery` + handler
4. Change tracking mekanizması (entity'lerde UpdatedAt bazlı delta)
5. `SyncPullResponse` DTO (created, updated, deleted per entity type)
6. `SyncPushResponse` DTO (processed, conflicts, errors)
7. `SyncOperation` model (entityType, entityId, operationType, data, timestamp)
8. API endpoint'leri
9. Integration test

### Oluşturulacak Endpointler
- `POST /v1/sync/pull` 🔒 Authenticated
- `POST /v1/sync/push` 🔒 Authenticated
- `GET /v1/sync/status` 🔒 Authenticated

### Bağımlı Olduğu Görevler
- TASK 5.4 (Training repository — sync verisi)
- TASK 7.4 (Certificate repository — sync verisi)

### Acceptance Criteria
- [ ] Pull: lastSyncTimestamp'ten sonra değişen veriler dönüyor
- [ ] Push: offline yapılan değişiklikler başarıyla uygulanıyor
- [ ] Conflict detection: aynı entity server'da da değiştiyse conflict raporu
- [ ] hasMore pagination desteği (büyük delta'lar için)

### Performans Notları
- UpdatedAt index'i critical (delta query performansı)
- Bulk insert/update batch processing
- Response pagination (büyük sync paketlerinde)

### Tahmini Süre
**1 gün**

### Önerilen Git Commit Mesajı
```
feat(api): add delta sync API for offline-first mobile support
```

---

## TASK 10.2 — Conflict Resolution Stratejisi

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 10 — Offline Synchronization |
| **Feature** | Conflict Resolution |
| **User Story** | Geliştirici olarak, aynı verinin hem offline hem online değiştirilmesi durumunda çakışma çözüm stratejisi istiyorum. |

### Yapılacak Teknik İşler
1. `IConflictResolver` interface
2. `LastWriteWinsResolver` implementasyonu (varsayılan)
3. `ServerWinsResolver` implementasyonu (kritik veriler için)
4. `ConflictRecord` entity (hangi entity, ne değişti, çözüm sonucu)
5. Conflict log ve admin görüntüleme
6. Unit test'ler

### Conflict Resolution Kuralları
- Eğitim katılım sonucu: **Server wins** (yetkili kişi onaylar)
- Çalışan bilgileri: **Last write wins** (timestamp karşılaştırma)
- Sertifika status: **Server wins** (güvenlik kritik)

### Bağımlı Olduğu Görevler
- TASK 10.1 (Sync API)

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(infrastructure): add conflict resolution strategies for offline sync
```

---

## TASK 10.3 — Retry Policy ve Queue Yönetimi

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 10 — Offline Synchronization |
| **Feature** | Sync Retry & Queue |
| **User Story** | Sistem olarak, sync başarısız olursa otomatik yeniden deneme yapılmasını istiyorum. |

### Yapılacak Teknik İşler
1. Retry policy tanımlama (exponential backoff: 1s, 5s, 30s, 5m, 30m)
2. `SyncJobStatus` tracking (Pending, InProgress, Completed, Failed, Retrying)
3. Maximum retry count (5 deneme)
4. Failed sync admin alerting
5. Sync health check endpoint güncellemesi

### Bağımlı Olduğu Görevler
- TASK 10.1 (Sync API)

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(infrastructure): add sync retry policy with exponential backoff
```

---

## TASK 10.4 — Sync Audit Log ve Monitoring

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 10 — Offline Synchronization |
| **Feature** | Sync Monitoring |
| **User Story** | Sistem yöneticisi olarak, sync işlemlerini izlemek ve hataları tespit etmek istiyorum. |

### Yapılacak Teknik İşler
1. `SyncLog` entity (DeviceId, Direction, EntityCount, Status, Duration, ErrorMessage)
2. `SyncLogConfiguration` — EF Core
3. Migration
4. Sync metrikler: başarılı/başarısız oranı, ortalama süre, conflict sayısı
5. Admin sync dashboard verisi

### Database Değişiklikleri
- `sync_logs` tablosu
- `sync_conflicts` tablosu

### Bağımlı Olduğu Görevler
- TASK 10.1, 10.2

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(infrastructure): add sync audit logging and monitoring
```

---

## EPIC 10 — Tamamlanma Özeti

| Metrik | Değer |
|--------|-------|
| **Toplam Görev** | 4 |
| **Toplam Tahmini Süre** | 24 saat (~3 iş günü) |
| **Teknik Borç** | Flutter client-side sync henüz yok (Flutter epic gerekir) |
| **Refactoring Önerisi** | Delta sync Debezium/CDC ile güçlendirilebilir (gelecek) |

### Code Review Checklist — EPIC 10
- [ ] Delta query UpdatedAt index kullanıyor
- [ ] Conflict resolution stratejisi entity tipine göre konfigüre edilebilir
- [ ] Retry policy exponential backoff ve max retry doğru
- [ ] Sync log'lar tenant-scoped
- [ ] Büyük sync paketleri streaming/pagination destekliyor
