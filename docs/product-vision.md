# SafeFlow-AI — Product Vision & Platform Identity

> **Versiyon:** 1.0  
> **Tarih:** 2026-07-07  
> **Durum:** Onaylandı

---

## Platform Kimliği

**SafeFlow-AI**, yalnızca bir İSG yazılımı değildir.

İş Sağlığı ve Güvenliği süreçlerini **dijitalleştiren**, **otomatikleştiren** ve **yapay zekâ ile karar desteği sağlayan modüler bir operasyon platformudur**.

Eğitim, risk değerlendirme, saha denetimi, KKD yönetimi, olay yönetimi ve mevzuat uyumluluğunu tek çatı altında birleştirir.

---

## Vizyon

> Türkiye'nin ve bölgenin en kapsamlı, AI-destekli İSG operasyon platformu olmak.

---

## Misyon

İşletmelerin İSG süreçlerini uçtan uca dijitalleştirerek:

- **Uyumluluğu** artırmak (mevzuat, standart, iç politika)
- **Riskleri** proaktif olarak yönetmek (AI önerileri, trend analizi)
- **Verimliliği** maksimize etmek (otomasyon, self-service, mobil erişim)
- **Karar kalitesini** yükseltmek (veri odaklı dashboard, AI insights)
- **İnsan hayatını** korumak (erken uyarı, saha denetimi, KKD tespiti)

---

## Platform Sütunları

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        SafeFlow-AI Platform                             │
├─────────────┬─────────────┬──────────────┬──────────────┬──────────────┤
│  📚 Eğitim   │  ⚠️ Risk     │  🔍 Denetim   │  🚨 Olay     │  📋 Uyumluluk │
│  Yönetimi   │  Yönetimi   │  Yönetimi    │  Yönetimi    │  Yönetimi    │
├─────────────┼─────────────┼──────────────┼──────────────┼──────────────┤
│ • Eğitim    │ • Fine-     │ • Saha       │ • İş Kazası  │ • Mevzuat    │
│   Planlama  │   Kinney    │   Denetimi   │ • Ramak Kala │   Takibi     │
│ • Oturum    │ • 5x5       │ • Checklist  │ • Soruşturma │ • Periyodik  │
│   Yönetimi  │   Matris    │ • Bulgu      │ • Kök Neden  │   Kontrol    │
│ • Katılım   │ • L-Tipi    │   Yönetimi   │   Analizi    │ • Acil Durum │
│   Takibi    │   Matris    │ • DÖF        │ • DÖF        │   Planı      │
│ • Sertifika │ • Kontrol   │   Entegrasyon│   Entegrasyon│ • Tatbikat   │
│   Üretimi   │   Önlemleri │              │              │   Yönetimi   │
├─────────────┴─────────────┴──────────────┴──────────────┴──────────────┤
│                         🤖 AI Katmanı                                   │
│  Risk Önerileri • YOLO KKD Tespiti • Trend Analizi • Anomali Tespiti   │
├────────────────────────────────────────────────────────────────────────-─┤
│                      📊 Dashboard & Raporlama                           │
│  Trend Grafikleri • Departman İstatistikleri • PDF/Excel/CSV Export     │
├─────────────────────────────────────────────────────────────────────────┤
│                      🔐 Güvenlik & Altyapı                              │
│  Multi-Tenant • RBAC • JWT • Offline-First • Sync • Health Check       │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Hedef Kullanıcılar

| Persona | Rol | Platform Etkileşimi |
|---------|-----|---------------------|
| **İSG Uzmanı** | Operasyonel kullanıcı | Eğitim planlama, risk değerlendirme, denetim yürütme, olay yönetimi, DÖF takibi |
| **İşyeri Hekimi** | Klinik danışman | Sağlık gözetimi, periyodik muayene, meslek hastalığı takibi |
| **İşveren / Üst Yönetim** | Karar verici | Dashboard, trend analizi, uyumluluk raporları, AI önerileri |
| **Departman Müdürü** | Operasyonel yönetici | Departman uyumu, eğitim katılımı, risk durumu |
| **Çalışan** | Son kullanıcı | Eğitim katılımı, sertifika erişimi, bildirim alma, olay bildirimi |
| **Sistem Yöneticisi** | Teknik yönetici | Tenant yönetimi, kullanıcı yönetimi, sistem konfigürasyonu |

---

## Rekabetçi Farklılaşma

| Özellik | Geleneksel İSG Yazılımları | SafeFlow-AI |
|---------|---------------------------|-------------|
| **Mimari** | Monolitik | Modüler, event-driven, ölçeklenebilir |
| **AI Desteği** | Yok | Risk önerileri, KKD tespiti (YOLO), trend analizi |
| **Mobil** | Sınırlı veya yok | Offline-first Flutter, saha denetimi |
| **Entegrasyon** | Kapalı | REST API, webhook, genişletilebilir |
| **UX** | Eski nesil | Modern, responsive, micro-animation |
| **Ölçeklenebilirlik** | Tek sunucu | Horizontal scaling, Docker, K8s ready |
| **Mevzuat** | Statik | Dinamik güncelleme, AI mevzuat eşleştirme |
| **Raporlama** | Temel | Trend analizi, departman karşılaştırma, PDF/Excel/CSV |

