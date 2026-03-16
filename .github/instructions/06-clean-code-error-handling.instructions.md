---
name: Clean Code - Hata Yönetimi
description: Robert C. Martin 'Clean Code' Chapter 7 — Error Handling. C# kodunda exception tabanlı hata yönetimi, null döndürmeme ve hata bağlamı kuralları.
applyTo: "**/*.cs"
---

# Clean Code: Hata Yönetimi (Chapter 7)

**Kaynak:** Robert C. Martin — *Clean Code: A Handbook of Agile Software Craftsmanship*, Chapter 7.

**Not:** Bu kurallar projedeki Clean Architecture'ın exception tabanlı hata yönetimi kararıyla (`00-architecture.instructions.md`, Bölüm 3.3 ve 6.2) tam uyumludur.

---

## 1. Hata Kodu Değil, Exception Kullan

Hata kodu döndürmek çağıran kodu if-else zinciriyle kirlitir. Exception fırlat; temizlik akışı ayrıştırılır.

```csharp
// YANLIS — hata kodu donduruluyor
public enum DeleteResult { Success, NotFound, HasActiveOrders }

public async Task<DeleteResult> DeleteProductAsync(Guid id, CancellationToken ct)
{
    var product = await _repository.GetByIdAsync(id, ct);
    if (product is null) return DeleteResult.NotFound;
    if (product.HasActiveOrders()) return DeleteResult.HasActiveOrders;

    _repository.Remove(product);
    await _unitOfWork.SaveChangesAsync(ct);
    return DeleteResult.Success;
}

// Cagiran kod kirleniyor:
var result = await DeleteProductAsync(id, ct);
if (result == DeleteResult.NotFound) { /* ... */ }
else if (result == DeleteResult.HasActiveOrders) { /* ... */ }

// DOGRU — exception firlatilir; cagiran kod temiz kalir
public async Task DeleteProductAsync(Guid id, CancellationToken ct)
{
    var product = await _repository.GetByIdAsync(id, ct)
        ?? throw new NotFoundException(nameof(Product), id);

    if (product.HasActiveOrders())
        throw new BusinessRuleViolationException("Bu ürünün aktif siparişleri var; silinemez.");

    _repository.Remove(product);
    await _unitOfWork.SaveChangesAsync(ct);
}
```

---

## 2. Try-Catch-Finally ile Başla

Dış kaynaklara erişen (I/O, ağ, veritabanı) kodda önce try-catch-finally yaz. Bu sayede kaynak sızıntısı riski önlenir ve hata bağlamı kaybolmaz.

```csharp
// YANLIS — harici kaynak; exception yonetimi yok
public async Task<string> ReadConfigAsync(string path)
{
    var stream = File.OpenRead(path);
    using var reader = new StreamReader(stream);
    return await reader.ReadToEndAsync();
}

// DOGRU — try ile kaynak yuklenir; hata context ile yeniden firlatilir
public async Task<string> ReadConfigAsync(string path)
{
    try
    {
        await using var stream = File.OpenRead(path);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
    catch (FileNotFoundException ex)
    {
        throw new ConfigurationException($"Konfigürasyon dosyası bulunamadı: {path}", ex);
    }
}
```

---

## 3. Exception ile Bağlam Sağla

Exception mesajı hatanın ne olduğunu, nerede oluştuğunu ve neden oluştuğunu açıklamalıdır. Inner exception zinciri korunmalıdır.

```csharp
// YANLIS — bağlam yok
throw new Exception("Hata oluştu.");

// YANLIS — inner exception kayboldu
try { /* ... */ }
catch (Exception) { throw new ServiceException("İşlem başarısız."); }

// DOGRU — bağlam var, inner exception zinciri korundu
try
{
    await _emailClient.SendAsync(message, ct);
}
catch (SmtpException ex)
{
    throw new NotificationFailedException(
        $"Sipariş onay e-postası gönderilemedi. Alıcı: {message.To}, SiparişId: {orderId}",
        ex);
}
```

