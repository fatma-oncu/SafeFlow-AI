# SafeFlow-AI — C4 Architecture Diagrams

> **Versiyon:** 1.1  
> **Tarih:** 2026-07-07  
> **Durum:** Taslak — Onay Bekliyor

> [!IMPORTANT]
> **SafeFlow-AI**, yalnızca bir İSG yazılımı değildir. İş Sağlığı ve Güvenliği süreçlerini
> dijitalleştiren, otomatikleştiren ve yapay zekâ ile karar desteği sağlayan **modüler bir
> operasyon platformudur**. Eğitim, risk değerlendirme, saha denetimi, KKD yönetimi, olay
> yönetimi ve mevzuat uyumluluğunu tek çatı altında birleştirir.

---

## 1. Level 1 — System Context Diagram

Sistemin dış dünya ile etkileşimini gösterir. SafeFlow-AI'ın kullanıcıları (persona'lar) ve bağımlı olduğu harici sistemler bu seviyede tanımlanır.

```mermaid
C4Context
    title SafeFlow-AI — System Context Diagram

    Person(isg_uzmani, "İSG Uzmanı", "Eğitim planlar, risk değerlendirmesi yapar, denetim yönetir")
    Person(isveren, "İşveren / Yönetici", "Dashboard izler, raporları inceler, onay verir")
    Person(calisan, "Çalışan", "Eğitimlere katılır, bildirim alır, sertifikalarını görüntüler")
    Person(sistem_admin, "Sistem Yöneticisi", "Tenant yönetimi, kullanıcı yönetimi, sistem konfigürasyonu")

    System(safeflow, "SafeFlow-AI Platform", "İSG süreçlerini dijitalleştiren, otomatikleştiren ve AI ile karar desteği sağlayan modüler operasyon platformu. Eğitim, risk değerlendirme, saha denetimi, KKD yönetimi, olay yönetimi ve mevzuat uyumluluğunu tek çatı altında birleştirir.")

    System_Ext(email_service, "E-Posta Servisi", "SMTP / SendGrid üzerinden bildirim e-postaları gönderir")
    System_Ext(sms_service, "SMS Servisi", "Acil durum ve kritik bildirimler için SMS gönderir")
    System_Ext(ai_service, "AI Servis", "FastAPI tabanlı bağımsız AI modülü. Risk önerileri, YOLO KKD tespiti")
    System_Ext(file_storage, "Dosya Depolama", "Eğitim materyalleri, sertifika PDF'leri, fotoğraflar için blob storage")
    System_Ext(identity_provider, "Harici Identity Provider", "Gelecekte SSO / OAuth2 entegrasyonu için")

    Rel(isg_uzmani, safeflow, "Eğitim planlar, risk değerlendirir, denetim yapar", "HTTPS")
    Rel(isveren, safeflow, "Dashboard izler, raporları inceler", "HTTPS")
    Rel(calisan, safeflow, "Eğitime katılır, sertifika görüntüler", "HTTPS")
    Rel(sistem_admin, safeflow, "Sistem yapılandırır, tenant yönetir", "HTTPS")

    Rel(safeflow, email_service, "Bildirim e-postaları gönderir", "SMTP/API")
    Rel(safeflow, sms_service, "Acil durum SMS'leri gönderir", "API")
    Rel(safeflow, ai_service, "Risk önerisi ve KKD tespiti ister", "REST/gRPC")
    Rel(safeflow, file_storage, "Dosya yükler/indirir", "API")
    Rel(safeflow, identity_provider, "SSO token doğrulama", "OAuth2/OIDC")
```

### Persona Detayları