---

## Ürün Yol Haritası

### Phase 0 — Foundation (MVP Altyapı)
- [x] Mimari dokümanlar
- [ ] Clean Architecture solution yapısı
- [ ] Domain model implementasyonu
- [ ] Veritabanı migration altyapısı
- [ ] Authentication altyapısı (JWT + Refresh Token)
- [ ] Multi-tenant altyapısı
- [ ] Health Check endpoint'leri
- [ ] CI/CD pipeline temel yapısı

### Phase 1 — Core MVP
- [ ] **Kullanıcı Yönetimi** — Kayıt, giriş, profil, rol atama
- [ ] **Şirket Yönetimi** — Şirket bilgileri, departman, lokasyon
- [ ] **Çalışan Yönetimi** — Çalışan CRUD, departman atama, transfer
- [ ] **Eğitim Yönetimi** — Eğitim planlama, oturum, katılım, materyal
- [ ] **Sertifika Yönetimi** — Sertifika üretimi, PDF, geçerlilik takibi
- [ ] **Bildirim Yönetimi** — E-posta + in-app bildirim (temel)
- [ ] **Dashboard** — Özet istatistikler, trend grafikleri
- [ ] **Mobil Uygulama** — Flutter offline-first temel yapı
- [ ] **Web Uygulama** — Flutter web yönetim paneli

### Phase 2 — Risk & Denetim
- [ ] **Risk Değerlendirme** — Fine-Kinney, 5x5, L-Tipi Matris
- [ ] **Kontrol Önlemleri** — Aksiyon takibi, sorumluluk atama
- [ ] **Denetim Yönetimi** — Saha denetimi, checklist, bulgu, DÖF
- [ ] **DÖF Yönetimi** — Düzeltici/Önleyici faaliyet, takip
- [ ] **Raporlama** — PDF, Excel, CSV export
- [ ] **MassTransit + RabbitMQ** — Event-driven messaging
- [ ] **Redis Cache** — Distributed caching

### Phase 3 — Olay & Uyumluluk
- [ ] **İş Kazası Yönetimi** — Olay kaydı, soruşturma, kök neden analizi
- [ ] **Ramak Kala Yönetimi** — Bildirim, sınıflandırma, DÖF entegrasyonu
- [ ] **KKD Yönetimi** — KKD tanımlama, atama, periyodik kontrol
- [ ] **Sağlık Gözetimi** — İşe giriş/periyodik muayene, meslek hastalığı
- [ ] **Periyodik Kontroller** — Ekipman, tesis, çevre kontrolleri
- [ ] **Mevzuat Yönetimi** — Mevzuat takibi, güncelleme bildirimi
- [ ] **Acil Durum Planları** — Plan oluşturma, güncelleme
- [ ] **Tatbikat Yönetimi** — Planlama, uygulama, raporlama

### Phase 4 — AI & Gelişmiş Analitik
- [ ] **AI Risk Önerileri** — Geçmiş veri bazlı risk tahminleme
- [ ] **YOLO KKD Tespiti** — Kamera görüntüsünden KKD uyumluluk kontrolü
- [ ] **AI Dashboard Insights** — Anomali tespiti, proaktif uyarılar
- [ ] **AI Mevzuat Eşleştirme** — Doğal dil işleme ile mevzuat analizi
- [ ] **Rule Engine** — Kod değişmeden iş kuralı tanımlama
- [ ] **Webhook & Entegrasyon** — Harici sistem entegrasyonları

---

## Teknik Vizyon

```
Prensip                          Açıklama
──────────────────────────────── ───────────────────────────────────────
Domain-Driven Design             Her modül bağımsız Bounded Context
Clean Architecture               Katmanlı, test edilebilir, sürdürülebilir
Event-Driven                     MediatR (MVP) → MassTransit (Production)
CQRS                             Command/Query ayrımı, performans
Offline-First                    Flutter + SQLite, delta sync
AI-Ready                         FastAPI mikroservis, model versiyonlama
Multi-Tenant                     Row-Level Security, tenant isolation
Security-First                   JWT, RBAC, rate limiting, CSP, XSS koruması
Observable                       Correlation ID, structured logging, health check
Rule Engine Ready                IRuleEngine soyutlaması, configurable iş kuralları
```

---

## Başarı Metrikleri

| Metrik | Hedef |
|--------|-------|
| **Eğitim Uyum Oranı** | İşletme bazında %95+ |
| **Sertifika Geçerlilik** | Süresi geçen sertifika oranı <%5 |
| **Risk Tespiti** | AI ile erken tespit oranı %80+ |
| **Mobil Kullanım** | Saha çalışanlarının %70+'ı mobil aktif |
| **Sistem Uptime** | %99.9+ (SLA) |
| **API Response Time** | P95 < 200ms |
| **Offline Sync** | Çakışma oranı <%1 |
