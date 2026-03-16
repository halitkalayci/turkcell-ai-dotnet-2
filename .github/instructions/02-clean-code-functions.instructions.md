---
name: Clean Code - Fonksiyonlar
description: Robert C. Martin 'Clean Code' Chapter 3 — Functions. C# metodlarının boyutu, sorumluluğu, parametreleri ve yapısı için zorunlu kurallar.
applyTo: "**/*.cs"
---

# Clean Code: Fonksiyonlar (Chapter 3)

**Kaynak:** Robert C. Martin — *Clean Code: A Handbook of Agile Software Craftsmanship*, Chapter 3.

---

## 1. Küçük Olmalı

Bir method küçük olmalı; daha da küçük olmalı. Maksimum uzunluk için kesin bir sayı yoktur; ancak 20 satırı geçen herhangi bir method bölünme adayıdır.

```csharp
// YANLIS — cok uzun, birden fazla is yapan method
public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
{
    if (request == null) return BadRequest();
    if (string.IsNullOrEmpty(request.CustomerId)) return BadRequest("Customer required");

    var customer = await _db.Customers.FindAsync(request.CustomerId);
    if (customer == null) return NotFound("Customer not found");

    decimal total = 0;
    foreach (var item in request.Items)
    {
        var product = await _db.Products.FindAsync(item.ProductId);
        if (product == null) return NotFound($"Product {item.ProductId} not found");
        if (product.Stock < item.Quantity) return BadRequest("Insufficient stock");
        total += product.Price * item.Quantity;
        product.Stock -= item.Quantity;
    }
    // ... devam ediyor
}

// DOGRU — her method tek bir isi yapar
public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken ct)
{
    var customer = await GetCustomerOrThrowAsync(request.CustomerId, ct);
    var orderLines = await BuildOrderLinesAsync(request.Items, ct);
    var order = Order.Create(customer.Id, orderLines);

    await _orderRepository.AddAsync(order, ct);
    await _unitOfWork.SaveChangesAsync(ct);

    return order.Id;
}
```

---

## 2. Tek Bir İş Yap (SRP)

Bir fonksiyon yalnızca bir iş yapmalı, onu iyi yapmalı ve yalnızca onu yapmalıdır. Fonksiyonun birden fazla bölüme ayrılabilmesi tek işten fazlasını yaptığının göstergesidir.

```csharp
// YANLIS — dogrulama + is mantigi + kaydetme bir arada
public async Task ProcessPayment(PaymentRequest request)
{
    // Dogrulama
    if (request.Amount <= 0) throw new ArgumentException("Invalid amount");
    if (string.IsNullOrEmpty(request.CardNumber)) throw new ArgumentException("Card required");

    // Is mantigi
    var fee = CalculateTransactionFee(request.Amount);
    var total = request.Amount + fee;

    // Kaydetme
    var payment = new Payment(request.CardNumber, total);
    await _repository.AddAsync(payment);
    await _unitOfWork.SaveChangesAsync();
}

// DOGRU — her method tek sorumluluk
public async Task<Guid> Handle(ProcessPaymentCommand request, CancellationToken ct)
{
    var totalAmount = CalculateTotalWithFee(request.Amount);
    var payment = Payment.Create(request.CardNumber, totalAmount);

    await _paymentRepository.AddAsync(payment, ct);
    await _unitOfWork.SaveChangesAsync(ct);

    return payment.Id;
}

private decimal CalculateTotalWithFee(decimal amount)
{
    var fee = amount * TransactionFeeRate;
    return amount + fee;
}
```

---

## 3. Fonksiyon Başına Tek Soyutlama Seviyesi

Yüksek seviyeli kavramları (iş mantığı) düşük seviyeli detaylarla (string işleme, bit operasyonları) karıştırma.

