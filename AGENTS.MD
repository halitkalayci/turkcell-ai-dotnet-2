# AGENTS.MD

Bu dosya, .NET ile yazılacak E-Ticaret Sistemi Backend projesi için anayasal kuralları içeren en temel yönetişim dosyasıdır.

Bu projede herhangi bir teknolojisel-mimarisel karar sorgulanıyor ise [DECISIONS.MD](.github/DECISIONS.MD) dosyası incelenmelidir.

Bağımlılık kararları DECISIONS.MD ile senkronize olmak zorundadır. Herhangi bir infrastructure paketi seçiminden önce DECISIONS.MD okunmalı; seçilen teknoloji ADR kayıtlarıyla çelişirse implementasyon durdurulup kullanıcıya sorulmalıdır.

---

### 1) NASIL ÇALIŞILIR? (ZORUNLU İŞ AKIŞI)

#### 1.1) UYDURMA YASAK

Eğer herhangi bir bilgi eksikliğine/yanlışlığına rastlarsan bu bilgiyi uydurma yaparak ekleyemez/düzeltemezsin. Yapman gereken işlemi durdurup kullanıcıya sorarak ilerlemen.

#### 1.2) ÖNCE PLANLA, SONRA KODLA

Kod üretmeden önce şunları yapmak zorundasın;

- Bir dosya dökümü hazırla (hangi dosyalar değişecek/eklenecek + neden)

- Eğer varsa yeni eklenecek bağımlılıklar.

- Implementasyon planı.

> PLANI KULLANICI ONAYLAMADAN HİÇ BİR İMPLEMENTASYON YAPMA!

#### 1.3) KOD OLUŞTURMA STRATEJİSİ

Dosya oluşturma işlemleri bütünsel olarak ele alınamaz. Bütün implementasyon planını maksimum 5'er dosyalık gruplara böl (bölme stratejisi birbiriyle alakalı dosyalar olsun.) dosya dökümünde bu grup planını da paylaş.

#### 1.4) ÇIKTI FORMATI

- Resmi bir dil kullan, emoji kullanma.

- Yapılan işi minimal cümleler ile özet olarak maksimum 2-3 cümlelik özetlere böl. Detay özellikle istenirse ver.

- İlgili promptta öneri istenmediği sürece öneride bulunma.

- Dosya dökümlerinde yalnızca dosya ismi yaz, DOSYA YOLU EKLEME.

- Eğer bir/birden fazla yetenek kullanıldı ise kullanılan yetenekleri yaz, kontrol listesinde eksik var mı? Özet geç.

- Her implementasyon sonrası ilgili implementasyonu test edebileceğimiz `Happy-Path Testi` ver.

Örneğin `ProductService`'e yeni endpoint eklendiğini varsayalım:

````pwsh
cd src/AppHost
dotnet run
# Aspire Dashboard → http://localhost:15888
# productservice → Running, ardından:
curl http://localhost:{PRODUCTSERVICE_PORT}/api/v1/products
````
> Dashboard'da her iki servis "Running" durumundaysa ve endpoint yanıt veriyorsa test başarılı.

> **Not:** Port numaraları Aspire tarafından dinamik atanır. Gerçek port Dashboard üzerinden veya konsol çıktısından okunur.

#### 1.5) ÖZEL AGENT KULLANIMI (ZORUNLU)

Kompleks işlerde (katmanlar arası implementasyon, CRUD operasyonları, servis geliştirme gibi) varsa o işle ilgili özel agent kullanılması zorunludur.

Genel agent yerine domain-spesifik agent tercih edilmelidir:
- .NET backend işleri → "Senior Dotnet Dev." agent
- Java modernizasyonu → "modernize-java" agent  
- Codebase araştırması → "Explore" agent

> İmplementasyona başlamadan önce hangi agent ile çalışılacağını belirle ve o agent ile devam et.

### 2) KLASÖR YAPISI (ZORUNLU)

Her servis kendi klasoru altinda asagidaki yapiya gore organize edilir:

- src/{ServiceName}/API
- src/{ServiceName}/Application
- src/{ServiceName}/Domain
- src/{ServiceName}/Infrastructure

Yeni bir servis eklendiginde bu yapiya uygun klasorleme zorunludur.

Aspire orkestrasyon katmanı aşağıdaki sabit konumlarda bulunur ve bu yapıya dokunulmaz:

- src/AppHost — Tüm servisleri orkestre eden AppHost projesi
- src/ServiceDefaults — Ortak health check, OpenTelemetry ve service discovery konfigürasyonu
