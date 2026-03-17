# DECISIONS.MD (ADR.MD)

Bu dosya projedeki bütün mimari kararları içeren döküman dosyasıdır.

---

## ADR-0001 - Veritabanı

**Son Güncellenme**: 17.03.2026

**Seçim**: PostgreSQL

**Neden**: Aspire entegrasyonuyla container tabanlı, production'a yakın yerel geliştirme ortamı sağlar. `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL` paketi ile sağlık kontrolü ve distributed tracing desteği sunulur. EF Core Migrations ile şema yönetimi yapılır.

**Alternatifler**: In-Memory (önceki seçim), MSSQL, MySQL


## ADR-0002 - Mimari

**Son Güncellenme**: 16.03.2026

**Seçim**: Clean Architecture

**Neden**: En temiz kurumsal mimari.

**Alternatifler**: N-Layered, Hexagonal

## ADR-0003 - .NET Runtime

**Son Güncellenme**: 16.03.2026

**Seçim**: 9.0

**Neden**: Son kararlı sürüm.

**Alternatifler**: 8.0 (LTS), 7.0


## ADR-0004 - Orkestrasyon Katmanı

**Son Güncellenme**: 17.03.2026

**Seçim**: .NET Aspire 9.1.0

**Neden**: ProductService ve OrderService'i tek komutla başlatma, yerleşik Aspire Dashboard üzerinden distributed tracing / log / metrik görönürlüğü, ilerleyen aşamalarda servisler arası HTTP iletimiyle birlikte service discovery.

**Kapsam**: Yalnızca orkestrasyon ve observability; iş mantığına dokunulmaz.

**Alternatifler**: Docker Compose, Tye (deprecated)


## ADR-0005 - API Gateway

**Son Güncellenme**: 17.03.2026

**Seçim**: YARP (Yet Another Reverse Proxy) 2.2.0

**Neden**: Microsoft'un resmi .NET reverse proxy kütüphanesi. `Microsoft.Extensions.ServiceDiscovery.Yarp` paketi ile Aspire service discovery native entegrasyonu sağlar. `appsettings.json` tabanlı route/cluster konfigürasyonu ile sıfır kod değişikliğiyle yönlendirme kuralları güncellenebilir.

**Kapsam**: `src/Gateway` projesi; ProductService ve OrderService önünde tek giriş noktası. `/api/v1/products/**` → productservice, `/api/v1/orders/**` → orderservice.

**Alternatifler**: Ocelot (daha az aktif geliştirilmekte), Nginx (container tabanlı, .NET ekosistemi dışı)