```csharp
// YANLIS — farkli soyutlama seviyeleri karisik
public Order BuildOrder(CreateOrderCommand command)
{
    // Yuksek seviye
    var customer = _customerRepository.GetById(command.CustomerId);

    // Dusuk seviye detay — burada olmamali
    var addressParts = command.ShippingAddress.Split(',');
    var street = addressParts[0].Trim();
    var city = addressParts[1].Trim();
    var address = new Address(street, city);

    return Order.Create(customer.Id, address);
}

// DOGRU — ayni soyutlama seviyesi
public Order BuildOrder(CreateOrderCommand command)
{
    var customer = _customerRepository.GetById(command.CustomerId);
    var shippingAddress = Address.Parse(command.ShippingAddress);
    return Order.Create(customer.Id, shippingAddress);
}
```

---

## 4. Parametre Sayısı

Sıfır parametre idealdir. Bir parametre iyidir. İki parametre kabul edilebilir. Üç ve üzeri parametre zorunlu gerekçe gerektirir. Üçten fazlası yasaktır.

```csharp
// YANLIS — cok fazla parametre
public Product Create(string name, decimal price, int stock, string category,
                      string description, bool isActive, DateTime createdAt)

// DOGRU — parametre nesnesi kullan
public sealed record CreateProductCommand(
    string Name,
    decimal Price,
    int Stock,
    string Category,
    string Description);

public Product Handle(CreateProductCommand command) { }
```

---

## 5. Flag Parametrelerinden Kaçın

Boolean flag parametresi fonksiyonun birden fazla iş yaptığının işaretidir. İki ayrı fonksiyon yaz.

```csharp
// YANLIS
public void RenderProduct(Product product, bool includeDetails) { }

// DOGRU
public void RenderProductSummary(Product product) { }
public void RenderProductDetail(Product product) { }
```

---

## 6. Komut-Sorgu Ayrımı (CQS)

Bir fonksiyon bir işlem ya yapar ya da bir değer döndürür; ikisini birden yapmaz.

```csharp
// YANLIS — hem state degistiriyor hem bool donduruyor
public bool AddToStock(Product product, int quantity)
{
    if (product.IsActive)
    {
        product.IncreaseStock(quantity);
        return true;
    }
    return false;
}

// DOGRU — sorgula, sonra isle
public bool CanAddToStock(Product product) => product.IsActive;

public void AddToStock(Product product, int quantity)
{
    product.IncreaseStock(quantity);
}
```

---

## 7. Hata Kodu Döndürmek Yerine Exception Fırlat

Hata kodu döndürmek çağıran kodu if-else zincirine zorlar. Exception kullan; bu Clean Architecture'daki global middleware kuralıyla da uyumludur.

```csharp
// YANLIS
public int DeleteProduct(Guid id)
{
    var product = _repository.GetById(id);
    if (product == null) return -1;
    if (!product.CanBeDeleted()) return -2;

    _repository.Remove(product);
    return 0;
}

// DOGRU
public void DeleteProduct(Guid id)
{
    var product = _repository.GetById(id)
        ?? throw new NotFoundException(nameof(Product), id);

    if (!product.CanBeDeleted())
        throw new BusinessRuleViolationException("Product has active orders and cannot be deleted.");

    _repository.Remove(product);
}
```

---

## 8. Kendini Tekrar Etme (DRY)

Kopyala-yapıştır kod, soyutlama fırsatı kaçırıldığının göstergesidir.

```csharp
// YANLIS — tekrar eden null/not-found kontrolleri her handler'da
var product = await _productRepository.GetByIdAsync(id, ct);
if (product == null)
    throw new NotFoundException(nameof(Product), id);

// DOGRU — extension method veya yardimci metod
public static class RepositoryExtensions
{
    public static async Task<T> GetByIdOrThrowAsync<T>(
        this IRepository<T> repository,
        Guid id,
        CancellationToken ct = default) where T : BaseEntity<Guid>
    {
        return await repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(typeof(T).Name, id);
    }
}
```
