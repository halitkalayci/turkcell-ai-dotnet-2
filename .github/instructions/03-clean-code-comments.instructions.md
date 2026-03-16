---
name: Clean Code - Yorumlar
description: Robert C. Martin 'Clean Code' Chapter 4 — Comments. C# kodunda hangi yorumların zorunlu, hangilerinin yasak olduğuna dair kurallar.
applyTo: "**/*.cs"
---

# Clean Code: Yorumlar (Chapter 4)

**Kaynak:** Robert C. Martin — *Clean Code: A Handbook of Agile Software Craftsmanship*, Chapter 4.

**Temel prensip:** Yorum, kodla ifade edilemeyen bir şeyi açıklamak için son çaredir. Yorum ihtiyacı çoğunlukla kodun kendisini geliştirme fırsatıdır.

---

## 1. İYİ Yorumlar (Kabul Edilir)

### 1.1 Hukuki Yorumlar

Lisans veya telif hakkı bildirimleri dosya başına yerleştirilebilir.

```csharp
// Copyright (c) 2026 Turkcell. All rights reserved.
// Licensed under the MIT License.
```

### 1.2 Niyet Açıklaması

Bir kararın *neden* alındığını açıklayan yorum kodda kalabilir.

```csharp
// EF Core, owned entity'leri ayrı tablo yerine aynı tabloya map eder;
// adres bilgisini ayrı tabloya taşımak için bu konfigürasyon gereklidir.
builder.OwnsOne(p => p.Address, addressBuilder =>
{
    addressBuilder.ToTable("ProductAddresses");
});
```

### 1.3 Uyarı Yorumları

Diğer geliştiricileri ciddi bir sonuç konusunda uyarır.

```csharp
// UYARI: Bu metod thread-safe değildir.
// Paralel senaryolarda ConcurrentProductCache kullan.
public Product GetFromCache(Guid id) { }
```

### 1.4 TODO Yorumları

Henüz yapılmayacak ama yapılması gereken işleri işaretler. IDE tarafından takip edilebilir. Sonsuza kadar bırakılmaz.

```csharp
// TODO: Fiyat hesaplamasına KDV desteği eklenecek — #Issue-142
public decimal CalculatePrice(decimal basePrice) => basePrice;
```

---

## 2. KÖTÜ Yorumlar (Yasak)

### 2.1 Gereksiz / Tekrarlayan Yorumlar

Kodu olduğu gibi yeniden anlatan yorum bilgi katmaz, gürültü yaratır.

```csharp
// YANLIS
// Ürünü id ile getirir
public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct)
    => await _context.Products.FindAsync(id, ct);

// DOGRU — kod zaten açık; yoruma gerek yok
public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct)
    => await _context.Products.FindAsync(id, ct);
```

### 2.2 Yanıltıcı Yorumlar

Kodun yaptığından farklı bir şey anlatan yorum, kodu yorumdan daha tehlikeli hale getirir.

```csharp
// YANLIS — yorum "ilk ürünü" döndürdüğünü söylüyor; kod tümünü döndürüyor
// İlk aktif ürünü döndürür
public IReadOnlyList<Product> GetActiveProducts() { }
```

### 2.3 Journal (Değişiklik Günlüğü) Yorumları

Versiyon kontrolü bu görevi üstlenir. Kod içinde tarih-isim günlüğü tutulmaz.

```csharp
// YANLIS
// 2025-01-10 ahmet: fiyat validasyonu eklendi
// 2025-03-05 mehmet: stok kontrolü güncellendi
// 2025-11-21 ali: kategori alanı eklendi
public sealed record CreateProductCommand(...);
```

### 2.4 Yorum Satırı Haline Getirilmiş Kod

Devre dışı bırakılmış kod satırları silinir. Gerekirse git geçmişinden alınır.

```csharp
// YANLIS
public decimal CalculateDiscount(decimal price)
{
    // var oldDiscount = price * 0.05m;
    // if (oldDiscount > 100) oldDiscount = 100;
    return price * 0.10m;
}

// DOGRU
public decimal CalculateDiscount(decimal price) => price * 0.10m;
```

### 2.5 Konuma İşaret Eden Yorumlar (Banner)

```csharp
// YANLIS
////////////////////////////////////////////////////
// Properties
////////////////////////////////////////////////////
public string Name { get; private set; }

////////////////////////////////////////////////////
// Methods
////////////////////////////////////////////////////
public void UpdateName(string name) { }
```

### 2.6 Kapanan Parantez Yorumları

Metod kısalsa kapanış parantezine yorum gerekmez.

```csharp
// YANLIS
public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct)
{
    if (_cache.IsValid)
    {
        return _cache.Items;
    } // if cache valid
    return await _repository.GetAllAsync(ct);
} // GetAllAsync
```

### 2.7 Yorumla Açıklamak Yerine Kodu İyileştir

```csharp
// YANLIS — yorum kötü isimlendirmeyi telafi ediyor
// bs: birim sayısı, up: birim fiyatı
public decimal Calc(int bs, decimal up) => bs * up;

// DOGRU — yorum yok, isimler açıklıyor
public decimal CalculateTotalPrice(int unitCount, decimal unitPrice)
    => unitCount * unitPrice;
```

---

## 3. XML Dokümantasyon Yorumları

`///` XML yorumları yalnızca **public API** sınır noktalarında (public library'ler, SDK metodları) kullanılır. Internal uygulama kodu için zorunlu değildir; anlamsız yere eklenmez.

```csharp
// Kabul edilir — public API
/// <summary>
/// Belirtilen ID'ye sahip ürünü getirir.
/// </summary>
/// <param name="id">Ürün kimliği.</param>
/// <exception cref="NotFoundException">Ürün bulunamazsa fırlatılır.</exception>
public async Task<ProductDto> GetByIdAsync(Guid id, CancellationToken ct) { }

// Yasak — internal handler; XML yorum ekleme
/// <summary>Ürün oluşturur.</summary>  ← gereksiz
internal sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{ }
```
