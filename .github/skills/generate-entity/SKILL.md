---
name: generate-entity
description: Dotnet uygulamalarında kurumsal standartlara uygun veritabanı nesnesini temsil eden C# Entity kodunu oluşturan ve/veya varolan bu kodlara bu standartları uygulayan yetenek.
argument-hint: Entity adı ve alanları (örn. Product - Name, Price, StockCount)
---

# generate-entity

## Amaç

Bu yetenek, .NET mikro-servis projelerinde **Domain katmanına** ait C# `Entity` sınıflarını kurumsal standartlara ve Clean Code prensiplerine uygun biçimde üretir veya mevcut entity'leri bu standartlara uyarlar.

---

## 1. Temel Kavram

Entity; kimlik (Id) ile ayırt edilen, durumu ve yaşam döngüsü olan domain nesnesidir. Sadece veri taşımaz; iş kurallarını da barındırır.

### 1.1 Yerleşim

```
src/{ServiceName}/Domain/
  Entities/
    Product.cs
    OrderItem.cs
  Abstractions/
    BaseEntity.cs
```

---

## 2. BaseEntity

Tüm entity'ler `BaseEntity<TId>` sınıfından türer.

```csharp
// BaseEntity.cs
public abstract class BaseEntity<TId>
{
    public TId Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    protected BaseEntity(TId id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    // EF Core zorunluluğu
    protected BaseEntity() { }

    protected void MarkUpdated() => UpdatedAt = DateTime.UtcNow;

    public void SoftDelete()
    {
        IsDeleted = true;
        MarkUpdated();
    }
}
```

**Kural:** Kimlik tipi varsayılan olarak `Guid`'dir. Farklı bir tip yalnızca geçerli bir gerekçeyle tercih edilebilir.

---

## 3. Entity Kod Yapısı

### 3.1 Bölüm Sırası (zorunlu)

```
1. private field'lar
2. private/protected EF constructor
3. private primary constructor
4. public static factory method (Create)
5. public property'ler (private set)
6. domain method'lar
```

### 3.2 Tam Şablon

```csharp
// Product.cs
public sealed class Product : BaseEntity<Guid>
{
    // 1. Fields
    private readonly List<ProductImage> _images = [];

    // 2. EF Constructor
    private Product() { }

    // 3. Primary Constructor
    private Product(Guid id, string name, decimal price, int stockCount)
        : base(id)
    {
        Name = name;
        Price = price;
        StockCount = stockCount;
    }

    // 4. Factory Method
    public static Product Create(string name, decimal price, int stockCount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);
        ArgumentOutOfRangeException.ThrowIfNegative(stockCount);

        return new Product(Guid.NewGuid(), name, price, stockCount);
    }

    // 5. Properties
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public int StockCount { get; private set; }
    public IReadOnlyList<ProductImage> Images => _images.AsReadOnly();

    // 6. Domain Methods
    public void UpdatePrice(decimal newPrice)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newPrice);
        Price = newPrice;
        MarkUpdated();
    }

    public void AddImage(ProductImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        _images.Add(image);
        MarkUpdated();
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity > StockCount)
            throw new InvalidOperationException("Yetersiz stok.");

        StockCount -= quantity;
        MarkUpdated();
    }
}
```

---

## 4. Adlandırma Kuralları

| Konu | Kural | Dogru | Yanlis |
|---|---|---|---|
| Sinif adi | PascalCase, tekil isim | `Product` | `Products`, `productEntity` |
| Property | PascalCase | `StockCount` | `stockCount`, `stock_count` |
| Private field | `_camelCase` | `_images` | `images`, `mImages` |
| Factory method | `Create` | `Product.Create(...)` | `new Product(...)` (public) |
| Domain method | Fiil + nesne | `UpdatePrice`, `AddImage` | `SetPrice`, `DoChangePrice` |
| Soft-delete | `SoftDelete()` | `product.SoftDelete()` | `product.IsDeleted = true` |

---

## 5. Encapsulation Kuralları

### 5.1 Property Setter Erişimi

```csharp
// DOGRU
public string Name { get; private set; }

// YANLIS: dışarıdan doğrudan atama mümkün
public string Name { get; set; }

// YANLIS: init setter EF ile runtime sorunlarına yol açar
public string Name { get; init; }
```

### 5.2 Koleksiyon Sarmalama

```csharp
// DOGRU
private readonly List<OrderItem> _items = [];
public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

// YANLIS: koleksiyonu doğrudan expose etme
public List<OrderItem> Items { get; set; } = [];
```

### 5.3 Constructor Erişimi

