# EPIC 6 — Hybrid Training (Online + Yüz Yüze)

> **Epic Sahibi:** Tech Lead  
> **Öncelik:** P1 — High  
> **Tahmini Toplam Süre:** 3 iş günü  
> **Bağımlılık:** EPIC 5 (Training Management)  
> **Hedef Sprint:** Sprint 5

---

## TASK 6.1 — Training Delivery Mode Desteği

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 6 — Hybrid Training |
| **Feature** | Training Delivery Mode |
| **User Story** | İSG Uzmanı olarak, eğitimleri yüz yüze, online veya hibrit olarak tanımlamak istiyorum ki farklı formatlardaki eğitimleri tek sistemde yönetebilyrityim. |

### Neden Yapılıyor?
Covid sonrası İSG eğitimlerinin önemli bir kısmı online veya hibrit formatta gerçekleşmektedir. Platform sadece yüz yüze eğitimi desteklerse müşteri ihtiyaçlarını karşılayamaz.

### Ürüne Hangi Değeri Katıyor?
- Çoklu eğitim formatı desteği (rekabetçi avantaj)
- Online eğitim ile coğrafi bağımsızlık
- Hibrit model ile esneklik

### Yapılacak Teknik İşler
1. `DeliveryMode` enumeration ekle (InPerson, Online, Hybrid)
2. `OnlineSessionInfo` value object (Platform, MeetingUrl, MeetingId, RecordingUrl)
3. Training entity'ye DeliveryMode ve OnlineSessionInfo ekle
4. TrainingSession entity'ye OnlineSessionInfo ekle
5. Migration güncelleme
6. API DTO'larını güncelle (create/update/response)
7. Unit test güncellemeleri

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Domain.Trainings.Enums.DeliveryMode
SafeFlow.Domain.Trainings.ValueObjects.OnlineSessionInfo
```

### Database Değişiklikleri
- `trainings` tablosuna `delivery_mode` kolonu
- `training_sessions` tablosuna `platform`, `meeting_url`, `meeting_id`, `recording_url` kolonları

### Migration Gerekiyor mu?
- **Evet** — `AddHybridTraining` migration

### Bağımlı Olduğu Görevler
- TASK 5.1 (Training entity)
- TASK 5.3 (Training migration)

### Acceptance Criteria
- [ ] Eğitim InPerson, Online veya Hybrid modda oluşturulabiliyor
- [ ] Online oturumlarda meeting URL zorunlu
- [ ] Hibrit eğitimde bazı oturumlar online, bazıları yüz yüze olabilir
- [ ] API response'larda delivery mode ve online bilgisi dönüyor

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(domain): add hybrid training support with online session info
```

---

## TASK 6.2 — Online Eğitim İçerik Takibi

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 6 — Hybrid Training |
| **Feature** | Online Content Progress Tracking |
| **User Story** | Çalışan olarak, online eğitim materyallerini izleme ilerlemesimin takip edilmesini istiyorum ki eğitimi tamamladığım doğrulanabilsin. |

### Yapılacak Teknik İşler
1. `ContentProgress` entity (EmployeeId, MaterialId, ProgressPercent, LastAccessedAt, CompletedAt)
2. `UpdateContentProgressCommand` + handler
3. `GetContentProgressQuery` + handler
4. Otomatik tamamlama: tüm materyaller %100 → attendance = Attended
5. API endpoint'leri
6. Unit test + integration test

### Oluşturulacak Endpointler
- `POST /v1/trainings/{id}/materials/{materialId}/progress` 🔒 Authenticated
- `GET /v1/trainings/{id}/progress` 🔒 Authenticated

### Bağımlı Olduğu Görevler
- TASK 5.8 (Material yükleme)
- TASK 6.1 (Delivery mode)

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(api): add online training content progress tracking
```

---

## TASK 6.3 — Online Eğitim Değerlendirme (Quiz)

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 6 — Hybrid Training |
| **Feature** | Training Quiz/Assessment |
| **User Story** | İSG Uzmanı olarak, online eğitimlere sınav/değerlendirme eklemek istiyorum ki çalışanların bilgi seviyesi ölçülebilsin. |

### Yapılacak Teknik İşler
1. `Quiz` entity (TrainingId, Title, PassScore, TimeLimit, Questions)
2. `QuizQuestion` entity (Text, Options, CorrectOption, Points)
3. `QuizAttempt` entity (EmployeeId, Score, IsPassed, StartedAt, CompletedAt)
4. `CreateQuizCommand` + handler
5. `SubmitQuizCommand` + handler (otomatik skorlama)
6. `GetQuizResultsQuery` + handler
7. API endpoint'leri
8. Migration

### Oluşturulacak Endpointler
- `POST /v1/trainings/{id}/quiz` 🔒 `trainings.update`
- `POST /v1/trainings/{id}/quiz/submit` 🔒 Authenticated
- `GET /v1/trainings/{id}/quiz/results` 🔒 `trainings.view`

### Database Değişiklikleri
- `quizzes` tablosu
- `quiz_questions` tablosu
- `quiz_attempts` tablosu

### Migration Gerekiyor mu?
- **Evet**

### Bağımlı Olduğu Görevler
- TASK 6.1 (Hybrid training)

### Tahmini Süre
**1 gün**

### Önerilen Git Commit Mesajı
```
feat(api): add quiz/assessment system for online trainings
```

---

## EPIC 6 — Tamamlanma Özeti

| Metrik | Değer |
|--------|-------|
| **Toplam Görev** | 3 |
| **Toplam Tahmini Süre** | 16 saat (~2 iş günü) |
| **Teknik Borç** | Video streaming henüz yok (material download ile MVP) |
| **Refactoring Önerisi** | Quiz engine gelecekte daha gelişmiş soru tipleri destekleyebilir |

### Code Review Checklist — EPIC 6
- [ ] Online eğitim meeting URL format validation var
- [ ] Content progress race condition koruması var
- [ ] Quiz skorlama doğru çalışıyor
- [ ] Tamamlama kriterleri (quiz + content progress) doğru entegre
