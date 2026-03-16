---
name: Clean Code - Nesneler ve Veri Yapıları
description: Robert C. Martin 'Clean Code' Chapter 6 — Objects and Data Structures. C# sınıflarında veri soyutlaması, Law of Demeter ve DTO kullanım kuralları.
applyTo: "**/*.cs"
---

# Clean Code: Nesneler ve Veri Yapıları (Chapter 6)

**Kaynak:** Robert C. Martin — *Clean Code: A Handbook of Agile Software Craftsmanship*, Chapter 6.

---

## 1. Veri Soyutlaması

İç veri yapısını dışarıya aynen yansıtan property'ler somut soyutlamadır; soyutlama değil. Sınıf, verinin nasıl saklandığını gizlemeli; ne anlama geldiğini ortaya çıkarmalıdır.

```csharp
// YANLIS — ici disari sizmis; depo detayi ortada
public class Vehicle
{
    public double FuelTankCapacityInGallons { get; set; }
    public double GallonsOfGasoline { get; set; }
}
// Kullanim: vehicle.GallonsOfGasoline / vehicle.FuelTankCapacityInGallons

// DOGRU — soyut arayuz; depolama detayi gizli
public interface IVehicle
{
    double GetFuelLevelPercentage();  // 0.0 - 1.0
    void FillTank(double liters);
}
```

```csharp
// YANLIS — property getter/setter ile ham veriyi ifsa eden entity
public class Product
{
    public decimal Price { get; set; }
    public decimal DiscountRate { get; set; }
}
// Kullanim: var net = p.Price * (1 - p.DiscountRate);  ← is mantigi disarida

// DOGRU — domain davranisi kapsullennis
public sealed class Product : BaseEntity<Guid>
{
    public decimal Price { get; private set; }
    private decimal _discountRate;

    public decimal GetNetPrice() => Price * (1 - _discountRate);

    public void ApplyDiscount(decimal rate)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(rate);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(rate, 1m);
        _discountRate = rate;
    }
}
```

---

## 2. Nesne / Veri Yapısı Anti-Simetrisi

| | **Nesne (Object)** | **Veri Yapısı (Data Structure)** |
|---|---|---|
| Veri | Gizli (private) | Açık (public) |
| Davranış | Metod aracılığıyla | Dışarıda işlenir |
| Yeni tip eklemek | Kolay (yeni sınıf) | Zor (tüm fonksiyonlar güncellenir) |
| Yeni fonksiyon eklemek | Zor (tüm sınıflar güncellenir) | Kolay |

**Kural:** Domain katmanındaki entity'ler **nesne** davranışı sergiler (davranış içerir, veriyi gizler). Application katmanındaki DTO'lar ve Command/Query record'ları **veri yapısı** rolündedir (veri taşır, davranış içermez).

```csharp
// Domain — nesne; davranisini kendisi yonetir
public sealed class Order : BaseEntity<Guid>
{
    private readonly List<OrderLine> _lines = [];

    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();

    public void AddLine(Product product, int quantity)
    {
        if (_lines.Any(l => l.ProductId == product.Id))
            throw new BusinessRuleViolationException("Product already in order.");

        _lines.Add(OrderLine.Create(product.Id, product.Price, quantity));
    }

    public decimal GetTotal() => _lines.Sum(l => l.Subtotal);
}

// Application — veri yapisi; DTO salt veri tasir
public sealed record OrderDto(
    Guid Id,
    IReadOnlyList<OrderLineDto> Lines,
    decimal Total);
```

---

## 3. Demeter Yasası (Law of Demeter)

Bir metod yalnızca şunların metodlarını çağırabilir:
1. Kendi sınıfının metodları
2. Parametre olarak aldığı nesnelerin metodları
3. Oluşturduğu nesnelerin metodları
4. Doğrudan sahip olduğu bileşen nesnelerinin metodları

"Yabancılarla konuşma; yalnızca arkadaşlarınla konuş." Zincirleme çağrılar (`a.GetB().GetC().DoSomething()`) genellikle bu yasayı ihlal eder.

```csharp
// YANLIS — "tren kazasi" (train wreck) — birden fazla noktayi geciyor
public string GetCustomerCityFromOrder(Order order)
{
    return order.GetCustomer().GetAddress().GetCity().ToUpperInvariant();
}

// DOGRU — Order nesnesi gereken bilgiyi expose ediyor
public string GetCustomerCityFromOrder(Order order)
{
    return order.GetShippingCity();
}

// Order icinde:
public string GetShippingCity() => _shippingAddress.City.ToUpperInvariant();
```

```csharp
// YANLIS — handler, repository'nin ic yapisini manipule ediyor
public async Task Handle(UpdateProductCommand request, CancellationToken ct)
{
    var product = await _productRepository.GetByIdAsync(request.Id, ct);
    product.Price = request.Price;  // dogrudan field manipulasyonu degil ama...
    _productRepository.Context.SaveChanges();  // ← repository'nin Context'ine erisiyor!
}

// DOGRU — her nesne kendi sorumluluğunu yerine getirir
public async Task Handle(UpdateProductCommand request, CancellationToken ct)
{
    var product = await _productRepository.GetByIdOrThrowAsync(request.Id, ct);
    product.UpdatePrice(request.Price);
    await _unitOfWork.SaveChangesAsync(ct);
}
```

---

## 4. DTO Kuralları

DTO'lar salt veri transfer nesnesidir; iş mantığı içermez.

```csharp
// YANLIS — DTO icinde is mantigi
public sealed record ProductDto(Guid Id, decimal Price, decimal DiscountRate)
{
    public decimal NetPrice => Price * (1 - DiscountRate);  // is mantigi DTO'ya sizmis
}

// DOGRU — DTO salt veri tasir; hesaplama kaynakta yapilip sonuc tasinir
public sealed record ProductDto(
    Guid Id,
    string Name,
    decimal Price,
    decimal NetPrice,   // handler tarafindan hesaplanarak doldurulur
    int Stock);
```

```csharp
// Handler icinde mapping — explicit, AutoMapper yok
public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken ct)
{
    var product = await _productRepository.GetByIdOrThrowAsync(request.Id, ct);

    return new ProductDto(
        product.Id,
        product.Name,
        product.Price,
        product.GetNetPrice(),
        product.Stock);
}
```

---

## 5. Active Record Anti-Pattern

Active Record (veri + CRUD metodları birlikte) domain entity'lerde kullanılmaz. Entity, veri erişim kodunu bilmez.

```csharp
// YANLIS — entity kendi kaydetme metodunu biliyor
public class Product
{
    public void Save() => Database.Execute("INSERT INTO Products...");
    public void Delete() => Database.Execute("DELETE FROM Products...");
}

// DOGRU — veri erisimi repository'de; entity temiz kaliyor
public sealed class Product : BaseEntity<Guid>
{
    // Sadece domain davranislari
    public void UpdatePrice(decimal newPrice) { }
    public bool IsAvailable() => Stock > 0 && IsActive;
}
```
