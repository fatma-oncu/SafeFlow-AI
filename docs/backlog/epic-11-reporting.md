# EPIC 11 — Reporting

> **Epic Sahibi:** Tech Lead  
> **Öncelik:** P1 — High  
> **Tahmini Toplam Süre:** 4 iş günü  
> **Bağımlılık:** EPIC 5, 7 (Training, Certificate)  
> **Hedef Sprint:** Sprint 7–8

---

## TASK 11.1 — Report Service Altyapısı

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 11 — Reporting |
| **Feature** | Report Service Infrastructure |
| **User Story** | İSG Uzmanı olarak, eğitim uyum raporları, sertifika durum raporları ve departman raporlarını PDF, Excel ve CSV formatlarında üretmek istiyorum. |

### Neden Yapılıyor?
İSG mevzuatı gereği belirli raporlar üretilmeli ve denetimlerde sunulmalıdır. Dijital raporlama denetim sürecini hızlandırır.

### Ürüne Hangi Değeri Katıyor?
- Yasal zorunluluk (İSG raporlama)
- Yönetici kararları için veri (eğitim, sertifika, uyum)
- Çoklu format desteği (PDF, Excel, CSV)

### Teknik Amaç
`IReportService` soyutlaması, rapor tipleri ve format generator'lar.

### Yapılacak Teknik İşler
1. `IReportService` interface (GenerateAsync, GetStatusAsync, GetDownloadAsync)
2. `IReportGenerator` interface (PDF, Excel, CSV implementasyonları)
3. `PdfReportGenerator` implementasyonu (QuestPDF)
4. `ExcelReportGenerator` implementasyonu (ClosedXML)
5. `CsvReportGenerator` implementasyonu
6. `ReportRequest` model (reportType, format, parameters)
7. `ReportStatus` entity (Id, Type, Format, Status, FilePath, CreatedAt, CompletedAt)
8. `ReportType` enumeration (TrainingCompliance, CertificateStatus, EmployeeTrainingHistory, DepartmentCompliance, ExpiringCertificates, TrainingAttendance)
9. `ReportFormat` enumeration (PDF, Excel, CSV)
10. Migration (report_jobs tablosu)

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Application.Common.Interfaces.IReportService
SafeFlow.Application.Reports.Interfaces.IReportGenerator
SafeFlow.Infrastructure.Reports.ReportService
SafeFlow.Infrastructure.Reports.Generators.PdfReportGenerator
SafeFlow.Infrastructure.Reports.Generators.ExcelReportGenerator
SafeFlow.Infrastructure.Reports.Generators.CsvReportGenerator
SafeFlow.Domain.Reports.Entities.ReportJob
SafeFlow.Domain.Reports.Enums.ReportType
SafeFlow.Domain.Reports.Enums.ReportFormat
SafeFlow.Domain.Reports.Enums.ReportJobStatus
```

### Gerekli NuGet Paketleri
- `QuestPDF` (Infrastructure — zaten TASK 7.5'te eklendi)
- `ClosedXML` (Infrastructure)
- `CsvHelper` (Infrastructure)

### Database Değişiklikleri
- `report_jobs` tablosu (id, tenant_id, type, format, parameters_json, status, file_path, error_message, created_at, completed_at, created_by)

### Migration Gerekiyor mu?
- **Evet**

### Bağımlı Olduğu Görevler
- TASK 5.4 (Training data)
- TASK 7.4 (Certificate data)

### Tahmini Süre
**1 gün**

### Önerilen Git Commit Mesajı
```
feat(infrastructure): add report service with PDF, Excel, CSV generators
```

---

## TASK 11.2 — Eğitim Uyum Raporu

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 11 — Reporting |
| **Feature** | Training Compliance Report |
| **User Story** | İSG Uzmanı olarak, departman bazlı eğitim uyum raporu üretmek istiyorum ki hangi çalışanların eğitimi eksik olduğunu tespit edebilyrityim. |

### Yapılacak Teknik İşler
1. `TrainingComplianceReportData` query builder
2. Rapor içeriği: çalışan listesi, tamamlanan eğitimler, eksik eğitimler, uyum oranı
3. Departman bazlı filtreleme
4. Tarih aralığı filtreleme
5. PDF, Excel, CSV çıktı testi

### Bağımlı Olduğu Görevler
- TASK 11.1 (Report service)

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(reports): add training compliance report with department filtering
```

---

## TASK 11.3 — Sertifika Durum Raporu

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 11 — Reporting |
| **Feature** | Certificate Status Report |
| **User Story** | İSG Uzmanı olarak, sertifika durum raporu üretmek istiyorum ki süresi dolmuş ve dolmak üzere olan sertifikaları listeleyebilyrityim. |

### Yapılacak Teknik İşler
1. `CertificateStatusReportData` query builder
2. Rapor: aktif, dolacak, dolmuş, iptal sertifika listesi
3. Çalışan bazlı sertifika özeti
4. PDF, Excel, CSV çıktı testi

### Bağımlı Olduğu Görevler
- TASK 11.1

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(reports): add certificate status report
```

---

## TASK 11.4 — Report API Endpoint'leri

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 11 — Reporting |
| **Feature** | Report API |
| **User Story** | İSG Uzmanı olarak, raporları API üzerinden üretmek, durumunu kontrol etmek ve indirmek istiyorum. |

### Yapılacak Teknik İşler
1. `GenerateReportCommand` + handler (async rapor üretimi — Hangfire job)
2. `GetReportStatusQuery` + handler
3. `GET /v1/reports/{id}/download` endpoint
4. API endpoint'leri
5. Integration test

### Oluşturulacak Endpointler
- `POST /v1/reports/generate` 🔒 `reports.generate` — **202 Accepted**
- `GET /v1/reports/{id}/status` 🔒 `reports.view`
- `GET /v1/reports/{id}/download` 🔒 `reports.view`

### Bağımlı Olduğu Görevler
- TASK 11.1, 11.2, 11.3
- TASK 9.5 (Hangfire)

### Tahmini Süre
**4 saat**

### Önerilen Git Commit Mesajı
```
feat(api): add async report generation and download endpoints
```

---

## EPIC 11 — Tamamlanma Özeti

| Metrik | Değer |
|--------|-------|
| **Toplam Görev** | 4 |
| **Toplam Tahmini Süre** | 20 saat (~2.5 iş günü) |
| **Teknik Borç** | Rapor şablonları henüz konfigüre edilemiyor |
| **Refactoring Önerisi** | Report generator Strategy Pattern ile daha temiz hale getirilebilir |

### Code Review Checklist — EPIC 11
- [ ] Rapor üretimi async (UI block etmiyor)
- [ ] Rapor dosyaları tenant-scoped dizinde saklanıyor
- [ ] Büyük rapor verileri streaming ile işleniyor (memory kontrolü)
- [ ] PDF Türkçe karakter desteği doğru
- [ ] Excel çıktısında tablo formatı profesyonel
