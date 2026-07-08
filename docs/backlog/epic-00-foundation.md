# EPIC 0 — Foundation

> **Epic Sahibi:** Tech Lead  
> **Öncelik:** P0 — Critical  
> **Tahmini Toplam Süre:** 5 iş günü  
> **Bağımlılık:** Yok (ilk epic)  
> **Hedef Sprint:** Sprint 1

---

## TASK 0.1 — Solution Yapısı ve Clean Architecture Proje Kurulumu

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 0 — Foundation |
| **Feature** | Clean Architecture Solution Yapısı |
| **User Story** | Geliştirici olarak, Clean Architecture prensiplerine uygun bir .NET solution yapısı istiyorum ki tüm ekip üyeleri tutarlı bir proje yapısında çalışabilsin. |

### Neden Yapılıyor?
Tüm projenin temelini oluşturur. Clean Architecture katman ayrımı olmadan domain logic, infrastructure ile birbirine bağımlı hale gelir ve test edilemez kod ortaya çıkar.

### Ürüne Hangi Değeri Katıyor?
- Sürdürülebilir, test edilebilir, modüler kod tabanı
- Yeni geliştiricilerin hızla projeye adapte olması
- Gelecekte domain modüllerinin bağımsız ölçeklenmesi

### Teknik Amaç
.NET 8 tabanlı, Clean Architecture katmanlı solution yapısını oluşturmak.

### Teknik Açıklama
Solution 4 ana katmandan oluşacak: Domain, Application, Infrastructure, API (Presentation). Her katman bağımsız bir Class Library/Web projesi olacak. Bağımlılık yönü dıştan içe doğru olacak (Domain hiçbir şeye bağımlı olmayacak).

