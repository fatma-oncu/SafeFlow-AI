# EPIC 8 — Dashboard

> **Epic Sahibi:** Tech Lead  
> **Öncelik:** P1 — High  
> **Tahmini Toplam Süre:** 4 iş günü  
> **Bağımlılık:** EPIC 5, 7 (Training, Certificate)  
> **Hedef Sprint:** Sprint 6

---

## TASK 8.1 — Dashboard Summary Endpoint

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 8 — Dashboard |
| **Feature** | Dashboard Summary |
| **User Story** | İşveren/Yönetici olarak, tek bakışta İSG uyum durumunu görmek istiyorum ki hızlı kararlar alabilyrityim. |

### Neden Yapılıyor?
Dashboard, platformun "vitrinidir". Yönetici ilk açtığında organizasyonun İSG durumunu anında görmeli.

### Ürüne Hangi Değeri Katıyor?
- Anlık İSG durum görünürlüğü
- Karar destek (hangi departman riskli, hangi eğitim eksik)
- Yönetici memnuniyeti (proaktif bilgi sunumu)

### Yapılacak Teknik İşler
1. `GetDashboardSummaryQuery` + handler
2. `DashboardSummaryResponse` DTO (overview, trainingStats, certificateStats, upcomingTrainings, recentActivities)
3. Dashboard veri hesaplamaları (aggregate query'ler)
4. `GET /v1/dashboard/summary` endpoint
5. Cache entegrasyonu (5 dakika TTL)
6. Integration test

### Oluşturulacak Endpointler
- `GET /v1/dashboard/summary` 🔒 `dashboard.view`

### Cache
- **Evet** — 5 dakika TTL, tenant bazlı cache key

### Bağımlı Olduğu Görevler
- TASK 5.5 (Training endpoints — veri)
- TASK 7.4 (Certificate endpoints — veri)

### Performans Notları
- Aggregate query'ler SQL Server Indexed Views ile optimize edilebilir (gelecek)
- Cache invalidation: TrainingCompletedDomainEvent, CertificateIssuedDomainEvent event'lerinde

### Tahmini Süre
**1 gün**

### Önerilen Git Commit Mesajı
```
feat(api): add dashboard summary endpoint with caching
```

---

## TASK 8.2 — Dashboard Trends Endpoint

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 8 — Dashboard |
| **Feature** | Trend Analysis |
| **User Story** | İşveren/Yönetici olarak, eğitim uyum oranı ve sertifika durumunun aylık trendlerini görmek istiyorum ki iyileşme veya bozulma eğilimlerini fark edebilyrityim. |

### Yapılacak Teknik İşler
1. `GetDashboardTrendsQuery` + handler (period: 1m/3m/6m/1y, departmentId opsiyonel)
2. `DashboardTrendsResponse` DTO (trainingTrend, complianceTrend, departmentComparison)
3. Aylık bazlı aggregate query'ler (GROUP BY month)
4. Departman karşılaştırma (compliance rate per department)
5. `GET /v1/dashboard/trends` endpoint
6. Cache (10 dakika TTL)

### Oluşturulacak Endpointler
- `GET /v1/dashboard/trends` 🔒 `dashboard.view`

### Cache
- **Evet** — 10 dakika TTL

### Bağımlı Olduğu Görevler
- TASK 8.1 (Dashboard summary)
- TASK 3.4 (Department — karşılaştırma)

### Tahmini Süre
**1 gün**

### Önerilen Git Commit Mesajı
```
feat(api): add dashboard trends endpoint with department comparison
```

---

## TASK 8.3 — Dashboard Widget Sistemi (Altyapı)

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 8 — Dashboard |
| **Feature** | Widget-Based Dashboard Architecture |
| **User Story** | Geliştirici olarak, dashboard'u widget bazlı tasarlamak istiyorum ki gelecekte yeni widget'lar (AI önerileri, risk durumu) kolayca eklenebilsin. |

### Yapılacak Teknik İşler
1. `IDashboardWidget` interface (WidgetType, GetData, GetConfiguration)
2. `TrainingComplianceWidget` implementasyonu
3. `CertificateStatusWidget` implementasyonu
4. `UpcomingTrainingsWidget` implementasyonu
5. `RecentActivitiesWidget` implementasyonu
6. Widget registry ve DI entegrasyonu
7. `GET /v1/dashboard/widgets` endpoint (tüm widget'ları listele)
8. `GET /v1/dashboard/widgets/{type}` endpoint (tekil widget verisi)

### Oluşturulacak Endpointler
- `GET /v1/dashboard/widgets` 🔒 `dashboard.view`
- `GET /v1/dashboard/widgets/{type}` 🔒 `dashboard.view`

### Bağımlı Olduğu Görevler
- TASK 8.1 (Dashboard summary — veri altyapısı)

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(api): add widget-based dashboard architecture with extensible widget registry
```

---

## EPIC 8 — Tamamlanma Özeti

| Metrik | Değer |
|--------|-------|
| **Toplam Görev** | 3 |
| **Toplam Tahmini Süre** | 20 saat (~2.5 iş günü) |
| **Teknik Borç** | Materialized view optimizasyonu gerekebilir (büyük veri) |
| **Refactoring Önerisi** | Widget sistemi event-driven cache invalidation ile güçlendirilebilir |

### Code Review Checklist — EPIC 8
- [ ] Dashboard query'ler N+1 sorunu yok
- [ ] Cache invalidation event handler'larla yapılıyor
- [ ] Department comparison yalnızca kendi tenant departmanları
- [ ] Trend hesaplamaları UTC datetime kullanıyor
- [ ] Widget registry yeni widget eklemeye açık