---

## 4. Çağırana Göre Exception Sınıflandır

Kütüphane veya dış servis exception'larını doğrudan yaymak yerine, çağıranın anlayacağı soyutlama seviyesinde sarmalayıcı exception kullan.

```csharp
// YANLIS — üçüncü taraf kütüphane exception'ı doğrudan yayılıyor
public async Task<PaymentResult> ChargeAsync(PaymentRequest request, CancellationToken ct)
{
    return await _stripeClient.CreateChargeAsync(request.CardToken, request.Amount);
    // StripeException caller'a sızıyor — uygulama Stripe'a bağımlı hale gelir
}

// DOGRU — sarmalanmış; caller, Stripe'ı bilmek zorunda değil
public async Task<PaymentResult> ChargeAsync(PaymentRequest request, CancellationToken ct)
{
    try
    {
        return await _stripeClient.CreateChargeAsync(request.CardToken, request.Amount);
    }
    catch (StripeException ex) when (ex.StripeError?.Type == "card_error")
    {
        throw new PaymentDeclinedException(ex.StripeError.Message, ex);
    }
    catch (StripeException ex)
    {
        throw new PaymentGatewayException("Ödeme altyapısı hatası.", ex);
    }
}
```

---

## 5. Normal Akışı Tanımla (Special Case Pattern)

Hata durumu gerçek bir hata değil; beklenen bir durumsa (boş liste, bulunamayan opsiyonel kaynak), null veya exception döndürmek yerine özel durum nesnesi dön.

```csharp
// YANLIS — null kontrolü çağıran kodu kirletiyor
public async Task<decimal> GetDiscountAsync(Guid customerId, CancellationToken ct)
{
    var loyalty = await _loyaltyRepository.GetByCustomerIdAsync(customerId, ct);
    if (loyalty == null) return 0;  // ← çağıran her yerde null kontrolü
    return loyalty.DiscountRate;
}

// DOGRU — Null Object / Special Case Pattern
public sealed class NullLoyalty : ILoyalty
{
    public decimal DiscountRate => 0m;
    public bool IsActive => false;
}

public async Task<ILoyalty> GetLoyaltyAsync(Guid customerId, CancellationToken ct)
{
    return await _repository.GetByCustomerIdAsync(customerId, ct)
        ?? new NullLoyalty();
}
```

---

## 6. Null Döndürme

Metod null döndürürse çağıran kod null kontrolüyle kirlenir ve `NullReferenceException` riski artar.

```csharp
// YANLIS
public List<Product> GetActiveProducts()
{
    if (_cache == null) return null;  // ← çağıran null kontrolü yapmak zorunda
    return _cache.ActiveProducts;
}

// DOGRU — bos koleksiyon don
public IReadOnlyList<Product> GetActiveProducts()
{
    if (_cache == null) return [];
    return _cache.ActiveProducts;
}
```

**Kural:** Koleksiyon döndüren metodlar `null` döndürmez; boş koleksiyon döndürür. Tekil nesne döndüran metodlar `T?` (nullable) ile imzalanır ve çağıran `?? throw` ile ele alır.

---

## 7. Null Geçirme

Metoda null geçirmek null döndürmekten daha kötüdür; metodun içini savunmacı kontrollerle doldurur.

```csharp
// YANLIS — null gecirilebilir
public decimal CalculateTax(Product product)
{
    if (product == null) throw new ArgumentNullException(nameof(product));
    return product.Price * 0.18m;
}

// DOGRU — nullable olmayan referans tipi; null gecirilemez
// Proje genelinde <Nullable>enable</Nullable> aktif edilir.
public decimal CalculateTax(Product product)  // product null gelemez; compiler garanti eder
    => product.Price * 0.18m;
```

**Kural:** Proje dosyalarında `<Nullable>enable</Nullable>` zorunludur. Nullable reference type uyarılarını suppress etmek için `!` (null-forgiving) operatörü gerekçesiz kullanılmaz.