### Yapılacak Teknik İşler
1. `SafeFlow.sln` solution dosyası oluştur
2. `src/SafeFlow.Domain` class library projesi oluştur (.NET 8)
3. `src/SafeFlow.Application` class library projesi oluştur
4. `src/SafeFlow.Infrastructure` class library projesi oluştur
5. `src/SafeFlow.API` web API projesi oluştur
6. `tests/SafeFlow.Domain.Tests` xUnit test projesi oluştur
7. `tests/SafeFlow.Application.Tests` xUnit test projesi oluştur
8. `tests/SafeFlow.Infrastructure.Tests` xUnit test projesi oluştur
9. `tests/SafeFlow.API.Tests` xUnit test projesi oluştur
10. Proje referanslarını doğru yönde kur (Domain ← Application ← Infrastructure ← API)
11. `.editorconfig` dosyası oluştur (C# coding conventions)
12. `Directory.Build.props` ile ortak proje ayarları tanımla
13. `.gitignore` dosyası oluştur (.NET + IDE)
14. `global.json` ile SDK versiyonunu sabitle
15. `README.md` başlangıç dokümanı oluştur

### Oluşturulacak Sınıflar
- Yok (yapısal kurulum)

### Oluşturulacak Dosyalar
```
SafeFlow.sln
global.json
.editorconfig
.gitignore
Directory.Build.props
README.md
src/SafeFlow.Domain/SafeFlow.Domain.csproj
src/SafeFlow.Application/SafeFlow.Application.csproj
src/SafeFlow.Infrastructure/SafeFlow.Infrastructure.csproj
src/SafeFlow.API/SafeFlow.API.csproj
tests/SafeFlow.Domain.Tests/SafeFlow.Domain.Tests.csproj
tests/SafeFlow.Application.Tests/SafeFlow.Application.Tests.csproj
tests/SafeFlow.Infrastructure.Tests/SafeFlow.Infrastructure.Tests.csproj
tests/SafeFlow.API.Tests/SafeFlow.API.Tests.csproj
```

### Oluşturulacak Endpointler
- Yok

### Gerekli NuGet Paketleri
- `xunit` (test projeleri)
- `xunit.runner.visualstudio` (test projeleri)
- `Microsoft.NET.Test.Sdk` (test projeleri)
- `FluentAssertions` (test projeleri)
- `Moq` (test projeleri)
- `coverlet.collector` (code coverage)

### Database Değişiklikleri
- Yok

### Migration Gerekiyor mu?
- Hayır

### Bağımlı Olduğu Görevler
- Yok (ilk görev)

### Acceptance Criteria
- [ ] Solution derleniyor (`dotnet build` başarılı)
- [ ] Tüm test projeleri çalışıyor (`dotnet test` başarılı)
- [ ] Domain projesi hiçbir src projesine referans vermiyor
- [ ] Application projesi yalnızca Domain'e referans veriyor
- [ ] Infrastructure projesi Application ve Domain'e referans veriyor
- [ ] API projesi tüm src projelerine referans veriyor
- [ ] .editorconfig dosyası mevcut ve C# convention'lar tanımlı
- [ ] .gitignore .NET ve IDE dosyalarını kapsıyor

### Definition of Done (DoD)
- [ ] Kod derlenebilir durumda
- [ ] Solution yapısı Clean Architecture'a uygun
- [ ] Code review tamamlandı
- [ ] Git'e commit edildi
- [ ] README güncel

### Test Gereksinimleri

| Test Tipi | Gereksinim |
|-----------|-----------|
| **Unit Test** | Yok (yapısal kurulum) |
| **Integration Test** | `dotnet build` ve `dotnet test` başarılı çalışmalı |
| **Manual Test** | Solution IDE'de (VS/Rider) açılıp derlenebilmeli |

### SOLID & Design Pattern
- **SRP:** Her katman tek bir sorumluluk taşır
- **DIP:** Bağımlılık yönü dıştan içe doğru
- **Design Pattern:** Layered Architecture, Dependency Inversion

### Clean Architecture Katmanları
- Tüm katmanlar (yapısal oluşturma)

### Domain Event
- Yok

### Validation Kuralları
- Yok

### Authorization Policy
- Yok

### Log Kayıtları
- Yok

### Audit Log
- Hayır

### Cache
- Hayır

### Transaction
- Hayır

### Olası Exception'lar
- Build hatası (proje referans döngüsü)

### Performans Notları
- `Directory.Build.props` ile implicit usings ve nullable reference types merkezi yönetilmeli

### Beklenen Çıktılar
- Derlenebilir, Clean Architecture uyumlu .NET 8 solution

### Önerilen Git Commit Mesajı
```
feat: initialize Clean Architecture solution structure

- Add Domain, Application, Infrastructure, API layers
- Add xUnit test projects for all layers
- Configure .editorconfig, Directory.Build.props, global.json
- Set project references following dependency inversion
```

### Tahmini Süre
**4 saat**

---

## TASK 0.2 — Domain Katmanı Temel Soyutlamaları

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 0 — Foundation |
| **Feature** | Domain Base Classes |
| **User Story** | Geliştirici olarak, tüm entity ve aggregate'ler için temel soyut sınıflar istiyorum ki domain modelini tutarlı bir şekilde inşa edebileyim. |

### Neden Yapılıyor?
Domain modeli dokümanda tanımlanan BaseEntity, AggregateRoot, ValueObject ve IDomainEvent soyutlamalarına ihtiyaç duyar. Bu sınıflar olmadan domain katmanı inşa edilemez.

### Ürüne Hangi Değeri Katıyor?
- Tutarlı domain modeli (tüm entity'ler aynı temelden türer)
- Otomatik audit bilgisi (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
- Soft delete desteği
- Domain event mekanizması altyapısı

### Teknik Amaç
Domain katmanının temel abstract class'larını ve interface'lerini oluşturmak.

### Teknik Açıklama
Mimari dokümanda tanımlanan `BaseEntity`, `AggregateRoot`, `ValueObject`, `IDomainEvent`, `DomainEvent`, `ITenantEntity` ve `IAuditableEntity` soyutlamaları oluşturulacak. Tüm gelecek entity'ler bu sınıflardan türeyecek.

### Yapılacak Teknik İşler
1. `BaseEntity` abstract class oluştur (Id, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted)
2. `AggregateRoot` abstract class oluştur (DomainEvents listesi, AddDomainEvent, ClearDomainEvents)
3. `ValueObject` abstract class oluştur (equality, GetHashCode)
4. `IDomainEvent` interface oluştur (MediatR.INotification türeyen)
5. `DomainEvent` abstract record oluştur (EventId, OccurredOn, EventType)
6. `ITenantEntity` interface oluştur (TenantId)
7. `IAuditableEntity` interface oluştur
8. `Enumeration` base class oluştur (smart enum pattern)
9. BaseEntity ve AggregateRoot için unit test'ler yaz
10. ValueObject equality için unit test'ler yaz

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Domain.Common.BaseEntity
SafeFlow.Domain.Common.AggregateRoot
SafeFlow.Domain.Common.ValueObject
SafeFlow.Domain.Common.Enumeration
SafeFlow.Domain.Common.IDomainEvent
SafeFlow.Domain.Common.DomainEvent
SafeFlow.Domain.Common.ITenantEntity
SafeFlow.Domain.Common.IAuditableEntity
```

### Oluşturulacak Dosyalar
```
src/SafeFlow.Domain/Common/BaseEntity.cs
src/SafeFlow.Domain/Common/AggregateRoot.cs
src/SafeFlow.Domain/Common/ValueObject.cs
src/SafeFlow.Domain/Common/Enumeration.cs
src/SafeFlow.Domain/Common/IDomainEvent.cs
src/SafeFlow.Domain/Common/DomainEvent.cs
src/SafeFlow.Domain/Common/ITenantEntity.cs
src/SafeFlow.Domain/Common/IAuditableEntity.cs
tests/SafeFlow.Domain.Tests/Common/BaseEntityTests.cs
tests/SafeFlow.Domain.Tests/Common/AggregateRootTests.cs
tests/SafeFlow.Domain.Tests/Common/ValueObjectTests.cs
```

### Oluşturulacak Endpointler
- Yok

### Gerekli NuGet Paketleri
- `MediatR.Contracts` (Domain projesine — yalnızca INotification interface için)

### Database Değişiklikleri
- Yok

### Migration Gerekiyor mu?
- Hayır

### Bağımlı Olduğu Görevler
- TASK 0.1 (Solution yapısı)

### Acceptance Criteria
- [ ] BaseEntity Id, audit alanları ve soft delete içeriyor
- [ ] AggregateRoot domain event koleksiyonu yönetiyor
- [ ] ValueObject structural equality sağlıyor
- [ ] IDomainEvent MediatR.INotification'dan türüyor
- [ ] DomainEvent record EventId ve OccurredOn otomatik atıyor
- [ ] ITenantEntity TenantId property içeriyor
- [ ] Tüm unit test'ler geçiyor

### Definition of Done (DoD)
- [ ] Kod derlenebilir
- [ ] Unit test'ler yazıldı ve geçiyor (min %90 coverage)
- [ ] Code review tamamlandı
- [ ] Mimari dokümana uygunluk doğrulandı

### Test Gereksinimleri

| Test Tipi | Gereksinim |
|-----------|-----------|
| **Unit Test** | BaseEntity Id ataması, AggregateRoot event ekleme/temizleme, ValueObject equality/inequality, DomainEvent otomatik alan ataması |
| **Integration Test** | Yok |
| **Manual Test** | Yok |

### SOLID & Design Pattern
- **SRP:** Her sınıf tek bir domain kavramını temsil eder
- **OCP:** Abstract sınıflar extension'a açık, modification'a kapalı
- **LSP:** Türetilmiş sınıflar base davranışı bozmamalı
- **Design Pattern:** Template Method (ValueObject.GetEqualityComponents), Marker Interface (ITenantEntity)

### Clean Architecture Katmanları
- **Domain** (tek etkilenen katman)

### Domain Event
- Yok (altyapı oluşturuluyor)

### Validation Kuralları
- BaseEntity.Id boş olamaz
- AggregateRoot.DomainEvents null dönemez (boş liste dönmeli)

### Authorization Policy
- Yok

### Log Kayıtları
- Yok

### Audit Log
- Hayır (altyapı henüz yok)

### Cache
- Hayır

### Transaction
- Hayır

### Olası Exception'lar
- `ArgumentNullException` (ValueObject null equality kontrolü)

### Performans Notları
- ValueObject.GetHashCode() her çağrıda hesaplanır — immutable olduğu için sorun yok
- DomainEvents listesi `List<T>` olarak tutulur, `AsReadOnly()` ile dışarı verilir

### Beklenen Çıktılar
- Domain katmanının tüm soyut tipleri hazır
- Diğer görevler bu soyutlamaları kullanabilir durumda

### Önerilen Git Commit Mesajı
```
feat(domain): add base entity, aggregate root, value object abstractions

- Implement BaseEntity with Id, audit fields, soft delete
- Implement AggregateRoot with domain event management
- Implement ValueObject with structural equality
- Add IDomainEvent, DomainEvent, ITenantEntity, IAuditableEntity
- Add unit tests for all base classes
```

### Tahmini Süre
**4 saat**

---

## TASK 0.3 — Application Katmanı CQRS Soyutlamaları ve Result Pattern

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 0 — Foundation |
| **Feature** | CQRS Base Types & Result Pattern |
| **User Story** | Geliştirici olarak, tüm command ve query handler'lar için standart bir CQRS yapısı ve tutarlı sonuç dönüş mekanizması istiyorum ki hata yönetimi ve API response'ları tutarlı olsun. |

### Neden Yapılıyor?
CQRS olmadan command/query ayrımı yapılamaz. Result pattern olmadan exception-based error handling'e bağımlı kalınır ve API'den tutarlı response üretilemez.

### Ürüne Hangi Değeri Katıyor?
- Tutarlı API response formatı (başarılı/hatalı)
- Exception yerine Result-based error handling
- Command/Query ayrımı ile performans optimizasyonu

### Teknik Amaç
MediatR tabanlı CQRS base type'ları ve Result<T> pattern implementasyonu.

### Teknik Açıklama
`ICommand<T>`, `IQuery<T>`, `ICommandHandler<T>`, `IQueryHandler<T>` abstraction'ları MediatR üzerine inşa edilecek. `Result<T>` sınıfı success/failure durumlarını, hata mesajlarını ve validation hatalarını taşıyacak.

### Yapılacak Teknik İşler
1. `ICommand<TResponse>` interface oluştur (MediatR.IRequest<Result<TResponse>> türeyen)
2. `ICommand` interface oluştur (response'suz command'lar için)
3. `IQuery<TResponse>` interface oluştur (MediatR.IRequest<Result<TResponse>> türeyen)
4. `ICommandHandler<TCommand, TResponse>` interface oluştur
5. `IQueryHandler<TQuery, TResponse>` interface oluştur
6. `Result` sınıfı oluştur (IsSuccess, IsFailure, Error)
7. `Result<T>` generic sınıfı oluştur (Value, implicit operators)
8. `Error` record oluştur (Code, Message, Type enum)
9. `ValidationError` record oluştur (PropertyName, ErrorMessage)
10. `PagedResult<T>` oluştur (Items, Page, PageSize, TotalCount, TotalPages)
11. Unit test'ler yaz

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Application.Common.CQRS.ICommand
SafeFlow.Application.Common.CQRS.ICommand<TResponse>
SafeFlow.Application.Common.CQRS.IQuery<TResponse>
SafeFlow.Application.Common.CQRS.ICommandHandler<TCommand, TResponse>
SafeFlow.Application.Common.CQRS.IQueryHandler<TQuery, TResponse>
SafeFlow.Application.Common.Results.Result
SafeFlow.Application.Common.Results.Result<T>
SafeFlow.Application.Common.Results.Error
SafeFlow.Application.Common.Results.ValidationError
SafeFlow.Application.Common.Results.PagedResult<T>
```

### Oluşturulacak Dosyalar
```
src/SafeFlow.Application/Common/CQRS/ICommand.cs
src/SafeFlow.Application/Common/CQRS/IQuery.cs
src/SafeFlow.Application/Common/CQRS/ICommandHandler.cs
src/SafeFlow.Application/Common/CQRS/IQueryHandler.cs
src/SafeFlow.Application/Common/Results/Result.cs
src/SafeFlow.Application/Common/Results/Error.cs
src/SafeFlow.Application/Common/Results/ValidationError.cs
src/SafeFlow.Application/Common/Results/PagedResult.cs
tests/SafeFlow.Application.Tests/Common/ResultTests.cs
tests/SafeFlow.Application.Tests/Common/PagedResultTests.cs
```

### Oluşturulacak Endpointler
- Yok

### Gerekli NuGet Paketleri
- `MediatR` (Application projesine)

### Database Değişiklikleri
- Yok

### Migration Gerekiyor mu?
- Hayır

### Bağımlı Olduğu Görevler
- TASK 0.1 (Solution yapısı)

### Acceptance Criteria
- [ ] Result.Success() başarılı sonuç döndürüyor
- [ ] Result.Failure(error) hatalı sonuç döndürüyor
- [ ] Result<T>.Value başarılı durumlarda değer döndürüyor
- [ ] ICommand ve IQuery MediatR.IRequest'ten türüyor
- [ ] PagedResult sayfalama bilgilerini doğru hesaplıyor
- [ ] Tüm unit test'ler geçiyor

### Definition of Done (DoD)
- [ ] Kod derlenebilir
- [ ] Unit test'ler yazıldı ve geçiyor
- [ ] Code review tamamlandı

### Test Gereksinimleri

| Test Tipi | Gereksinim |
|-----------|-----------|
| **Unit Test** | Result success/failure, Result<T> value erişim, Error oluşturma, PagedResult hesaplama |
| **Integration Test** | Yok |
| **Manual Test** | Yok |

### SOLID & Design Pattern
- **SRP:** Command ve Query ayrı sorumluluk
- **ISP:** ICommand ve IQuery ayrı interface'ler
- **Design Pattern:** CQRS, Result/Either Pattern, Mediator Pattern

### Clean Architecture Katmanları
- **Application** (tek etkilenen katman)

### Domain Event
- Yok

### Validation Kuralları
- Result.Value erişiminde IsSuccess kontrol edilmeli

### Authorization Policy
- Yok

### Log Kayıtları
- Yok

### Audit Log / Cache / Transaction
- Hayır

### Olası Exception'lar
- `InvalidOperationException` (Failure durumunda Value'ya erişim)

### Performans Notları
- Result<T> struct yerine class olarak implemente edilecek (nullable reference type uyumu)
- Implicit operator aşırı kullanımından kaçınılmalı

### Beklenen Çıktılar
- CQRS base type'ları hazır
- Result pattern tüm handler'larda kullanılabilir

### Önerilen Git Commit Mesajı
```
feat(application): add CQRS abstractions and Result pattern

- Implement ICommand, IQuery, ICommandHandler, IQueryHandler
- Implement Result, Result<T> with success/failure states
- Add Error, ValidationError types
- Add PagedResult<T> for paginated queries
- Add unit tests
```

### Tahmini Süre
**4 saat**

---

## TASK 0.4 — MediatR Pipeline Behavior'ları

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 0 — Foundation |
| **Feature** | Cross-Cutting Pipeline Behaviors |
| **User Story** | Geliştirici olarak, her command/query handler'ın otomatik olarak logging, validation ve transaction yönetiminden geçmesini istiyorum ki her handler'da tekrar eden kodu yazmak zorunda kalmayayım. |

### Neden Yapılıyor?
Cross-cutting concern'ler (logging, validation, performance, transaction) her handler'da tekrar eder. Pipeline behavior'lar bu tekrarı ortadan kaldırır ve tek noktadan yönetim sağlar.

### Ürüne Hangi Değeri Katıyor?
- Tüm command'larda otomatik validation
- Tüm isteklerde otomatik loglama
- Yavaş sorguların otomatik tespiti
- Hatasız handler kodu (cross-cutting endişeleri ayırılmış)

### Teknik Amaç
MediatR `IPipelineBehavior<TRequest, TResponse>` implementasyonları oluşturmak.

### Teknik Açıklama
4 pipeline behavior oluşturulacak: LoggingBehavior (her istek loglanır), ValidationBehavior (FluentValidation ile otomatik doğrulama), PerformanceBehavior (yavaş istekler uyarı loglar), UnhandledExceptionBehavior (beklenmeyen hatalar loglanır).

### Yapılacak Teknik İşler
1. `LoggingBehavior<TRequest, TResponse>` — Her isteğin başlangıç/bitiş loglaması
2. `ValidationBehavior<TRequest, TResponse>` — FluentValidation entegrasyonu
3. `PerformanceBehavior<TRequest, TResponse>` — 500ms üzeri istekleri uyarı ile logla
4. `UnhandledExceptionBehavior<TRequest, TResponse>` — Beklenmeyen hataları logla
5. DI registration (behavior sıralaması kritik)
6. Unit test'ler yaz

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Application.Common.Behaviors.LoggingBehavior<TRequest, TResponse>
SafeFlow.Application.Common.Behaviors.ValidationBehavior<TRequest, TResponse>
SafeFlow.Application.Common.Behaviors.PerformanceBehavior<TRequest, TResponse>
SafeFlow.Application.Common.Behaviors.UnhandledExceptionBehavior<TRequest, TResponse>
```

### Oluşturulacak Dosyalar
```
src/SafeFlow.Application/Common/Behaviors/LoggingBehavior.cs
src/SafeFlow.Application/Common/Behaviors/ValidationBehavior.cs
src/SafeFlow.Application/Common/Behaviors/PerformanceBehavior.cs
src/SafeFlow.Application/Common/Behaviors/UnhandledExceptionBehavior.cs
src/SafeFlow.Application/DependencyInjection.cs
tests/SafeFlow.Application.Tests/Common/Behaviors/ValidationBehaviorTests.cs
tests/SafeFlow.Application.Tests/Common/Behaviors/LoggingBehaviorTests.cs
```

### Oluşturulacak Endpointler
- Yok

### Gerekli NuGet Paketleri
- `FluentValidation` (Application projesine)
- `FluentValidation.DependencyInjectionExtensions` (Application projesine)

### Database Değişiklikleri
- Yok

### Migration Gerekiyor mu?
- Hayır

### Bağımlı Olduğu Görevler
- TASK 0.3 (CQRS soyutlamaları)

### Acceptance Criteria
- [ ] ValidationBehavior validation hatalarında Result.Failure döndürüyor
- [ ] LoggingBehavior her isteğin adını ve süresini logluyor
- [ ] PerformanceBehavior 500ms üzeri isteklerde Warning log üretiyor
- [ ] Behavior'lar doğru sırada çalışıyor (Logging → Validation → Performance → Handler)
- [ ] Unit test'ler geçiyor

### Definition of Done (DoD)
- [ ] Kod derlenebilir
- [ ] Unit test'ler geçiyor
- [ ] DI registration doğru sırada
- [ ] Code review tamamlandı

### Test Gereksinimleri

| Test Tipi | Gereksinim |
|-----------|-----------|
| **Unit Test** | ValidationBehavior: geçerli/geçersiz istek, LoggingBehavior: log üretimi, PerformanceBehavior: yavaş istek tespiti |
| **Integration Test** | Yok |
| **Manual Test** | Yok |

### SOLID & Design Pattern
- **OCP:** Yeni behavior eklemek mevcut kodu değiştirmiyor
- **SRP:** Her behavior tek bir cross-cutting concern
- **Design Pattern:** Chain of Responsibility, Decorator Pattern, Pipeline Pattern

### Clean Architecture Katmanları
- **Application**

### Domain Event / Validation / Authorization
- Yok

### Log Kayıtları
- `[LOG] Handling {RequestName}` (başlangıç)
- `[LOG] Handled {RequestName} in {ElapsedMs}ms` (bitiş)
- `[WARN] Long running request: {RequestName} ({ElapsedMs}ms)` (performance)
- `[ERROR] Unhandled exception for {RequestName}` (hata)

### Audit Log / Cache / Transaction
- Hayır

### Olası Exception'lar
- `ValidationException` (FluentValidation, ancak Result.Failure'a dönüştürülür)

### Performans Notları
- PerformanceBehavior threshold değeri konfigürasyondan okunmalı
- Stopwatch kullanımı minimal overhead

### Beklenen Çıktılar
- Tüm MediatR pipeline behavior'ları hazır ve DI'da kayıtlı

### Önerilen Git Commit Mesajı
```
feat(application): add MediatR pipeline behaviors

- Add LoggingBehavior for request/response logging
- Add ValidationBehavior with FluentValidation integration
- Add PerformanceBehavior for slow request detection
- Add UnhandledExceptionBehavior for error logging
- Register behaviors in DI with correct order
```

### Tahmini Süre
**4 saat**

---

## TASK 0.5 — Infrastructure Katmanı: EF Core DbContext ve Konfigürasyon

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 0 — Foundation |
| **Feature** | EF Core DbContext Altyapısı |
| **User Story** | Geliştirici olarak, PostgreSQL bağlantısı ve EF Core DbContext altyapısı istiyorum ki domain entity'lerini veritabanına persist edebileyim. |

### Neden Yapılıyor?
Veritabanı erişimi olmadan hiçbir veri kalıcı hale getirilemez. DbContext tüm data access işlemlerinin temelidir.

### Ürüne Hangi Değeri Katıyor?
- Veritabanı kalıcılığı
- Otomatik audit bilgisi (SaveChanges override)
- Domain event dispatch mekanizması
- Soft delete global query filter

### Teknik Amaç
PostgreSQL bağlantılı EF Core DbContext, otomatik audit, soft delete filter ve domain event dispatch altyapısını kurmak.

### Teknik Açıklama
`SafeFlowDbContext` oluşturulacak. `SaveChangesAsync` override edilerek audit alanları otomatik doldurulacak, domain event'ler MediatR üzerinden dispatch edilecek. Global query filter ile soft-deleted entity'ler otomatik filtrelenecek.

### Yapılacak Teknik İşler
1. `SafeFlowDbContext` sınıfı oluştur
2. `SaveChangesAsync` override — audit alanları otomatik set
3. `SaveChangesAsync` override — domain event dispatch (MediatR)
4. Soft delete global query filter ekle
5. `IUnitOfWork` implementasyonu (DbContext üzerinden)
6. `appsettings.json` connection string yapılandırması
7. `ICurrentUserService` interface tanımla (CreatedBy/UpdatedBy için)
8. EF Core convention configuration (snake_case naming, vb.)
9. DI registration (`AddDbContext`, connection string)
10. Basit integration test yaz (InMemory veya TestContainer)

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Infrastructure.Persistence.SafeFlowDbContext
SafeFlow.Infrastructure.Persistence.UnitOfWork
SafeFlow.Application.Common.Interfaces.ICurrentUserService
SafeFlow.Application.Common.Interfaces.IUnitOfWork
SafeFlow.Application.Common.Interfaces.IDbContext
```

### Oluşturulacak Dosyalar
```
src/SafeFlow.Infrastructure/Persistence/SafeFlowDbContext.cs
src/SafeFlow.Infrastructure/Persistence/UnitOfWork.cs
src/SafeFlow.Infrastructure/DependencyInjection.cs
src/SafeFlow.Application/Common/Interfaces/ICurrentUserService.cs
src/SafeFlow.Application/Common/Interfaces/IUnitOfWork.cs
src/SafeFlow.Application/Common/Interfaces/IDbContext.cs
src/SafeFlow.API/appsettings.json (connection string ekleme)
src/SafeFlow.API/appsettings.Development.json
```

### Oluşturulacak Endpointler
- Yok

### Gerekli NuGet Paketleri
- `Microsoft.EntityFrameworkCore` (Infrastructure)
- `Npgsql.EntityFrameworkCore.PostgreSQL` (Infrastructure)
- `Microsoft.EntityFrameworkCore.Tools` (API — migration için)
- `Microsoft.EntityFrameworkCore.Design` (API — migration için)

### Database Değişiklikleri
- PostgreSQL veritabanı oluşturulacak (Docker ile)

### Migration Gerekiyor mu?
- Hayır (henüz entity yok, InitialCreate migration TASK 1.x'te)

### Bağımlı Olduğu Görevler
- TASK 0.1 (Solution yapısı)
- TASK 0.2 (Domain base classes)

### Acceptance Criteria
- [ ] DbContext PostgreSQL'e bağlanabiliyor
- [ ] SaveChangesAsync audit alanlarını otomatik dolduruyor
- [ ] SaveChangesAsync domain event'leri dispatch ediyor
- [ ] Soft delete filter çalışıyor
- [ ] IUnitOfWork implementasyonu mevcut
- [ ] Connection string appsettings'ten okunuyor

### Definition of Done (DoD)
- [ ] Kod derlenebilir
- [ ] PostgreSQL bağlantısı doğrulanmış
- [ ] Code review tamamlandı

### Test Gereksinimleri

| Test Tipi | Gereksinim |
|-----------|-----------|
| **Unit Test** | Audit field doldurma, domain event toplama |
| **Integration Test** | PostgreSQL bağlantı testi (Docker ile) |
| **Manual Test** | Connection string ile bağlantı doğrulama |

### SOLID & Design Pattern
- **DIP:** IUnitOfWork interface, DbContext implementasyon
- **SRP:** DbContext veri erişimi, UnitOfWork transaction
- **Design Pattern:** Unit of Work, Repository (implicitly via DbContext)

### Clean Architecture Katmanları
- **Application** (interface tanımları)
- **Infrastructure** (implementasyonlar)

### Domain Event
- Dispatch mekanizması kurulacak (event üretimi henüz yok)

### Validation Kuralları
- Connection string boş olamaz

### Authorization Policy
- Yok

### Log Kayıtları
- `[INFO] SaveChangesAsync: {EntityCount} entities saved`
- `[INFO] Domain events dispatched: {EventCount}`
- `[ERROR] Database connection failed`

### Audit Log
- Evet — SaveChangesAsync otomatik audit alanları doldurur

### Cache
- Hayır

### Transaction
- Evet — SaveChangesAsync tek transaction içinde

### Olası Exception'lar
- `DbUpdateException` (constraint violation)
- `DbUpdateConcurrencyException` (concurrent update)
- `NpgsqlException` (bağlantı hatası)

### Performans Notları
- Domain event dispatch SaveChanges içinde yapılacak (same transaction)
- Büyük batch insert'lerde dikkat (memory)
- `AsSplitQuery()` kullanımına dikkat (N+1 prevention)

### Beklenen Çıktılar
- PostgreSQL bağlantılı, audit ve event dispatch destekli DbContext

### Önerilen Git Commit Mesajı
```
feat(infrastructure): add EF Core DbContext with PostgreSQL

- Implement SafeFlowDbContext with audit field auto-fill
- Add domain event dispatching on SaveChangesAsync
- Add soft delete global query filter
- Implement IUnitOfWork via DbContext
- Configure PostgreSQL connection
```

### Tahmini Süre
**4 saat**

---

## TASK 0.6 — API Katmanı: Program.cs, Service Registration ve Middleware Pipeline

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 0 — Foundation |
| **Feature** | API Bootstrap ve Middleware Pipeline |
| **User Story** | Geliştirici olarak, API'nin Correlation ID, exception handling, request logging ve Swagger desteği ile ayağa kalkmasını istiyorum ki geliştirme sürecinde hızlı debug yapabileyim. |

### Neden Yapılıyor?
API projesi tüm HTTP isteklerinin giriş noktasıdır. Middleware pipeline olmadan correlation ID, hata yönetimi ve request tracing çalışmaz.

### Ürüne Hangi Değeri Katıyor?
- Tutarlı hata response'ları (RFC 7807)
- İstek izlenebilirliği (Correlation ID)
- API dokümantasyonu (Swagger/OpenAPI)
- Geliştirme hızı (hot reload, Swagger UI)

### Teknik Amaç
API katmanının bootstrap, middleware pipeline ve service registration yapısını kurmak.

### Teknik Açıklama
`Program.cs` minimal API yapısında kurulacak. Middleware sırası: Correlation ID → Exception Handler → Request Logging → Authentication → Authorization → Rate Limiting → Routing. `GlobalExceptionHandler` RFC 7807 formatında hata döndürecek.

### Yapılacak Teknik İşler
1. `Program.cs` — Service registration (MediatR, FluentValidation, EF Core, Swagger)
2. `CorrelationIdMiddleware` — Her isteğe benzersiz correlation ID ata
3. `GlobalExceptionHandlerMiddleware` — RFC 7807 Problem Details formatında hata dön
4. `RequestLoggingMiddleware` — İstek başlangıç/bitiş, süre, status code logla
5. Swagger/OpenAPI yapılandırması (JWT bearer auth destekli)
6. `ICurrentUserService` implementasyonu (HttpContext'ten JWT claim okuma)
7. API response envelope helper (ApiResponse<T>)
8. Controller base class (BaseApiController)
9. Appsettings yapılandırma sınıfları (JwtSettings, vb.)
10. Middleware sıralaması testi

### Oluşturulacak Sınıflar
```csharp
SafeFlow.API.Middleware.CorrelationIdMiddleware
SafeFlow.API.Middleware.GlobalExceptionHandlerMiddleware
SafeFlow.API.Middleware.RequestLoggingMiddleware
SafeFlow.API.Services.CurrentUserService
SafeFlow.API.Common.BaseApiController
SafeFlow.API.Common.ApiResponse<T>
SafeFlow.API.Common.ApiErrorResponse
```

### Oluşturulacak Dosyalar
```
src/SafeFlow.API/Program.cs
src/SafeFlow.API/Middleware/CorrelationIdMiddleware.cs
src/SafeFlow.API/Middleware/GlobalExceptionHandlerMiddleware.cs
src/SafeFlow.API/Middleware/RequestLoggingMiddleware.cs
src/SafeFlow.API/Services/CurrentUserService.cs
src/SafeFlow.API/Common/BaseApiController.cs
src/SafeFlow.API/Common/ApiResponse.cs
src/SafeFlow.API/Common/ApiErrorResponse.cs
src/SafeFlow.API/appsettings.json (güncelleme)
```

### Oluşturulacak Endpointler
- `GET /` — API bilgisi (versiyon, ortam)

### Gerekli NuGet Paketleri
- `Swashbuckle.AspNetCore` (API)
- `Microsoft.AspNetCore.Authentication.JwtBearer` (API)

### Database Değişiklikleri
- Yok

### Migration Gerekiyor mu?
- Hayır

### Bağımlı Olduğu Görevler
- TASK 0.4 (Pipeline behaviors — DI registration)
- TASK 0.5 (DbContext — DI registration)

### Acceptance Criteria
- [ ] API başarıyla ayağa kalkıyor (`dotnet run`)
- [ ] Her response X-Correlation-Id header içeriyor
- [ ] Beklenmeyen hata RFC 7807 formatında dönüyor
- [ ] Swagger UI erişilebilir (`/swagger`)
- [ ] Request log'ları üretiliyor (method, path, status, duration)
- [ ] ICurrentUserService HttpContext claim'lerden okuyor

### Definition of Done (DoD)
- [ ] API çalışır durumda
- [ ] Middleware'ler test edildi
- [ ] Swagger UI erişilebilir
- [ ] Code review tamamlandı

### Test Gereksinimleri

| Test Tipi | Gereksinim |
|-----------|-----------|
| **Unit Test** | CorrelationIdMiddleware header set etme, ExceptionHandler error format |
| **Integration Test** | API'yi ayağa kaldırıp GET / isteği atma, correlation ID doğrulama |
| **Manual Test** | Swagger UI açılması, bir istek atılıp response header kontrolü |

### SOLID & Design Pattern
- **SRP:** Her middleware tek bir concern
- **OCP:** Yeni middleware eklemek mevcut pipeline'ı bozmaz
- **Design Pattern:** Chain of Responsibility (middleware pipeline), Decorator

### Clean Architecture Katmanları
- **API/Presentation**
- **Application** (ICurrentUserService interface)

### Log Kayıtları
- `[INFO] Request {Method} {Path} started [CorrelationId: {Id}]`
- `[INFO] Request {Method} {Path} completed {StatusCode} in {Duration}ms`
- `[ERROR] Unhandled exception: {Message} [CorrelationId: {Id}]`

### Audit Log / Cache / Transaction
- Hayır

### Olası Exception'lar
- Middleware'deki hatalar `GlobalExceptionHandler` tarafından yakalanır

### Performans Notları
- CorrelationId middleware'i header kontrolü yapmalı (varsa mevcut ID kullan)
- RequestLoggingMiddleware body okumaktan kaçınmalı (performance hit)

### Beklenen Çıktılar
- Çalışan API, Swagger, correlation ID, hata yönetimi

### Önerilen Git Commit Mesajı
```
feat(api): add middleware pipeline and service registration

- Add CorrelationIdMiddleware for request tracing
- Add GlobalExceptionHandlerMiddleware with RFC 7807
- Add RequestLoggingMiddleware
- Configure Swagger with JWT bearer auth
- Implement ICurrentUserService from HttpContext
- Add BaseApiController and ApiResponse
```

### Tahmini Süre
**4 saat**

---

## TASK 0.7 — Health Check Endpoint'leri

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 0 — Foundation |
| **Feature** | Health Check & Readiness/Liveness Probes |
| **User Story** | DevOps mühendisi olarak, uygulamanın sağlık durumunu kontrol edebileceğim endpoint'ler istiyorum ki Kubernetes/Docker ortamında otomatik monitoring yapabileyim. |

### Neden Yapılıyor?
Production deployment'ta health check olmadan container orchestrator (K8s) uygulamanın sağlıklı olup olmadığını bilemez ve trafiği düzgün yönlendiremez.

### Ürüne Hangi Değeri Katıyor?
- Zero-downtime deployment
- Otomatik container restart (unhealthy durumda)
- Monitoring ve alerting altyapısı

### Teknik Amaç
ASP.NET Health Checks ile `/health`, `/health/live`, `/health/ready` endpoint'lerini oluşturmak.

### Teknik Açıklama
3 health check endpoint: General (`/health`), Liveness (`/health/live` — uygulama çalışıyor mu?), Readiness (`/health/ready` — bağımlılıklar hazır mı?). Readiness check DB ve cache bağlantısını doğrulayacak.

### Yapılacak Teknik İşler
1. ASP.NET Health Checks NuGet paketini ekle
2. `DbContextHealthCheck` — PostgreSQL bağlantı kontrolü
3. General health check endpoint `/health` yapılandır
4. Liveness endpoint `/health/live` yapılandır
5. Readiness endpoint `/health/ready` yapılandır (DB check dahil)
6. Health check response JSON formatı özelleştir
7. Integration test yaz

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Infrastructure.HealthChecks.DatabaseHealthCheck
SafeFlow.API.HealthChecks.HealthCheckResponseWriter
```

### Oluşturulacak Dosyalar
```
src/SafeFlow.Infrastructure/HealthChecks/DatabaseHealthCheck.cs
src/SafeFlow.API/HealthChecks/HealthCheckResponseWriter.cs
```

### Oluşturulacak Endpointler
- `GET /health` — Genel sağlık durumu
- `GET /health/live` — Liveness probe
- `GET /health/ready` — Readiness probe (DB + cache)

### Gerekli NuGet Paketleri
- `AspNetCore.HealthChecks.NpgSql` (Infrastructure)
- `Microsoft.Extensions.Diagnostics.HealthChecks` (Infrastructure)

### Database Değişiklikleri
- Yok

### Migration Gerekiyor mu?
- Hayır

### Bağımlı Olduğu Görevler
- TASK 0.5 (DbContext)
- TASK 0.6 (Program.cs)

### Acceptance Criteria
- [ ] `GET /health` 200 OK döndürüyor
- [ ] `GET /health/live` uygulama çalışıyorsa 200 döndürüyor
- [ ] `GET /health/ready` DB bağlantısı varsa 200, yoksa 503 döndürüyor
- [ ] Response JSON formatında ve detaylı

### Definition of Done (DoD)
- [ ] Endpoint'ler çalışıyor
- [ ] Integration test geçiyor
- [ ] Code review tamamlandı

### Test Gereksinimleri

| Test Tipi | Gereksinim |
|-----------|-----------|
| **Unit Test** | Yok |
| **Integration Test** | Üç endpoint'e istek atıp status code doğrulama |
| **Manual Test** | Browser/Postman ile endpoint'leri test etme |

### SOLID & Design Pattern
- **SRP:** Her health check tek bir bağımlılığı kontrol eder

### Clean Architecture Katmanları
- **Infrastructure** (health check implementasyonları)
- **API** (endpoint registration, response writer)

### Log Kayıtları
- `[WARN] Health check failed: {CheckName} - {Reason}`

### Beklenen Çıktılar
- 3 çalışan health check endpoint'i

### Önerilen Git Commit Mesajı
```
feat(api): add health check endpoints

- Add /health, /health/live, /health/ready endpoints
- Add DatabaseHealthCheck for PostgreSQL
- Customize health check JSON response format
```

### Tahmini Süre
**2 saat**

---

## TASK 0.8 — Structured Logging (Serilog)

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 0 — Foundation |
| **Feature** | Structured Logging Altyapısı |
| **User Story** | DevOps mühendisi olarak, uygulamadan structured log çıktısı almak istiyorum ki log'ları kolayca arayabileyim ve analiz edebileyim. |

### Neden Yapılıyor?
Console.WriteLine ile debug loglama production'da işe yaramaz. Structured logging olmadan log'larda arama, filtreleme ve korelasyon yapılamaz.

### Ürüne Hangi Değeri Katıyor?
- Sorun tespiti hızlanır
- Correlation ID bazlı istek takibi
- Log analizi ve alerting altyapısı

### Teknik Amaç
Serilog ile structured logging, console + file sink yapılandırması.

### Teknik Açıklama
Serilog konfigürasyonu `appsettings.json` üzerinden yapılacak. MVP'de Console ve File sink, gelecekte Seq/ELK eklenmek üzere. Tüm log'lar structured (JSON) formatında olacak. Enricher'lar ile Correlation ID, Machine Name, Environment otomatik eklenecek.

### Yapılacak Teknik İşler
1. Serilog NuGet paketlerini ekle
2. `Program.cs`'de Serilog bootstrap yapılandırması
3. `appsettings.json` Serilog configuration bölümü
4. Console Sink (development) ve File Sink (rolling file)
5. Enricher'lar: CorrelationId, MachineName, Environment, ThreadId
6. Request logging middleware Serilog entegrasyonu
7. Log seviyesi yapılandırması (appsettings'ten)
8. Startup log mesajları

### Oluşturulacak Sınıflar
```csharp
SafeFlow.API.Logging.CorrelationIdEnricher
```

### Oluşturulacak Dosyalar
```
src/SafeFlow.API/Logging/CorrelationIdEnricher.cs
src/SafeFlow.API/appsettings.json (Serilog bölümü ekleme)
```

### Oluşturulacak Endpointler
- Yok

### Gerekli NuGet Paketleri
- `Serilog.AspNetCore` (API)
- `Serilog.Enrichers.Environment` (API)
- `Serilog.Enrichers.Thread` (API)
- `Serilog.Sinks.Console` (API)
- `Serilog.Sinks.File` (API)

### Database Değişiklikleri
- Yok

### Migration Gerekiyor mu?
- Hayır

### Bağımlı Olduğu Görevler
- TASK 0.6 (Program.cs)

### Acceptance Criteria
- [ ] Log'lar structured JSON formatında Console'a yazılıyor
- [ ] Log'lar rolling file'a yazılıyor (logs/ dizini)
- [ ] Correlation ID log property'lerinde mevcut
- [ ] Log seviyesi appsettings'ten değiştirilebiliyor
- [ ] Request başlangıç/bitiş logları üretiliyor

### Definition of Done (DoD)
- [ ] Serilog yapılandırması tamamlandı
- [ ] Log çıktıları doğrulandı
- [ ] Code review tamamlandı

### Test Gereksinimleri

| Test Tipi | Gereksinim |
|-----------|-----------|
| **Unit Test** | CorrelationIdEnricher property ekleme |
| **Integration Test** | API'ye istek at, log dosyasında çıktı doğrula |
| **Manual Test** | Console çıktısını gözlemle |

### SOLID & Design Pattern
- **OCP:** Yeni sink eklemek mevcut yapıyı bozmaz
- **DIP:** ILogger<T> injection

### Clean Architecture Katmanları
- **API** (Serilog yapılandırması)

### Beklenen Çıktılar
- Structured logging çalışır durumda

### Önerilen Git Commit Mesajı
```
feat(api): add Serilog structured logging

- Configure Serilog with Console and File sinks
- Add CorrelationIdEnricher for request tracing
- Configure rolling file logging in logs/ directory
- Add structured JSON log format
```

### Tahmini Süre
**2 saat**

---

## TASK 0.9 — Cache Service Soyutlaması

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 0 — Foundation |
| **Feature** | Cache Service Abstraction |
| **User Story** | Geliştirici olarak, cache mekanizmasını soyutlanmış bir interface üzerinden kullanmak istiyorum ki MVP'de IMemoryCache, production'da Redis'e geçişte kod değişikliği gerekmesiz. |

### Neden Yapılıyor?
Mimari karar: MVP'de `IMemoryCache`, production'da Redis. Bu geçişin sorunsuz olması için soyutlama şart.

### Ürüne Hangi Değeri Katıyor?
- Performans iyileştirmesi (sık erişilen veri cache'leme)
- Production'a geçişte sıfır kod değişikliği
- Test edilebilirlik (mock cache)

### Teknik Amaç
`ICacheService` interface ve `MemoryCacheService` implementasyonu oluşturmak.

### Teknik Açıklama
`ICacheService` generic metotlar sunacak: `GetAsync<T>`, `SetAsync<T>`, `RemoveAsync`, `ExistsAsync`. MVP implementasyonu `IMemoryCache` üzerinden çalışacak. Cache key convention'ı standardize edilecek.

### Yapılacak Teknik İşler
1. `ICacheService` interface tanımla
2. `MemoryCacheService` implementasyonu (IMemoryCache)
3. `CacheKeys` static class (key convention)
4. DI registration
5. Unit test'ler yaz

### Oluşturulacak Sınıflar
```csharp
SafeFlow.Application.Common.Interfaces.ICacheService
SafeFlow.Infrastructure.Caching.MemoryCacheService
SafeFlow.Infrastructure.Caching.CacheKeys
```

### Oluşturulacak Dosyalar
```
src/SafeFlow.Application/Common/Interfaces/ICacheService.cs
src/SafeFlow.Infrastructure/Caching/MemoryCacheService.cs
src/SafeFlow.Infrastructure/Caching/CacheKeys.cs
tests/SafeFlow.Infrastructure.Tests/Caching/MemoryCacheServiceTests.cs
```

### Oluşturulacak Endpointler
- Yok

### Gerekli NuGet Paketleri
- `Microsoft.Extensions.Caching.Memory` (Infrastructure)

### Database Değişiklikleri
- Yok

### Migration Gerekiyor mu?
- Hayır

### Bağımlı Olduğu Görevler
- TASK 0.1 (Solution yapısı)

### Acceptance Criteria
- [ ] ICacheService Get/Set/Remove/Exists çalışıyor
- [ ] MemoryCacheService IMemoryCache kullanıyor
- [ ] Cache expiration (absolute + sliding) destekleniyor
- [ ] CacheKeys convention tutarlı
- [ ] Unit test'ler geçiyor

### Definition of Done (DoD)
- [ ] Kod derlenebilir
- [ ] Unit test'ler geçiyor
- [ ] Code review tamamlandı

### Test Gereksinimleri

| Test Tipi | Gereksinim |
|-----------|-----------|
| **Unit Test** | Set/Get, expiration, Remove, Exists |
| **Integration Test** | Yok |
| **Manual Test** | Yok |

### SOLID & Design Pattern
- **DIP:** ICacheService interface, MemoryCacheService implementasyon
- **OCP:** Redis implementasyonu eklemek mevcut kodu bozmaz
- **Design Pattern:** Strategy Pattern (cache implementasyonu swap edilebilir)

### Clean Architecture Katmanları
- **Application** (ICacheService interface)
- **Infrastructure** (MemoryCacheService)

### Beklenen Çıktılar
- Cache altyapısı hazır, tüm modüller kullanabilir

### Önerilen Git Commit Mesajı
```
feat(infrastructure): add ICacheService abstraction with MemoryCache

- Define ICacheService interface in Application layer
- Implement MemoryCacheService with IMemoryCache
- Add CacheKeys static class for key conventions
- Add unit tests for cache operations
```

### Tahmini Süre
**2 saat**

---

## TASK 0.10 — Docker Compose Geliştirme Ortamı

| Alan | Değer |
|------|-------|
| **Epic** | EPIC 0 — Foundation |
| **Feature** | Docker Development Environment |
| **User Story** | Geliştirici olarak, tek komutla PostgreSQL ve tüm bağımlılıkların ayağa kalkmasını istiyorum ki yerel geliştirme ortamında hızlıca çalışmaya başlayabileyim. |

### Neden Yapılıyor?
Her geliştiricinin yerel makinesine PostgreSQL kurması yerine, Docker Compose ile standart geliştirme ortamı sağlanır.

### Ürüne Hangi Değeri Katıyor?
- Onboarding süresi düşer
- Tutarlı geliştirme ortamı
- CI/CD pipeline ile aynı ortam

### Teknik Amaç
Docker Compose ile PostgreSQL (ve gelecekte Redis, Seq) geliştirme ortamını hazırlamak.

### Teknik Açıklama
`docker-compose.yml` dosyası PostgreSQL 16, pgAdmin (opsiyonel) ve uygulama için network tanımlayacak. Volume ile veri kalıcılığı sağlanacak.

### Yapılacak Teknik İşler
1. `docker-compose.yml` oluştur (PostgreSQL 16)
2. `docker-compose.override.yml` — geliştirme ortamı ayarları
3. `.env` dosyası (environment variables)
4. PostgreSQL init script (veritabanı oluşturma)
5. Dockerfile (API projesi için — opsiyonel, gelecek sprint)
6. `README.md` güncelle (Docker kullanım talimatları)

### Oluşturulacak Dosyalar
```
docker-compose.yml
docker-compose.override.yml
.env
docker/postgres/init.sql
```

### Oluşturulacak Endpointler
- Yok

### Gerekli NuGet Paketleri
- Yok

### Database Değişiklikleri
- PostgreSQL `safeflow_dev` veritabanı oluşturulur

### Migration Gerekiyor mu?
- Hayır (init.sql ile DB oluşturma)

### Bağımlı Olduğu Görevler
- Yok (paralel çalışabilir)

### Acceptance Criteria
- [ ] `docker-compose up -d` ile PostgreSQL ayağa kalkıyor
- [ ] API PostgreSQL'e bağlanabiliyor
- [ ] Volume ile veri restart sonrası korunuyor
- [ ] `.env` dosyasından environment variables okunuyor

### Definition of Done (DoD)
- [ ] Docker Compose çalışıyor
- [ ] README güncel
- [ ] Code review tamamlandı

### Test Gereksinimleri

| Test Tipi | Gereksinim |
|-----------|-----------|
| **Unit Test** | Yok |
| **Integration Test** | Yok |
| **Manual Test** | `docker-compose up -d` → API bağlantı testi |

### Beklenen Çıktılar
- Tek komutla çalışan geliştirme ortamı

### Önerilen Git Commit Mesajı
```
feat(devops): add Docker Compose development environment

- Add PostgreSQL 16 service configuration
- Add init.sql for database creation
- Add .env file for environment variables
- Update README with setup instructions
```

### Tahmini Süre
**2 saat**

---

## EPIC 0 — Tamamlanma Özeti

| Metrik | Değer |
|--------|-------|
| **Toplam Görev** | 10 |
| **Toplam Tahmini Süre** | 32 saat (~4 iş günü) |
| **İlerleme** | %0 (henüz başlanmadı) |
| **Teknik Borç** | Yok (temiz başlangıç) |
| **Refactoring Önerisi** | Yok |

### Görev Bağımlılık Grafiği
```
TASK 0.1 (Solution) ──┬──→ TASK 0.2 (Domain Base) ──→ TASK 0.5 (DbContext) ──→ TASK 0.7 (Health)
                      │                                         ↓
                      ├──→ TASK 0.3 (CQRS/Result) ──→ TASK 0.4 (Behaviors) ──→ TASK 0.6 (Program.cs) ──→ TASK 0.8 (Serilog)
                      │
                      └──→ TASK 0.9 (Cache)
                      
TASK 0.10 (Docker) → Paralel çalışabilir
```

### Code Review Checklist — EPIC 0
- [ ] Clean Architecture katman referansları doğru yönde
- [ ] Domain katmanı hiçbir infrastructure paketine bağımlı değil
- [ ] Tüm interface'ler Application katmanında
- [ ] Tüm implementasyonlar Infrastructure katmanında
- [ ] Naming convention tutarlı (PascalCase classes, camelCase params)
- [ ] Nullable reference types aktif
- [ ] XML documentation ana sınıflarda mevcut
- [ ] Unit test coverage minimum %80