| Persona | Rol | Temel Eylemler | Erişim |
|---------|-----|----------------|--------|
| **İSG Uzmanı** | Operasyonel kullanıcı | Eğitim planlama, risk değerlendirme, denetim, DÖF, iş kazası kaydı | Web + Mobil |
| **İşveren / Yönetici** | Karar verici | Dashboard, raporlar, onay süreçleri, trend analizi | Web |
| **Çalışan** | Son kullanıcı | Eğitime katılım, sertifika görüntüleme, bildirim alma | Mobil (öncelikli) |
| **Sistem Yöneticisi** | Teknik yönetici | Tenant yönetimi, kullanıcı yönetimi, sistem konfigürasyonu | Web |

---

## 2. Level 2 — Container Diagram

Sistemin teknik bileşenlerini (container'lar) ve aralarındaki iletişimi gösterir.

```mermaid
C4Container
    title SafeFlow-AI — Container Diagram

    Person(user, "Kullanıcı", "İSG Uzmanı, İşveren, Çalışan veya Sistem Yöneticisi")

    System_Boundary(safeflow_boundary, "SafeFlow-AI Platform") {
        Container(mobile_app, "Flutter Mobil Uygulama", "Flutter / Dart", "Offline-first mobil uygulama. SQLite ile local veri, sync servisi ile senkronizasyon")
        Container(web_app, "Flutter Web Uygulama", "Flutter / Dart", "Responsive web arayüzü. Dashboard, raporlama ve yönetim paneli")
        Container(api_gateway, "API Gateway", ".NET 8 / YARP", "Rate limiting, authentication, request routing, correlation ID")
        Container(backend_api, "SafeFlow API", ".NET 8 / Clean Architecture", "Modüler operasyon platformu. İSG domain servisleri, CQRS + MediatR, Rule Engine hazırlığı")
        Container(database, "PostgreSQL", "PostgreSQL 16", "Ana veritabanı. Multi-tenant veri, audit log, tüm domain verileri")
        Container(cache, "Cache Layer", "IMemoryCache → Redis", "Sık erişilen verilerin cache'lenmesi. Session, lookup data, dashboard")
        Container(background_jobs, "Background Jobs", ".NET 8 / Hangfire", "Zamanlanmış görevler: sertifika hatırlatma, rapor üretimi, sync")
    }

    System_Ext(ai_service, "AI Servis", "FastAPI / Python. Risk önerileri, YOLO KKD tespiti, model versiyonlama")
    System_Ext(email_service, "E-Posta Servisi", "SendGrid / SMTP")
    System_Ext(file_storage, "Dosya Depolama", "Azure Blob / MinIO")
    System_Ext(monitoring, "Monitoring Stack", "Serilog + Seq / ELK. Structured logging, health check")

    Rel(user, mobile_app, "Kullanır", "HTTPS")
    Rel(user, web_app, "Kullanır", "HTTPS")
    Rel(mobile_app, api_gateway, "API çağrıları", "HTTPS / REST")
    Rel(web_app, api_gateway, "API çağrıları", "HTTPS / REST")
    Rel(api_gateway, backend_api, "Yönlendirir", "HTTP")
    Rel(backend_api, database, "Okur/Yazar", "TCP / EF Core")
    Rel(backend_api, cache, "Cache okur/yazar", "In-Process / TCP")
    Rel(backend_api, ai_service, "Risk/KKD analiz ister", "REST")
    Rel(backend_api, email_service, "E-posta gönderir", "SMTP/API")
    Rel(backend_api, file_storage, "Dosya yükler/indirir", "REST")
    Rel(backend_api, monitoring, "Log gönderir", "HTTP/TCP")
    Rel(background_jobs, database, "Zamanlanmış işler", "TCP / EF Core")
    Rel(background_jobs, email_service, "Hatırlatma e-postaları", "SMTP/API")
```

### Container Sorumlulukları

| Container | Teknoloji | Sorumluluk | MVP Durumu |
|-----------|-----------|------------|------------|
| **Flutter Mobil App** | Flutter 3.x / Dart | Offline-first mobil deneyim, SQLite local DB, sync servisi | ✅ MVP |
| **Flutter Web App** | Flutter 3.x / Dart | Responsive web arayüzü, dashboard, yönetim paneli | ✅ MVP |
| **API Gateway** | .NET 8 / YARP | Rate limiting, auth, routing, correlation ID, request tracing | ✅ MVP (basit) |
| **SafeFlow API** | .NET 8 / Clean Architecture | Tüm iş mantığı, CQRS, MediatR, domain servisleri | ✅ MVP |
| **PostgreSQL** | PostgreSQL 16 | Multi-tenant veritabanı, Row Level Security | ✅ MVP |
| **Cache Layer** | IMemoryCache → Redis | Performans optimizasyonu | ✅ MVP (Memory) |
| **Background Jobs** | Hangfire | Zamanlanmış görevler, hatırlatmalar | ✅ MVP (temel) |
| **AI Servis** | FastAPI / Python | Risk önerileri, YOLO KKD tespiti | ⏳ Phase 2 |

### MVP → Production Geçiş Yolu

```
MVP (Phase 0-1)                    Production (Phase 2+)
─────────────────                  ─────────────────────
IMemoryCache                  →    Redis Cluster
MediatR (in-process events)   →    MassTransit + RabbitMQ
Basit API Gateway             →    YARP + Ocelot
Local file storage            →    Azure Blob Storage
Console logging               →    Serilog + Seq/ELK
Hangfire (in-process)         →    Hangfire (dedicated server)
Single instance               →    Horizontal scaling + Load Balancer
```

---

## 3. Level 3 — Component Diagram (SafeFlow API)

Backend API'nin iç bileşenlerini detaylı olarak gösterir.

```mermaid
C4Component
    title SafeFlow API — Component Diagram

    Container_Boundary(api_boundary, "SafeFlow API (.NET 8)") {

        Component(auth_module, "Authentication Module", "JWT + Refresh Token", "Kimlik doğrulama, token üretimi, token rotation, permission-based yetkilendirme")
        Component(user_module, "User Management", "CQRS + MediatR", "Kullanıcı CRUD, rol atama, profil yönetimi")
        Component(tenant_module, "Tenant Management", "Multi-Tenant", "Şirket/tenant oluşturma, tenant isolation, ayarlar")
        Component(training_module, "Training Module", "CQRS + MediatR", "Eğitim planlama, oturum yönetimi, katılım takibi, materyal yönetimi")
        Component(cert_module, "Certificate Module", "CQRS + MediatR", "Sertifika üretimi, şablon yönetimi, geçerlilik takibi, yenileme hatırlatma")
        Component(company_module, "Company Module", "CQRS + MediatR", "Departman, lokasyon, çalışan yönetimi")
        Component(dashboard_module, "Dashboard Module", "Query Only", "Widget tabanlı dashboard, trend analizi, departman istatistikleri, AI önerileri")
        Component(report_module, "Report Module", "QuestPDF + ClosedXML", "PDF, Excel, CSV rapor üretimi, şablon tabanlı")
        Component(notification_module, "Notification Module", "MediatR Events", "E-posta, push notification, in-app bildirim")
        Component(health_module, "Health & Observability", "ASP.NET Health Checks", "Liveness, readiness, DB health, cache health")
        Component(sync_module, "Sync Module", "REST API", "Mobil offline sync, conflict resolution, delta sync")

        Component(middleware_stack, "Middleware Stack", "ASP.NET Middleware", "Correlation ID, request tracing, exception handling, rate limiting")
        Component(domain_events, "Domain Event Bus", "MediatR INotification", "In-process domain event dispatching")
        Component(rule_engine, "Rule Engine Interface", "IRuleEngine", "İş kuralları soyutlaması, gelecekte configurable rules")
    }

    ContainerDb(database, "PostgreSQL", "Ana veritabanı")
    Container(cache, "Cache", "IMemoryCache / Redis")
    System_Ext(ai_service, "AI Service", "FastAPI")
    System_Ext(email, "Email Service", "SendGrid")
    System_Ext(storage, "File Storage", "Blob Storage")

    Rel(auth_module, database, "Kullanıcı/token okur-yazar")
    Rel(user_module, database, "Kullanıcı verileri")
    Rel(tenant_module, database, "Tenant verileri")
    Rel(training_module, database, "Eğitim verileri")
    Rel(cert_module, database, "Sertifika verileri")
    Rel(company_module, database, "Şirket verileri")
    Rel(dashboard_module, database, "İstatistik sorguları")
    Rel(dashboard_module, cache, "Cache'lenmiş dashboard verileri")
    Rel(report_module, database, "Rapor verileri")
    Rel(notification_module, email, "E-posta gönderimi")
    Rel(training_module, domain_events, "TrainingCompleted event yayınlar")
    Rel(cert_module, domain_events, "CertificateIssued event yayınlar")
    Rel(domain_events, notification_module, "Event'leri dinler, bildirim gönderir")
    Rel(dashboard_module, ai_service, "AI önerisi ister")
    Rel(cert_module, storage, "PDF sertifika depolar")
    Rel(training_module, storage, "Eğitim materyali depolar")
```

### Modül Detay Matrisi

| Modül | Katman | Pattern | Domain Events | MVP |
|-------|--------|---------|---------------|-----|
| **Authentication** | Infrastructure | JWT + Refresh Token | `UserLoggedIn`, `TokenRefreshed` | ✅ |
| **User Management** | Application + Domain | CQRS | `UserCreated`, `UserUpdated`, `RoleAssigned` | ✅ |
| **Tenant Management** | Application + Domain | CQRS | `TenantCreated`, `TenantSettingsUpdated` | ✅ |
| **Training** | Application + Domain | CQRS + State Machine | `TrainingCreated`, `TrainingScheduled`, `TrainingCompleted`, `ParticipantEnrolled` | ✅ |
| **Certificate** | Application + Domain | CQRS + State Machine | `CertificateIssued`, `CertificateExpiring`, `CertificateRevoked` | ✅ |
| **Company** | Application + Domain | CQRS | `DepartmentCreated`, `EmployeeAssigned` | ✅ |
| **Dashboard** | Application (Query) | Read Model | — | ✅ |
| **Report** | Application | Template Pattern | `ReportGenerated` | ✅ |
| **Notification** | Infrastructure | Event Handler | — | ✅ |
| **Sync** | Application | Delta Sync | `SyncCompleted`, `ConflictDetected` | ✅ |
| **Health** | Infrastructure | Health Check | — | ✅ |

---

## 4. Level 3 — Component Diagram (Flutter Mobil App)

```mermaid
C4Component
    title Flutter Mobile App — Component Diagram

    Container_Boundary(flutter_boundary, "Flutter Mobile App") {

        Component(ui_layer, "UI Layer", "Flutter Widgets", "Ekranlar, formlar, dashboard widgets, responsive layout")
        Component(state_mgmt, "State Management", "Riverpod / Bloc", "Uygulama state yönetimi, reactive UI güncellemeleri")
        Component(repository_layer, "Repository Layer", "Dart", "Veri erişim soyutlaması, online/offline routing")
        Component(local_db, "Local Database", "SQLite / Drift", "Offline veri depolama, pending operations queue")
        Component(sync_service, "Sync Service", "Dart", "Background sync, conflict resolution, retry policy")
        Component(api_client, "API Client", "Dio / HTTP", "REST API çağrıları, JWT token yönetimi, interceptors")
        Component(auth_manager, "Auth Manager", "Dart", "Token storage, refresh, biometric auth")
        Component(notification_handler, "Notification Handler", "Firebase / Local", "Push notification, local reminder")
        Component(offline_queue, "Offline Queue", "SQLite", "Pending CRUD operations, priority queue, retry metadata")
    }

    System_Ext(backend, "SafeFlow API", ".NET 8 Backend")
    System_Ext(push_service, "Push Service", "Firebase Cloud Messaging")

    Rel(ui_layer, state_mgmt, "State okur/günceller")
    Rel(state_mgmt, repository_layer, "Veri ister/gönderir")
    Rel(repository_layer, local_db, "Offline veri okur/yazar")
    Rel(repository_layer, api_client, "Online veri okur/yazar")
    Rel(sync_service, offline_queue, "Pending ops okur")
    Rel(sync_service, api_client, "Sync verisi gönderir")
    Rel(sync_service, local_db, "Sync sonucu yazar")
    Rel(api_client, backend, "REST API", "HTTPS")
    Rel(api_client, auth_manager, "Token alır/yeniler")
    Rel(notification_handler, push_service, "Push notification alır")
```

---

## 5. Deployment Diagram

```mermaid
graph TB
    subgraph "Client Tier"
        MOBILE["📱 Flutter Mobile App<br/>(iOS / Android)<br/>SQLite Local DB"]
        WEB["🌐 Flutter Web App<br/>(Browser)<br/>SPA"]
    end

    subgraph "Edge / CDN"
        CDN["☁️ CDN<br/>Static Assets<br/>Web App Hosting"]
        LB["⚖️ Load Balancer<br/>SSL Termination<br/>Rate Limiting"]
    end

    subgraph "Application Tier"
        API1["🖥️ SafeFlow API<br/>Instance 1<br/>.NET 8"]
        API2["🖥️ SafeFlow API<br/>Instance 2<br/>.NET 8"]
        BG["⏰ Background Jobs<br/>Hangfire Server<br/>.NET 8"]
    end

    subgraph "AI Tier"
        AI["🤖 AI Service<br/>FastAPI<br/>Docker Container"]
    end

    subgraph "Data Tier"
        PG_PRIMARY["🐘 PostgreSQL<br/>Primary<br/>Read/Write"]
        PG_REPLICA["🐘 PostgreSQL<br/>Replica<br/>Read Only"]
        REDIS["💾 Redis<br/>Cache + Session<br/>(Production)"]
        BLOB["📦 Blob Storage<br/>Files, PDFs<br/>Certificates"]
    end

    subgraph "Observability"
        SEQ["📊 Seq / ELK<br/>Structured Logs"]
        HEALTH["❤️ Health Checks<br/>/health/live<br/>/health/ready"]
    end

    MOBILE -->|HTTPS| LB
    WEB -->|HTTPS| CDN
    CDN -->|HTTPS| LB
    LB --> API1
    LB --> API2
    API1 --> PG_PRIMARY
    API2 --> PG_PRIMARY
    API1 --> REDIS
    API2 --> REDIS
    API1 --> AI
    API1 --> BLOB
    BG --> PG_PRIMARY
    BG --> REDIS
    PG_PRIMARY -->|Streaming Replication| PG_REPLICA
    API1 --> SEQ
    API2 --> SEQ
    API1 --> HEALTH
```

### Ortam Matrisi

| Ortam | API Instances | DB | Cache | AI | Monitoring |
|-------|--------------|-----|-------|-----|------------|
| **Development** | 1 | PostgreSQL (Docker) | IMemoryCache | Mock/Local | Console |
| **Staging** | 2 | PostgreSQL (Managed) | Redis (Single) | FastAPI (Docker) | Seq |
| **Production** | 2+ (Auto-scale) | PostgreSQL (HA Cluster) | Redis (Cluster) | FastAPI (K8s) | Seq + ELK |

---

## 6. Cross-Cutting Concerns

### 6.1 Observability Stack

```mermaid
graph LR
    subgraph "Request Pipeline"
        REQ["Incoming Request"] --> CID["Correlation ID<br/>Middleware"]
        CID --> LOG["Request Logging<br/>Middleware"]
        LOG --> AUTH["Authentication<br/>Middleware"]
        AUTH --> RATE["Rate Limiting<br/>Middleware"]
        RATE --> HANDLER["Request Handler"]
        HANDLER --> RESP["Response"]
    end

    subgraph "Logging"
        LOG --> SERILOG["Serilog"]
        SERILOG --> CONSOLE["Console Sink"]
        SERILOG --> SEQ["Seq Sink"]
        SERILOG --> FILE["File Sink"]
    end

    subgraph "Health Checks"
        HC["/health"] --> DB_HC["DB Check"]
        HC --> CACHE_HC["Cache Check"]
        HC --> AI_HC["AI Service Check"]
        LIVE["/health/live"] --> PROCESS["Process Check"]
        READY["/health/ready"] --> DB_HC
        READY --> CACHE_HC
    end
```

### 6.2 Security Architecture

```mermaid
graph TB
    subgraph "Authentication Flow"
        LOGIN["Login Request"] --> VALIDATE["Credential Validation"]
        VALIDATE --> JWT["JWT Access Token<br/>(15 min)"]
        VALIDATE --> RT["Refresh Token<br/>(7 days)"]
        JWT --> API["API Request"]
        RT --> ROTATE["Token Rotation"]
        ROTATE --> JWT2["New Access Token"]
        ROTATE --> RT2["New Refresh Token"]
    end

    subgraph "Authorization"
        API --> PERM["Permission Check"]
        PERM --> TENANT["Tenant Isolation"]
        TENANT --> RESOURCE["Resource Access"]
    end

    subgraph "Protection Layers"
        RATE["Rate Limiting"]
        CSP["Content Security Policy"]
        XSS["XSS Protection"]
        SQLI["SQL Injection<br/>(Parameterized via EF Core)"]
        UPLOAD["File Upload Validation<br/>(MIME, Size, Virus Scan)"]
        SECRETS["Secret Management<br/>(User Secrets → Key Vault)"]
    end
```

---

## 7. Veri Akışı — Eğitim Tamamlama Senaryosu

Bu senaryo, C4 bileşenlerinin nasıl birlikte çalıştığını gösterir.

```mermaid
sequenceDiagram
    participant M as 📱 Mobile App
    participant GW as 🔀 API Gateway
    participant API as 🖥️ SafeFlow API
    participant DB as 🐘 PostgreSQL
    participant EVT as 📨 Domain Events (MediatR)
    participant CERT as 📜 Certificate Module
    participant NOTIF as 🔔 Notification Module
    participant EMAIL as ✉️ Email Service
    participant CACHE as 💾 Cache

    M->>GW: POST /api/trainings/{id}/complete
    GW->>GW: Correlation ID oluştur
    GW->>GW: Rate Limit kontrol
    GW->>GW: JWT doğrula
    GW->>API: İsteği yönlendir

    API->>DB: Training durumunu güncelle
    API->>EVT: TrainingCompleted event yayınla

    par Paralel Event Handler'lar
        EVT->>CERT: Sertifika oluştur
        CERT->>DB: Sertifika kaydet
        CERT->>EVT: CertificateIssued event yayınla
    and
        EVT->>NOTIF: Bildirim gönder
        NOTIF->>EMAIL: Tebrik e-postası
    and
        EVT->>CACHE: Dashboard cache invalidate
    end

    API-->>GW: 200 OK + Completion Result
    GW-->>M: Response + Correlation ID
```

---

## Açık Sorular

| # | Soru | Etki |
|---|------|------|
| 1 | API Gateway olarak YARP mı yoksa basit reverse proxy mi (MVP)? | Container diagram |
| 2 | Flutter state management: Riverpod mı Bloc mu? | Mobile component diagram |
| 3 | File storage MVP'de local disk mi yoksa MinIO mu? | Deployment diagram |
| 4 | Monitoring MVP'de Seq mi yoksa sadece console + file mı? | Observability stack |
