# DECISIONS.MD (ADR.MD)

Bu dosya projedeki bütün mimari kararları içeren döküman dosyasıdır.

---

## ADR-0001 - Veritabanı

**Son Güncellenme**: 16.03.2026

**Seçim**: In-Memory

**Neden**: Localde en hızlı test birimi.

**Alternatifler**: PostgreSQL, MSSQL, MySQL


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