```csharp
// DOGRU
private Product() { }
private Product(Guid id, string name, decimal price) : base(id) { ... }
public static Product Create(string name, decimal price) => new(Guid.NewGuid(), name, price);

// YANLIS: public constructor nesneyi kural dışı durumda bırakabilir
public Product(string name, decimal price) { Name = name; Price = price; }
```

---

## 6. Domain Doğrulama

Doğrulama yalnızca factory method ve domain method içinde, **BCL guard yöntemleriyle** yapılır.

```csharp
// DOGRU: BCL guard
public static Order Create(Guid customerId, string address)
{
    if (customerId == Guid.Empty)
        throw new ArgumentException("CustomerId boş olamaz.", nameof(customerId));

    ArgumentException.ThrowIfNullOrWhiteSpace(address);

    return new Order(Guid.NewGuid(), customerId, address);
}

// YANLIS: entity içinde FluentValidation veya external servis
public static Order Create(string address)
{
    var validator = new OrderValidator();
    validator.ValidateAndThrow(address); // entity bir servise bağımlı olmamalı
    ...
}
```

---

## 7. Enum Property

```csharp
// OrderStatus.cs
public enum OrderStatus
{
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}

// Order.cs
public sealed class Order : BaseEntity<Guid>
{
    private Order() { }

    private Order(Guid id, Guid customerId) : base(id)
    {
        CustomerId = customerId;
        Status = OrderStatus.Pending; // başlangıç durumu factory'de atanır
    }

    public static Order Create(Guid customerId)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException(nameof(customerId));

        return new Order(Guid.NewGuid(), customerId);
    }

    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }

    public void Ship()
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationException("Yalnızca işlemdeki sipariş gönderilebilir.");

        Status = OrderStatus.Shipped;
        MarkUpdated();
    }

    public void Cancel()
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered)
            throw new InvalidOperationException("Gönderilmiş sipariş iptal edilemez.");

        Status = OrderStatus.Cancelled;
        MarkUpdated();
    }
}
```

---

## 8. Navigation Property Kuralları

```csharp
// DOGRU: foreign key + navigation; virtual yok
public Guid CategoryId { get; private set; }
public Category Category { get; private set; }

// YANLIS: virtual — lazy loading açık hata kaynağıdır
public virtual Category Category { get; set; }

// YANLIS: navigation property public set
public Category Category { get; set; }
```

---

## 9. sealed Kullanımı

```csharp
// DOGRU: hiyerarşi yoksa sealed
public sealed class Product : BaseEntity<Guid> { }

// DOGRU: hiyerarşi varsa abstract base + sealed leaf
public abstract class Payment : BaseEntity<Guid> { }
public sealed class CreditCardPayment : Payment { }
public sealed class BankTransferPayment : Payment { }

// YANLIS: gereksiz kalıtıma açık bırakma
public class Product : BaseEntity<Guid> { }
```

---

## 10. Tam Örnek — OrderItem

```csharp
// OrderItem.cs
public sealed class OrderItem : BaseEntity<Guid>
{
    private OrderItem() { }

    private OrderItem(Guid id, Guid orderId, Guid productId, int quantity, decimal unitPrice)
        : base(id)
    {
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public static OrderItem Create(Guid orderId, Guid productId, int quantity, decimal unitPrice)
    {
        if (orderId == Guid.Empty) throw new ArgumentException(nameof(orderId));
        if (productId == Guid.Empty) throw new ArgumentException(nameof(productId));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(unitPrice);

        return new OrderItem(Guid.NewGuid(), orderId, productId, quantity, unitPrice);
    }

    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    // Hesaplanan property: değer store edilmez
    public decimal TotalPrice => Quantity * UnitPrice;

    public void ChangeQuantity(int newQuantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newQuantity);
        Quantity = newQuantity;
        MarkUpdated();
    }
}
```

---

## 11. Kontrol Listesi

Entity kodu üretildikten veya uyarlandıktan sonra aşağıdaki tüm maddeler karşılanmalıdır:

- [ ] `BaseEntity<Guid>` miras alınmış
- [ ] Public constructor yok; `private` EF constructor var
- [ ] `Create(...)` factory method var; tüm zorunlu alanlar parametre
- [ ] Tüm property'ler `private set`
- [ ] Koleksiyonlar `IReadOnlyList<T>` olarak expose ediliyor
- [ ] Domain method'lar durum değişikliğinde `MarkUpdated()` çağırıyor
- [ ] Doğrulama BCL guard ile yapılıyor
- [ ] Navigation property'ler `virtual` değil
- [ ] Gerekli olmayan yerlerde `sealed` kullanılmış
- [ ] Dosya `src/{ServiceName}/Domain/Entities/` altında konumlanmış